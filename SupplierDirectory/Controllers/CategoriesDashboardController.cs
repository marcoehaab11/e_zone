using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SupplierDirectory.Application;
using SupplierDirectory.Domain;
using SupplierDirectory.Infrastructure;
using System.Globalization;

namespace SupplierDirectory.Controllers;

[Authorize(Roles = "Admin")]
public sealed class CategoriesDashboardController(AppDbContext db, IFileStorageService fileStorage) : Controller
{
    private async Task<IReadOnlyList<(int Id, string Name, string? NameEn)>> GetParentCategories(int? excludeId = null)
    {
        var query = db.Categories.AsNoTracking().Where(c => c.IsActive && c.ParentCategoryId == null);
        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }
        var list = await query.OrderBy(c => c.Name).Select(c => new { c.Id, c.Name, c.NameEn }).ToListAsync();
        return list.Select(c => (c.Id, c.Name, c.NameEn)).ToList();
    }

    [HttpGet("dashboard/categories-list")]
    public async Task<IActionResult> Index([FromQuery] CategoryListQuery query)
    {
        var english = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "en";
        var dbQuery = db.Categories.Include(c => c.ParentCategory)
            .Include(c => c.Children)
            .Include(c => c.SupplierCategories)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            dbQuery = dbQuery.Where(x => x.Name.Contains(query.Search) || (x.Description ?? "").Contains(query.Search));
        }

        if (query.ParentId.HasValue)
        {
            if (query.ParentId.Value == 0) // Main categories only
                dbQuery = dbQuery.Where(x => x.ParentCategoryId == null);
            else
                dbQuery = dbQuery.Where(x => x.ParentCategoryId == query.ParentId.Value);
        }

        var total = await dbQuery.CountAsync();
        var items = await dbQuery.OrderBy(x => x.ParentCategoryId)
            .ThenBy(x => x.Name)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(x => new CategoryListItem
            {
                Id = x.Id,
                Name = english ? (x.NameEn ?? x.Name) : x.Name,
                Description = english ? (x.DescriptionEn ?? x.Description) : x.Description,
                ImageUrl = x.ImageUrl,
                ParentCategoryId = x.ParentCategoryId,
                ParentName = x.ParentCategory != null ? (english ? (x.ParentCategory.NameEn ?? x.ParentCategory.Name) : x.ParentCategory.Name) : null,
                SuppliersCount = x.SupplierCategories.Count,
                SubCategoriesCount = x.Children.Count,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync();

        var parents = await GetParentCategories();

        return View(new CategoryListViewModel
        {
            Items = items,
            Parents = parents,
            Query = query,
            TotalCount = total
        });
    }

    [HttpGet("dashboard/categories-list/create")]
    public async Task<IActionResult> Create()
    {
        return View("Form", new CategoryFormViewModel
        {
            Parents = await GetParentCategories()
        });
    }

    [HttpPost("dashboard/categories-list/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoryFormViewModel model, CancellationToken ct)
    {
        if (model.ParentCategoryId.HasValue && !await db.Categories.AnyAsync(x => x.Id == model.ParentCategoryId.Value, ct))
        {
            ModelState.AddModelError(nameof(model.ParentCategoryId), "التصنيف الرئيسي المحدد غير موجود");
        }

        if (!ModelState.IsValid)
        {
            model.Parents = await GetParentCategories();
            return View("Form", model);
        }

        string? imageUrl = null;
        if (model.ImageFile != null && model.ImageFile.Length > 0)
        {
            imageUrl = await fileStorage.SaveImageAsync(model.ImageFile, "categories", ct);
        }

        var category = new Category
        {
            Name = model.Name.Trim(),
            NameEn = model.NameEn?.Trim(),
            Description = model.Description?.Trim(),
            DescriptionEn = model.DescriptionEn?.Trim(),
            ParentCategoryId = model.ParentCategoryId,
            ImageUrl = imageUrl,
            IsActive = model.IsActive
        };

        db.Categories.Add(category);
        await db.SaveChangesAsync(ct);

        TempData["SuccessMessage"] = "تمت إضافة تصنيف المورد بنجاح";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("dashboard/categories-list/{id:int}/edit")]
    public async Task<IActionResult> Edit(int id)
    {
        var cat = await db.Categories.FindAsync(id);
        if (cat == null) return NotFound();

        var vm = new CategoryFormViewModel
        {
            Id = cat.Id,
            Name = cat.Name,
            NameEn = cat.NameEn,
            Description = cat.Description,
            DescriptionEn = cat.DescriptionEn,
            ParentCategoryId = cat.ParentCategoryId,
            ExistingImageUrl = cat.ImageUrl,
            IsActive = cat.IsActive,
            Parents = await GetParentCategories(id)
        };

        return View("Form", vm);
    }

    [HttpPost("dashboard/categories-list/{id:int}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CategoryFormViewModel model, CancellationToken ct)
    {
        var cat = await db.Categories.FindAsync([id], ct);
        if (cat == null) return NotFound();

        if (model.ParentCategoryId == id)
        {
            ModelState.AddModelError(nameof(model.ParentCategoryId), "لا يمكن للتصنيف أن يكون رئيسياً لنفسه");
        }

        if (model.ParentCategoryId.HasValue && !await db.Categories.AnyAsync(c => c.Id == model.ParentCategoryId.Value, ct))
        {
            ModelState.AddModelError(nameof(model.ParentCategoryId), "التصنيف الرئيسي المحدد غير موجود");
        }

        if (!ModelState.IsValid)
        {
            model.Id = id;
            model.ExistingImageUrl = cat.ImageUrl;
            model.Parents = await GetParentCategories(id);
            return View("Form", model);
        }

        if (model.ImageFile != null && model.ImageFile.Length > 0)
        {
            if (!string.IsNullOrWhiteSpace(cat.ImageUrl))
            {
                await fileStorage.DeleteAsync(cat.ImageUrl);
            }
            cat.ImageUrl = await fileStorage.SaveImageAsync(model.ImageFile, "categories", ct);
        }

        cat.Name = model.Name.Trim();
        cat.NameEn = model.NameEn?.Trim();
        cat.Description = model.Description?.Trim();
        cat.DescriptionEn = model.DescriptionEn?.Trim();
        cat.ParentCategoryId = model.ParentCategoryId;
        cat.IsActive = model.IsActive;
        cat.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
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
        TempData["SuccessMessage"] = cat.IsActive ? "تم تفعيل التصنيف بنجاح" : "تم تعطيل التصنيف بنجاح";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("dashboard/categories-list/{id:int}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var cat = await db.Categories.FindAsync(id);
        if (cat == null) return NotFound();

        if (await db.Categories.AnyAsync(c => c.ParentCategoryId == id))
        {
            TempData["ErrorMessage"] = "لا يمكن حذف تصنيف يحتوي على تصنيفات فرعية تابعة له";
            return RedirectToAction(nameof(Index));
        }

        if (await db.SupplierCategories.AnyAsync(sc => sc.CategoryId == id))
        {
            TempData["ErrorMessage"] = "لا يمكن حذف التصنيف لوجود موردين مرتبطين به";
            return RedirectToAction(nameof(Index));
        }

        cat.IsDeleted = true;
        await db.SaveChangesAsync();
        TempData["SuccessMessage"] = "تم نقل التصنيف إلى سلة المحذوفات بنجاح";
        return RedirectToAction(nameof(Index));
    }
}
