using Microsoft.AspNetCore.Mvc;

namespace EmlakPortal.UI.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Dashboard() => View();
        public IActionResult Estates() => View();
        public IActionResult Categories() => View();
        public IActionResult Users() => View();
        public IActionResult Profile()
        {
            return View();
        }
    }
}