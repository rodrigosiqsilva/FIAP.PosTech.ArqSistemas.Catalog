using Confluent.Kafka;
using FIAP.PosTech.ArqSistemas.CatalogAPI.Models;
using FIAP.PosTech.ArqSistemas.CatalogAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace FIAP.PosTech.ArqSistemas.CatalogAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BibliotecaUsuarioController : Controller
    {

        private readonly IUserService _userService;
        private readonly ILogger<BibliotecaUsuarioController> _logger;
        private readonly IBibliotecaUsuarioService _bibliotecaUsuario;
        private readonly IGameService _gameService; 

        public BibliotecaUsuarioController(IUserService userService, ILogger<BibliotecaUsuarioController> logger, 
                IBibliotecaUsuarioService bibliotecaUsuario, IGameService gameService)
        {
            _userService = userService;
            _logger = logger;
            _bibliotecaUsuario = bibliotecaUsuario;
            _gameService = gameService;
        }

        /// <summary>
        /// Obtém o CorrelationId do contexto HTTP
        /// </summary>
        private string? GetCorrelationId() => HttpContext.Items["CorrelationId"]?.ToString();


        /// <summary>
        /// Adiciona jogo à biblioteca do usuário. Verifica se o usuário e o jogo existem antes de adicionar. 
        /// Retorna o recurso criado ou erros de validação.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<BibliotecaUsuario>>> AdicionarNaBiblioteca([FromBody] BibliotecaUsuario bibliotecaUsuario)
        {
            try
            {

                if (bibliotecaUsuario == null)
                {
                    var errorResponse = ApiResponse<BibliotecaUsuario>.Erro("Corpo da requisição não pode estar vazio", "Validação falhou");
                    errorResponse.CorrelationId = GetCorrelationId();
                    return BadRequest(errorResponse);
                }

                // Usuário existe
                var userJson = await _userService.GetUserAsync(bibliotecaUsuario.IdUser);

                if (userJson == null)
                {
                    var responseUser = ApiResponse<BibliotecaUsuario>.Erro("Usuário não existe");
                    responseUser.CorrelationId = GetCorrelationId();
                    return StatusCode(StatusCodes.Status404NotFound, responseUser);
                }

                // Jogo existe
                var gameJson = await _gameService.ObterPorId(bibliotecaUsuario.IdGame);

                if (gameJson == null)
                {
                    var responseJogo = ApiResponse<BibliotecaUsuario>.Erro("Jogo não existe");
                    responseJogo.CorrelationId = GetCorrelationId();
                    return StatusCode(StatusCodes.Status404NotFound, responseJogo);
                }

                var (sucesso, mensagem, gameAdicionado) = _bibliotecaUsuario.AdicionarNaBiblioteca(bibliotecaUsuario.IdUser, bibliotecaUsuario.IdGame);

                if (!sucesso)
                {
                    var errorResponse = ApiResponse<BibliotecaUsuario>.Erro("Erro ao adicionar jogo à biblioteca", "Validação falhou");
                    errorResponse.CorrelationId = GetCorrelationId();
                    return BadRequest(errorResponse);
                }

                var response = ApiResponse<BibliotecaUsuario>.SucessoCreate(gameAdicionado, mensagem);
                response.CorrelationId = GetCorrelationId();

                return CreatedAtAction(nameof(AdicionarNaBiblioteca), new { idGame = gameAdicionado.IdGame, idUser = gameAdicionado.IdUser }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao adicionar jogo à biblioteca");
                var response = ApiResponse<BibliotecaUsuario>.Erro(ex.Message, "Erro ao adicionar jogo à biblioteca");
                response.CorrelationId = GetCorrelationId();
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        /// <summary>
        /// Obtém uma biblioteca de jogos pelo Id do usuário. Retorna 404 se não encontrado ou erros de validação.
        /// </summary>
        /// <param name="id">Id do jogo</param>
        /// <returns>Biblioteca de jogos encontrada</returns>
        [HttpGet("user/{idUser}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<BibliotecaUsuario>>> ObterBibliotecaUsuario(int idUser)
        {
            try
            {
                if (idUser  <= 0)
                {
                    var errorResponse = ApiResponse<BibliotecaUsuario>.Erro("Id deve ser um número positivo", "Validação falhou");
                    errorResponse.CorrelationId = GetCorrelationId();
                    return BadRequest(errorResponse);
                }

                // Usuário existe
                var userJson = await _userService.GetUserAsync(idUser);

                if (userJson == null)
                {
                    var responseUser = ApiResponse<List<BibliotecaUsuario>>.Erro("Usuário não existe");
                    responseUser.CorrelationId = GetCorrelationId();
                    return StatusCode(StatusCodes.Status404NotFound, responseUser);
                }


                var biblioteca = await _bibliotecaUsuario.ObterBibliotecaUsuario(idUser);

                if (biblioteca == null)
                {
                    var notFoundResponse = ApiResponse<List<BibliotecaUsuario>>.NotFound($"Biblioteca do usuário com Id {idUser} não encontrada");
                    notFoundResponse.CorrelationId = GetCorrelationId();
                    return NotFound(notFoundResponse);
                }

                var response = ApiResponse<List<BibliotecaUsuario>>.SucessoOk(biblioteca, "Biblioteca do usuário encontrada com sucesso");
                response.CorrelationId = GetCorrelationId();
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter biblioteca do usuário com Id {IdUser}", idUser);
                var response = ApiResponse<List<BibliotecaUsuario>>.Erro(ex.Message, "Erro ao obter biblioteca do usuário");
                response.CorrelationId = GetCorrelationId();
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        /// <summary>
        /// Obtém uma biblioteca de jogos pelo Id do jogo. Retorna 404 se não encontrado ou erros de validação.
        /// </summary>
        /// <param name="id">Id do jogo</param>
        /// <returns>Biblioteca de jogos encontrada</returns>
        [HttpGet("game/{idGame}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<List<BibliotecaUsuario>>>> ObterBibliotecaJogo(int idGame)
        {
            try
            {
                if (idGame <= 0)
                {
                    var errorResponse = ApiResponse<List<BibliotecaUsuario>>.Erro("Id deve ser um número positivo", "Validação falhou");
                    errorResponse.CorrelationId = GetCorrelationId();
                    return BadRequest(errorResponse);
                }


                // Jogo existe
                var gameJson = await _gameService.ObterPorId(idGame);

                if (gameJson == null)
                {
                    var responseJogo = ApiResponse<List<BibliotecaUsuario>>.Erro("Jogo não existe");
                    responseJogo.CorrelationId = GetCorrelationId();
                    return StatusCode(StatusCodes.Status404NotFound, responseJogo);
                }

                var biblioteca = await _bibliotecaUsuario.ObterBibliotecaJogo(idGame);

                if (biblioteca == null)
                {
                    var notFoundResponse = ApiResponse<List<BibliotecaUsuario>>.NotFound($"Biblioteca do jogo com Id {idGame} não encontrada");
                    notFoundResponse.CorrelationId = GetCorrelationId();
                    return NotFound(notFoundResponse);
                }

                var response = ApiResponse<List<BibliotecaUsuario>>.SucessoOk(biblioteca, "Biblioteca do jogo encontrada com sucesso");
                response.CorrelationId = GetCorrelationId();
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter biblioteca do jogo com Id {IdGame}", idGame);
                var response = ApiResponse<List<BibliotecaUsuario>>.Erro(ex.Message, "Erro ao obter biblioteca do jogo");
                response.CorrelationId = GetCorrelationId();
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }
    }
}
