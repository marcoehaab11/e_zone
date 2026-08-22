using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SupplierDirectory.Domain;
using SupplierDirectory.Application;
using SupplierDirectory.Infrastructure;

namespace SupplierDirectory.Controllers;

[Authorize(Roles = "Admin")]
public sealed class AdsDashboardController(AppDbContext db, IFileStorageService fileStorage) : Controller
{
    private async Task PopulateAvailableAreas(AdvertisementFormViewModel model)
    {
        model.AvailableAreas = await db.Areas.AsNoTracking().Where(a => !a.IsDeleted && a.IsActive).OrderBy(a => a.Name).ToListAsync();
    }

    [HttpGet("dashboard/ads")]
    public async Task<IActionResult> Index([FromQuery] string search = "", [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var query = db.Advertisements.Include(a => a.Area).AsNoTracking().Where(a => !a.IsDeleted);
        
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x => x.Title.Contains(search) || (x.Description ?? "").Contains(search));
        }

        var total = await query.CountAsync();
        var items = await query.OrderBy(x => x.DisplayOrder).ThenByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.Search = search;
        ViewBag.Page = page;
        ViewBag.PageSize = pageSize;
        ViewBag.TotalPages = (int)Math.Ceiling(total / (double)pageSize);

        return View(items);
    }

    [HttpGet("dashboard/ads/create")]
    public async Task<IActionResult> Create()
    {
        var model = new AdvertisementFormViewModel { StartDate = DateTime.Today, EndDate = DateTime.Today.AddMonths(1) };
        await PopulateAvailableAreas(model);
        return View("Form", model);
    }

    [HttpPost("dashboard/ads/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AdvertisementFormViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            await PopulateAvailableAreas(model);
            return View("Form", model);
        }

        var ad = new Advertisement
        {
            Title = model.Title.Trim(),
            Description = model.Description?.Trim(),
            Link = model.Link?.Trim(),
            StartDate = model.StartDate,
            EndDate = model.EndDate,
            IsActive = model.IsActive,
            DisplayOrder = model.DisplayOrder,
            AreaId = model.AreaId
        };

        if (model.ImageFile != null)
        {
            try { ad.ImageUrl = await fileStorage.SaveImageAsync(model.ImageFile, "advertisements", ct); }
            catch (Exception ex) { ModelState.AddModelError("ImageFile", ex.Message); await PopulateAvailableAreas(model); return View("Form", model); }
        }

        db.Advertisements.Add(ad);
        await db.SaveChangesAsync(ct);
        TempData["SuccessMessage"] = "تمت إضافة الإعلان بنجاح";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("dashboard/ads/{id:int}/edit")]
    public async Task<IActionResult> Edit(int id)
    {
        var ad = await db.Advertisements.FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);
        if (ad == null) return NotFound();
        var vm = new AdvertisementFormViewModel
        {
            Id = ad.Id,
            Title = ad.Title,
            Description = ad.Description,
            Link = ad.Link,
            StartDate = ad.StartDate,
            EndDate = ad.EndDate,
            IsActive = ad.IsActive,
            DisplayOrder = ad.DisplayOrder,
            ImageUrl = ad.ImageUrl,
            AreaId = ad.AreaId
        };
        await PopulateAvailableAreas(vm);
        return View("Form", vm);
    }

    [HttpPost("dashboard/ads/{id:int}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, AdvertisementFormViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            await PopulateAvailableAreas(model);
            return View("Form", model);
        }
        
        var ad = await db.Advertisements.FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);
        if (ad == null) return NotFound();

        ad.Title = model.Title.Trim();
        ad.Description = model.Description?.Trim();
        ad.Link = model.Link?.Trim();
        ad.StartDate = model.StartDate;
        ad.EndDate = model.EndDate;
        ad.IsActive = model.IsActive;
        ad.DisplayOrder = model.DisplayOrder;
        ad.AreaId = model.AreaId;
        ad.UpdatedAt = DateTime.UtcNow;

        if (model.ImageFile != null)
        {
            if (!string.IsNullOrEmpty(ad.ImageUrl)) await fileStorage.DeleteAsync(ad.ImageUrl);
            try { ad.ImageUrl = await fileStorage.SaveImageAsync(model.ImageFile, "advertisements", ct); }
            catch (Exception ex) { ModelState.AddModelError("ImageFile", ex.Message); await PopulateAvailableAreas(model); return View("Form", model); }
        }
        
        await db.SaveChangesAsync(ct);
        TempData["SuccessMessage"] = "تم تعديل الإعلان بنجاح";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("dashboard/ads/{id:int}/toggle")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int id)
    {
        var ad = await db.Advertisements.FindAsync(id);
        if (ad == null) return NotFound();
        ad.IsActive = !ad.IsActive;
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("dashboard/ads/{id:int}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var ad = await db.Advertisements.FindAsync(id);
        if (ad == null) return NotFound();
        ad.IsDeleted = true;
        await db.SaveChangesAsync();
        TempData["SuccessMessage"] = "تم حذف الإعلان بنجاح";
        return RedirectToAction(nameof(Index));
    }
}
