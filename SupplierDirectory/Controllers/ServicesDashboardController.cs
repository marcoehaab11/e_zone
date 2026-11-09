using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SupplierDirectory.Domain;
using SupplierDirectory.Application;
using SupplierDirectory.Infrastructure;

namespace SupplierDirectory.Controllers;

[Authorize(Roles = "Admin")]
public sealed class ServicesDashboardController(AppDbContext db, IFileStorageService fileStorage) : Controller
{
    private async Task PopulateCategories(ServiceFormViewModel model) => model.AvailableCategories = await db.ServiceCategories.AsNoTracking().Where(c => c.IsActive).OrderBy(c => c.Name).ToListAsync();
    [HttpGet("dashboard/services")]
    public async Task<IActionResult> Index([FromQuery] ServiceListQuery q)
    {
        var query = db.Services.Include(s => s.Images).AsNoTracking().Where(s => !s.IsDeleted);
        
        if (!string.IsNullOrWhiteSpace(q.Search))
            query = query.Where(x => x.Name.Contains(q.Search) || (x.Description != null && x.Description.Contains(q.Search)));
        if (q.ServiceCategoryId.HasValue)
            query = query.Where(x => x.ServiceCategoryId == q.ServiceCategoryId);

        var total = await query.CountAsync();
        var items = await query.OrderBy(x => x.DisplayOrder)
            .ThenByDescending(x => x.CreatedAt)
            .Skip((q.Page - 1) * q.PageSize)
            .Take(q.PageSize)
            .ToListAsync();

        ViewBag.Search = q.Search;
        ViewBag.Page = q.Page;
        ViewBag.PageSize = q.PageSize;
        ViewBag.TotalPages = (int)Math.Ceiling(total / (double)q.PageSize);
        ViewBag.ServiceCategoryId = q.ServiceCategoryId;
        ViewBag.ServiceCategories = await db.ServiceCategories.AsNoTracking().Where(c => c.IsActive).OrderBy(c => c.Name).ToListAsync();

        return View(items);
    }

    [HttpGet("dashboard/services/create")]
    public async Task<IActionResult> Create()
    {
        var model = new ServiceFormViewModel(); await PopulateCategories(model); return View("Form", model);
    }

    [HttpPost("dashboard/services/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ServiceFormViewModel model, CancellationToken ct)
    {
        if (model.NewImages != null)
        {
            model.NewImages = model.NewImages.Where(f => f != null && f.Length > 0 && !string.IsNullOrWhiteSpace(f.FileName)).ToList();
            if (!model.NewImages.Any()) ModelState.Remove(nameof(model.NewImages));
        }

        if (!ModelState.IsValid)
        {
            await PopulateCategories(model);
            return View("Form", model);
        }

        var service = new Service
        {
            Name = model.Name.Trim(),
            Description = model.Description?.Trim(),
            Details = model.Details?.Trim(),
            DisplayOrder = model.DisplayOrder,
            ServiceCategoryId = model.ServiceCategoryId,
            IsActive = model.IsActive
        };

        if (model.NewImages != null && model.NewImages.Any())
        {
            int order = 1;
            foreach (var imgFile in model.NewImages)
            {
                if (imgFile.Length > 0)
                {
                    var url = await fileStorage.SaveImageAsync(imgFile, "services/images", ct);
                    service.Images.Add(new ServiceImage { ImageUrl = url, DisplayOrder = order++ });
                }
            }
        }

        db.Services.Add(service);
        await db.SaveChangesAsync(ct);
        TempData["SuccessMessage"] = "تمت إضافة الخدمة بنجاح";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("dashboard/services/{id:int}/edit")]
    public async Task<IActionResult> Edit(int id)
    {
        var service = await db.Services.Include(s => s.Images).FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
        if (service == null) return NotFound();

        var vm = new ServiceFormViewModel
        {
            Id = service.Id,
            Name = service.Name,
            Description = service.Description,
            Details = service.Details,
            DisplayOrder = service.DisplayOrder,
            ServiceCategoryId = service.ServiceCategoryId,
            IsActive = service.IsActive,
            ExistingImages = service.Images.OrderBy(i => i.DisplayOrder).ToList()
        };
        await PopulateCategories(vm);
        return View("Form", vm);
    }

    [HttpPost("dashboard/services/{id:int}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ServiceFormViewModel model, CancellationToken ct)
    {
        if (model.NewImages != null)
        {
            model.NewImages = model.NewImages.Where(f => f != null && f.Length > 0 && !string.IsNullOrWhiteSpace(f.FileName)).ToList();
            if (!model.NewImages.Any()) ModelState.Remove(nameof(model.NewImages));
        }

        if (!ModelState.IsValid)
        {
            await PopulateCategories(model);
            return View("Form", model);
        }

        var service = await db.Services.Include(s => s.Images).FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
        if (service == null) return NotFound();

        service.Name = model.Name.Trim();
        service.Description = model.Description?.Trim();
        service.Details = model.Details?.Trim();
        service.DisplayOrder = model.DisplayOrder;
        service.ServiceCategoryId = model.ServiceCategoryId;
        service.IsActive = model.IsActive;
        service.UpdatedAt = DateTime.UtcNow;

        if (model.NewImages != null && model.NewImages.Any())
        {
            int order = service.Images.Any() ? service.Images.Max(i => i.DisplayOrder) + 1 : 1;
            foreach (var imgFile in model.NewImages)
            {
                if (imgFile.Length > 0)
                {
                    var url = await fileStorage.SaveImageAsync(imgFile, "services/images", ct);
                    service.Images.Add(new ServiceImage { ImageUrl = url, DisplayOrder = order++ });
                }
            }
        }

        await db.SaveChangesAsync(ct);
        TempData["SuccessMessage"] = "تم تعديل الخدمة بنجاح";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("dashboard/services/{id:int}/toggle")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int id)
    {
        var service = await db.Services.FindAsync(id);
        if (service == null) return NotFound();
        service.IsActive = !service.IsActive;
        await db.SaveChangesAsync();
        TempData["SuccessMessage"] = service.IsActive ? "تم تفعيل الخدمة بنجاح" : "تم تعطيل الخدمة بنجاح";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("dashboard/services/{id:int}/delete-image/{imageId:int}")]
    public async Task<IActionResult> DeleteImage(int id, int imageId)
    {
        var service = await db.Services.Include(s => s.Images).FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
        if (service == null) return NotFound();

        var img = service.Images.FirstOrDefault(i => i.Id == imageId);
        if (img != null)
        {
            await fileStorage.DeleteAsync(img.ImageUrl);
            db.ServiceImages.Remove(img);
            await db.SaveChangesAsync();
        }
        return Ok();
    }

    [HttpPost("dashboard/services/{id:int}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var service = await db.Services.FindAsync(id);
        if (service == null) return NotFound();
        service.IsDeleted = true;
        await db.SaveChangesAsync();
        TempData["SuccessMessage"] = "تم نقل الخدمة إلى سلة المحذوفات بنجاح";
        return RedirectToAction(nameof(Index));
    }
}
