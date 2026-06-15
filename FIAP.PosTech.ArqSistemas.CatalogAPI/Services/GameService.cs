using FIAP.PosTech.ArqSistemas.CatalogAPI.DTOs;
using FIAP.PosTech.ArqSistemas.CatalogAPI.Models;

namespace FIAP.PosTech.ArqSistemas.CatalogAPI.Services
{
    public class GameService : IGameService
    {

        private readonly List<Game> _game;
        private readonly ILogger<GameService> _logger;
        private int _proximoId = 6;

        public GameService(ILogger<GameService> logger)
        {
            _logger = logger;
            _game = new List<Game>();
            InicializarDados();
        }

        /// <summary>
        /// Inicializa 5 registros fictícios para testes
        /// </summary>
        private void InicializarDados()
        {
            _game.AddRange(new[]
            {
                new Game { Id = 1, Nome = "Minecraft", Preco = 100.00m, Ativo = true },
                new Game { Id = 2, Nome = "Grand Theft Auto V", Preco = 200.00m, Ativo = true },
                new Game { Id = 3, Nome = "EA SPORTS FC 26", Preco = 150.00m, Ativo = true },
                new Game { Id = 4, Nome = "Forza Horizon 5", Preco = 300.00m, Ativo = true },
                new Game { Id = 5, Nome = "Destiny 2", Preco = 250.00m, Ativo = true }
            });

            _logger.LogInformation("Dados iniciais de jogos carregados com sucesso. Total de registros: {TotalRegistros}", _game.Count);
        }

        public List<Game> ObterTodos()
        {
            _logger.LogInformation("Obtendo todos os jogos. Total: {Total}", _game.Count);
            return _game.ToList();
        }

        public async Task<Game> ObterPorId(int id)
        {
            var game = _game.FirstOrDefault(c => c.Id == id);
            if (game == null)
            {
                _logger.LogWarning("Jogo com Id {Id} não encontrado", id);
            }
            else
            {
                _logger.LogInformation("Jogo com Id {Id} encontrado: {Nome}", id, game.Nome);
            }
            return game;
        }

        public (bool Sucesso, string Mensagem, Game Game) Criar(Game game)
        {
            var erros = new List<string>();

            // Validar obrigatoriedade
            if (string.IsNullOrWhiteSpace(game.Nome))
                erros.Add("Nome é obrigatório");

            if (game.Preco <= 0)
                erros.Add("Preço é obrigatório e deve ser positivo");

            if (erros.Count > 0)
            {
                var mensagem = string.Join("; ", erros);
                _logger.LogWarning("Erro ao criar jogo: {Erros}", mensagem);
                return (false, mensagem, null);
            }

            // Criar novo jogo com Id gerado
            var novoGame = new Game
            {
                Id = _proximoId++,
                Nome = game.Nome.Trim(),
                Preco = game.Preco,
                Ativo = game.Ativo   
            };

            _game.Add(novoGame);
            _logger.LogInformation("Jogo criado com sucesso. Id: {Id}, Nome: {Nome}, Preco: {Preco}",
                novoGame.Id, novoGame.Nome, novoGame.Preco);

            return (true, "Jogo criado com sucesso", novoGame);
        }

        public (bool Sucesso, string Mensagem, Game Game) Alterar(int id, AtualizarGameDto gameAtualizado)
        {
            var erros = new List<string>();

            // Validar Id obrigatório
            if (id <= 0)
                erros.Add("Id deve ser um número positivo");

            var gameExistente = _game.FirstOrDefault(g => g.Id == id);
            if (gameExistente == null)
            {
                _logger.LogWarning("Erro ao alterar: Jogo com Id {Id} não encontrado", id);
                return (false, "Jogo não encontrado", null);
            }

            // Validar e atualizar Nome (se fornecido)
            if (!string.IsNullOrWhiteSpace(gameAtualizado.Nome))
            {
                gameExistente.Nome = gameAtualizado.Nome.Trim();
                _logger.LogInformation("Campo Nome atualizado para o jogo Id {Id}", id);
            }

            // Validar e atualizar Preco (se fornecido)
            if (gameAtualizado.Preco.HasValue && gameAtualizado.Preco.Value > 0)
            {
                gameExistente.Preco = gameAtualizado.Preco.Value;
                _logger.LogInformation("Campo Preco atualizado para o jogo Id {Id}", id);
            }

            // Validar e atualizar Ativo (se fornecido)
            if (gameAtualizado.Ativo.HasValue)
            {
                gameExistente.Ativo = gameAtualizado.Ativo.Value;
                _logger.LogInformation("Campo Ativo atualizado para o jogo Id {Id}", id);
            }

            if (erros.Count > 0)
            {
                var mensagem = string.Join("; ", erros);
                _logger.LogWarning("Erro ao alterar jogo {Id}: {Erros}", id, mensagem);
                return (false, mensagem, null);
            }

            _logger.LogInformation("Jogo alterado com sucesso. Id: {Id}, Nome: {Nome}, Preco: {Preco}, Ativo: {Ativo}",
                gameExistente.Id, gameExistente.Nome, gameExistente.Preco, gameExistente.Ativo);

            return (true, "Jogo alterado com sucesso", gameExistente);
        }

        public (bool Sucesso, string Mensagem) Excluir(int id)
        {
            // Validar Id
            if (id <= 0)
                return (false, "Id deve ser um número positivo");

            var gameExistente = _game.FirstOrDefault(g => g.Id == id);
            if (gameExistente == null)
            {
                _logger.LogWarning("Erro ao excluir: Jogo com Id {Id} não encontrado", id);
                return (false, "Jogo não encontrado");
            }

            _game.Remove(gameExistente);
            _logger.LogInformation("Jogo excluído com sucesso. Id: {Id}, Nome: {Nome}", id, gameExistente.Nome);

            return (true, "Jogo excluído com sucesso");
        }
    }
}
