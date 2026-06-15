using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FIAP.PosTech.ArqSistemas.CatalogAPI.Models;
using FIAP.PosTech.ArqSistemas.CatalogAPI.Services;
using FIAP.PosTech.ArqSistemas.CatalogAPI.DTOs;

namespace FIAP.PosTech.ArqSistemas.CatalogAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GameController : ControllerBase
    {
        private readonly IGameService _gameService;
        private readonly ILogger<GameController> _logger;

        public GameController(IGameService gameService, ILogger<GameController> logger )
        {
            _gameService = gameService;
            _logger = logger;
        }

        /// <summary>
        /// Obtém o CorrelationId do contexto HTTP
        /// </summary>
        private string? GetCorrelationId() => HttpContext.Items["CorrelationId"]?.ToString();

        /// <summary>
        /// Obtém todos os jogos
        /// </summary>
        /// <returns>Lista de todos os jogos</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<ApiResponse<List<Game>>> ObterTodos()
        {
            try
            {
                var games = _gameService.ObterTodos();
                var response = ApiResponse<List<Game>>.SucessoList(games, $"Total de {games.Count} jogos encontrados");
                response.CorrelationId = GetCorrelationId();
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter todos os jogos");
                var response = ApiResponse<List<Game>>.Erro(ex.Message, "Erro ao obter jogos");
                response.CorrelationId = GetCorrelationId();
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        /// <summary>
        /// Obtém um jogo pelo Id
        /// </summary>
        /// <param name="id">Id do jogo</param>
        /// <returns>Jogo encontrado</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]       
        public async Task<ActionResult<ApiResponse<Game>>> ObterPorId(int id)
        {
            try
            {
                if (id <= 0)
                {
                    var errorResponse = ApiResponse<Game>.Erro("Id deve ser um número positivo", "Validação falhou");
                    errorResponse.CorrelationId = GetCorrelationId();
                    return BadRequest(errorResponse);
                }

                var game = await _gameService.ObterPorId(id);

                if (game == null)
                {
                    var notFoundResponse = ApiResponse<Game>.NotFound($"Jogo com Id {id} não encontrado");
                    notFoundResponse.CorrelationId = GetCorrelationId();
                    return NotFound(notFoundResponse);
                }

                var response = ApiResponse<Game>.SucessoOk(game, "Jogo encontrado com sucesso");
                response.CorrelationId = GetCorrelationId();
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter jogo com Id {Id}", id);
                var response = ApiResponse<Game>.Erro(ex.Message, "Erro ao obter jogo");
                response.CorrelationId = GetCorrelationId();
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        /// <summary>
        /// Cria um novo jogo
        /// </summary>
        /// <param name="game">Dados do jogo a ser criado (não incluir Id)</param>
        /// <returns>Jogo criado com Id gerado automaticamente</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<ApiResponse<Game>> Criar([FromBody] Game game)
        {
            try
            {
                if (game == null)
                {
                    var errorResponse = ApiResponse<Game>.Erro("Corpo da requisição não pode estar vazio", "Validação falhou");
                    errorResponse.CorrelationId = GetCorrelationId();
                    return BadRequest(errorResponse);
                }

                var (sucesso, mensagem, gameCriado) = _gameService.Criar(game);

                if (!sucesso)
                {
                    var errorResponse = ApiResponse<Game>.Erro(mensagem, "Erro ao criar jogo");
                    errorResponse.CorrelationId = GetCorrelationId();
                    return BadRequest(errorResponse);
                }

                var response = ApiResponse<Game>.SucessoCreate(gameCriado, mensagem);
                response.CorrelationId = GetCorrelationId();

                return CreatedAtAction(nameof(ObterPorId), new { id = gameCriado.Id }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar jogo");
                var response = ApiResponse<Game>.Erro(ex.Message, "Erro ao criar jogo");
                response.CorrelationId = GetCorrelationId();
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        /// <summary>
        /// Altera um jogo existente (partial update)
        /// </summary>
        /// <param name="id">Id do jogo a ser alterado (obrigatório)</param>
        /// <param name="gameAtualizado">Dados a serem atualizados. Todos os campos são opcionais - apenas os fornecidos serão alterados.</param>
        /// <returns>Jogo alterado</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<ApiResponse<Game>> Alterar(int id, [FromBody] AtualizarGameDto gameAtualizado)
        {
            try
            {
                if (gameAtualizado == null)
                {
                    var errorResponse = ApiResponse<Game>.Erro("Corpo da requisição não pode estar vazio", "Validação falhou");
                    errorResponse.CorrelationId = GetCorrelationId();
                    return BadRequest(errorResponse);
                }

                if (id <= 0)
                {
                    var errorResponse = ApiResponse<Game>.Erro("Id deve ser um número positivo", "Validação falhou");
                    errorResponse.CorrelationId = GetCorrelationId();
                    return BadRequest(errorResponse);
                }

                var (sucesso, mensagem, gameAlterado) = _gameService.Alterar(id, gameAtualizado);

                if (!sucesso)
                {
                    if (mensagem == "Jogo não encontrado")
                    {
                        var notFoundResponse = ApiResponse<Game>.NotFound(mensagem);
                        notFoundResponse.CorrelationId = GetCorrelationId();
                        return NotFound(notFoundResponse);
                    }

                    var errorResponse = ApiResponse<Game>.Erro(mensagem, "Erro ao alterar jogo");
                    errorResponse.CorrelationId = GetCorrelationId();
                    return BadRequest(errorResponse);
                }

                var response = ApiResponse<Game>.SucessoOk(gameAlterado, mensagem);
                response.CorrelationId = GetCorrelationId();
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao alterar jogo com Id {Id}", id);
                var response = ApiResponse<Game>.Erro(ex.Message, "Erro ao alterar jogo");
                response.CorrelationId = GetCorrelationId();
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        /// <summary>
        /// Exclui um jogo existente
        /// </summary>
        /// <param name="id">Id do jogo a ser excluído (obrigatório)</param>
        /// <returns>Resultado da exclusão</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<ApiResponse<object?>> Excluir(int id)
        {
            try
            {
                if (id <= 0)
                {
                    var errorResponse = ApiResponse<object?>.Erro("Id deve ser um número positivo", "Validação falhou");
                    errorResponse.CorrelationId = GetCorrelationId();
                    return BadRequest(errorResponse);
                }

                var (sucesso, mensagem) = _gameService.Excluir(id);

                if (!sucesso)
                {
                    if (mensagem == "Jogo não encontrado")
                    {
                        var notFoundResponse = ApiResponse<object?>.NotFound(mensagem);
                        notFoundResponse.CorrelationId = GetCorrelationId();
                        return NotFound(notFoundResponse);
                    }

                    var errorResponse = ApiResponse<object?>.Erro(mensagem, "Erro ao excluir jogo");
                    errorResponse.CorrelationId = GetCorrelationId();
                    return BadRequest(errorResponse);
                }

                var response = ApiResponse<object?>.SucessoOk(null, mensagem);
                response.CorrelationId = GetCorrelationId();
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir jogo com Id {Id}", id);
                var response = ApiResponse<object?>.Erro(ex.Message, "Erro ao excluir jogo");
                response.CorrelationId = GetCorrelationId();
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }
    }
}
