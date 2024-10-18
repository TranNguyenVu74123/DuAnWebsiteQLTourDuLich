using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WEBSAIGONGLISTEN.Models;

namespace WEBSAIGONGLISTEN.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class RecordVideoController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
