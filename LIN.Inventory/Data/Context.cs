namespace LIN.Inventory.Data;


public class Context : DbContext
{



    /// <summary>
    /// Tabla de usuarios
    /// </summary>
    public DbSet<ProfileModel> Profiles { get; set; }



    /// <summary>
    /// Contactos
    /// </summary>
    public DbSet<ContactDataModel> Contactos { get; set; }



    /// <summary>
    /// Inventarios
    /// </summary>
    public DbSet<InventoryDataModel> Inventarios { get; set; }



    /// <summary>
    /// Entradas
    /// </summary>
    public DbSet<InflowDataModel> Entradas { get; set; }



    /// <summary>
    /// Detalles de entradas
    /// </summary>
    public DbSet<InflowDetailsDataModel> DetallesEntradas { get; set; }



    /// <summary>
    /// Salidas
    /// </summary>
    public DbSet<OutflowDataModel> Salidas { get; set; }



    /// <summary>
    /// Detalles de salidas
    /// </summary>
    public DbSet<OutflowDetailsDataModel> DetallesSalidas { get; set; }



    /// <summary>
    /// Productos
    /// </summary>
    public DbSet<ProductoTable> Productos { get; set; }



    /// <summary>
    /// Detalles de productos
    /// </summary>
    public DbSet<ProductoDetailTable> ProductoDetalles { get; set; }



    /// <summary>
    /// Plantillas de productos
    /// </summary>
    public DbSet<ProductTemplateTable> PlantillaProductos { get; set; }



    /// <summary>
    /// Acceso a los Inventarios
    /// </summary>
    public DbSet<InventoryAcessDataModel> AccesoInventarios { get; set; }




    /// <summary>
    /// Nuevo contexto a la base de datos
    /// </summary>
    public Context(DbContextOptions<Context> options) : base(options) { }




    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        // Indices y identidad
        modelBuilder.Entity<ProfileModel>()
           .HasIndex(e => e.AccountID)
           .IsUnique();


        modelBuilder.Entity<ContactDataModel>().HasIndex(e => e.ID);
        modelBuilder.Entity<InventoryDataModel>().HasIndex(e => e.ID);
        modelBuilder.Entity<ProductoTable>().HasIndex(e => e.ID);
        modelBuilder.Entity<ProductoDetailTable>().HasIndex(e => e.ID);
        modelBuilder.Entity<ProductTemplateTable>().HasIndex(e => e.ID);

        // Nombre de la tablas
        modelBuilder.Entity<ProfileModel>().ToTable("PROFILES");
        modelBuilder.Entity<ContactDataModel>().ToTable("CONTACTOS");
        modelBuilder.Entity<InventoryDataModel>().ToTable("INVENTARIOS");
        modelBuilder.Entity<InventoryAcessDataModel>().ToTable("ACCESOS_INVENTARIO");
        modelBuilder.Entity<ProductoTable>().ToTable("PRODUCTOS");
        modelBuilder.Entity<ProductoDetailTable>().ToTable("PRODUCTOS_DETALLES");
        modelBuilder.Entity<ProductTemplateTable>().ToTable("PLANTILLA_PRODUCTOS");
        modelBuilder.Entity<InflowDataModel>().ToTable("ENTRADAS");
        modelBuilder.Entity<InflowDetailsDataModel>().ToTable("ENTRADA_DETALLES");
        modelBuilder.Entity<OutflowDataModel>().ToTable("SALIDAS");
        modelBuilder.Entity<OutflowDetailsDataModel>().ToTable("SALIDA_DETALLES");

    }



}
