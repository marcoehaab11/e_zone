using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SupplierDirectory.Domain;
using SupplierDirectory.Application;
using SupplierDirectory.Infrastructure;

namespace SupplierDirectory.Controllers;

[Authorize(Roles = "Admin")]
public sealed class ProductsDashboardController(AppDbContext db, IFileStorageService fileStorage) : Controller
{
    private async Task PopulateAreas(ProductFormViewModel model)
    {
        model.AvailableAreas = await db.Areas.AsNoTracking().Where(a => !a.IsDeleted && a.IsActive).OrderBy(a => a.Name).ToListAsync();
    }

    [HttpGet("dashboard/products")]
    public async Task<IActionResult> Index([FromQuery] ProductListQuery q)
    {
        var query = db.Products.Include(p => p.Area).Include(p => p.Images).AsNoTracking().Where(p => !p.IsDeleted);
        
        if (!string.IsNullOrWhiteSpace(q.Search))
            query = query.Where(x => x.Name.Contains(q.Search) || (x.Description != null && x.Description.Contains(q.Search)));
            
        if (q.AreaId.HasValue)
            query = query.Where(x => x.AreaId == q.AreaId);

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(x => x.CreatedAt)
            .Skip((q.Page - 1) * q.PageSize)
            .Take(q.PageSize)
            .ToListAsync();

        ViewBag.Search = q.Search;
        ViewBag.AreaId = q.AreaId;
        ViewBag.Page = q.Page;
        ViewBag.PageSize = q.PageSize;
        ViewBag.TotalPages = (int)Math.Ceiling(total / (double)q.PageSize);
        ViewBag.Areas = await db.Areas.AsNoTracking().Where(a => !a.IsDeleted).ToListAsync();

        return View(items);
    }

    [HttpGet("dashboard/products/create")]
    public async Task<IActionResult> Create()
    {
        var vm = new ProductFormViewModel();
        await PopulateAreas(vm);
        return View("Form", vm);
    }

    [HttpPost("dashboard/products/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductFormViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            await PopulateAreas(model);
            return View("Form", model);
        }

        var product = new Product
        {
            Name = model.Name.Trim(),
            Description = model.Description?.Trim(),
            Details = model.Details?.Trim(),
            AreaId = model.AreaId,
            IsActive = model.IsActive
        };

        if (model.NewImages.Any())
        {
            int order = 1;
            foreach (var imgFile in model.NewImages)
            {
                if (imgFile.Length > 0)
                {
                    var url = await fileStorage.SaveImageAsync(imgFile, "products/images", ct);
                    product.Images.Add(new ProductImage { ImageUrl = url, DisplayOrder = order++ });
                }
            }
        }

        db.Products.Add(product);
        await db.SaveChangesAsync(ct);
        TempData["SuccessMessage"] = "تمت إضافة المنتج بنجاح";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("dashboard/products/{id:int}/edit")]
    public async Task<IActionResult> Edit(int id)
    {
        var product = await db.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
        if (product == null) return NotFound();

        var vm = new ProductFormViewModel
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Details = product.Details,
            AreaId = product.AreaId,
            IsActive = product.IsActive,
            ExistingImages = product.Images.OrderBy(i => i.DisplayOrder).ToList()
        };
        await PopulateAreas(vm);
        return View("Form", vm);
    }

    [HttpPost("dashboard/products/{id:int}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProductFormViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            await PopulateAreas(model);
            return View("Form", model);
        }

        var product = await db.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
        if (product == null) return NotFound();

        product.Name = model.Name.Trim();
        product.Description = model.Description?.Trim();
        product.Details = model.Details?.Trim();
        product.AreaId = model.AreaId;
        product.IsActive = model.IsActive;
        product.UpdatedAt = DateTime.UtcNow;

        if (model.NewImages.Any())
        {
            int order = product.Images.Any() ? product.Images.Max(i => i.DisplayOrder) + 1 : 1;
            foreach (var imgFile in model.NewImages)
            {
                if (imgFile.Length > 0)
                {
                    var url = await fileStorage.SaveImageAsync(imgFile, "products/images", ct);
                    product.Images.Add(new ProductImage { ImageUrl = url, DisplayOrder = order++ });
                }
            }
        }

        await db.SaveChangesAsync(ct);
        TempData["SuccessMessage"] = "تم تعديل المنتج بنجاح";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("dashboard/products/{id:int}/delete-image/{imageId:int}")]
    public async Task<IActionResult> DeleteImage(int id, int imageId)
    {
        var product = await db.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
        if (product == null) return NotFound();

        var img = product.Images.FirstOrDefault(i => i.Id == imageId);
        if (img != null)
        {
            await fileStorage.DeleteAsync(img.ImageUrl);
            db.ProductImages.Remove(img);
            await db.SaveChangesAsync();
        }
        return Ok();
    }

    [HttpPost("dashboard/products/{id:int}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await db.Products.FindAsync(id);
        if (product == null) return NotFound();
        product.IsDeleted = true;
        await db.SaveChangesAsync();
        TempData["SuccessMessage"] = "تم حذف المنتج بنجاح";
        return RedirectToAction(nameof(Index));
    }
}
