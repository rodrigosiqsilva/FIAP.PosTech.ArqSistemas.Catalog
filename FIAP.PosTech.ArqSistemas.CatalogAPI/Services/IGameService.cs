using FIAP.PosTech.ArqSistemas.CatalogAPI.DTOs;
using FIAP.PosTech.ArqSistemas.CatalogAPI.Models;

namespace FIAP.PosTech.ArqSistemas.CatalogAPI.Services
{
    public interface IGameService
    {
        /// <summary>
        /// Obtém todos os usuários
        /// </summary>
        List<Game> ObterTodos();

        /// <summary>
        /// Obtém um usuário pelo Id
        /// </summary>
        Task<Game> ObterPorId(int id);

        /// <summary>
        /// Cria um novo usuário
        /// </summary>
        (bool Sucesso, string Mensagem, Game Game) Criar(Game game);

        /// <summary>
        /// Altera um usuário existente (partial update)
        /// Apenas o Id é obrigatório. Os demais campos são opcionais e serão atualizados somente se fornecidos.
        /// </summary>
        (bool Sucesso, string Mensagem, Game Game) Alterar(int id, AtualizarGameDto gameAtualizado);

        /// <summary>
        /// Exclui um usuário pelo Id
        /// </summary>
        (bool Sucesso, string Mensagem) Excluir(int id);
    }
}
