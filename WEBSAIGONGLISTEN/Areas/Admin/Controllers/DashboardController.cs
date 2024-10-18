using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WEBSAIGONGLISTEN.Models;

namespace WEBSAIGONGLISTEN.Areas.Admin.Controllers
{

    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class DashboardController : Controller
    {

        private readonly ApplicationDbContext _dbcontext;

        public DashboardController (ApplicationDbContext context)
        {
            _dbcontext = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public List<object> GetTotalOders()
        {
            List<object> data = new List<object>();
            List<string> labels = _dbcontext.OrderDetails.Select(m => m.Product.Name).Take(5).ToList();
            List<int> total = _dbcontext.OrderDetails.Select(t => t.Quantity).Take(5).ToList();
            data.Add(labels);
            data.Add(total);
            return data;
        }
    }
}
