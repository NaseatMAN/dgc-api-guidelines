using DGC.Sample.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace DGC.Sample.Infrastructure.Identity;

public class HttpTenantAccessor(IHttpContextAccessor http) : ITenantAccessor
{
    public string TenantId => http.HttpContext?.User.FindFirst("tenant_id")?.Value ?? string.Empty;
}
