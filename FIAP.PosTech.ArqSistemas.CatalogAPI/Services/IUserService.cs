namespace FIAP.PosTech.ArqSistemas.CatalogAPI.Services
{
    using FIAP.PosTech.ArqSistemas.CatalogAPI.Models;
    using FIAP.PosTech.ArqSistemas.UserAPI.Models;

    public interface IUserService
    {
        Task<ApiResponse<User>> GetUserAsync(int userId);
    }
}
