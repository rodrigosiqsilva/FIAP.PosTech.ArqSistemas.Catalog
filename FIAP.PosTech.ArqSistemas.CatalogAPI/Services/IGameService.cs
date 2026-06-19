using FIAP.PosTech.ArqSistemas.CatalogAPI.DTOs;
using FIAP.PosTech.ArqSistemas.CatalogAPI.Models;

namespace FIAP.PosTech.ArqSistemas.CatalogAPI.Services
{
    public interface IGameService
    {
        /// <summary>
        /// Obtém todos os jogos do catálogo
        /// </summary>
        List<Game> ObterTodos();

        /// <summary>
        /// Obtém um jogo pelo Id
        /// </summary>
        Task<Game> ObterPorId(int id);

        /// <summary>
        /// Cria um novo jogo no catálogo
        /// </summary>
        (bool Sucesso, string Mensagem, Game Game) Criar(Game game);

        /// <summary>
        /// Altera um jogo existente (partial update)
        /// Apenas o Id é obrigatório. Os demais campos são opcionais e serão atualizados somente se fornecidos.
        /// </summary>
        (bool Sucesso, string Mensagem, Game Game) Alterar(int id, AtualizarGameDto gameAtualizado);

        /// <summary>
        /// Exclui um jogo pelo Id
        /// </summary>
        (bool Sucesso, string Mensagem) Excluir(int id);
    }
}
