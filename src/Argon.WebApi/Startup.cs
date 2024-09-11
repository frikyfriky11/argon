namespace Argon.WebApi;

public class Startup(
  IConfiguration configuration,
  IWebHostEnvironment webHostEnvironment
)
{
  public void ConfigureServices(IServiceCollection services)
  {
    // wire up the application layer
    services.AddApplicationServices();

    // wire up the infrastructure layer
    services.AddInfrastructureServices(configuration, webHostEnvironment);

    // configure the API layer
    services.AddWebApiServices(configuration);
  }

  public void Configure(IApplicationBuilder app, IWebHostEnvironment webHostEnvironment, ApplicationDbContextInitializer dbContextInitializer)
  {
    app.UseSerilogRequestLogging();

    // initialize and seed the database context only if not running for NSwag generation
    if (!configuration.GetValue<bool>("RUNNING_NSWAG"))
    {
      dbContextInitializer.Initialize();
      dbContextInitializer.Seed();
    }

    // configure the OpenAPI generators and the Swagger GUI
    app.UseOpenApi();
    app.UseSwaggerUi3();

    // add the health checks endpoint
    app.UseHealthChecks("/healthz");

    // add the attribute routing capability
    app.UseRouting();

    // use the CORS policy configured
    app.UseCors("CorsPolicy");

    // add the attribute authorization capability
    app.UseAuthentication();
    app.UseAuthorization();

    // map the controller endpoints
    app.UseEndpoints(endpoints => endpoints.MapControllers());
  }
}
