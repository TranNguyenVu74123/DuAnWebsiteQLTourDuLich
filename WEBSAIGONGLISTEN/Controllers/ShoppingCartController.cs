using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WEBSAIGONGLISTEN.Models;
using WEBSAIGONGLISTEN.Services;
using WEBSAIGONGLISTEN.Extensions;
using WEBSAIGONGLISTEN.Setting;

namespace WEBSAIGONGLISTEN.Controllers
{
    [Authorize]
    public class ShoppingCartController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMomoService _momoService;
        //private readonly IMailService _mailService;

        public ShoppingCartController(ApplicationDbContext context, UserManager<ApplicationUser> userManager,
            IProductRepository productRepository, IMomoService momoService/*, IMailService mailService*/)
        {
            _productRepository = productRepository;
            _context = context;
            _userManager = userManager;
            _momoService = momoService;
            //_mailService = mailService;
        }

        public async Task<IActionResult> AddToCart(int productId, int quantity)
        {
            // Giả sử bạn có phương thức lấy thông tin sản phẩm từ productId
            var product = await GetProductFromDatabase(productId);

            var cartItem = new CartItem
            {
                ProductId = productId,
                Name = product.Name,
                Price = product.Price,
                Quantity = quantity
            };

            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
            cart.AddItem(cartItem);

            HttpContext.Session.SetObjectAsJson("Cart", cart);

            return RedirectToAction("Index");
        }

        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
            return View(cart);
        }

        // Các actions khác...

        private async Task<Product> GetProductFromDatabase(int productId)
        {
            // Truy vấn cơ sở dữ liệu để lấy thông tin sản phẩm
            var product = await _productRepository.GetByIdAsync(productId);
            return product;
        }
        public IActionResult Checkout()
        {
            return View(new Order());
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(Order order)
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
            if (cart == null || !cart.Items.Any())
            {
                // Xử lý giỏ hàng trống...
                return RedirectToAction("Index");
            }

            var user = await _userManager.GetUserAsync(User);
            order.UserId = user.Id;
            order.OrderDate = DateTime.UtcNow;
            order.TotalPrice = (double)cart.Items.Sum(i => i.Price * i.Quantity);

            // Kiểm tra số lượng sản phẩm
            foreach (var item in cart.Items)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product == null)
                {
                    // Nếu sản phẩm không tồn tại, thông báo lỗi
                    ModelState.AddModelError("", $"Sản phẩm với ID {item.ProductId} không tồn tại.");
                    return View(order); // Trả lại trang checkout với thông báo lỗi
                }
                else if (product.Quantity <= 0)
                {
                    // Nếu số lượng sản phẩm bằng 0, thông báo lỗi
                    ModelState.AddModelError("", $"Sản phẩm {product.Name} đã hết hàng.");
                    return View(order); // Trả lại trang checkout với thông báo lỗi
                }
                else if (product.Quantity < item.Quantity)
                {
                    // Nếu số lượng yêu cầu lớn hơn số lượng có sẵn, thông báo lỗi
                    ModelState.AddModelError("", $"Sản phẩm {product.Name} không đủ số lượng. Chỉ còn {product.Quantity} sản phẩm.");
                    return View(order); // Trả lại trang checkout với thông báo lỗi
                }
            }

            // Tạo danh sách OrderDetails
            order.OrderDetails = cart.Items.Select(i => new OrderDetail
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                Price = (double)i.Price,
            }).ToList();

            // Thêm đơn hàng vào cơ sở dữ liệu
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Cập nhật số lượng sản phẩm
            foreach (var item in cart.Items)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null)
                {
                    product.Quantity -= item.Quantity; // Trừ số lượng sản phẩm
                }
            }
            await _context.SaveChangesAsync(); // Lưu thay đổi

            HttpContext.Session.Remove("Cart");

            return View("OrderCompleted", order.Id); // Trang xác nhận hoàn thành đơn hàng
        }

        public IActionResult RemoveFromCart(int productId)
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");

            if (cart is not null)
            {
                cart.RemoveItem(productId);

                // Lưu lại giỏ hàng vào Session sau khi đã xóa mục
                HttpContext.Session.SetObjectAsJson("Cart", cart);
            }

            return RedirectToAction("Index");
        }

        // Action để xóa toàn bộ mặt hàng trong giỏ hàng
        [HttpPost]
        public IActionResult ClearCart()
        {
            HttpContext.Session.Remove("Cart");
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Increase(int productId)
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
            if (cart != null)
            {
                var cartItem = cart.Items.FirstOrDefault(item => item.ProductId == productId);
                if (cartItem != null)
                {
                    cartItem.Quantity++;
                    HttpContext.Session.SetObjectAsJson("Cart", cart);
                }
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Decrease(int productId)
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
            if (cart != null)
            {
                var cartItem = cart.Items.FirstOrDefault(item => item.ProductId == productId);
                if (cartItem != null)
                {
                    if (cartItem.Quantity > 1)
                    {
                        cartItem.Quantity--;
                    }
                    else
                    {
                        cart.Items.Remove(cartItem);
                    }
                    HttpContext.Session.SetObjectAsJson("Cart", cart);
                }
            }
            return RedirectToAction("Index");
        }

        //public IActionResult Pay()
        //{

        //    var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
        //    if (cart == null || !cart.Items.Any())
        //    {
        //        // Xử lý giỏ hàng trống
        //        return RedirectToAction("Index");
        //    }

        //    var totalPrice = cart.Items.Sum(i => i.Price * i.Quantity);

        //    var order = new Order
        //    {
        //        TotalPrice = (double)totalPrice
        //    };

        //    return View(order);
        //}

        public IActionResult Pay()
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
            if (cart == null || !cart.Items.Any())
            {
                // Xử lý giỏ hàng trống
                TempData["ErrorMessage"] = "Giỏ hàng của bạn đang trống. Vui lòng thêm sản phẩm trước khi thanh toán.";
                return RedirectToAction("Checkout"); // Quay lại trang Checkout
            }

            // Kiểm tra số lượng sản phẩm
            foreach (var item in cart.Items)
            {
                var product = _context.Products.Find(item.ProductId);
                if (product == null)
                {
                    TempData["ErrorMessage"] = $"Sản phẩm với ID {item.ProductId} không tồn tại.";
                    return RedirectToAction("Checkout"); // Quay lại trang Checkout nếu lỗi
                }
                else if (product.Quantity < item.Quantity)
                {
                    TempData["ErrorMessage"] = $"Sản phẩm {product.Name} không đủ số lượng. Chỉ còn {product.Quantity} sản phẩm.";
                    return RedirectToAction("Checkout"); // Quay lại trang Checkout nếu lỗi
                }
            }

            // Tính toán tổng giá trị đơn hàng
            var totalPrice = cart.Items.Sum(i => i.Price * i.Quantity);

            // Tạo đơn hàng mới
            var order = new Order
            {
                TotalPrice = (double)totalPrice
            };

            // Hiển thị view thanh toán với đơn hàng
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
        public async Task<IActionResult> PaymentCallBack()
        {
            // Nhận phản hồi từ Momo
            var response = _momoService.PaymentExecuteAsync(HttpContext.Request.Query);

            // Xác nhận thanh toán thành công từ Momo (giả sử ErrorCode = 0 là thanh toán thành công)
            if (response != null)
            {
                // Lấy giỏ hàng từ Session (hoặc lưu trữ khác)
                var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");

                if (cart != null && cart.Items.Any())
                {
                    foreach (var item in cart.Items)
                    {
                        // Tìm sản phẩm trong cơ sở dữ liệu
                        var product = await _context.Products.FindAsync(item.ProductId);

                        if (product != null)
                        {
                            // Giảm số lượng sản phẩm
                            product.Quantity -= item.Quantity;
                        }
                    }

                    // Lưu thay đổi vào cơ sở dữ liệu
                    await _context.SaveChangesAsync();

                    // Xóa giỏ hàng sau khi xử lý thanh toán thành công
                    HttpContext.Session.Remove("Cart");
                }
            }
            else
            {
                // Xử lý khi thanh toán thất bại (ví dụ: hiển thị thông báo lỗi)
                return RedirectToAction("Error", "ShoppingCart");
            }

            // Trả về view xác nhận thanh toán thành công
            return View(response);
        }


    }
}
