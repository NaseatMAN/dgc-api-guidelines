using DGC.Sample.Application.Features.Orders.Dtos;

namespace DGC.Sample.Application.Abstractions.Interfaces;

public interface IOrderService
{
    Task<IReadOnlyList<OrderResponse>> GetAllAsync(CancellationToken cancellationToken);
    Task<OrderResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<OrderResponse> CreateAsync(OrderCreateRequest request, CancellationToken cancellationToken);
    Task<OrderResponse?> UpdateAsync(Guid id, OrderUpdateRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
