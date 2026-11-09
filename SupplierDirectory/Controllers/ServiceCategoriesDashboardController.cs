using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SupplierDirectory.Application;
using SupplierDirectory.Domain;
using SupplierDirectory.Infrastructure;

namespace SupplierDirectory.Controllers;

[Authorize(Roles = "Admin")]
public sealed class ServiceCategoriesDashboardController(AppDbContext db) : Controller
{
    async Task Parents(int? exclude = null) => ViewBag.Parents = await db.ServiceCategories.AsNoTracking().Where(c => c.IsActive && (!exclude.HasValue || c.Id != exclude)).OrderBy(c => c.Name).ToListAsync();
    [HttpGet("dashboard/service-categories")]
    public async Task<IActionResult> Index([FromQuery] PageQuery q, [FromQuery] int? parentId)
    {
        var query = db.ServiceCategories.Include(c => c.ParentServiceCategory).Include(c => c.Children).Include(c => c.Services).AsNoTracking();
        if (!string.IsNullOrWhiteSpace(q.Search)) query = query.Where(c => c.Name.Contains(q.Search) || (c.Description ?? "").Contains(q.Search));
        if (parentId.HasValue) query = parentId == 0 ? query.Where(c => c.ParentServiceCategoryId == null) : query.Where(c => c.ParentServiceCategoryId == parentId);
        var total = await query.CountAsync(); ViewBag.Query = q; ViewBag.ParentId = parentId; ViewBag.TotalPages = Math.Max(1, (int)Math.Ceiling(total / (double)q.PageSize)); await Parents();
        return View(await query.OrderBy(c => c.ParentServiceCategoryId).ThenBy(c => c.Name).Skip((q.Page - 1) * q.PageSize).Take(q.PageSize).ToListAsync());
    }
    [HttpGet("dashboard/service-categories/create")] public async Task<IActionResult> Create() { await Parents(); return View("Form", new ServiceCategoryRequest()); }
    [HttpPost("dashboard/service-categories/create"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ServiceCategoryRequest model)
    {
        if (model.ParentServiceCategoryId.HasValue && !await db.ServiceCategories.AnyAsync(c => c.Id == model.ParentServiceCategoryId)) ModelState.AddModelError(nameof(model.ParentServiceCategoryId), "التصنيف الأب غير موجود");
        if (await db.ServiceCategories.AnyAsync(c => c.Name == model.Name.Trim())) ModelState.AddModelError(nameof(model.Name), "هذا التصنيف موجود بالفعل");
        if (!ModelState.IsValid) { await Parents(); return View("Form", model); }
        db.ServiceCategories.Add(new ServiceCategory { Name = model.Name.Trim(), Description = model.Description?.Trim(), ParentServiceCategoryId = model.ParentServiceCategoryId, IsActive = model.IsActive }); await db.SaveChangesAsync(); TempData["SuccessMessage"] = "تمت إضافة تصنيف الخدمة"; return RedirectToAction(nameof(Index));
    }
    [HttpGet("dashboard/service-categories/{id:int}/edit")]
    public async Task<IActionResult> Edit(int id) { var c = await db.ServiceCategories.FindAsync(id); if (c is null) return NotFound(); await Parents(id); return View("Form", new ServiceCategoryRequest { Name = c.Name, Description = c.Description, ParentServiceCategoryId = c.ParentServiceCategoryId, IsActive = c.IsActive }); }
    [HttpPost("dashboard/service-categories/{id:int}/edit"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ServiceCategoryRequest model)
    {
        var c = await db.ServiceCategories.FindAsync(id); if (c is null) return NotFound(); if (model.ParentServiceCategoryId == id) ModelState.AddModelError(nameof(model.ParentServiceCategoryId), "لا يمكن للتصنيف أن يكون أبًا لنفسه");
        if (model.ParentServiceCategoryId.HasValue && !await db.ServiceCategories.AnyAsync(x => x.Id == model.ParentServiceCategoryId)) ModelState.AddModelError(nameof(model.ParentServiceCategoryId), "التصنيف الأب غير موجود");
        if (await db.ServiceCategories.AnyAsync(x => x.Id != id && x.Name == model.Name.Trim())) ModelState.AddModelError(nameof(model.Name), "هذا التصنيف موجود بالفعل"); if (!ModelState.IsValid) { await Parents(id); return View("Form", model); }
        c.Name = model.Name.Trim(); c.Description = model.Description?.Trim(); c.ParentServiceCategoryId = model.ParentServiceCategoryId; c.IsActive = model.IsActive; await db.SaveChangesAsync(); TempData["SuccessMessage"] = "تم تعديل تصنيف الخدمة"; return RedirectToAction(nameof(Index));
    }
    [HttpPost("dashboard/service-categories/{id:int}/toggle"), ValidateAntiForgeryToken] public async Task<IActionResult> Toggle(int id) { var c = await db.ServiceCategories.FindAsync(id); if (c is null) return NotFound(); c.IsActive = !c.IsActive; await db.SaveChangesAsync(); return RedirectToAction(nameof(Index)); }
    [HttpPost("dashboard/service-categories/{id:int}/delete"), ValidateAntiForgeryToken] public async Task<IActionResult> Delete(int id) { var c = await db.ServiceCategories.Include(x => x.Children).Include(x => x.Services).FirstOrDefaultAsync(x => x.Id == id); if (c is null) return NotFound(); if (c.Children.Any(x => !x.IsDeleted) || c.Services.Any(s => !s.IsDeleted)) { TempData["ErrorMessage"] = "لا يمكن حذف تصنيف يحتوي على تصنيفات فرعية أو خدمات"; return RedirectToAction(nameof(Index)); } c.IsDeleted = true; await db.SaveChangesAsync(); TempData["SuccessMessage"] = "تم حذف التصنيف"; return RedirectToAction(nameof(Index)); }
}
