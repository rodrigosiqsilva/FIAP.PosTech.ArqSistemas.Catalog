using System.Diagnostics;

namespace FIAP.PosTech.ArqSistemas.CatalogAPI.Middlewares
{
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CorrelationIdMiddleware> _logger;
        private const string CorrelationIdHeader = "X-Correlation-Id";

        public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Obtém ou gera correlationId
            string correlationId = context.Request.Headers.ContainsKey(CorrelationIdHeader)
                ? context.Request.Headers[CorrelationIdHeader].ToString()
                : Guid.NewGuid().ToString();

            // Adiciona correlationId aos headers de resposta (usando indexer em vez de Add)
            context.Response.Headers[CorrelationIdHeader] = correlationId;

            // Armazena no contexto para uso nos logs
            context.Items["CorrelationId"] = correlationId;

            var stopwatch = Stopwatch.StartNew();

            try
            {
                // Log de requisição
                _logger.LogInformation(
                    "Requisição iniciada - CorrelationId: {CorrelationId}, Método: {Method}, Rota: {Path}",
                    correlationId, context.Request.Method, context.Request.Path);

                await _next(context);

                stopwatch.Stop();

                // Log de sucesso
                _logger.LogInformation(
                    "Requisição concluída - CorrelationId: {CorrelationId}, Status: {StatusCode}, Duração: {Duration}ms",
                    correlationId, context.Response.StatusCode, stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                // Log de erro
                _logger.LogError(
                    "Erro na requisição - CorrelationId: {CorrelationId}, Mensagem: {Message}, Duração: {Duration}ms, StackTrace: {StackTrace}",
                    correlationId, ex.Message, stopwatch.ElapsedMilliseconds, ex.StackTrace);

                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/json";

                var response = new { erro = "Erro interno do servidor", correlationId };
                await context.Response.WriteAsJsonAsync(response);
            }
        }
    }

    public static class CorrelationIdMiddlewareExtensions
    {
        public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CorrelationIdMiddleware>();
        }
    }
}
