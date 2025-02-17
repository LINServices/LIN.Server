using LIN.Types.Inventory.Models;
using Microsoft.EntityFrameworkCore;

namespace LIN.Inventory.Persistence.Context;

/// <summary>
/// Nuevo contexto a la base de datos
/// </summary>
public class Context(DbContextOptions<Context> options) : DbContext(options)
{

    /// <summary>
    /// Tabla de usuarios
    /// </summary>
    public DbSet<ProfileModel> Profiles { get; set; }


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
    public DbSet<ProductModel> Productos { get; set; }


    /// <summary>
    /// Detalles de productos
    /// </summary>
    public DbSet<ProductDetailModel> ProductoDetalles { get; set; }


    /// <summary>
    /// Acceso a los Inventarios
    /// </summary>
    public DbSet<InventoryAcessDataModel> AccesoInventarios { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        // Indices y identidad
        modelBuilder.Entity<ProfileModel>()
           .HasIndex(e => e.AccountId)
           .IsUnique();

        modelBuilder.Entity<ProductModel>()
            .HasMany(t => t.Details)
            .WithOne(t => t.Product)
            .HasForeignKey(t => t.ProductId);


        modelBuilder.Entity<InventoryDataModel>()
         .HasMany(t => t.Products)
         .WithOne(t => t.Inventory)
         .HasForeignKey(t => t.InventoryId);

        modelBuilder.Entity<InventoryDataModel>()
          .HasMany(t => t.Inflows)
          .WithOne(t => t.Inventory)
          .HasForeignKey(t => t.InventoryId);


        modelBuilder.Entity<InventoryDataModel>()
         .HasMany(t => t.Outflows)
         .WithOne(t => t.Inventory)
         .HasForeignKey(t => t.InventoryId);

        modelBuilder.Entity<InflowDataModel>()
         .HasMany(t => t.Details)
         .WithOne(t => t.Movement)
         .HasForeignKey(t => t.MovementId);

        modelBuilder.Entity<InflowDetailsDataModel>()
          .HasOne(t => t.ProductDetail)
          .WithMany()
          .HasForeignKey(t => t.ProductDetailId)
          .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<OutflowDataModel>()
             .HasMany(t => t.Details)
             .WithOne(t => t.Movement)
             .HasForeignKey(t => t.MovementId);

        modelBuilder.Entity<OutflowDetailsDataModel>()
          .HasOne(t => t.ProductDetail)
          .WithMany()
          .HasForeignKey(t => t.ProductDetailId)
          .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<ProfileModel>().ToTable("Profile");
        modelBuilder.Entity<InventoryDataModel>().ToTable("Inventory");
        modelBuilder.Entity<InflowDataModel>().ToTable("Inflow");
        modelBuilder.Entity<OutflowDataModel>().ToTable("Outflow");
        modelBuilder.Entity<InflowDetailsDataModel>().ToTable("InflowDetail");
        modelBuilder.Entity<OutflowDetailsDataModel>().ToTable("OutflowDetail");
        modelBuilder.Entity<ProductDetailModel>().ToTable("ProductDetail");
        modelBuilder.Entity<ProductModel>().ToTable("Product");
        modelBuilder.Entity<InventoryAcessDataModel>().ToTable("InventoryAccess");
    }

}