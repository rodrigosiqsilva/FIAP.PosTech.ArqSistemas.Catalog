using FIAP.PosTech.ArqSistemas.CatalogAPI.Models;

namespace FIAP.PosTech.ArqSistemas.CatalogAPI.Services
{
    public interface IBibliotecaUsuarioService
    {
        /// <summary>
        /// Adicionar novo jogo na biblioteca do usuário
        /// </summary>
        (bool Sucesso, string Mensagem, BibliotecaUsuario BibliotecaUsuarios) AdicionarNaBiblioteca(int idUser, int idGame);

        /// <summary>
        /// Obtém biblioteca de jogos do usuário
        /// </summary>
        Task<List<BibliotecaUsuario>> ObterBibliotecaUsuario(int idUser);


        /// <summary>
        /// Obtém biblioteca de usuários do jogo
        /// </summary>
        Task<List<BibliotecaUsuario>> ObterBibliotecaJogo(int idGame);
    }
}
