namespace FIAP.PosTech.ArqSistemas.CatalogAPI.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int GameId { get; set; }
        public decimal Price { get; set; }
    }
}
