using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using SupplierDirectory.Domain;
using SupplierDirectory.Infrastructure.Validation;

namespace SupplierDirectory.Application;

public class ProductListQuery
{
    public string? Search { get; set; }
    public int? AreaId { get; set; }
    public int? ProductCategoryId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class ProductFormViewModel
{
    public int? Id { get; set; }
    
    [Required(ErrorMessage = "اسم المنتج مطلوب")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "اسم المنتج يجب أن يكون بين 2 و 200 حرف")]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(1000, ErrorMessage = "الوصف يجب ألا يتجاوز 1000 حرف")]
    public string? Description { get; set; }

    [StringLength(2000, ErrorMessage = "المعلومات يجب ألا تتجاوز 2000 حرف")]
    public string? Details { get; set; }
    
    public int? AreaId { get; set; }
    public int? ProductCategoryId { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    [MaxFileSize(5 * 1024 * 1024)]
    [AllowedExtensions(new[] { ".jpg", ".png", ".jpeg", ".webp" })]
    public List<IFormFile>? NewImages { get; set; } = new();
    
    public List<ProductImage> ExistingImages { get; set; } = new();
    
    public IReadOnlyList<Area> AvailableAreas { get; set; } = Array.Empty<Area>();
    public IReadOnlyList<ProductCategory> AvailableCategories { get; set; } = Array.Empty<ProductCategory>();
}
