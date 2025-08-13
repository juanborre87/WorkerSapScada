using Domain.Dtos;
using Domain.Models;

namespace Application.Interfaces
{
    public interface ISapOrderService
    {
        Task<SapResponse> SendOrderConfirmationAsync(OrderConfirmationRequest confirmation);
    }
}
