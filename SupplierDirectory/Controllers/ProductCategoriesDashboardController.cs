using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SupplierDirectory.Application;
using SupplierDirectory.Domain;
using SupplierDirectory.Infrastructure;

namespace SupplierDirectory.Controllers;

[Authorize(Roles = "Admin")]
public sealed class ProductCategoriesDashboardController(AppDbContext db, IFileStorageService fileStorage) : Controller
{
    private async Task<IReadOnlyList<(int Id, string Name)>> GetParentCategories(int? excludeId = null)
    {
        var query = db.ProductCategories.AsNoTracking().Where(c => c.IsActive && c.ParentCategoryId == null);
        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }
        var list = await query.OrderBy(c => c.Name).Select(c => new { c.Id, c.Name }).ToListAsync();
        return list.Select(c => (c.Id, c.Name)).ToList();
    }

    [HttpGet("dashboard/product-categories")]
    public async Task<IActionResult> Index([FromQuery] ProductCategoryListQuery query)
    {
        var dbQuery = db.ProductCategories.Include(c => c.ParentCategory)
            .Include(c => c.Children)
            .Include(c => c.Products)
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
            .Select(x => new ProductCategoryListItem
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                ImageUrl = x.ImageUrl,
                ParentCategoryId = x.ParentCategoryId,
                ParentName = x.ParentCategory != null ? x.ParentCategory.Name : null,
                ProductsCount = x.Products.Count,
                SubCategoriesCount = x.Children.Count,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync();

        var parents = await GetParentCategories();

        return View(new ProductCategoryListViewModel
        {
            Items = items,
            Parents = parents,
            Query = query,
            TotalCount = total
        });
    }

    [HttpGet("dashboard/product-categories/create")]
    public async Task<IActionResult> Create()
    {
        return View("Form", new ProductCategoryFormViewModel
        {
            Parents = await GetParentCategories()
        });
    }

    [HttpPost("dashboard/product-categories/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductCategoryFormViewModel model, CancellationToken ct)
    {
        if (model.ParentCategoryId.HasValue && !await db.ProductCategories.AnyAsync(x => x.Id == model.ParentCategoryId.Value, ct))
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
            imageUrl = await fileStorage.SaveImageAsync(model.ImageFile, "product-categories", ct);
        }

        var category = new ProductCategory
        {
            Name = model.Name.Trim(),
            Description = model.Description?.Trim(),
            ParentCategoryId = model.ParentCategoryId,
            ImageUrl = imageUrl,
            IsActive = model.IsActive
        };

        db.ProductCategories.Add(category);
        await db.SaveChangesAsync(ct);

        TempData["SuccessMessage"] = "تمت إضافة تصنيف المنتج بنجاح";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("dashboard/product-categories/{id:int}/edit")]
    public async Task<IActionResult> Edit(int id)
    {
        var cat = await db.ProductCategories.FindAsync(id);
        if (cat == null) return NotFound();

        var vm = new ProductCategoryFormViewModel
        {
            Id = cat.Id,
            Name = cat.Name,
            Description = cat.Description,
            ParentCategoryId = cat.ParentCategoryId,
            ExistingImageUrl = cat.ImageUrl,
            IsActive = cat.IsActive,
            Parents = await GetParentCategories(id)
        };

        return View("Form", vm);
    }

    [HttpPost("dashboard/product-categories/{id:int}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProductCategoryFormViewModel model, CancellationToken ct)
    {
        var cat = await db.ProductCategories.FindAsync([id], ct);
        if (cat == null) return NotFound();

        if (model.ParentCategoryId == id)
        {
            ModelState.AddModelError(nameof(model.ParentCategoryId), "لا يمكن للتصنيف أن يكون رئيسياً لنفسه");
        }

        if (model.ParentCategoryId.HasValue && !await db.ProductCategories.AnyAsync(c => c.Id == model.ParentCategoryId.Value, ct))
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
            cat.ImageUrl = await fileStorage.SaveImageAsync(model.ImageFile, "product-categories", ct);
        }

        cat.Name = model.Name.Trim();
        cat.Description = model.Description?.Trim();
        cat.ParentCategoryId = model.ParentCategoryId;
        cat.IsActive = model.IsActive;
        cat.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        TempData["SuccessMessage"] = "تم تعديل تصنيف المنتج بنجاح";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("dashboard/product-categories/{id:int}/toggle")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int id)
    {
        var cat = await db.ProductCategories.FindAsync(id);
        if (cat == null) return NotFound();
        cat.IsActive = !cat.IsActive;
        await db.SaveChangesAsync();
        TempData["SuccessMessage"] = cat.IsActive ? "تم تفعيل التصنيف بنجاح" : "تم تعطيل التصنيف بنجاح";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("dashboard/product-categories/{id:int}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var cat = await db.ProductCategories.FindAsync(id);
        if (cat == null) return NotFound();

        if (await db.ProductCategories.AnyAsync(c => c.ParentCategoryId == id))
        {
            TempData["ErrorMessage"] = "لا يمكن حذف تصنيف يحتوي على تصنيفات فرعية تابعة له";
            return RedirectToAction(nameof(Index));
        }

        if (await db.Products.AnyAsync(p => p.ProductCategoryId == id))
        {
            TempData["ErrorMessage"] = "لا يمكن حذف التصنيف لوجود منتجات مرتبطة به";
            return RedirectToAction(nameof(Index));
        }

        cat.IsDeleted = true;
        await db.SaveChangesAsync();
        TempData["SuccessMessage"] = "تم نقل تصنيف المنتج إلى سلة المحذوفات بنجاح";
        return RedirectToAction(nameof(Index));
    }
}
