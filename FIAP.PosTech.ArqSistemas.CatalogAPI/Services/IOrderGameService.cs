using FIAP.PosTech.ArqSistemas.CatalogAPI.DTOs;
using FIAP.PosTech.ArqSistemas.CatalogAPI.Enums;
using FIAP.PosTech.ArqSistemas.CatalogAPI.Models;

namespace FIAP.PosTech.ArqSistemas.CatalogAPI.Services
{
    public interface IOrderGameService
    {

        /// <summary>
        /// Obtém todos os pepidos de compra
        /// </summary>
        List<Order> ObterTodos();


        /// <summary>
        /// Obtém um usuário pelo Id
        /// </summary>
        Order ObterPorId(int id);

        /// <summary>
        /// Cria um novo pedido de compra para um jogo específico.
        /// </summary>
        (bool Sucesso, string Mensagem, Order Order) Criar(Order order);


        /// <summary>
        /// Alterar o status de um pedido pelo Id
        /// </summary>
        (bool Sucesso, string Mensagem, Order Order) AlterarStatus(int id, OrderStatus newState);
    }
}
