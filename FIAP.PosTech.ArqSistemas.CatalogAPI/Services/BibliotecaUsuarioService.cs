using FIAP.PosTech.ArqSistemas.CatalogAPI.Models;

namespace FIAP.PosTech.ArqSistemas.CatalogAPI.Services
{
    public class BibliotecaUsuarioService: IBibliotecaUsuarioService
    {

        private static readonly List<BibliotecaUsuario> _bibliotecaUsuarios = new List<BibliotecaUsuario>();
        private readonly ILogger<BibliotecaUsuarioService> _logger;

        public BibliotecaUsuarioService(ILogger<BibliotecaUsuarioService> logger)
        {
            _logger = logger;
        }

        public async Task<List<BibliotecaUsuario>> ObterBibliotecaUsuario(int idUser)
        {
            var biblioteca = _bibliotecaUsuarios.Where(b => b.IdUser == idUser).ToList();
            if (biblioteca == null)
            {
                _logger.LogWarning("Biblioteca do usuário com Id {IdUser} não encontrada", idUser);
            }
            else
            {
                _logger.LogInformation("Biblioteca do usuário com Id {IdUser} encontrada", idUser);
            }
            return biblioteca;
        }

        public async Task<List<BibliotecaUsuario>> ObterBibliotecaJogo(int idGame)
        {
            var biblioteca = _bibliotecaUsuarios.Where(b => b.IdGame == idGame).ToList();
            if (biblioteca == null)
            {
                _logger.LogWarning("Biblioteca do jogo com Id {IdGame} não encontrada", idGame);
            }
            else
            {
                _logger.LogInformation("Biblioteca do jogo com Id {IdGame} encontrada", idGame);
            }
            return biblioteca;
        }
          
        public (bool Sucesso, string Mensagem, BibliotecaUsuario BibliotecaUsuarios) AdicionarNaBiblioteca(int idUser, int idGame)
        {
            var erros = new List<string>();

            if (idGame <= 0)
                erros.Add("Id do jogo é obrigatório e deve ser positivo");

            if (idUser <= 0)
                erros.Add("Id do usuário é obrigatório e deve ser positivo");

            if (erros.Count > 0)
            {
                var mensagem = string.Join("; ", erros);
                _logger.LogWarning("Erro ao adicionar jogo à biblioteca do usuário {IdUser}: {Erros}", idUser, mensagem);
                return (false, mensagem, null);
            }

            // Criar jogo na biblioteca do usuário
            var novoJogoBiblioteca = new Models.BibliotecaUsuario
            {
                IdGame = idGame,
                IdUser = idUser
            };

            _bibliotecaUsuarios.Add(novoJogoBiblioteca);
            _logger.LogInformation("Jogo adicionado à biblioteca do usuário {IdUser} com sucesso. IdGame: {IdGame}", idUser, idGame);

            return (true, "Jogo adicionado à biblioteca com sucesso", novoJogoBiblioteca);
        }
    }
}
