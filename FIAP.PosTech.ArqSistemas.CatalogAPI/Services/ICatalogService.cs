using FIAP.PosTech.ArqSistemas.CatalogAPI.DTOs;
using FIAP.PosTech.ArqSistemas.CatalogAPI.Models;

namespace FIAP.PosTech.ArqSistemas.CatalogAPI.Services
{
    public interface ICatalogService
    {
        /// <summary>
        /// Obtém todos os usuários
        /// </summary>
        List<Catalog> ObterTodos();

        /// <summary>
        /// Obtém um usuário pelo Id
        /// </summary>
        Catalog ObterPorId(int id);

        /// <summary>
        /// Cria um novo usuário
        /// </summary>
        (bool Sucesso, string Mensagem, Catalog Catalog) Criar(Catalog catalog);

        /// <summary>
        /// Altera um usuário existente (partial update)
        /// Apenas o Id é obrigatório. Os demais campos são opcionais e serão atualizados somente se fornecidos.
        /// </summary>
        (bool Sucesso, string Mensagem, Catalog Catalog) Alterar(int id, AtualizarCatalogDto catalogAtualizado);

        /// <summary>
        /// Exclui um usuário pelo Id
        /// </summary>
        (bool Sucesso, string Mensagem) Excluir(int id);
    }
}
