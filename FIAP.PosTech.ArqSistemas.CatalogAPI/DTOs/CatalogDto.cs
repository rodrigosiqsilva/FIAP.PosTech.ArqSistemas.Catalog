namespace FIAP.PosTech.ArqSistemas.CatalogAPI.DTOs
{
    public class CriarCatalogDto
    {
        public string Nome { get; set; }
        public decimal Preco { get; set; }
        public bool Ativo { get; set; }
    }

    public class AtualizarCatalogDto
    {
        public string? Nome { get; set; }
        public decimal? Preco { get; set; }
        public bool? Ativo { get; set; }
    }
}
