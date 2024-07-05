using Microsoft.AspNetCore.Mvc;

namespace SANATIFY.Controllers;

public class UserController : Controller
{
    public IActionResult SignUp()
    {
        return View("SignUp");
    }

    public IActionResult Login()
    {
        return View("Login");
    }

    public IActionResult Browse()
    {
        throw new NotImplementedException();
    }

    public IActionResult Library()
    {
        throw new NotImplementedException();
    }

    public IActionResult LikedSongs()
    {
        throw new NotImplementedException();
    }
}