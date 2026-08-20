using System.ComponentModel.DataAnnotations;
namespace SupplierDirectory.Application;
public sealed record ApiResponse<T>(bool Success, string Message, T? Data, object? Pagination = null);
public sealed record PageResult<T>(IReadOnlyList<T> Items, int CurrentPage, int PageSize, int TotalCount) { public object Meta => new { currentPage=CurrentPage,pageSize=PageSize,totalCount=TotalCount,totalPages=(int)Math.Ceiling(TotalCount/(double)PageSize),hasNext=CurrentPage*PageSize<TotalCount,hasPrevious=CurrentPage>1 }; }
public class PageQuery { [Range(1,int.MaxValue)] public int Page {get;set;}=1; [Range(1,100)] public int PageSize {get;set;}=20; public string? Search {get;set;} public string? Sort {get;set;} }
public sealed class SupplierQuery : PageQuery { public int? CategoryId {get;set;} public int? AreaId {get;set;} }
public sealed class AreaRequest { [Required(ErrorMessage="اسم المنطقة مطلوب")] public string Name {get;set;}=""; public string? Description {get;set;} public int? ParentAreaId {get;set;} public bool IsActive {get;set;}=true; }
public sealed class CategoryRequest { [Required(ErrorMessage="اسم التصنيف مطلوب")] public string Name {get;set;}=""; public string? Description {get;set;} public bool IsActive {get;set;}=true; }
public sealed class SupplierRequest { [Required(ErrorMessage="اسم المورد مطلوب")] public string Name {get;set;}=""; public string? Description {get;set;} [Phone] public string? PhoneNumber {get;set;} public string? AdditionalPhoneNumbers {get;set;} [Phone] public string? WhatsAppNumber {get;set;} [EmailAddress] public string? Email {get;set;} [Url] public string? Website {get;set;} public string? Address {get;set;} [Range(-90,90)] public decimal? Latitude {get;set;} [Range(-180,180)] public decimal? Longitude {get;set;} [Url] public string? GoogleMapsUrl {get;set;} public bool IsActive {get;set;}=true; public List<int> CategoryIds {get;set;}=[]; public List<int> AreaIds {get;set;}=[]; }
public sealed class AdvertisementRequest { [Required(ErrorMessage="العنوان مطلوب")] public string Title {get;set;}=""; public string? Description {get;set;} [Url] public string? Link {get;set;} public DateTime? StartDate {get;set;} public DateTime? EndDate {get;set;} public bool IsActive {get;set;}=true; public int DisplayOrder {get;set;} }
public sealed class CompanyRequest { public string? CompanyName {get;set;} public string? About {get;set;} public string? Mission {get;set;} public string? Vision {get;set;} public string? PlatformDescription {get;set;} public string? PlatformServices {get;set;} public string? ContactPhone {get;set;} public string? WhatsApp {get;set;} [EmailAddress] public string? Email {get;set;} [Url] public string? Website {get;set;} public string? SocialLinksJson {get;set;} }
public sealed class LoginRequest { [Required,EmailAddress] public string Email {get;set;}=""; [Required] public string Password {get;set;}=""; }

public sealed record SupplierListDto(int Id, string Name, string? Description, string? LogoUrl, string? PhoneNumber, string? WhatsAppNumber, string? Address, decimal? Latitude, decimal? Longitude, string? GoogleMapsUrl, IEnumerable<string> Categories, IEnumerable<string> Areas);

public sealed record SupplierDetailsDto(int Id, string Name, string? Description, string? LogoUrl, string? PhoneNumber, string? AdditionalPhoneNumbers, string? WhatsAppNumber, string? Email, string? Website, string? Address, decimal? Latitude, decimal? Longitude, string? GoogleMapsUrl, IEnumerable<object> Images, IEnumerable<object> Categories, IEnumerable<object> Areas);

public sealed record AreaDto(int Id, string Name, string? Description, int? ParentAreaId);

public sealed record CategoryDto(int Id, string Name, string? Description, string? ImageUrl);

