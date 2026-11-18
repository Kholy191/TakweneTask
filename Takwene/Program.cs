using System.Text.Json.Serialization;
using Serilog;
using Takwene.API.ErrorHandling;
using Takwene.API.Filters;
using Takwene.API.OpenApi;
using Takwene.Application;
using Takwene.Infrastructure;
using Takwene.Infrastructure.Persistence.Seeding;

// Bootstrap logger catches startup failures before configuration is loaded.
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Sinks (Console + Seq) and levels come from the "Serilog" config section.
    builder.Services.AddSerilog((services, logger) => logger
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    builder.Services.AddApplicationServices();
    builder.Services.AddInfrastructureServices(builder.Configuration);

    // FluentValidation runs for every action argument; enums travel as names ("Draft").
    builder.Services
        .AddControllers(options => options.Filters.Add<ValidationFilter>())
        .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

    // The OpenAPI generator reads these (not the MVC ones), so enums are documented as strings too.
    builder.Services.ConfigureHttpJsonOptions(options => options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

    // The Angular app runs on another origin; allowed origins come from "Cors:AllowedOrigins".
    var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
    builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy
        .WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod()));
    builder.Services.AddOpenApi(options => options.AddDocumentTransformer<BearerSecuritySchemeTransformer>());
    builder.Services.AddProblemDetails();
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

    var app = builder.Build();

    await app.Services.InitializeDatabaseAsync();

    app.UseExceptionHandler();
    app.UseSerilogRequestLogging();

    // Always on, so reviewers get the docs in any environment; Swagger UI is the home page ("/").
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Takwene API v1");
        options.RoutePrefix = string.Empty;
    });

    app.UseHttpsRedirection();
    app.UseCors();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    await app.RunAsync();
}
// HostAbortedException is thrown on purpose by "dotnet ef" tooling; not a real failure.
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}
