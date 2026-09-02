using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using SupplierDirectory.Domain;
using SupplierDirectory.Infrastructure.Validation;

namespace SupplierDirectory.Application;

public class SupplierListQuery
{
    public string? Search { get; set; }
    public int? CategoryId { get; set; }
    public int? AreaId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class SupplierListViewModel
{
    public IReadOnlyList<Supplier> Items { get; set; } = Array.Empty<Supplier>();
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)Query.PageSize);
    public SupplierListQuery Query { get; set; } = new();
    
    public IReadOnlyList<Category> Categories { get; set; } = Array.Empty<Category>();
    public IReadOnlyList<Area> Areas { get; set; } = Array.Empty<Area>();
}

public class SupplierFormViewModel
{
    public int? Id { get; set; }
    
    [Required(ErrorMessage = "اسم المورد مطلوب")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "اسم المورد يجب أن يكون بين 3 و 200 حرف")]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(1000, ErrorMessage = "الوصف يجب ألا يتجاوز 1000 حرف")]
    public string? Description { get; set; }
    
    [MaxFileSize(5 * 1024 * 1024)]
    [AllowedExtensions(new[] { ".jpg", ".png", ".jpeg", ".webp" })]
    public IFormFile? LogoFile { get; set; }
    
    public string? ExistingLogoUrl { get; set; }
    
    [Phone(ErrorMessage = "رقم الهاتف غير صحيح")]
    [StringLength(20, ErrorMessage = "الرقم لا يمكن أن يتجاوز 20 حرف")]
    public string? PhoneNumber { get; set; }
    
    [StringLength(100, ErrorMessage = "الأرقام الإضافية طويلة جداً")]
    public string? AdditionalPhoneNumbers { get; set; }
    
    [Phone(ErrorMessage = "رقم الواتساب غير صحيح")]
    [StringLength(20, ErrorMessage = "رقم الواتساب لا يمكن أن يتجاوز 20 حرف")]
    public string? WhatsAppNumber { get; set; }
    
    [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صحيح")]
    [StringLength(100, ErrorMessage = "البريد الإلكتروني يجب ألا يتجاوز 100 حرف")]
    public string? Email { get; set; }
    
    [Url(ErrorMessage = "رابط الموقع غير صحيح")]
    [StringLength(300, ErrorMessage = "الرابط يجب ألا يتجاوز 300 حرف")]
    public string? Website { get; set; }
    
    [StringLength(300, ErrorMessage = "العنوان يجب ألا يتجاوز 300 حرف")]
    public string? Address { get; set; }
    
    [StringLength(100, ErrorMessage = "العنوان المختصر يجب ألا يتجاوز 100 حرف")]
    public string? ShortAddress { get; set; }
    
    public bool HasTechnicians { get; set; }
    
    [Range(-90, 90, ErrorMessage = "خط العرض يجب أن يكون بين -90 و 90")]
    public decimal? Latitude { get; set; }
    
    [Range(-180, 180, ErrorMessage = "خط الطول يجب أن يكون بين -180 و 180")]
    public decimal? Longitude { get; set; }
    
    [Url(ErrorMessage = "رابط الخرائط غير صحيح")]
    [StringLength(500, ErrorMessage = "الرابط يجب ألا يتجاوز 500 حرف")]
    public string? GoogleMapsUrl { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public List<int> SelectedCategoryIds { get; set; } = new();
    public List<int> SelectedAreaIds { get; set; } = new();
    
    [MaxFileSize(5 * 1024 * 1024)]
    [AllowedExtensions(new[] { ".jpg", ".png", ".jpeg", ".webp" })]
    public List<IFormFile>? NewImages { get; set; } = new();
    
    public List<SupplierImage> ExistingImages { get; set; } = new();
    
    public IReadOnlyList<Category> AvailableCategories { get; set; } = Array.Empty<Category>();
    public IReadOnlyList<Area> AvailableAreas { get; set; } = Array.Empty<Area>();
}
