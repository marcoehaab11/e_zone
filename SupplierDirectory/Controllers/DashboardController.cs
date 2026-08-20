using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SupplierDirectory.Domain;
using SupplierDirectory.Application;
using SupplierDirectory.Infrastructure;

namespace SupplierDirectory.Controllers;

[Authorize(Roles = "Admin")]
public sealed class DashboardController(AppDbContext db, IFileStorageService fileStorage) : Controller
{
    [HttpGet("")]
    [HttpGet("dashboard")]
    public async Task<IActionResult> Index()
    {
        ViewBag.Stats = new {
            Suppliers = await db.Suppliers.CountAsync(x => !x.IsDeleted),
            ActiveSuppliers = await db.Suppliers.CountAsync(x => !x.IsDeleted && x.IsActive),
            Areas = await db.Areas.CountAsync(x => !x.IsDeleted),
            Categories = await db.Categories.CountAsync(x => !x.IsDeleted),
            Ads = await db.Advertisements.CountAsync(x => !x.IsDeleted),
            ActiveAds = await db.Advertisements.CountAsync(x => !x.IsDeleted && x.IsActive)
        };
        ViewBag.Suppliers = await db.Suppliers.Where(x => !x.IsDeleted).OrderByDescending(x => x.CreatedAt).Take(5).ToListAsync();
        return View();
    }

    [HttpGet("dashboard/company")]
    public async Task<IActionResult> Company()
    {
        var info = await db.CompanyInfos.FirstOrDefaultAsync() ?? new CompanyInfo();
        var vm = new CompanyFormViewModel
        {
            Id = info.Id,
            CompanyName = info.CompanyName,
            LogoUrl = info.LogoUrl,
            CoverImageUrl = info.CoverImageUrl,
            ContactPhone = info.ContactPhone,
            WhatsApp = info.WhatsApp,
            Email = info.Email,
            Website = info.Website,
            About = info.About,
            Mission = info.Mission,
            Vision = info.Vision,
            PlatformDescription = info.PlatformDescription,
            PlatformServices = info.PlatformServices,
            SocialLinksJson = info.SocialLinksJson
        };
        return View(vm);
    }

    [HttpPost("dashboard/company")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Company(CompanyFormViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var existing = await db.CompanyInfos.FirstOrDefaultAsync();
        
        if (existing == null)
        {
            existing = new CompanyInfo();
            db.CompanyInfos.Add(existing);
        }

        existing.CompanyName = model.CompanyName?.Trim();
        existing.About = model.About?.Trim();
        existing.Mission = model.Mission?.Trim();
        existing.Vision = model.Vision?.Trim();
        existing.PlatformDescription = model.PlatformDescription?.Trim();
        existing.PlatformServices = model.PlatformServices?.Trim();
        existing.ContactPhone = model.ContactPhone?.Trim();
        existing.WhatsApp = model.WhatsApp?.Trim();
        existing.Email = model.Email?.Trim();
        existing.Website = model.Website?.Trim();
        existing.SocialLinksJson = model.SocialLinksJson?.Trim(); 
        existing.UpdatedAt = DateTime.UtcNow;

        if (model.LogoFile != null)
        {
            if (!string.IsNullOrEmpty(existing.LogoUrl)) await fileStorage.DeleteAsync(existing.LogoUrl);
            try { existing.LogoUrl = await fileStorage.SaveImageAsync(model.LogoFile, "company", ct); }
            catch (Exception ex) { ModelState.AddModelError("LogoFile", ex.Message); return View(model); }
        }

        if (model.CoverFile != null)
        {
            if (!string.IsNullOrEmpty(existing.CoverImageUrl)) await fileStorage.DeleteAsync(existing.CoverImageUrl);
            try { existing.CoverImageUrl = await fileStorage.SaveImageAsync(model.CoverFile, "company", ct); }
            catch (Exception ex) { ModelState.AddModelError("CoverFile", ex.Message); return View(model); }
        }

        await db.SaveChangesAsync(ct);
        TempData["SuccessMessage"] = "طھظ… طھط­ط¯ظٹط« ظ…ط¹ظ„ظˆظ…ط§طھ ط§ظ„ط´ط±ظƒط© ط¨ظ†ط¬ط§ط­";
        
        return RedirectToAction(nameof(Company));
    }
}

