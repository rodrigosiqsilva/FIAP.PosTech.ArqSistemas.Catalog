namespace FIAP.PosTech.ArqSistemas.CatalogAPI.Services
{
    using FIAP.PosTech.ArqSistemas.UserAPI.Models;

    public interface IUserService
    {
        Task<User> GetUserAsync(int userId);
    }
}
