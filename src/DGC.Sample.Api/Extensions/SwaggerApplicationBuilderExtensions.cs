using Asp.Versioning.ApiExplorer;

namespace DGC.Sample.Api.Extensions;

public static class SwaggerApplicationBuilderExtensions
{
    public static IApplicationBuilder UseSwaggerConfiguration(this IApplicationBuilder app)
    {
        var provider = app.ApplicationServices.GetRequiredService<IApiVersionDescriptionProvider>();

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            var descriptions = provider.ApiVersionDescriptions;
            foreach (var description in descriptions)
            {
                var url = $"/swagger/{description.GroupName}/swagger.json";
                var name = description.GroupName.ToUpperInvariant();
                options.SwaggerEndpoint(url, name);
            }
        });

        return app;
    }
}
