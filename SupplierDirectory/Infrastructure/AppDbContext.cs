using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SupplierDirectory.Domain;

namespace SupplierDirectory.Infrastructure;
public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
 public DbSet<Area> Areas => Set<Area>(); public DbSet<Category> Categories => Set<Category>(); public DbSet<Supplier> Suppliers => Set<Supplier>(); public DbSet<SupplierImage> SupplierImages => Set<SupplierImage>(); public DbSet<SupplierCategory> SupplierCategories => Set<SupplierCategory>(); public DbSet<SupplierArea> SupplierAreas => Set<SupplierArea>(); public DbSet<Advertisement> Advertisements => Set<Advertisement>(); public DbSet<CompanyInfo> CompanyInfos => Set<CompanyInfo>();
 protected override void OnModelCreating(ModelBuilder b) { base.OnModelCreating(b);
  b.Entity<Area>().HasQueryFilter(x=>!x.IsDeleted); b.Entity<Category>().HasQueryFilter(x=>!x.IsDeleted); b.Entity<Supplier>().HasQueryFilter(x=>!x.IsDeleted); b.Entity<Advertisement>().HasQueryFilter(x=>!x.IsDeleted);
  b.Entity<Area>().HasIndex(x=>x.Name); b.Entity<Category>().HasIndex(x=>x.Name).IsUnique(); b.Entity<Supplier>().HasIndex(x=>x.Name); b.Entity<Advertisement>().HasIndex(x=>new { x.IsActive,x.StartDate,x.EndDate });
  b.Entity<Area>().HasOne(x=>x.ParentArea).WithMany(x=>x.Children).HasForeignKey(x=>x.ParentAreaId).OnDelete(DeleteBehavior.Restrict);
  b.Entity<SupplierCategory>().HasKey(x=>new {x.SupplierId,x.CategoryId}); b.Entity<SupplierArea>().HasKey(x=>new {x.SupplierId,x.AreaId});
 }
 public override Task<int> SaveChangesAsync(CancellationToken ct=default) { foreach(var e in ChangeTracker.Entries<AuditableEntity>().Where(x=>x.State==EntityState.Modified)) e.Entity.UpdatedAt=DateTime.UtcNow; return base.SaveChangesAsync(ct); }
}
