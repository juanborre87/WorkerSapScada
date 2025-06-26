using Domain.Models;

namespace Application.Interfaces
{
    public interface ISapOrderService
    {
        Task<bool> SendOrderConfirmationAsync(OrderConfirmationRequest confirmation);
    }
}
