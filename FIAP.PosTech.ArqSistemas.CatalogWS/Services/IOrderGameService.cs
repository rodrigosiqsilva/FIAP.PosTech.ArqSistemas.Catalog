using System;
using System.Collections.Generic;
using System.Text;
using FIAP.PosTech.ArqSistemas.CatalogWS.DTOs;
using FIAP.PosTech.ArqSistemas.CatalogWS.Models;

namespace FIAP.PosTech.ArqSistemas.CatalogWS.Services
{
    public interface IOrderGameService
    {
        Task<ApiResponse<OrderDto>> GetOrderAsync(int orderId);


        Task<bool> AproveOrderAsync(int orderId);
    }
}
