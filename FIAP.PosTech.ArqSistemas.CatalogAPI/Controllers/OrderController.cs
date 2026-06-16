using FIAP.PosTech.ArqSistemas.CatalogAPI.DTOs;
using FIAP.PosTech.ArqSistemas.CatalogAPI.Models;
using FIAP.PosTech.ArqSistemas.CatalogAPI.Services;
using FIAP.PosTech.ArqSistemas.UserAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace FIAP.PosTech.ArqSistemas.CatalogAPI.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {

        private readonly IOrderGameService _orderGameService;
        private readonly IUserService _userService;
        private readonly IGameService _gameService;
        private readonly ILogger<OrderController> _logger;
        private readonly IOrderPlacedService _orderPlacedService;

        public OrderController(IOrderGameService orderGameService, ILogger<OrderController> logger, 
            IGameService gameService, IUserService userService, IOrderPlacedService orderPlacedService)
        {
            _orderGameService = orderGameService;
            _logger = logger;
            _gameService = gameService;
            _userService = userService;
            _orderPlacedService = orderPlacedService;
        }

        /// <summary>
        /// Obtém o CorrelationId do contexto HTTP
        /// </summary>
        private string? GetCorrelationId() => HttpContext.Items["CorrelationId"]?.ToString();

        /// <summary>
        /// Obtém um pedido pelo Id
        /// </summary>
        /// <param name="id">Id do pedido</param>
        /// <returns>Pedido encontrado</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<ApiResponse<Order>> ObterPorId(int id)
        {
            try
            {
                if (id <= 0)
                {
                    var errorResponse = ApiResponse<Order>.Erro("Id deve ser um número positivo", "Validação falhou");
                    errorResponse.CorrelationId = GetCorrelationId();
                    return BadRequest(errorResponse);
                }

                var order = _orderGameService.ObterPorId(id);

                if (order == null)
                {
                    var notFoundResponse = ApiResponse<Order>.NotFound($"Pedido com Id {id} não encontrado");
                    notFoundResponse.CorrelationId = GetCorrelationId();
                    return NotFound(notFoundResponse);
                }

                var response = ApiResponse<Order>.SucessoOk(order, "Pedido encontrado com sucesso");
                response.CorrelationId = GetCorrelationId();
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter pedido com Id {Id}", id);
                var response = ApiResponse<Order>.Erro(ex.Message, "Erro ao obter pedido");
                response.CorrelationId = GetCorrelationId();
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }


        /// <summary>
        /// Cria um novo pedido de compra para um jogo específico. 
        /// </summary>
        /// <param name="order">Dados do pedido a ser criado</param>
        /// <returns>Pedido criado com Id gerado automaticamente</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<Order>>> Criar([FromBody] Order order)
        {
            try
            {
                if (order == null)
                {
                    var errorResponse = ApiResponse<Order>.Erro("Corpo da requisição não pode estar vazio", "Validação falhou");
                    errorResponse.CorrelationId = GetCorrelationId();
                    return BadRequest(errorResponse);
                }

                // Usuário existe
                var userJson = await _userService.GetUserAsync(order.UserId);

                if (userJson == null)
                {
                    return NotFound(new { mensagem = $"Usuário com ID {order.UserId} não foi encontrado na API de origem." });
                }

                // Jogo existe
                var gameJson = await _gameService.ObterPorId(order.GameId);

                if (gameJson == null)
                {
                    return NotFound(new { mensagem = $"Jogo com ID {order.GameId} não foi encontrado na API de origem." });
                }

                var (sucesso, mensagem, orderCriado) = _orderGameService.Criar(order);

                if (!sucesso)
                {
                    var errorResponse = ApiResponse<Order>.Erro(mensagem, "Erro ao criar pedido");
                    errorResponse.CorrelationId = GetCorrelationId();
                    return BadRequest(errorResponse);
                }

                var response = ApiResponse<Order>.SucessoCreate(orderCriado, mensagem);
                response.CorrelationId = GetCorrelationId();

                var orderCriadoDetalhe = new OrderDto
                {
                    Id = orderCriado.Id,
                    IdUser = orderCriado.UserId,
                    Usuario = userJson.Nome,
                    IdGame = orderCriado.GameId,
                    Game = gameJson.Nome,
                    Preco = orderCriado.Price,
                    EmailUser = userJson.Email,
                    Status = orderCriado.Status
                };

                _orderPlacedService.SendNotificationUser(orderCriadoDetalhe, GetCorrelationId());

                return CreatedAtAction(nameof(ObterPorId), new { id = orderCriado.Id }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar pedido");
                var response = ApiResponse<Order>.Erro(ex.Message, "Erro ao criar pedido");
                response.CorrelationId = GetCorrelationId();
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }
    }
}
