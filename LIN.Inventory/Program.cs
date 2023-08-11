global using LIN.Server;
global using LIN.Server.Controllers;
global using LIN.Server.DBModels;
global using LIN.Server.Hubs;
global using LIN.Server.Services;
global using LIN.Shared.Enumerations;
global using LIN.Shared.Models;
global using LIN.Types.Responses;
global using Http.ResponsesList;
global using LIN.Shared.Security;
global using LIN.Shared.Tools;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.SignalR;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.IdentityModel.Tokens;
global using Newtonsoft.Json;
global using System.Diagnostics;
global using System.Text;
using LIN.Server.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

try
{

    builder.Services.AddSignalR();


    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAnyOrigin",
            builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyHeader()
                       .AllowAnyMethod();
            });
    });


    // Add services to the container.
    string sqlConnection = string.Empty;

    string devServiceUrl = string.Empty;

#if AZURE
    sqlConnection = builder.Configuration["ConnectionStrings:Azure"] ?? string.Empty;
    devServiceUrl = builder.Configuration["lin:developer:Azure"] ?? string.Empty;
#elif SOMEE
    sqlConnection = builder.Configuration["ConnectionStrings:Somee"] ?? string.Empty;
    devServiceUrl = builder.Configuration["lin:developer:Somee"] ?? string.Empty;
#endif

    Conexión.SetStringConnection(sqlConnection);


    if (sqlConnection.Length > 0)
    {
        // SQL Server
        builder.Services.AddDbContext<Context>(options =>
        {
            options.UseSqlServer(sqlConnection);
        });
    }



    // LIN Services
    Developers.SetKey(builder.Configuration["lin:key"] ?? string.Empty);
    Developers.SetUrl(devServiceUrl);


    builder.Services.AddControllers();


    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new()
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidAudience = null,
            ValidIssuer = null,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["jwt:key"] ?? string.Empty))
        };
    });

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.AddHttpContextAccessor();

    var app = builder.Build();


    try
    {
        // Si la base de datos no existe
        using var scope = app.Services.CreateScope();
        var dataContext = scope.ServiceProvider.GetRequiredService<Context>();
        var res = dataContext.Database.EnsureCreated();
    }
    catch
    { }





    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
    }

    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseCors("AllowAnyOrigin");
    app.UseAuthentication();
    app.UseAuthorization();



    app.MapControllers();

    // Rutas de servicios de tiempo real
    app.MapHub<ProductsHub>("/Realtime/productos");
    app.MapHub<AccountHub>("/Realtime/account");
    app.MapHub<PassKeyHub>("/Realtime/passkey");

    app.UseRouting();

    app.MapGet("/", () => "LIN APP Services esta funcionando");


    // Inicia las conexiones
    _ = Conexión.StartConnections();

    // Inicia el servicio de mails
    EmailWorker.StarService();

    // Inicio de Jwt
    Jwt.Open();


    // Estado del servidor
    ServerLogger.OpenDate = DateTime.Now;


    app.Run();
}
catch (Exception ex)
{
    ServerLogger.LogError("--Servidor--: " + ex.Message);
}