using Microsoft.AspNetCore.Mvc;

namespace BE.Controllers
{
    public class StockController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
