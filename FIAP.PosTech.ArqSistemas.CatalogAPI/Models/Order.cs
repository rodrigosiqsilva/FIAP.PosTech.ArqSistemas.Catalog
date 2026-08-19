using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using FIAP.PosTech.ArqSistemas.CatalogAPI.Enums;

namespace FIAP.PosTech.ArqSistemas.CatalogAPI.Models
{
    public class Order
    {
        [BsonId]
        public int Id { get; set; }
        public int UserId { get; set; }
        public int GameId { get; set; }
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal Price { get; set; }
        [BsonRepresentation(BsonType.String)]
        public OrderStatus Status { get; set; } = OrderStatus.Rejected;
    }
}
