using DGC.Sample.Application.Dtos;

namespace DGC.Sample.Application.Interfaces.ExternalServices;

public interface IPublicUserLookupClient
{
    Task<PublicUserResponse?> GetUserByIdAsync(int id, CancellationToken cancellationToken);
}
