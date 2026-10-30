using Microsoft.AspNetCore.Mvc;

namespace RentalGameDemo.Controllers
{
    public class RentalController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
