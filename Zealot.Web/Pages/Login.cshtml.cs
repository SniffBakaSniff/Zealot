using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Zealot.Web.Pages;

public class LoginModel(IConfiguration configuration) : PageModel
{
    [TempData]
    public string? ErrorMessage { get; set; }

    public IActionResult OnGet()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToPage("/Dashboard");
        }

        return Page();
    }

    public IActionResult OnPost()
    {
        if (string.IsNullOrWhiteSpace(configuration["Discord:ClientId"]) ||
            string.IsNullOrWhiteSpace(configuration["Discord:ClientSecret"]))
        {
            ErrorMessage = "Discord OAuth is not configured yet. Set Discord:ClientId and Discord:ClientSecret before logging in.";
            return RedirectToPage("/Login");
        }

        var properties = new AuthenticationProperties
        {
            RedirectUri = Url.Page("/Dashboard")
        };

        return Challenge(properties, "Discord");
    }
}
