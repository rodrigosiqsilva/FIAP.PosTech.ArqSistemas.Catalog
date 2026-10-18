namespace FIAP.PosTech.ArqSistemas.CatalogAPI.DTOs
{
    public class CriarGameDto
    {
        public string Nome { get; set; }
        public decimal Preco { get; set; }
        public bool Ativo { get; set; }
    }

    public class AtualizarGameDto
    {
        public string? Nome { get; set; }
        public decimal? Preco { get; set; }
        public bool? Ativo { get; set; }
    }

    public class GameSearchResponseDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public decimal Preco { get; set; }
        public bool Ativo { get; set; }
        public double? RelevanciaScore { get; set; }
    }
}
