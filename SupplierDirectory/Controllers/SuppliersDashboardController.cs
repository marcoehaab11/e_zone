using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SupplierDirectory.Application;
using SupplierDirectory.Domain;
using SupplierDirectory.Infrastructure;

namespace SupplierDirectory.Controllers;

[Authorize(Roles = "Admin")]
public sealed class SuppliersDashboardController(AppDbContext db, IFileStorageService fileStorage) : Controller
{
    [HttpGet("dashboard/suppliers")]
    public async Task<IActionResult> Index([FromQuery] SupplierListQuery query)
    {
        var suppliersQuery = db.Suppliers
            .Include(s => s.SupplierCategories).ThenInclude(sc => sc.Category)
            .Include(s => s.SupplierAreas).ThenInclude(sa => sa.Area)
            .Where(s => !s.IsDeleted);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            suppliersQuery = suppliersQuery.Where(s => 
                s.Name.Contains(query.Search) || 
                (s.PhoneNumber != null && s.PhoneNumber.Contains(query.Search)) || 
                (s.Description != null && s.Description.Contains(query.Search))
            );
        }

        if (query.CategoryId.HasValue)
        {
            suppliersQuery = suppliersQuery.Where(s => s.SupplierCategories.Any(c => c.CategoryId == query.CategoryId));
        }

        if (query.AreaId.HasValue)
        {
            suppliersQuery = suppliersQuery.Where(s => s.SupplierAreas.Any(a => a.AreaId == query.AreaId));
        }

        var total = await suppliersQuery.CountAsync();
        var items = await suppliersQuery
            .OrderByDescending(s => s.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        var vm = new SupplierListViewModel
        {
            Items = items,
            TotalCount = total,
            Query = query,
            Categories = await db.Categories.Where(c => !c.IsDeleted && c.IsActive).OrderBy(c => c.Name).ToListAsync(),
            Areas = await db.Areas.Where(a => !a.IsDeleted && a.IsActive).OrderBy(a => a.Name).ToListAsync()
        };

        return View(vm);
    }

    [HttpGet("dashboard/suppliers/create")]
    public async Task<IActionResult> Create()
    {
        var vm = new SupplierFormViewModel
        {
            AvailableCategories = await db.Categories.Where(c => !c.IsDeleted && c.IsActive).OrderBy(c => c.Name).ToListAsync(),
            AvailableAreas = await db.Areas.Where(a => !a.IsDeleted && a.IsActive).OrderBy(a => a.Name).ToListAsync()
        };
        return View("Form", vm);
    }

    [HttpPost("dashboard/suppliers/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SupplierFormViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            model.AvailableCategories = await db.Categories.Where(c => !c.IsDeleted && c.IsActive).OrderBy(c => c.Name).ToListAsync();
            model.AvailableAreas = await db.Areas.Where(a => !a.IsDeleted && a.IsActive).OrderBy(a => a.Name).ToListAsync();
            return View("Form", model);
        }

        var supplier = new Supplier
        {
            Name = model.Name.Trim(),
            Description = model.Description?.Trim(),
            PhoneNumber = model.PhoneNumber?.Trim(),
            AdditionalPhoneNumbers = model.AdditionalPhoneNumbers?.Trim(),
            WhatsAppNumber = model.WhatsAppNumber?.Trim(),
            Email = model.Email?.Trim(),
            Website = model.Website?.Trim(),
            Address = model.Address?.Trim(),
            ShortAddress = model.ShortAddress?.Trim(),
            HasTechnicians = model.HasTechnicians,
            Latitude = model.Latitude,
            Longitude = model.Longitude,
            GoogleMapsUrl = model.GoogleMapsUrl?.Trim(),
            IsActive = model.IsActive
        };

        if (model.LogoFile != null)
        {
            try { supplier.LogoUrl = await fileStorage.SaveImageAsync(model.LogoFile, "suppliers/logos", ct); }
            catch (Exception ex) { ModelState.AddModelError("LogoFile", ex.Message); }
        }

        if (ModelState.ErrorCount > 0)
        {
            model.AvailableCategories = await db.Categories.Where(c => !c.IsDeleted && c.IsActive).OrderBy(c => c.Name).ToListAsync();
            model.AvailableAreas = await db.Areas.Where(a => !a.IsDeleted && a.IsActive).OrderBy(a => a.Name).ToListAsync();
            return View("Form", model);
        }

        foreach (var catId in model.SelectedCategoryIds)
            supplier.SupplierCategories.Add(new SupplierCategory { CategoryId = catId });

        foreach (var areaId in model.SelectedAreaIds)
            supplier.SupplierAreas.Add(new SupplierArea { AreaId = areaId });

        if (model.NewImages.Any())
        {
            int order = 1;
            foreach (var imgFile in model.NewImages)
            {
                if (imgFile.Length > 0)
                {
                    var url = await fileStorage.SaveImageAsync(imgFile, "suppliers/images", ct);
                    supplier.Images.Add(new SupplierImage { ImageUrl = url, DisplayOrder = order++ });
                }
            }
        }

        db.Suppliers.Add(supplier);
        await db.SaveChangesAsync(ct);
        TempData["SuccessMessage"] = "تمت إضافة المورد بنجاح";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("dashboard/suppliers/{id:int}/edit")]
    public async Task<IActionResult> Edit(int id)
    {
        var supplier = await db.Suppliers
            .Include(s => s.SupplierCategories)
            .Include(s => s.SupplierAreas)
            .Include(s => s.Images)
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

        if (supplier == null) return NotFound();

        var vm = new SupplierFormViewModel
        {
            Id = supplier.Id,
            Name = supplier.Name,
            Description = supplier.Description,
            ExistingLogoUrl = supplier.LogoUrl,
            PhoneNumber = supplier.PhoneNumber,
            AdditionalPhoneNumbers = supplier.AdditionalPhoneNumbers,
            WhatsAppNumber = supplier.WhatsAppNumber,
            Email = supplier.Email,
            Website = supplier.Website,
            Address = supplier.Address, ShortAddress = supplier.ShortAddress, HasTechnicians = supplier.HasTechnicians,
            Latitude = supplier.Latitude,
            Longitude = supplier.Longitude,
            GoogleMapsUrl = supplier.GoogleMapsUrl,
            IsActive = supplier.IsActive,
            SelectedCategoryIds = supplier.SupplierCategories.Select(c => c.CategoryId).ToList(),
            SelectedAreaIds = supplier.SupplierAreas.Select(a => a.AreaId).ToList(),
            ExistingImages = supplier.Images.OrderBy(i => i.DisplayOrder).ToList(),
            
            AvailableCategories = await db.Categories.Where(c => !c.IsDeleted && c.IsActive).OrderBy(c => c.Name).ToListAsync(),
            AvailableAreas = await db.Areas.Where(a => !a.IsDeleted && a.IsActive).OrderBy(a => a.Name).ToListAsync()
        };

        return View("Form", vm);
    }

