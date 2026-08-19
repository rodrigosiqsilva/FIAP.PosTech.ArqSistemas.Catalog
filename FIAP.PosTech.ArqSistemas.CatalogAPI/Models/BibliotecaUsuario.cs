using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FIAP.PosTech.ArqSistemas.CatalogAPI.Models
{
    public class BibliotecaUsuario
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonIgnoreIfDefault]
        public string? Id { get; set; }
        public int IdGame { get; set; }
        public int IdUser { get; set; }
    }
}
