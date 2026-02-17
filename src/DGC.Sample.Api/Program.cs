using Asp.Versioning.ApiExplorer;
using DGC.Sample.Api.Extensions;
using DGC.Sample.Api.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddApiControllersWithAzureValidation();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.ConfigureOptions<SwaggerGenOptionsSetup>();
builder.Services.AddDgcSampleServices(builder.Configuration);
builder.Services.AddCustomApiVersioning();

var app = builder.Build();

app.UseMiddleware<RequestIdMiddleware>();
app.UseMiddleware<GlobalExceptionMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
    app.UseSwaggerConfiguration(provider);
}


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
