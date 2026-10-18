using FIAP.PosTech.ArqSistemas.CatalogAPI.DTOs;
using FIAP.PosTech.ArqSistemas.CatalogAPI.Models;

namespace FIAP.PosTech.ArqSistemas.CatalogAPI.Services
{
    public interface IGameSearchService
    {
        Task IndexarAsync(Game game);
        Task AtualizarAsync(Game game);
        Task RemoverAsync(int id);
        Task<List<GameSearchResponseDto>> BuscarAsync(string termo);
        Task IndexarMuitosAsync(IEnumerable<Game> games);
    }
}
