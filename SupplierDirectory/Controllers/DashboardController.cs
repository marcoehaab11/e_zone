using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SupplierDirectory.Infrastructure;
namespace SupplierDirectory.Controllers;
[Authorize(Roles="Admin")]
public sealed class DashboardController(AppDbContext db) : Controller { public async Task<IActionResult> Index(){ViewBag.Stats=new { Suppliers=await db.Suppliers.CountAsync(),ActiveSuppliers=await db.Suppliers.CountAsync(x=>x.IsActive),Areas=await db.Areas.CountAsync(),Categories=await db.Categories.CountAsync(),Ads=await db.Advertisements.CountAsync(),ActiveAds=await db.Advertisements.CountAsync(x=>x.IsActive)};ViewBag.Suppliers=await db.Suppliers.OrderByDescending(x=>x.CreatedAt).Take(5).ToListAsync();return View();} }
