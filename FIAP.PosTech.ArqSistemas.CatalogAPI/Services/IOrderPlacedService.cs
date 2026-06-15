

using FIAP.PosTech.ArqSistemas.CatalogAPI.DTOs;

namespace FIAP.PosTech.ArqSistemas.UserAPI.Services
{
    public interface IOrderPlacedService
    {
        Task SendNotificationUser(OrderDto order, string? correlationId);
    }
}
