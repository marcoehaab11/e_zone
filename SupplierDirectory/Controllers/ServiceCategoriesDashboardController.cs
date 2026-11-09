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
    [HttpGet("dashboard/service-categories")]
    public async Task<IActionResult> Index([FromQuery] PageQuery query)
    {
        var categories = db.ServiceCategories.Include(c => c.Services).AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query.Search)) categories = categories.Where(c => c.Name.Contains(query.Search) || (c.Description ?? "").Contains(query.Search));
        var total = await categories.CountAsync(); ViewBag.Query = query; ViewBag.TotalPages = Math.Max(1, (int)Math.Ceiling(total / (double)query.PageSize));
        return View(await categories.OrderBy(c => c.Name).Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync());
    }
    [HttpGet("dashboard/service-categories/create")] public IActionResult Create() => View("Form", new ServiceCategoryRequest());
    [HttpPost("dashboard/service-categories/create"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ServiceCategoryRequest model)
    {
        if (await db.ServiceCategories.AnyAsync(c => c.Name == model.Name.Trim())) ModelState.AddModelError(nameof(model.Name), "هذا التصنيف موجود بالفعل");
        if (!ModelState.IsValid) return View("Form", model);
        db.ServiceCategories.Add(new ServiceCategory { Name = model.Name.Trim(), Description = model.Description?.Trim(), IsActive = model.IsActive }); await db.SaveChangesAsync(); TempData["SuccessMessage"] = "تمت إضافة تصنيف الخدمة"; return RedirectToAction(nameof(Index));
    }
    [HttpGet("dashboard/service-categories/{id:int}/edit")]
    public async Task<IActionResult> Edit(int id) { var c = await db.ServiceCategories.FindAsync(id); return c is null ? NotFound() : View("Form", new ServiceCategoryRequest { Name = c.Name, Description = c.Description, IsActive = c.IsActive }); }
    [HttpPost("dashboard/service-categories/{id:int}/edit"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ServiceCategoryRequest model)
    {
        var c = await db.ServiceCategories.FindAsync(id); if (c is null) return NotFound(); if (await db.ServiceCategories.AnyAsync(x => x.Id != id && x.Name == model.Name.Trim())) ModelState.AddModelError(nameof(model.Name), "هذا التصنيف موجود بالفعل"); if (!ModelState.IsValid) return View("Form", model);
        c.Name = model.Name.Trim(); c.Description = model.Description?.Trim(); c.IsActive = model.IsActive; await db.SaveChangesAsync(); TempData["SuccessMessage"] = "تم تعديل تصنيف الخدمة"; return RedirectToAction(nameof(Index));
    }
    [HttpPost("dashboard/service-categories/{id:int}/toggle"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int id) { var c = await db.ServiceCategories.FindAsync(id); if (c is null) return NotFound(); c.IsActive = !c.IsActive; await db.SaveChangesAsync(); return RedirectToAction(nameof(Index)); }
    [HttpPost("dashboard/service-categories/{id:int}/delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id) { var c = await db.ServiceCategories.Include(x => x.Services).FirstOrDefaultAsync(x => x.Id == id); if (c is null) return NotFound(); if (c.Services.Any(s => !s.IsDeleted)) { TempData["ErrorMessage"] = "لا يمكن حذف تصنيف مرتبط بخدمات"; return RedirectToAction(nameof(Index)); } c.IsDeleted = true; await db.SaveChangesAsync(); TempData["SuccessMessage"] = "تم حذف التصنيف"; return RedirectToAction(nameof(Index)); }
}
