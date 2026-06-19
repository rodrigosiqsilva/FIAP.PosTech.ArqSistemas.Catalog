using FIAP.PosTech.ArqSistemas.CatalogAPI.Models;
using FIAP.PosTech.ArqSistemas.CatalogAPI.Enums;

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
    }
}
