using CityInfo.API.DataStore;
using CityInfo.API.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.StaticFiles;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs/cotyInfo.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
//builder.Logging.ClearProviders();
//builder.Logging.AddConsole(); // Write Logs To Console

builder.Host.UseSerilog();
// Add services to the container.

builder.Services.AddControllers(options =>
{
    options.ReturnHttpNotAcceptable = true; // 406 NOT Acceptable
}).AddNewtonsoftJson()
  .AddXmlDataContractSerializerFormatters();


/* Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    Swashbuckele is an implementation of Open API or Swagger
    Swagger enables documentation of our API
*/
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Identify File extension
builder.Services.AddSingleton<FileExtensionContentTypeProvider>();

#if DEBUG
builder.Services.AddTransient<IMailService, LocalMailService>();
#else
builder.Services.AddTransient<IMailService, CloudMailService>();
#endif

builder.Services.AddSingleton<ICitiesDataStore, CitiesDataStore>();

WebApplication app = builder.Build();

//// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();   // generate Specification
    app.UseSwaggerUI(); // generate UI
}

app.UseHttpsRedirection(); //HTTP calls are redirected to HTTPS

app.UseRouting(); // Find the Route

app.UseAuthorization();

app.UseEndpoints // Execute the Route
    (endpoints =>
{
    endpoints.MapControllers(); // Routs will be defined on attributes on Controllers or EndPoints
});

app.Run();

/* Services - build in DI Container
   By adding Services, we can inject them where we want them in our Code
   Services - seen as common consumption in an app
   builder.Services.AddControllers(); - will enable build of Controllers
*/

/* Build Pipeline
 WebApplication : IApplicationBuilder
 - define the Application Request pipeline 
 */

/*.Net 6 conventions
 public static void Program(args) is hidden 
 app.MapControllers(); replaces app.UseRouting() and app.UseEndpoints() 
*/

/* Simulate a middleware Call.
    app.Run(async (context) =>
    {
        await context.Response.WriteAsync("Hello World");
    });
*/

/*  Content Negociation
 Output Formatter
 representation for a given response when multiple representations are available
   Accept Header - the media type we accept
 - application/json
 - application/xml

  Input formatters
   Content Type -> POST -> Json
*/

/* 406 NOT Acceptable
 If the GET request has the Accept - application/XML header
 The API will return the default result - JSON
 options.ReturnHttpNotAcceptable = true will ENFORCE -> 406 NOT Acceptable, if application/XML is not supported
*/

/*Add XML support
 .AddXmlDataContractSerializerFormatters();
*/

/*  IoC
 *  If we have a hard dependency like Logger and we need to replace it, we need to change the Code
 *  We can not replace the Concrete type with a Mock when Testing
 *  This it thight Coupling
 *  IoC delegates the selction of selecting a concrete implementation to an external component
 *  DI pattern is a specialization of the IoC principle. Uses a specialized object - the Container to initialize objects and provide the required dependencies to the object
 *  Services are registered to the Container
 *  The container provides the concrete classes
 *  Classes should be decoupled from the Concrete type   

 We use Constructor Injection but we can also access internally a service:
 
 Httpcontext.RequestServices.GetService
 public PointsOfInterestController(ILogger<PointsOfInterestController> _logger)
       {
           logger = _logger?? throw new ArgumentNullException(nameof(logger));
       }
*/

/* Services - DI Container
 * AddTransient - created each time is requested
 * AddScoped - created once per Request
 * AddSingleton - created first time they are reqested
 * builder.Services.AddTransient<IMailService, LocalMailService>();
*/

/* Define Configuration Parameters
   - Inject  - IConfiguration configuration
   - Access - configuration["mailSettings:mailToAddress"]         

   Configuration Files are environment dependent - Development/Production
   If a value in configured in multiple locations the more specific one will overwrite the general one
   appsettings.Production.json will overwrite appsettings.json
    "mailSettings": {
    "mailToAddress": "production@mycompany.com"
  }
 */

/*  Compiler directives for Mapping Interfaces(development/production):
    #if DEBUG
    builder.Services.AddTransient<IMailService, LocalMailService>();
    #else
    builder.Services.AddTransient<IMailService, CloudMailService>();
    #endif
 */