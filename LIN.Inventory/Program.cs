using Http.Extensions;
using LIN.Inventory.Data;

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


#if RELEASE 
    sqlConnection = builder.Configuration["ConnectionStrings:Somee"] ?? string.Empty;
    devServiceUrl = builder.Configuration["lin:developer:Somee"] ?? string.Empty;
#elif DEBUG
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



    LIN.Access.Auth.Build.SetAuth(builder.Configuration["lin:app"] ?? string.Empty);

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddLINHttp();

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




    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseAuthentication();
    app.UseAuthorization();



    app.MapControllers();

    app.UseLINHttp();

    // Rutas de servicios de tiempo real
    app.MapHub<InventoryHub>("/Realtime/inventory");

    app.UseRouting();

    // Inicia las conexiones
    _ = Conexión.StartConnections();

    // Inicio de Jwt
    Jwt.Open();

    LIN.Access.Auth.Build.Init();
    LIN.Access.Contacts.Build.Init();


    // Estado del servidor
    ServerLogger.OpenDate = DateTime.Now;


    app.Run();
}
catch (Exception ex)
{
    ServerLogger.LogError("--Servidor--: " + ex.Message);
}