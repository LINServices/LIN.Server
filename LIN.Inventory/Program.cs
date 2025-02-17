using Http.Extensions;
using LIN.Access.Auth;
using LIN.Access.Contacts;
using LIN.Inventory.Extensions;
using LIN.Inventory.Persistence.Extensions;

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
builder.Services.AddPersistence(builder.Configuration);

var app = builder.Build();

app.UsePersistence();
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
    var context = app.Services.GetRequiredService<LIN.Inventory.Persistence.Context.Context>();
    context.Profiles.Where(x => x.Id == 0).FirstOrDefaultAsync();
    return "Success";
});

app.Run();