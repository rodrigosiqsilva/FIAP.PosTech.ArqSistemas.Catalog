using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FIAP.PosTech.ArqSistemas.CatalogAPI.Models
{
    public class Game
    {
        [BsonId]
        public int Id { get; set; }
        public string Nome { get; set; }
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal Preco { get; set; }
        public bool Ativo { get; set; }
    }
}
