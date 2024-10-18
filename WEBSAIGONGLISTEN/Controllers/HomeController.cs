using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using WEBSAIGONGLISTEN.Extensions;
using WEBSAIGONGLISTEN.Models;
using WEBSAIGONGLISTEN.Services;

namespace WEBSAIGONGLISTEN.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ApplicationDbContext _context;// day la hung
        private readonly IMomoService _momoService;
        private readonly int _pageSize = 8;
        public HomeController(IProductRepository productRepository, ApplicationDbContext context, IMomoService momoService) // cho nay la hung
        {
            _context = context; // cho nay la hung
            _productRepository = productRepository;
            _momoService = momoService;
        }

        public async Task<IActionResult> Index(int page = 1, DateTime? departureDate = null, DateTime? returnDate = null)
        {
            IQueryable<Product> query = _context.Products;

          
            int totalProducts = await query.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalProducts / _pageSize);

            var paginatedProducts = await query
                //.OrderByDescending(p => p.DepartureDate)
                .Skip((page - 1) * _pageSize)
                .Take(_pageSize)
                .ToListAsync();

            ViewData["TotalPages"] = totalPages;
            ViewData["CurrentPage"] = page;
            ViewData["DepartureDate"] = departureDate ?? DateTime.Now;
            ViewData["ReturnDate"] = returnDate ?? DateTime.Now.AddDays(7);

            return View(paginatedProducts);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult weatherForecast()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public IActionResult Tour()
        {
            var tours = _context.Products.ToList(); // Lấy danh sách tour từ cơ sở dữ liệu
            return View(tours);
        }

        // Hiển thị thông tin chi tiết sản phẩm
        public async Task<IActionResult> Display(int? productId, int? quantity)
        {
            if (productId == null)
            {
                return NotFound();
            }
            //var product = await _productRepository.GetByIdAsync(productId.Value);

            var product = await _context.Products
        
           .FirstOrDefaultAsync(p => p.Id == productId);
            if (product == null)
            {
                return NotFound();
            }
            // Xử lý thêm cho quantity nếu cần
            return View(product);
        }

        public async Task<IActionResult> Search(string query, DateTime? departureDate = null, DateTime? returnDate = null, int page = 1)
        {
            // Kiểm tra xem query có null không
            if (query == null)
            {
                // Trả về một View trống hoặc thông báo không tìm thấy.
                return View("Search", new List<Product>());
            }

            // Loại bỏ các khoảng trắng không cần thiết từ query
            query = query.Trim();

            // Chuyển đổi query để tìm kiếm các sản phẩm chứa các từ được phân tách bởi khoảng trắng
            var keywords = query.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            // Bắt đầu với tất cả các sản phẩm
            IQueryable<Product> queryableProducts = _context.Products;

            // Lọc kết quả theo từ khóa tìm kiếm
            queryableProducts = queryableProducts.Where(p => keywords.Any(keyword => p.Name.Contains(keyword)));

         

            // Tính toán số lượng sản phẩm và số trang
            int totalProducts = await queryableProducts.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalProducts / _pageSize);

            // Lấy dữ liệu đã lọc
            var paginatedProducts = await queryableProducts
                //.OrderByDescending(p => p.DepartureDate)
                .Skip((page - 1) * _pageSize)
                .Take(_pageSize)
                .ToListAsync();

            // Truyền dữ liệu đến view
            ViewData["DepartureDate"] = departureDate ?? DateTime.Now;
            ViewData["ReturnDate"] = returnDate ?? DateTime.Now.AddDays(7);
            ViewData["TotalPages"] = totalPages;
            ViewData["CurrentPage"] = page;

            // Trả về view với dữ liệu đã lọc
            return View("Search", paginatedProducts);
        }

        public IActionResult Pay()
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
            if (cart == null || !cart.Items.Any())
            {
                // Xử lý giỏ hàng trống
                return RedirectToAction("Index");
            }

            var totalPrice = cart.Items.Sum(i => i.Price * i.Quantity);

            var order = new Order
            {
                TotalPrice = (double)totalPrice
            };

            return View(order);
        }


        [HttpPost]
        public async Task<IActionResult> CreatePaymentUrl(Transaction model)
        {
            var response = await _momoService.CreatePaymentAsync(model);

            if (!string.IsNullOrEmpty(response.PayUrl))
            {
                return Redirect(response.PayUrl);
            }
            else
            {
                return RedirectToAction("Error", "ShoppingCart");
            }
        }

        //[HttpGet]
        //public IActionResult PaymentCallBack()
        //{
        //    var response = _momoService.PaymentExecuteAsync(HttpContext.Request.Query);
        //    return View(response);
        //}

        [HttpGet]
        public IActionResult PaymentCallBack()
        {
            var response = _momoService.PaymentExecuteAsync(HttpContext.Request.Query);
            return View(response);
        }

        // Phương thức để gửi bình luận
        [HttpPost]
        public async Task<IActionResult> SubmitReview(int productId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return Json(new { success = false, message = "Nội dung bình luận không thể trống." });
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
            {
                return Json(new { success = false, message = "Sản phẩm không tồn tại." });
            }

            var comment = new Comment
            {
                ProductId = productId,
                UserName = User.Identity.Name,
                Content = content,
                CreatedAt = DateTime.Now
            };

            _context.Products.Update(product);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

    }
}