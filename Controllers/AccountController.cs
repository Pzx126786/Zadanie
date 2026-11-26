using BeFit.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
namespace BeFit.Controllers;
public class AccountController(UserManager<IdentityUser> users, SignInManager<IdentityUser> signIn) : Controller
{
    [HttpGet] public IActionResult Register() => View(new RegisterModel());
    [HttpPost]
    public async Task<IActionResult> Register(RegisterModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var user = new IdentityUser { UserName = model.Email, Email = model.Email }; var result = await users.CreateAsync(user, model.Password);
        if (result.Succeeded) { await signIn.SignInAsync(user, false); return RedirectToAction("Index", "TrainingSessions"); }
        ModelState.AddModelError("", "Nie udało się utworzyć konta. E-mail może być zajęty. Hasło musi mieć 10–128 znaków."); return View(model);
    }
    [HttpGet] public IActionResult Login() => View(new LoginModel());
    [HttpPost]
    public async Task<IActionResult> Login(LoginModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid) return View(model);
        var result = await signIn.PasswordSignInAsync(model.Email, model.Password, false, true);
        if (result.Succeeded) return Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl!) : RedirectToAction("Index", "TrainingSessions");
        ModelState.AddModelError("", result.IsLockedOut ? "Zbyt wiele prób. Spróbuj za kilka minut." : "Nieprawidłowy e-mail lub hasło."); return View(model);
    }
    [Authorize, HttpPost] public async Task<IActionResult> Logout() { await signIn.SignOutAsync(); return RedirectToAction("Index", "Home"); }
    public IActionResult Denied() { Response.StatusCode = 403; return View(); }
}
