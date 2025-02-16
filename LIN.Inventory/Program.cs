using Http.Extensions;
using LIN.Access.Auth;
using LIN.Access.Contacts;
using LIN.Access.Logger;
using LIN.Inventory.Data;
using LIN.Inventory.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Logger.
builder.Services.AddLocalServices();

// Servicios.
builder.Services.AddSignalR();
builder.Services.AddHttpContextAccessor();
builder.Services.AddLINHttp();
builder.Services.AddLocalServices();

// LIN Services.
builder.Services.AddAuthenticationService(builder.Configuration["services:auth"], builder.Configuration["lin:app"]);
builder.Services.AddContactsService(builder.Configuration["services:contacts"]);

builder.Services.AddDbContext<Context>(options =>
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

builder.Services.AddDatabaseAction(() =>
{
    var context = app.Services.GetRequiredService<Context>();
    context.Profiles.Where(x => x.Id == 0).FirstOrDefaultAsync();
    return "Success";
});

app.Run();