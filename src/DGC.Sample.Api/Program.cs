using DGC.Sample.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddApiControllersWithAzureValidation();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.ConfigureOptions<SwaggerGenOptionsSetup>();
builder.Services.AddApplicationServices();
builder.Services.AddApiInfrastructure(builder.Configuration);
builder.Services.AddNotificationInfrastructure(builder.Configuration);
builder.Services.AddQueueInfrastructure(builder.Configuration);
builder.Services.AddApiQueueWorkers(builder.Configuration);
builder.Services.AddCustomApiVersioning();

var app = builder.Build();
await app.ApplyDatabaseMigrationsAsync();
app.UseApiMiddlewares();
app.UseSwaggerConfiguration();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
