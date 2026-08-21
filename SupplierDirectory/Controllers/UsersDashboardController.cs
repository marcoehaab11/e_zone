using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SupplierDirectory.Application;
using SupplierDirectory.Domain;

namespace SupplierDirectory.Controllers;

[Authorize(Roles = "Admin")]
[Route("dashboard/users")]
public class UsersDashboardController(UserManager<ApplicationUser> userManager) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var users = await userManager.Users
            .Select(u => new UserDto(u.Id, u.Email!))
            .ToListAsync();
        return View(users);
    }

    [HttpGet("create")]
    public IActionResult Create()
    {
        return View("Form", new UserFormRequest());
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserFormRequest model)
    {
        if (string.IsNullOrWhiteSpace(model.Password))
        {
            ModelState.AddModelError("Password", "كلمة المرور مطلوبة عند إضافة مستخدم جديد");
        }

        if (!ModelState.IsValid)
            return View("Form", model);

        var user = new ApplicationUser { UserName = model.Email, Email = model.Email, EmailConfirmed = true };
        var result = await userManager.CreateAsync(user, model.Password!);

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, "Admin");
            return RedirectToAction(nameof(Index));
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError("", error.Description);

        return View("Form", model);
    }

    [HttpGet("{id}/edit")]
    public async Task<IActionResult> Edit(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        return View("Form", new UserFormRequest { Id = user.Id, Email = user.Email! });
    }

    [HttpPost("{id}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, UserFormRequest model)
    {
        if (id != model.Id) return BadRequest();

        var user = await userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        if (!ModelState.IsValid) return View("Form", model);

        user.Email = model.Email;
        user.UserName = model.Email;
        
        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            foreach (var error in updateResult.Errors) ModelState.AddModelError("", error.Description);
            return View("Form", model);
        }

        if (!string.IsNullOrEmpty(model.Password))
        {
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var passResult = await userManager.ResetPasswordAsync(user, token, model.Password);
            if (!passResult.Succeeded)
            {
                foreach (var error in passResult.Errors) ModelState.AddModelError("", error.Description);
                return View("Form", model);
            }
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("{id}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        if (user.Email == "admin@admin.com")
            return BadRequest("لا يمكن حذف المدير الأساسي");

        await userManager.DeleteAsync(user);
        return RedirectToAction(nameof(Index));
    }
}
