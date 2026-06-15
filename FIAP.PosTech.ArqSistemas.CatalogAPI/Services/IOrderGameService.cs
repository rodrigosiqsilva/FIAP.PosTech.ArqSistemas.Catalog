using FIAP.PosTech.ArqSistemas.CatalogAPI.Models;

namespace FIAP.PosTech.ArqSistemas.CatalogAPI.Services
{
    public interface IOrderGameService
    {

        /// <summary>
        /// Obtém um usuário pelo Id
        /// </summary>
        Order ObterPorId(int id);

        /// <summary>
        /// Cria um novo pedido de compra para um jogo específico.
        /// </summary>
        (bool Sucesso, string Mensagem, Order Order) Criar(Order order);
    }
}
