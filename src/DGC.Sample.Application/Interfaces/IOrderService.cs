using DGC.Sample.Application.Dtos;

namespace DGC.Sample.Application.Interfaces;

public interface IOrderService
{
    Task<IReadOnlyList<OrderResponse>> GetAllAsync(CancellationToken cancellationToken);
    Task<OrderResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<OrderResponse> CreateAsync(OrderCreateRequest request, CancellationToken cancellationToken);
    Task<OrderResponse?> UpdateAsync(Guid id, OrderUpdateRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
