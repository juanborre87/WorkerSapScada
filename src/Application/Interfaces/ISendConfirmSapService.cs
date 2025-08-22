using Domain.Dtos;
using Domain.Models;

namespace Application.Interfaces;

public interface ISendConfirmSapService
{
    Task<SapResponse> SendOrderConfirmationAsync(OrderConfirmationRequest confirmation);
}
