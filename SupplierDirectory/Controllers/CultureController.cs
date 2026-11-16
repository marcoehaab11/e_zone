using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace SupplierDirectory.Controllers;

public sealed class CultureController : Controller
{
    [HttpPost("culture/set")]
    [ValidateAntiForgeryToken]
    public IActionResult Set(string culture, string? returnUrl)
    {
        culture = culture?.StartsWith("en", StringComparison.OrdinalIgnoreCase) == true ? "en" : "ar";
        Response.Cookies.Append(CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
            new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), IsEssential = true, SameSite = SameSiteMode.Lax });
        return LocalRedirect(!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl) ? returnUrl : "/dashboard");
    }
}
