using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using SupplierDirectory.Infrastructure.Validation;

namespace SupplierDirectory.Application;

// =================== تصنيفات الموردين (Supplier Categories) ===================
public sealed class CategoryListQuery
{
    public string? Search { get; set; }
    public int? ParentId { get; set; }
    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;
    [Range(10, 100)]
    public int PageSize { get; set; } = 20;
}

public sealed class CategoryListItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int? ParentCategoryId { get; set; }
    public string? ParentName { get; set; }
    public int SuppliersCount { get; set; }
    public int SubCategoriesCount { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class CategoryListViewModel
{
    public required IReadOnlyList<CategoryListItem> Items { get; set; }
    public required IReadOnlyList<(int Id, string Name)> Parents { get; set; }
    public required CategoryListQuery Query { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalCount / (double)Query.PageSize));
}

public sealed class CategoryFormViewModel
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "اسم التصنيف مطلوب")]
    [StringLength(150, ErrorMessage = "اسم التصنيف يجب ألا يتجاوز 150 حرف")]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "الوصف يجب ألا يتجاوز 1000 حرف")]
    public string? Description { get; set; }

    public int? ParentCategoryId { get; set; }

    public bool IsActive { get; set; } = true;

    [MaxFileSize(5 * 1024 * 1024)]
    [AllowedExtensions(new[] { ".jpg", ".png", ".jpeg", ".webp" })]
    public IFormFile? ImageFile { get; set; }

    public string? ExistingImageUrl { get; set; }

    public IReadOnlyList<(int Id, string Name)> Parents { get; set; } = [];
}

// =================== تصنيفات المنتجات (Product Categories) ===================
public sealed class ProductCategoryListQuery
{
    public string? Search { get; set; }
    public int? ParentId { get; set; }
    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;
    [Range(10, 100)]
    public int PageSize { get; set; } = 20;
}

public sealed class ProductCategoryListItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int? ParentCategoryId { get; set; }
    public string? ParentName { get; set; }
    public int ProductsCount { get; set; }
    public int SubCategoriesCount { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class ProductCategoryListViewModel
{
    public required IReadOnlyList<ProductCategoryListItem> Items { get; set; }
    public required IReadOnlyList<(int Id, string Name)> Parents { get; set; }
    public required ProductCategoryListQuery Query { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalCount / (double)Query.PageSize));
}

public sealed class ProductCategoryFormViewModel
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "اسم تصنيف المنتج مطلوب")]
    [StringLength(150, ErrorMessage = "اسم التصنيف يجب ألا يتجاوز 150 حرف")]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "الوصف يجب ألا يتجاوز 1000 حرف")]
    public string? Description { get; set; }

    public int? ParentCategoryId { get; set; }

    public bool IsActive { get; set; } = true;

    [MaxFileSize(5 * 1024 * 1024)]
    [AllowedExtensions(new[] { ".jpg", ".png", ".jpeg", ".webp" })]
    public IFormFile? ImageFile { get; set; }

    public string? ExistingImageUrl { get; set; }

    public IReadOnlyList<(int Id, string Name)> Parents { get; set; } = [];
}
