using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using SupplierDirectory.Domain;
using SupplierDirectory.Infrastructure.Validation;

namespace SupplierDirectory.Application;

public class CompanyFormViewModel
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "اسم الشركة مطلوب")]
    [StringLength(200, ErrorMessage = "اسم الشركة يجب ألا يتجاوز 200 حرف")]
    public string? CompanyName { get; set; }

    [MaxFileSize(5 * 1024 * 1024)]
    [AllowedExtensions(new[] { ".jpg", ".png", ".jpeg", ".webp" })]
    public IFormFile? LogoFile { get; set; }

    [MaxFileSize(5 * 1024 * 1024)]
    [AllowedExtensions(new[] { ".jpg", ".png", ".jpeg", ".webp" })]
    public IFormFile? CoverFile { get; set; }

    public string? LogoUrl { get; set; }
    public string? CoverImageUrl { get; set; }

    [Phone(ErrorMessage = "رقم الهاتف غير صحيح")]
    public string? ContactPhone { get; set; }
    
    [Phone(ErrorMessage = "رقم الواتساب غير صحيح")]
    public string? WhatsApp { get; set; }
    
    [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صحيح")]
    public string? Email { get; set; }
    
    [Url(ErrorMessage = "رابط الموقع الإلكتروني غير صحيح")]
    public string? Website { get; set; }

    public string? About { get; set; }
    public string? Mission { get; set; }
    public string? Vision { get; set; }
    public string? PlatformDescription { get; set; }
    public string? PlatformServices { get; set; }
    public string? SocialLinksJson { get; set; }
}

public class AdvertisementFormViewModel
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "عنوان الإعلان مطلوب")]
    [StringLength(200, ErrorMessage = "العنوان طويل جداً")]
    public string Title { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    [MaxFileSize(5 * 1024 * 1024)]
    [AllowedExtensions(new[] { ".jpg", ".png", ".jpeg", ".webp", ".gif" })]
    public IFormFile? ImageFile { get; set; }
    
    public string? ImageUrl { get; set; }
    
    [Url(ErrorMessage = "الرابط غير صحيح")]
    public string? Link { get; set; }
    
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }
    public int? AreaId { get; set; }
    public IReadOnlyList<SupplierDirectory.Domain.Area> AvailableAreas { get; set; } = Array.Empty<SupplierDirectory.Domain.Area>();
}
public record UserDto(string Id, string Email);

public class UserFormRequest
{
    public string? Id { get; set; }

    [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
    [EmailAddress(ErrorMessage = "صيغة البريد الإلكتروني غير صحيحة")]
    public string Email { get; set; } = string.Empty;

    public string? Password { get; set; }

    [Compare("Password", ErrorMessage = "كلمة المرور غير متطابقة")]
    public string? ConfirmPassword { get; set; }
}

