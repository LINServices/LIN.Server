namespace LIN.Server.Data;


public class Context : DbContext
{



    /// <summary>
    /// Tabla de usuarios
    /// </summary>
    public DbSet<ProfileModel> Profiles { get; set; }


    /// <summary>
    /// Tabla de Links
    /// </summary>
    public DbSet<ChangeLink> Links { get; set; }




    /// <summary>
    /// Tabla de Links
    /// </summary>
    public DbSet<EmailLink> EmailLinks { get; set; }



    /// <summary>
    /// Logins
    /// </summary>
    public DbSet<UserAccessLogDataModel> Logins { get; set; }



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
    /// Plantillas de productos
    /// </summary>
    public DbSet<EmailDataModel> Emails { get; set; }




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

        // Indices y indentidad
        modelBuilder.Entity<UserDataModel>()
           .HasIndex(e => e.Usuario)
           .IsUnique();

        // Indices y indentidad
        modelBuilder.Entity<ChangeLink>()
           .HasIndex(e => e.Key)
           .IsUnique();

        // Indices y indentidad
        modelBuilder.Entity<EmailDataModel>()
           .HasIndex(e => e.Email)
           .IsUnique();

        // Indices y indentidad
        modelBuilder.Entity<EmailLink>()
           .HasIndex(e => e.Key)
           .IsUnique();

        modelBuilder.Entity<ContactDataModel>().HasIndex(e => e.ID);
        modelBuilder.Entity<InventoryDataModel>().HasIndex(e => e.ID);
        modelBuilder.Entity<ProductoTable>().HasIndex(e => e.ID);
        modelBuilder.Entity<ProductoDetailTable>().HasIndex(e => e.ID);
        modelBuilder.Entity<ProductTemplateTable>().HasIndex(e => e.ID);

        // Nombre de la tablas
        modelBuilder.Entity<UserDataModel>().ToTable("USUARIOS");
        modelBuilder.Entity<EmailDataModel>().ToTable("EMAILS");
        modelBuilder.Entity<ContactDataModel>().ToTable("CONTACTOS");
        modelBuilder.Entity<InventoryDataModel>().ToTable("INVENTARIOS");
        modelBuilder.Entity<InventoryAcessDataModel>().ToTable("ACCESOS_INVENTARIO");
        modelBuilder.Entity<ProductoTable>().ToTable("PRODUCTOS");
        modelBuilder.Entity<ProductoDetailTable>().ToTable("PRODUCTOS_DETALLES");
        modelBuilder.Entity<ProductTemplateTable>().ToTable("PLANTILLA_PRODUCTOS");
        modelBuilder.Entity<UserAccessLogDataModel>().ToTable("LOGINS");
        modelBuilder.Entity<InflowDataModel>().ToTable("ENTRADAS");
        modelBuilder.Entity<InflowDetailsDataModel>().ToTable("ENTRADA_DETALLES");
        modelBuilder.Entity<OutflowDataModel>().ToTable("SALIDAS");
        modelBuilder.Entity<OutflowDetailsDataModel>().ToTable("SALIDA_DETALLES");

    }



}
