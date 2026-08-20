using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SupplierDirectory.Domain;
using SupplierDirectory.Infrastructure;

namespace SupplierDirectory.Controllers;

[Authorize(Roles = "Admin")]
public sealed class CategoriesDashboardController(AppDbContext db) : Controller
{
    [HttpGet("dashboard/categories-list")]
    public async Task<IActionResult> Index([FromQuery] string search = "", [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var query = db.Categories.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x => x.Name.Contains(search) || (x.Description ?? "").Contains(search));
        }

        var total = await query.CountAsync();
        var items = await query.OrderBy(x => x.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.Search = search;
        ViewBag.Page = page;
        ViewBag.PageSize = pageSize;
        ViewBag.TotalPages = (int)Math.Ceiling(total / (double)pageSize);

        return View(items);
    }

    [HttpGet("dashboard/categories-list/create")]
    public IActionResult Create() => View("Form", new Category { Name = "" });

    [HttpPost("dashboard/categories-list/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Category model)
    {
        if (!ModelState.IsValid) return View("Form", model);

        db.Categories.Add(new Category
        {
            Name = model.Name.Trim(),
            Description = model.Description?.Trim(),
            IsActive = model.IsActive
        });
        await db.SaveChangesAsync();
        TempData["SuccessMessage"] = "تمت إضافة التصنيف بنجاح";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("dashboard/categories-list/{id:int}/edit")]
    public async Task<IActionResult> Edit(int id)
    {
        var cat = await db.Categories.FindAsync(id);
        if (cat == null) return NotFound();
        return View("Form", cat);
    }

    [HttpPost("dashboard/categories-list/{id:int}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Category model)
    {
        if (!ModelState.IsValid) return View("Form", model);
        
        var cat = await db.Categories.FindAsync(id);
        if (cat == null) return NotFound();

        cat.Name = model.Name.Trim();
        cat.Description = model.Description?.Trim();
        cat.IsActive = model.IsActive;
        
        await db.SaveChangesAsync();
        TempData["SuccessMessage"] = "تم تعديل التصنيف بنجاح";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("dashboard/categories-list/{id:int}/toggle")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int id)
    {
        var cat = await db.Categories.FindAsync(id);
        if (cat == null) return NotFound();
        cat.IsActive = !cat.IsActive;
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("dashboard/categories-list/{id:int}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var cat = await db.Categories.FindAsync(id);
        if (cat == null) return NotFound();
        
        // Soft delete
        cat.IsDeleted = true;
        await db.SaveChangesAsync();
        TempData["SuccessMessage"] = "تم حذف التصنيف بنجاح";
        return RedirectToAction(nameof(Index));
    }
}
