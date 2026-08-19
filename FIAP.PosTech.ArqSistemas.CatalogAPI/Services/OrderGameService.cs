using FIAP.PosTech.ArqSistemas.CatalogAPI.DTOs;
using FIAP.PosTech.ArqSistemas.CatalogAPI.Enums;
using FIAP.PosTech.ArqSistemas.CatalogAPI.Models;
using MongoDB.Driver;

namespace FIAP.PosTech.ArqSistemas.CatalogAPI.Services
{
    public class OrderGameService : IOrderGameService     
    {
        private readonly IMongoCollection<Order> _orderCollection;
        private readonly ILogger<OrderGameService> _logger;

        public OrderGameService(IMongoDatabase database, ILogger<OrderGameService> logger)
        {
            _logger = logger;
            _orderCollection = database.GetCollection<Order>("Orders");
        }

        public (bool Sucesso, string Mensagem, Order Order) AlterarStatus(int id, OrderStatus newState)
        {
            var erros = new List<string>();

            // Validar Id obrigatório
            if (id <= 0)
                erros.Add("Id deve ser um número positivo");

            var orderExistente = _orderCollection.Find(o => o.Id == id).FirstOrDefault();
            if (orderExistente == null)
            {
                _logger.LogWarning("Erro ao alterar status: Pedido com Id {Id} não encontrado no MongoDB", id);
                return (false, "Pedido não encontrado", null);
            }

            if (erros.Count > 0)
            {
                var mensagem = string.Join("; ", erros);
                _logger.LogWarning("Erro ao alterar status do pedido {Id}: {Erros}", id, mensagem);
                return (false, mensagem, null);
            }

            orderExistente.Status = newState;
            _orderCollection.ReplaceOne(o => o.Id == id, orderExistente);

            _logger.LogInformation("Status do pedido alterado com sucesso no MongoDB. Id: {Id}, Status: {Status}", orderExistente.Id, orderExistente.Status);

            return (true, "Status do pedido alterado com sucesso", orderExistente);
        }

        public (bool Sucesso, string Mensagem, Order Order) Criar(Order order)
        {
            var erros = new List<string>();

            if (order.Price <= 0)
                erros.Add("Preço é obrigatório e deve ser positivo");

            if (erros.Count > 0)
            {
                var mensagem = string.Join("; ", erros);
                _logger.LogWarning("Erro ao criar pedido: {Erros}", mensagem);
                return (false, mensagem, null);
            }

            var maxOrder = _orderCollection.Find(FilterDefinition<Order>.Empty)
                .SortByDescending(o => o.Id)
                .FirstOrDefault();

            int proximoId = maxOrder != null ? maxOrder.Id + 1 : 1;

            // Criar novo pedido com Id gerado
            var novoOrder = new Order
            {
                Id = proximoId,
                Price = order.Price,
                UserId = order.UserId,  
                GameId = order.GameId,
                Status = OrderStatus.Rejected
            };

            _orderCollection.InsertOne(novoOrder);
            _logger.LogInformation("Pedido criado no MongoDB com sucesso. Id: {Id}, Price: {Price}, UserId: {UserId}, GameId: {GameId}",
                novoOrder.Id, novoOrder.Price, novoOrder.UserId, novoOrder.GameId);

            return (true, "Pedido criado com sucesso", novoOrder);
        }

        public Order ObterPorId(int id)
        {
            var order = _orderCollection.Find(c => c.Id == id).FirstOrDefault();
            if (order == null)
            {
                _logger.LogWarning("Pedido com Id {Id} não encontrado no MongoDB", id);
            }
            else
            {
                _logger.LogInformation("Pedido com Id {Id} encontrado no MongoDB: {Price}", id, order.Price);
            }
            return order;
        }

        public List<Order> ObterTodos()
        {
            var orders = _orderCollection.Find(FilterDefinition<Order>.Empty).ToList();
            _logger.LogInformation("Obtendo todos os pedidos do MongoDB. Total: {Total}", orders.Count);
            return orders;
        }

    }
}
