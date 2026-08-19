using FIAP.PosTech.ArqSistemas.CatalogAPI.Models;
using MongoDB.Driver;

namespace FIAP.PosTech.ArqSistemas.CatalogAPI.Services
{
    public class BibliotecaUsuarioService: IBibliotecaUsuarioService
    {
        private readonly IMongoCollection<BibliotecaUsuario> _bibliotecaCollection;
        private readonly ILogger<BibliotecaUsuarioService> _logger;

        public BibliotecaUsuarioService(IMongoDatabase database, ILogger<BibliotecaUsuarioService> logger)
        {
            _logger = logger;
            _bibliotecaCollection = database.GetCollection<BibliotecaUsuario>("BibliotecaUsuarios");
        }

        public async Task<List<BibliotecaUsuario>> ObterBibliotecaUsuario(int idUser)
        {
            var biblioteca = await _bibliotecaCollection.Find(b => b.IdUser == idUser).ToListAsync();
            _logger.LogInformation("Biblioteca do usuário com Id {IdUser} obtida do MongoDB. Total: {Count}", idUser, biblioteca.Count);
            return biblioteca;
        }

        public async Task<List<BibliotecaUsuario>> ObterBibliotecaJogo(int idGame)
        {
            var biblioteca = await _bibliotecaCollection.Find(b => b.IdGame == idGame).ToListAsync();
            _logger.LogInformation("Biblioteca do jogo com Id {IdGame} obtida do MongoDB. Total: {Count}", idGame, biblioteca.Count);
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

            _bibliotecaCollection.InsertOne(novoJogoBiblioteca);
            _logger.LogInformation("Jogo adicionado à biblioteca do usuário {IdUser} com sucesso no MongoDB. IdGame: {IdGame}", idUser, idGame);

            return (true, "Jogo adicionado à biblioteca com sucesso", novoJogoBiblioteca);
        }
    }
}