    [HttpPost("dashboard/suppliers/{id:int}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, SupplierFormViewModel model, CancellationToken ct)
    {
        var supplier = await db.Suppliers
            .Include(s => s.SupplierCategories)
            .Include(s => s.SupplierAreas)
            .Include(s => s.Images)
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

        if (supplier == null) return NotFound();

        if (!ModelState.IsValid)
        {
            model.ExistingLogoUrl = supplier.LogoUrl;
            model.ExistingImages = supplier.Images.OrderBy(i => i.DisplayOrder).ToList();
            model.AvailableCategories = await db.Categories.Where(c => !c.IsDeleted && c.IsActive).OrderBy(c => c.Name).ToListAsync();
            model.AvailableAreas = await db.Areas.Where(a => !a.IsDeleted && a.IsActive).OrderBy(a => a.Name).ToListAsync();
            return View("Form", model);
        }

        supplier.Name = model.Name.Trim();
        supplier.Description = model.Description?.Trim();
        supplier.PhoneNumber = model.PhoneNumber?.Trim();
        supplier.AdditionalPhoneNumbers = model.AdditionalPhoneNumbers?.Trim();
        supplier.WhatsAppNumber = model.WhatsAppNumber?.Trim();
        supplier.Email = model.Email?.Trim();
        supplier.Website = model.Website?.Trim();
        supplier.Address = model.Address?.Trim();
        supplier.ShortAddress = model.ShortAddress?.Trim();
        supplier.HasTechnicians = model.HasTechnicians;
        supplier.Latitude = model.Latitude;
        supplier.Longitude = model.Longitude;
        supplier.GoogleMapsUrl = model.GoogleMapsUrl?.Trim();
        supplier.IsActive = model.IsActive;
        supplier.UpdatedAt = DateTime.UtcNow;

        if (model.LogoFile != null)
        {
            if (!string.IsNullOrEmpty(supplier.LogoUrl))
                await fileStorage.DeleteAsync(supplier.LogoUrl);
                
            supplier.LogoUrl = await fileStorage.SaveImageAsync(model.LogoFile, "suppliers/logos", ct);
        }

        // Update Categories
        supplier.SupplierCategories.Clear();
        foreach (var catId in model.SelectedCategoryIds)
            supplier.SupplierCategories.Add(new SupplierCategory { CategoryId = catId });

        // Update Areas
        supplier.SupplierAreas.Clear();
        foreach (var areaId in model.SelectedAreaIds)
            supplier.SupplierAreas.Add(new SupplierArea { AreaId = areaId });

        // Add New Images
        if (model.NewImages.Any())
        {
            int order = supplier.Images.Any() ? supplier.Images.Max(i => i.DisplayOrder) + 1 : 1;
            foreach (var imgFile in model.NewImages)
            {
                if (imgFile.Length > 0)
                {
                    var url = await fileStorage.SaveImageAsync(imgFile, "suppliers/images", ct);
                    supplier.Images.Add(new SupplierImage { ImageUrl = url, DisplayOrder = order++ });
                }
            }
        }

        await db.SaveChangesAsync(ct);
        TempData["SuccessMessage"] = "تم تعديل بيانات المورد بنجاح";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("dashboard/suppliers/{id:int}/delete-image/{imageId:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteImage(int id, int imageId)
    {
        var supplier = await db.Suppliers.Include(s => s.Images).FirstOrDefaultAsync(s => s.Id == id);
        if (supplier == null) return NotFound();

        var image = supplier.Images.FirstOrDefault(i => i.Id == imageId);
        if (image != null)
        {
            await fileStorage.DeleteAsync(image.ImageUrl);
            db.Set<SupplierImage>().Remove(image);
            await db.SaveChangesAsync();
        }
        
        return RedirectToAction(nameof(Edit), new { id });
    }

    [HttpPost("dashboard/suppliers/{id:int}/toggle")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int id)
    {
        var s = await db.Suppliers.FindAsync(id);
        if (s == null) return NotFound();
        s.IsActive = !s.IsActive;
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("dashboard/suppliers/{id:int}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var s = await db.Suppliers.FindAsync(id);
        if (s == null) return NotFound();
        s.IsDeleted = true;
        await db.SaveChangesAsync();
        TempData["SuccessMessage"] = "تم نقل المورد لسلة المحذوفات";
        return RedirectToAction(nameof(Index));
    }
}


