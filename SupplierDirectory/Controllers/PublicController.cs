using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SupplierDirectory.Application;
using SupplierDirectory.Infrastructure;
namespace SupplierDirectory.Controllers;
using SupplierDirectory.Infrastructure.Security;
[ApiController, Route("api")]
[ApiKeyAuth]
public sealed class PublicController(AppDbContext db) : ControllerBase {
 [HttpGet("suppliers")] public async Task<IActionResult> Suppliers([FromQuery] SupplierQuery q) { var query=db.Suppliers.AsNoTracking().Where(x=>x.IsActive&&!x.IsDeleted); if(!string.IsNullOrWhiteSpace(q.Search)) query=query.Where(x=>x.Name.Contains(q.Search)|| (x.Description!=null&&x.Description.Contains(q.Search)));if(q.CategoryId.HasValue)query=query.Where(x=>x.SupplierCategories.Any(c=>c.CategoryId==q.CategoryId));if(q.AreaId.HasValue)query=query.Where(x=>x.SupplierAreas.Any(a=>a.AreaId==q.AreaId)); query=q.Sort?.ToLower() == "name_desc" ? query.OrderByDescending(x=>x.Name):query.OrderBy(x=>x.Name); var total=await query.CountAsync();var items=await query.Skip((q.Page-1)*q.PageSize).Take(q.PageSize).Select(x=>new SupplierListDto(x.Id,x.Name,x.Description,x.LogoUrl,x.PhoneNumber,x.WhatsAppNumber,x.Address,x.ShortAddress,x.HasTechnicians,x.Latitude,x.Longitude,x.GoogleMapsUrl,x.SupplierCategories.Select(c=>c.Category.Name),x.SupplierAreas.Select(a=>a.Area.Name))).ToListAsync();var p=new PageResult<SupplierListDto>(items,q.Page,q.PageSize,total);return Ok(new ApiResponse<object>(true,"تم استرجاع البيانات",p.Items,p.Meta)); }
 [HttpGet("suppliers/{id:int}")] public async Task<IActionResult> Supplier(int id) { var item=await db.Suppliers.AsNoTracking().Where(x=>x.Id==id&&x.IsActive&&!x.IsDeleted).Select(x=>new SupplierDetailsDto(x.Id,x.Name,x.Description,x.LogoUrl,x.PhoneNumber,x.AdditionalPhoneNumbers,x.WhatsAppNumber,x.Email,x.Website,x.Address,x.ShortAddress,x.HasTechnicians,x.Latitude,x.Longitude,x.GoogleMapsUrl,x.Images.OrderBy(i=>i.DisplayOrder).Select(i=>new{i.Id,i.ImageUrl,i.DisplayOrder}),x.SupplierCategories.Select(c=>new{c.CategoryId,c.Category.Name}),x.SupplierAreas.Select(a=>new{a.AreaId,a.Area.Name}))).FirstOrDefaultAsync(); return item is null?NotFound(new ApiResponse<object>(false,"المورد غير موجود",null)):Ok(new ApiResponse<object>(true,"تم الاسترجاع",item)); }
 [HttpGet("areas")] public Task<IActionResult> Areas([FromQuery]PageQuery q)=>Page(db.Areas.AsNoTracking().Where(x=>x.IsActive&&!x.IsDeleted).OrderBy(x=>x.Name).Select(x=>new AreaDto(x.Id,x.Name,x.Description,x.ParentAreaId)),q);
 [HttpGet("categories")] public Task<IActionResult> Categories([FromQuery]PageQuery q)=>Page(db.Categories.AsNoTracking().Where(x=>x.IsActive&&!x.IsDeleted).OrderBy(x=>x.Name).Select(x=>new CategoryDto(x.Id,x.Name,x.Description,x.ImageUrl)),q);
     [HttpGet("advertisements")]
    public async Task<IActionResult> Ads([FromQuery] int? areaId)
    {
        var now = DateTime.UtcNow;
        var query = db.Advertisements.AsNoTracking()
            .Where(a => a.IsActive && !a.IsDeleted && 
                       (!a.StartDate.HasValue || a.StartDate <= now) && 
                       (!a.EndDate.HasValue || a.EndDate >= now));
                       
        if (areaId.HasValue)
        {
            query = query.Where(a => a.AreaId == null || a.AreaId == areaId);
        }
        else
        {
            query = query.Where(a => a.AreaId == null);
        }

        var results = await query.OrderBy(a => a.DisplayOrder)
            .Select(a => new {
                a.Id,
                a.Title,
                a.Description,
                a.ImageUrl,
                a.Link,
                a.DisplayOrder,
                a.AreaId,
                AreaName = a.Area != null ? a.Area.Name : null
            }).ToListAsync();
            
        return Ok(new ApiResponse<object>(true, "تم الاسترجاع", results));
    }
 [HttpGet("company")] public async Task<IActionResult> Company()=>Ok(new ApiResponse<object>(true,"تم الاسترجاع",await db.CompanyInfos.AsNoTracking().FirstOrDefaultAsync()));
     [HttpGet("products")]
    public async Task<IActionResult> Products([FromQuery] PageQuery q, [FromQuery] int? areaId)
    {
        var query = db.Products.AsNoTracking().Where(x => x.IsActive && !x.IsDeleted);
        
        if (!string.IsNullOrWhiteSpace(q.Search))
            query = query.Where(x => x.Name.Contains(q.Search) || (x.Description != null && x.Description.Contains(q.Search)));
            
        if (areaId.HasValue)
            query = query.Where(x => x.AreaId == null || x.AreaId == areaId);

        var total = await query.CountAsync();
        var items = await query.OrderBy(x => x.Name)
            .Skip((q.Page - 1) * q.PageSize)
            .Take(q.PageSize)
            .Select(x => new ProductDto(
                x.Id, x.Name, x.Description, x.Details, x.AreaId, 
                x.Area != null ? x.Area.Name : null, 
                x.Images.OrderBy(i => i.DisplayOrder).Select(i => new { i.Id, i.ImageUrl, i.DisplayOrder })
            )).ToListAsync();

        var p = new PageResult<ProductDto>(items, q.Page, q.PageSize, total);
        return Ok(new ApiResponse<object>(true, "تم استرجاع البيانات", p.Items, p.Meta));
    }

    [HttpGet("products/{id:int}")]
    public async Task<IActionResult> Product(int id)
    {
        var item = await db.Products.AsNoTracking().Where(x => x.Id == id && x.IsActive && !x.IsDeleted)
            .Select(x => new ProductDto(
                x.Id, x.Name, x.Description, x.Details, x.AreaId, 
                x.Area != null ? x.Area.Name : null, 
                x.Images.OrderBy(i => i.DisplayOrder).Select(i => new { i.Id, i.ImageUrl, i.DisplayOrder })
            )).FirstOrDefaultAsync();

        if (item == null) return NotFound(new ApiResponse<object>(false, "المنتج غير موجود", null));
        return Ok(new ApiResponse<object>(true, "تم الاسترجاع", item));
    }

    async Task<IActionResult> Page<T>(IQueryable<T> query,PageQuery q){var total=await query.CountAsync();var items=await query.Skip((q.Page-1)*q.PageSize).Take(q.PageSize).ToListAsync();var p=new PageResult<T>(items,q.Page,q.PageSize,total);return Ok(new ApiResponse<object>(true,"تم استرجاع البيانات",p.Items,p.Meta));}
}






