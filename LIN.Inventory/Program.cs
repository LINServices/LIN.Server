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

#if AZURE
    sqlConnection = builder.Configuration["ConnectionStrings:Azure"] ?? string.Empty;
    devServiceUrl = builder.Configuration["lin:developer:Azure"] ?? string.Empty;
#elif RELEASE 
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



    // LIN Services
    Developers.SetKey(builder.Configuration["lin:key"] ?? string.Empty);
    Developers.SetUrl(devServiceUrl);


    LIN.Access.Auth.Build.SetAuth(builder.Configuration["lin:app"] ?? string.Empty);

    builder.Services.AddControllers();

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
    app.MapHub<InventoryHub>("/Realtime/inventory");

    app.UseRouting();

    app.MapGet("/", () => "LIN APP Services esta funcionando");


    // Inicia las conexiones
    _ = Conexión.StartConnections();

    // Inicia el servicio de mails
    EmailWorker.StarService();

    // Inicio de Jwt
    Jwt.Open();

    LIN.Access.Auth.Build.Init();



    // Estado del servidor
    ServerLogger.OpenDate = DateTime.Now;


    app.Run();
}
catch (Exception ex)
{
    ServerLogger.LogError("--Servidor--: " + ex.Message);
}