using FIAP.PosTech.ArqSistemas.CatalogAPI.DTOs;
using FIAP.PosTech.ArqSistemas.CatalogAPI.Enums;
using FIAP.PosTech.ArqSistemas.CatalogAPI.Models;

namespace FIAP.PosTech.ArqSistemas.CatalogAPI.Services
{
    public class OrderGameService : IOrderGameService     
    {

        private readonly ILogger<OrderGameService> _logger;
        private static int _proximoId = 1;
        private static readonly List<Order> _order = new List<Order>();

        public OrderGameService(ILogger<OrderGameService> logger)
        {
            _logger = logger;
        }

        public (bool Sucesso, string Mensagem, Order Order) Aprovar(int id)
        {
            var erros = new List<string>();

            // Validar Id obrigatório
            if (id <= 0)
                erros.Add("Id deve ser um número positivo");

            var orderExistente = _order.FirstOrDefault(o => o.Id == id);
            if (orderExistente == null)
            {
                _logger.LogWarning("Erro ao aprovar: Pedido com Id {Id} não encontrado", id);
                return (false, "Pedido não encontrado", null);
            }

            if (erros.Count > 0)
            {
                var mensagem = string.Join("; ", erros);
                _logger.LogWarning("Erro ao aprovar pedido {Id}: {Erros}", id, mensagem);
                return (false, mensagem, null);
            }

            orderExistente.Status = OrderStatus.Approved;

            _logger.LogInformation("Pedido aprovado com sucesso. Id: {Id}, Status: {Status}", orderExistente.Id, orderExistente.Status);

            return (true, "Pedido aprovado com sucesso", orderExistente);
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

            // Criar novo pedido com Id gerado
            var novoOrder = new Order
            {
                Id = _proximoId++,
                Price = order.Price,
                UserId = order.UserId,  
                GameId = order.GameId,
                Status = OrderStatus.Rejected
            };

            _order.Add(novoOrder);
            _logger.LogInformation("Pedido criado com sucesso. Id: {Id}, Price: {Price}, UserId: {UserId}, GameId: {GameId}",
                novoOrder.Id, novoOrder.Price, novoOrder.UserId, novoOrder.GameId);

            return (true, "Pedido criado com sucesso", novoOrder);
        }

        public Order ObterPorId(int id)
        {
            var order = _order.FirstOrDefault(c => c.Id == id);
            if (order == null)
            {
                _logger.LogWarning("Pedido com Id {Id} não encontrado", id);
            }
            else
            {
                _logger.LogInformation("Pedido com Id {Id} encontrado: {Price}", id, order.Price);
            }
            return order;
        }

        public List<Order> ObterTodos()
        {
            _logger.LogInformation("Obtendo todos os pedidos. Total: {Total}", _order.Count);
            return _order.ToList();
        }

    }
}
