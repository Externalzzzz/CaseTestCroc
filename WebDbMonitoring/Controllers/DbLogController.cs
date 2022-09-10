using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebDbMonitoring.Controllers
{
    public class DbLogController : Controller
    {
        // GET: DbLogController
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult ShowDbLogs()
        {
            return View();
        }
    }
}
