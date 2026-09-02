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
    [HttpGet("dashboard/services")]
    public async Task<IActionResult> Index([FromQuery] ServiceListQuery q)
    {
        var query = db.Services.Include(s => s.Images).AsNoTracking().Where(s => !s.IsDeleted);
        
        if (!string.IsNullOrWhiteSpace(q.Search))
            query = query.Where(x => x.Name.Contains(q.Search) || (x.Description != null && x.Description.Contains(q.Search)));

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

        return View(items);
    }

    [HttpGet("dashboard/services/create")]
    public IActionResult Create()
    {
        return View("Form", new ServiceFormViewModel());
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
            return View("Form", model);
        }

        var service = new Service
        {
            Name = model.Name.Trim(),
            Description = model.Description?.Trim(),
            Details = model.Details?.Trim(),
            DisplayOrder = model.DisplayOrder,
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
            IsActive = service.IsActive,
            ExistingImages = service.Images.OrderBy(i => i.DisplayOrder).ToList()
        };
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
            return View("Form", model);
        }

        var service = await db.Services.Include(s => s.Images).FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
        if (service == null) return NotFound();

        service.Name = model.Name.Trim();
        service.Description = model.Description?.Trim();
        service.Details = model.Details?.Trim();
        service.DisplayOrder = model.DisplayOrder;
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
