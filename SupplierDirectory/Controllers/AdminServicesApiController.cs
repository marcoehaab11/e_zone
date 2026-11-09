using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SupplierDirectory.Application;
using SupplierDirectory.Domain;
using SupplierDirectory.Infrastructure;

namespace SupplierDirectory.Controllers;

[ApiController, Route("api/admin"), Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
public sealed class AdminServicesApiController(AppDbContext db) : ControllerBase
{
    [HttpGet("service-categories")]
    public async Task<IActionResult> Categories([FromQuery] PageQuery q, [FromQuery] int? parentId)
    {
        var query = db.ServiceCategories.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(q.Search)) query = query.Where(c => c.Name.Contains(q.Search));
        if (parentId.HasValue) query = parentId == 0 ? query.Where(c => c.ParentServiceCategoryId == null) : query.Where(c => c.ParentServiceCategoryId == parentId);
        var total = await query.CountAsync();
        var data = await query.OrderBy(c => c.Name).Skip((q.Page - 1) * q.PageSize).Take(q.PageSize)
            .Select(c => new ServiceCategoryDto(c.Id, c.Name, c.Description, c.ParentServiceCategoryId, c.ParentServiceCategory != null ? c.ParentServiceCategory.Name : null, c.Services.Count(s => !s.IsDeleted), c.Children.Count(x => !x.IsDeleted))).ToListAsync();
        var page = new PageResult<ServiceCategoryDto>(data, q.Page, q.PageSize, total); return Ok(new ApiResponse<object>(true, "تم الاسترجاع", page.Items, page.Meta));
    }
    [HttpPost("service-categories")]
    public async Task<IActionResult> CreateCategory(ServiceCategoryRequest request)
    {
        if (await db.ServiceCategories.AnyAsync(c => c.Name == request.Name.Trim())) return Conflict(new ApiResponse<object>(false, "اسم التصنيف مستخدم", null));
        if (request.ParentServiceCategoryId.HasValue && !await db.ServiceCategories.AnyAsync(c => c.Id == request.ParentServiceCategoryId)) return BadRequest(new ApiResponse<object>(false, "التصنيف الأب غير موجود", null));
        var category = new ServiceCategory { Name = request.Name.Trim(), Description = request.Description?.Trim(), ParentServiceCategoryId = request.ParentServiceCategoryId, IsActive = request.IsActive }; db.ServiceCategories.Add(category); await db.SaveChangesAsync();
        return Created($"/api/admin/service-categories/{category.Id}", new ApiResponse<object>(true, "تمت الإضافة", new ServiceCategoryDto(category.Id, category.Name, category.Description, category.ParentServiceCategoryId, null, 0, 0)));
    }
    [HttpPut("service-categories/{id:int}")]
    public async Task<IActionResult> UpdateCategory(int id, ServiceCategoryRequest request)
    {
        var category = await db.ServiceCategories.FindAsync(id); if (category is null) return NotFound(new ApiResponse<object>(false, "التصنيف غير موجود", null));
        if (request.ParentServiceCategoryId == id) return BadRequest(new ApiResponse<object>(false, "لا يمكن اختيار التصنيف نفسه كأب", null));
        if (request.ParentServiceCategoryId.HasValue && !await db.ServiceCategories.AnyAsync(c => c.Id == request.ParentServiceCategoryId)) return BadRequest(new ApiResponse<object>(false, "التصنيف الأب غير موجود", null));
        if (await db.ServiceCategories.AnyAsync(c => c.Id != id && c.Name == request.Name.Trim())) return Conflict(new ApiResponse<object>(false, "اسم التصنيف مستخدم", null));
        category.Name = request.Name.Trim(); category.Description = request.Description?.Trim(); category.ParentServiceCategoryId = request.ParentServiceCategoryId; category.IsActive = request.IsActive; await db.SaveChangesAsync(); return Ok(new ApiResponse<object>(true, "تم التحديث", new ServiceCategoryDto(category.Id, category.Name, category.Description, category.ParentServiceCategoryId, null, 0, 0)));
    }
    [HttpDelete("service-categories/{id:int}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await db.ServiceCategories.Include(c => c.Services).Include(c => c.Children).FirstOrDefaultAsync(c => c.Id == id); if (category is null) return NotFound();
        if (category.Services.Any(s => !s.IsDeleted) || category.Children.Any(c => !c.IsDeleted)) return Conflict(new ApiResponse<object>(false, "لا يمكن حذف تصنيف مرتبط بخدمات أو تصنيفات فرعية", null));
        category.IsDeleted = true; await db.SaveChangesAsync(); return NoContent();
    }
    [HttpGet("services")]
    public async Task<IActionResult> Services([FromQuery] PageQuery q, [FromQuery] int? categoryId)
    {
        var query = db.Services.AsNoTracking(); if (categoryId.HasValue) query = query.Where(s => s.ServiceCategoryId == categoryId); if (!string.IsNullOrWhiteSpace(q.Search)) query = query.Where(s => s.Name.Contains(q.Search));
        var total = await query.CountAsync(); var data = await query.OrderBy(s => s.DisplayOrder).Skip((q.Page - 1) * q.PageSize).Take(q.PageSize).Select(s => new ServiceDto(s.Id, s.Name, s.Description, s.Details, s.DisplayOrder, s.ServiceCategoryId, s.ServiceCategory != null ? s.ServiceCategory.Name : null, s.Images.OrderBy(i => i.DisplayOrder).Select(i => (object)new { i.Id, i.ImageUrl, i.DisplayOrder }))).ToListAsync();
        var page = new PageResult<ServiceDto>(data, q.Page, q.PageSize, total); return Ok(new ApiResponse<object>(true, "تم الاسترجاع", page.Items, page.Meta));
    }
    [HttpPost("services")]
    public async Task<IActionResult> CreateService(ServiceRequest request)
    {
        if (request.ServiceCategoryId.HasValue && !await db.ServiceCategories.AnyAsync(c => c.Id == request.ServiceCategoryId)) return BadRequest(new ApiResponse<object>(false, "تصنيف الخدمة غير موجود", null));
        var service = new Service { Name = request.Name.Trim(), Description = request.Description?.Trim(), Details = request.Details?.Trim(), DisplayOrder = request.DisplayOrder, IsActive = request.IsActive, ServiceCategoryId = request.ServiceCategoryId }; db.Services.Add(service); await db.SaveChangesAsync(); return Created($"/api/admin/services/{service.Id}", new ApiResponse<object>(true, "تمت الإضافة", new { service.Id }));
    }
    [HttpPut("services/{id:int}")]
    public async Task<IActionResult> UpdateService(int id, ServiceRequest request)
    {
        var service = await db.Services.FindAsync(id); if (service is null) return NotFound(new ApiResponse<object>(false, "الخدمة غير موجودة", null));
        if (request.ServiceCategoryId.HasValue && !await db.ServiceCategories.AnyAsync(c => c.Id == request.ServiceCategoryId)) return BadRequest(new ApiResponse<object>(false, "تصنيف الخدمة غير موجود", null));
        service.Name = request.Name.Trim(); service.Description = request.Description?.Trim(); service.Details = request.Details?.Trim(); service.DisplayOrder = request.DisplayOrder; service.IsActive = request.IsActive; service.ServiceCategoryId = request.ServiceCategoryId;
        await db.SaveChangesAsync(); return Ok(new ApiResponse<object>(true, "تم التحديث", new { service.Id }));
    }
    [HttpDelete("services/{id:int}")]
    public async Task<IActionResult> DeleteService(int id)
    {
        var service = await db.Services.FindAsync(id); if (service is null) return NotFound(); service.IsDeleted = true; await db.SaveChangesAsync(); return NoContent();
    }
}
