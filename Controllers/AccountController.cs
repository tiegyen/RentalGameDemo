using Microsoft.AspNetCore.Mvc;

namespace RentalGameDemo.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
