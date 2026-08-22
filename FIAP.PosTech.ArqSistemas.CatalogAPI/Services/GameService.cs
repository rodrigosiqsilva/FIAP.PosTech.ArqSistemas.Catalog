using System.Text.Json;
using FIAP.PosTech.ArqSistemas.CatalogAPI.DTOs;
using FIAP.PosTech.ArqSistemas.CatalogAPI.Models;
using Microsoft.Extensions.Caching.Distributed;
using MongoDB.Driver;

namespace FIAP.PosTech.ArqSistemas.CatalogAPI.Services
{
    public class GameService : IGameService
    {
        private readonly IMongoCollection<Game> _gameCollection;
        private readonly ILogger<GameService> _logger;
        private readonly IDistributedCache _cache;

        public GameService(IMongoDatabase database, ILogger<GameService> logger, IDistributedCache cache)
        {
            _logger = logger;
            _cache = cache;
            _gameCollection = database.GetCollection<Game>("Games");
            InicializarDados();
        }

        /// <summary>
        /// Inicializa 5 registros fictícios para testes se a coleção estiver vazia
        /// </summary>
        private void InicializarDados()
        {
            try
            {
                if (_gameCollection.CountDocuments(FilterDefinition<Game>.Empty) == 0)
                {
                    var jogosIniciais = new List<Game>
                    {
                        new Game { Id = 1, Nome = "Minecraft", Preco = 100.00m, Ativo = true },
                        new Game { Id = 2, Nome = "Grand Theft Auto V", Preco = 200.00m, Ativo = true },
                        new Game { Id = 3, Nome = "EA SPORTS FC 26", Preco = 150.00m, Ativo = true },
                        new Game { Id = 4, Nome = "Forza Horizon 5", Preco = 300.00m, Ativo = true },
                        new Game { Id = 5, Nome = "Destiny 2", Preco = 250.00m, Ativo = true }
                    };
                    _gameCollection.InsertMany(jogosIniciais);
                    _logger.LogInformation("Dados iniciais de jogos carregados com sucesso no MongoDB. Total de registros: {TotalRegistros}", jogosIniciais.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao inicializar dados de jogos no MongoDB");
            }
        }

        public List<Game> ObterTodos()
        {
            var list = _gameCollection.Find(FilterDefinition<Game>.Empty).ToList();
            _logger.LogInformation("Obtendo todos os jogos do MongoDB. Total: {Total}", list.Count);
            return list;
        }

        public async Task<Game?> ObterPorId(int id)
        {
            var cacheKey = $"game:{id}";

            // 1. Tenta obter do Cache (Redis)
            try
            {
                var cachedGame = await _cache.GetStringAsync(cacheKey);
                if (!string.IsNullOrEmpty(cachedGame))
                {
                    _logger.LogInformation("Jogo com Id {Id} obtido do CACHE (Redis)", id);
                    return JsonSerializer.Deserialize<Game>(cachedGame);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Falha ao consultar Redis para a chave {CacheKey}. Buscando no MongoDB.", cacheKey);
            }

            // 2. Não está no cache -> Consulta o MongoDB
            var game = await _gameCollection.Find(c => c.Id == id).FirstOrDefaultAsync();
            if (game == null)
            {
                _logger.LogWarning("Jogo com Id {Id} não encontrado no MongoDB", id);
                return null;
            }

            _logger.LogInformation("Jogo com Id {Id} encontrado no MongoDB: {Nome}. Gravando no CACHE (Redis)...", id, game.Nome);

            // 3. Salva no Cache com expiração de 15 minutos
            try
            {
                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15)
                };

                var serializedGame = JsonSerializer.Serialize(game);
                await _cache.SetStringAsync(cacheKey, serializedGame, cacheOptions);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Falha ao gravar jogo {Id} no Cache (Redis)", id);
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

            var maxIdGame = _gameCollection.Find(FilterDefinition<Game>.Empty)
                .SortByDescending(g => g.Id)
                .FirstOrDefault();

            int proximoId = maxIdGame != null ? maxIdGame.Id + 1 : 1;

            // Criar novo jogo com Id gerado
            var novoGame = new Game
            {
                Id = proximoId,
                Nome = game.Nome.Trim(),
                Preco = game.Preco,
                Ativo = game.Ativo   
            };

            _gameCollection.InsertOne(novoGame);
            _logger.LogInformation("Jogo criado no MongoDB com sucesso. Id: {Id}, Nome: {Nome}, Preco: {Preco}",
                novoGame.Id, novoGame.Nome, novoGame.Preco);

            return (true, "Jogo criado com sucesso", novoGame);
        }

        public (bool Sucesso, string Mensagem, Game Game) Alterar(int id, AtualizarGameDto gameAtualizado)
        {
            var erros = new List<string>();

            // Validar Id obrigatório
            if (id <= 0)
                erros.Add("Id deve ser um número positivo");

            var gameExistente = _gameCollection.Find(g => g.Id == id).FirstOrDefault();
            if (gameExistente == null)
            {
                _logger.LogWarning("Erro ao alterar: Jogo com Id {Id} não encontrado no MongoDB", id);
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

            _gameCollection.ReplaceOne(g => g.Id == id, gameExistente);
            
            // Invalida o cache
            try { _cache.Remove($"game:{id}"); } catch { /* Ignora se cache offline */ }

            _logger.LogInformation("Jogo alterado no MongoDB com sucesso. Id: {Id}, Nome: {Nome}, Preco: {Preco}, Ativo: {Ativo}",
                gameExistente.Id, gameExistente.Nome, gameExistente.Preco, gameExistente.Ativo);

            return (true, "Jogo alterado com sucesso", gameExistente);
        }

        public (bool Sucesso, string Mensagem) Excluir(int id)
        {
            // Validar Id
            if (id <= 0)
                return (false, "Id deve ser um número positivo");

            var result = _gameCollection.DeleteOne(g => g.Id == id);
            if (result.DeletedCount == 0)
            {
                _logger.LogWarning("Erro ao excluir: Jogo com Id {Id} não encontrado no MongoDB", id);
                return (false, "Jogo não encontrado");
            }

            // Invalida o cache
            try { _cache.Remove($"game:{id}"); } catch { /* Ignora se cache offline */ }

            _logger.LogInformation("Jogo excluído do MongoDB com sucesso. Id: {Id}", id);

            return (true, "Jogo excluído com sucesso");
        }

    }
}
