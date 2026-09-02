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
        var info = await db.CompanyInfos.Include(c => c.CoverImages).Include(c => c.Images).Include(c => c.Links).FirstOrDefaultAsync() ?? new CompanyInfo();
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
            SocialLinksJson = info.SocialLinksJson,
            ExistingCoverImages = info.CoverImages.OrderBy(i => i.DisplayOrder).ToList(),
            ExistingImages = info.Images.OrderBy(i => i.DisplayOrder).ToList(),
            Links = info.Links.OrderBy(l => l.DisplayOrder).Select(l => new CompanyLinkViewModel { Id = l.Id, Title = l.Title, Url = l.Url }).ToList()
        };
        return View(vm);
    }

    [HttpPost("dashboard/company")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Company(CompanyFormViewModel model, CancellationToken ct)
    {
        var existing = await db.CompanyInfos.Include(c => c.CoverImages).Include(c => c.Images).Include(c => c.Links).FirstOrDefaultAsync();
        
        if (model.LogoFile == null || model.LogoFile.Length == 0 || string.IsNullOrWhiteSpace(model.LogoFile.FileName))
        {
            model.LogoFile = null;
            ModelState.Remove(nameof(model.LogoFile));
        }

        if (model.CoverFile == null || model.CoverFile.Length == 0 || string.IsNullOrWhiteSpace(model.CoverFile.FileName))
        {
            model.CoverFile = null;
            ModelState.Remove(nameof(model.CoverFile));
        }

        if (model.NewCoverImages != null)
        {
            model.NewCoverImages = model.NewCoverImages.Where(f => f != null && f.Length > 0 && !string.IsNullOrWhiteSpace(f.FileName)).ToList();
            if (!model.NewCoverImages.Any()) ModelState.Remove(nameof(model.NewCoverImages));
        }

        if (model.NewImages != null)
        {
            model.NewImages = model.NewImages.Where(f => f != null && f.Length > 0 && !string.IsNullOrWhiteSpace(f.FileName)).ToList();
            if (!model.NewImages.Any()) ModelState.Remove(nameof(model.NewImages));
        }

        if (!ModelState.IsValid)
        {
            if (existing != null)
            {
                model.ExistingCoverImages = existing.CoverImages.OrderBy(i => i.DisplayOrder).ToList();
                model.ExistingImages = existing.Images.OrderBy(i => i.DisplayOrder).ToList();
            }
            return View(model);
        }

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
            catch (Exception ex) { ModelState.AddModelError("LogoFile", ex.Message); model.ExistingCoverImages = existing.CoverImages.OrderBy(i => i.DisplayOrder).ToList(); model.ExistingImages = existing.Images.OrderBy(i => i.DisplayOrder).ToList(); return View(model); }
        }

        if (model.CoverFile != null)
        {
            if (!string.IsNullOrEmpty(existing.CoverImageUrl)) await fileStorage.DeleteAsync(existing.CoverImageUrl);
            try { existing.CoverImageUrl = await fileStorage.SaveImageAsync(model.CoverFile, "company", ct); }
            catch (Exception ex) { ModelState.AddModelError("CoverFile", ex.Message); model.ExistingCoverImages = existing.CoverImages.OrderBy(i => i.DisplayOrder).ToList(); model.ExistingImages = existing.Images.OrderBy(i => i.DisplayOrder).ToList(); return View(model); }
        }

        if (model.NewCoverImages != null && model.NewCoverImages.Any())
        {
            int order = existing.CoverImages.Any() ? existing.CoverImages.Max(i => i.DisplayOrder) + 1 : 1;
            foreach (var imgFile in model.NewCoverImages)
            {
                if (imgFile.Length > 0)
                {
                    var url = await fileStorage.SaveImageAsync(imgFile, "company/covers", ct);
                    existing.CoverImages.Add(new CompanyCoverImage { ImageUrl = url, DisplayOrder = order++ });
                }
            }
            if (string.IsNullOrEmpty(existing.CoverImageUrl) && existing.CoverImages.Any())
            {
                existing.CoverImageUrl = existing.CoverImages.OrderBy(i => i.DisplayOrder).FirstOrDefault()?.ImageUrl;
            }
        }

        if (model.NewImages != null && model.NewImages.Any())
        {
            int order = existing.Images.Any() ? existing.Images.Max(i => i.DisplayOrder) + 1 : 1;
            foreach (var imgFile in model.NewImages)
            {
                if (imgFile.Length > 0)
                {
                    var url = await fileStorage.SaveImageAsync(imgFile, "company/images", ct);
                    existing.Images.Add(new CompanyImage { ImageUrl = url, DisplayOrder = order++ });
                }
            }
        }

        // Process Links
        var validLinks = model.Links?
            .Where(l => !string.IsNullOrWhiteSpace(l.Title) && !string.IsNullOrWhiteSpace(l.Url))
            .ToList() ?? new();

        var keepIds = validLinks.Where(l => l.Id.HasValue && l.Id.Value > 0).Select(l => l.Id!.Value).ToHashSet();
        var toRemove = existing.Links.Where(l => !keepIds.Contains(l.Id)).ToList();
        foreach (var item in toRemove)
        {
            db.CompanyLinks.Remove(item);
        }

        int linkOrder = 1;
        foreach (var linkDto in validLinks)
        {
            if (linkDto.Id.HasValue && linkDto.Id.Value > 0)
            {
                var currentLink = existing.Links.FirstOrDefault(l => l.Id == linkDto.Id.Value);
                if (currentLink != null)
                {
                    currentLink.Title = linkDto.Title!.Trim();
                    currentLink.Url = linkDto.Url!.Trim();
                    currentLink.DisplayOrder = linkOrder++;
                    currentLink.UpdatedAt = DateTime.UtcNow;
                }
            }
            else
            {
                existing.Links.Add(new CompanyLink
                {
                    Title = linkDto.Title!.Trim(),
                    Url = linkDto.Url!.Trim(),
                    DisplayOrder = linkOrder++
                });
            }
        }

        await db.SaveChangesAsync(ct);
        TempData["SuccessMessage"] = "تم تحديث معلومات الشركة بنجاح";
        
        return RedirectToAction(nameof(Company));
    }

    [HttpPost("dashboard/company/delete-cover-image/{imageId:int}")]
    public async Task<IActionResult> DeleteCompanyCoverImage(int imageId)
    {
        var info = await db.CompanyInfos.Include(c => c.CoverImages).FirstOrDefaultAsync();
        if (info == null) return NotFound();

        var img = info.CoverImages.FirstOrDefault(i => i.Id == imageId);
        if (img != null)
        {
            await fileStorage.DeleteAsync(img.ImageUrl);
            db.CompanyCoverImages.Remove(img);
            if (info.CoverImageUrl == img.ImageUrl)
            {
                info.CoverImageUrl = info.CoverImages.Where(i => i.Id != imageId).OrderBy(i => i.DisplayOrder).FirstOrDefault()?.ImageUrl;
            }
            await db.SaveChangesAsync();
        }
        return Ok();
    }

    [HttpPost("dashboard/company/delete-image/{imageId:int}")]
    public async Task<IActionResult> DeleteCompanyImage(int imageId)
    {
        var info = await db.CompanyInfos.Include(c => c.Images).FirstOrDefaultAsync();
        if (info == null) return NotFound();

        var img = info.Images.FirstOrDefault(i => i.Id == imageId);
        if (img != null)
        {
            await fileStorage.DeleteAsync(img.ImageUrl);
            db.CompanyImages.Remove(img);
            await db.SaveChangesAsync();
        }
        return Ok();
    }
}

