namespace FIAP.PosTech.ArqSistemas.CatalogAPI.Services
{
    using System.Threading.Tasks;

    public interface IUserService
    {
        Task<bool> GetUserAsync(int userId);
    }
}
