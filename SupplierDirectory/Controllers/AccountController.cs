using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SupplierDirectory.Domain;
namespace SupplierDirectory.Controllers;
public sealed class AccountController(SignInManager<ApplicationUser> signIn) : Controller {
 [AllowAnonymous,HttpGet("login")] public IActionResult Login(string? returnUrl=null){ViewBag.ReturnUrl=returnUrl;return View();}
 [AllowAnonymous,HttpPost("login"),ValidateAntiForgeryToken] public async Task<IActionResult> Login(string email,string password,string? returnUrl=null){var result=await signIn.PasswordSignInAsync(email,password,false,true);if(result.Succeeded)return LocalRedirect(returnUrl??"/");ModelState.AddModelError("","بيانات الدخول غير صحيحة أو الحساب مقفل");return View();}
 [HttpPost("logout"),ValidateAntiForgeryToken] public async Task<IActionResult> Logout(){await signIn.SignOutAsync();return RedirectToAction(nameof(Login));}
}
