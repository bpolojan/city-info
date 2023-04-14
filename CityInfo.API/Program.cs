using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.StaticFiles;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers(options =>
{
    options.ReturnHttpNotAcceptable = true;
}).AddXmlDataContractSerializerFormatters();

/* Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    Swashbuckele is an implementation of Open API or Swagger
    Swagger enables documentation of our API
*/
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Identitfy File type
builder.Services.AddSingleton<FileExtensionContentTypeProvider>();

WebApplication app = builder.Build();

//// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();   //generate Specification
    app.UseSwaggerUI(); //generate UI
}

app.UseHttpsRedirection(); //HTTP calls are redirected to HTTPS

app.UseRouting(); //find the Route

app.UseAuthorization();

app.UseEndpoints //execute the Route
    (endpoints =>
{
    endpoints.MapControllers(); //Routs will be defined on attributes
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
 If the GET request has the Accept - application/xml header
 The API will return the default result - JSON
 options.ReturnHttpNotAcceptable = true will ENFORCE -> 406 NOT Acceptable, if application/xml is not supported
*/

/*Add XML support
 .AddXmlDataContractSerializerFormatters();
*/