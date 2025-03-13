using CompanyEmployees.Extensions;
using LoggerService;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using NLog;

var logger = LogManager.Setup().LoadConfigurationFromFile("nlog.config").GetCurrentClassLogger();
try
{ 
var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration?.GetConnectionString("sqlConnection");
    // Add services to the container.
builder.Services.ConfigureCors();
builder.Services.ConfigureIISIntegration();
builder.Services.ConfigureLoggerService();
builder.Services.ConfigureSqlContext(connectionString);
builder.Services.ConfigureRepositoryManager();
    builder.Services.AddControllers(config =>
    {
        config.RespectBrowserAcceptHeader = true;
        config.ReturnHttpNotAcceptable = true;
    }).AddXmlDataContractSerializerFormatters();

    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(typeof(Program));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}
app.ConfigureExceptionHandler(new LoggerManager());
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseForwardedHeaders(new ForwardedHeadersOptions { ForwardedHeaders = ForwardedHeaders.All });
app.UseRouting(); 
app.UseCors("CorsPolicy");
app.UseAuthorization();
app.MapControllers();
app.Run();
}
catch (Exception ex)
{
    logger.Error(ex, "Application failed to start.");
throw;
}
finally
{
    NLog.LogManager.Shutdown();
}