namespace FIAP.PosTech.ArqSistemas.CatalogAPI.Models
{

    using FIAP.PosTech.ArqSistemas.CatalogAPI.Enums;

    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int GameId { get; set; }
        public decimal Price { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Rejected;
    }
}
