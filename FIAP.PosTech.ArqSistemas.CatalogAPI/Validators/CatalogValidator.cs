namespace FIAP.PosTech.ArqSistemas.CatalogAPI.Validators
{
    public class CatalogValidator
    {
        public static bool ValidarNome(string nome)
        {
            if (string.IsNullOrWhiteSpace(nome))
                return false;
            return true;
        }

        public static bool ValidarPreco(decimal preco)
        {
            if (preco <= 0)
                return false;
            return true;
        }
    }
}
