using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using SupplierDirectory.Domain;
using SupplierDirectory.Infrastructure.Validation;

namespace SupplierDirectory.Application;

public class ServiceListQuery
{
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class ServiceFormViewModel
{
    public int? Id { get; set; }
    
    [Required(ErrorMessage = "اسم الخدمة مطلوب")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "اسم الخدمة يجب أن يكون بين 2 و 200 حرف")]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(1000, ErrorMessage = "الوصف يجب ألا يتجاوز 1000 حرف")]
    public string? Description { get; set; }

    [StringLength(3000, ErrorMessage = "التفاصيل يجب ألا تتجاوز 3000 حرف")]
    public string? Details { get; set; }
    
    public int DisplayOrder { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    [MaxFileSize(5 * 1024 * 1024)]
    [AllowedExtensions(new[] { ".jpg", ".png", ".jpeg", ".webp" })]
    public List<IFormFile> NewImages { get; set; } = new();
    
    public List<ServiceImage> ExistingImages { get; set; } = new();
}
