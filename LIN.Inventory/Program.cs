using Http.Extensions;
using LIN.Access.Logger;
using LIN.Inventory.Data;
using LIN.Inventory.Extensions;
using LIN.Access.Auth;

var builder = WebApplication.CreateBuilder(args);

// Logger.
builder.Services.AddServiceLogging("LIN.INVENTORY");

// Servicios.
builder.Services.AddSignalR();
builder.Services.AddHttpContextAccessor();
builder.Services.AddLINHttp();
builder.Services.AddLocalServices();

// Autenticaci�n
builder.Services.AddAuthenticationService();


builder.Services.AddDbContextPool<Context>(options =>
  {
      options.UseSqlServer(builder.Configuration["ConnectionStrings:Somee"]);
  });

// Add services to the container.
string sqlConnection = builder.Configuration["ConnectionStrings:Somee"] ?? string.Empty;

if (sqlConnection.Length > 0)
{
    // SQL Server
    builder.Services.AddDbContext<Context>(options =>
    {
        options.UseSqlServer(sqlConnection);
    });
}

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
app.UseLocalServices(builder.Configuration);

LIN.Access.Contacts.Build.Init();

app.UseServiceLogging();

app.Run();