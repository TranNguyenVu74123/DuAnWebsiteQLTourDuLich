using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.EntityFrameworkCore;
using Owl.reCAPTCHA;
using WEBSAIGONGLISTEN.Models.Momo;
using WEBSAIGONGLISTEN.Services;
using WEBSAIGONGLISTEN.Models;
using Microsoft.AspNetCore.Authentication.OAuth;
using WEBSAIGONGLISTEN.Repositories;
using Microsoft.AspNetCore.Authentication;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using WEBSAIGONGLISTEN.Services;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
        .AddDefaultTokenProviders()
        .AddDefaultUI()
        .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = $"/Identity/Account/Login";
    options.LogoutPath = $"/Identity/Account/LogoutPath";
    options.LogoutPath = $"/Identity/Account/AccessDenied";
});

builder.Services.AddRazorPages();

builder.Services.Configure<IdentityOptions>(options =>
{
    // C?u h�nh ??ng nh?p.
    options.SignIn.RequireConfirmedEmail = true;            // C?u h�nh x�c th?c ??a ch? email (email ph?i t?n t?i)
    options.SignIn.RequireConfirmedPhoneNumber = false;     // X�c th?c s? ?i?n tho?i
    options.SignIn.RequireConfirmedAccount = true;
});

// ??t tr??c AddControllersWithViews();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddScoped<IProductRepository, EFProductRepository>();
builder.Services.AddScoped<ICategoryRepository, EFCategoryRepository>();
// Trong ph??ng th?c ConfigureServices c?a class Startup
builder.Services.AddScoped<IOrderRepository, EFOrderRepository>();
builder.Services.AddScoped<UserManager<ApplicationUser>>();
builder.Services.AddScoped<SignInManager<ApplicationUser>>();
builder.Services.Configure<MomoOptionModel>(builder.Configuration.GetSection("MomoAPI"));
builder.Services.AddScoped<IMomoService, MomoService>();

builder.Services.AddTransient<IEmailService, EmailService>();

builder.Services.AddTransient<IEmailSender, SendMailService>();

builder.Services.AddOptions();
var mailSettings = builder.Configuration.GetSection("MailSettings");
builder.Services.Configure<MailSettings>(mailSettings);

//builder.Services.AddTransient<IMailService, MailService>(serviceProvider =>
//{
//    var mailjetSettings = serviceProvider.GetRequiredService<IOptions<MailjetSettings>>().Value;
//    var logger = serviceProvider.GetRequiredService<ILogger<MailService>>();
//    return new MailService(mailjetSettings.ApiKey, mailjetSettings.ApiSecret, logger);
//});


builder.Services.AddreCAPTCHAV2(x =>
{
    x.SiteKey = "6LcorbkpAAAAAOSZZ5kTimR5bsbRmU4TeErp84bO";
    x.SiteSecret = "6LcorbkpAAAAAC5oGihIiVHY50_IsXRSnxrQUnHA";
});

builder.Services.AddAuthentication()
   .AddGoogle(options =>
   {
       IConfigurationSection googleAuthNSection = config.GetSection("Authentication:Google");
       options.ClientId = googleAuthNSection["ClientId"];
       options.ClientSecret = googleAuthNSection["ClientSecret"];
   })
   .AddFacebook(options =>
   {
       IConfigurationSection FBAuthNSection = config.GetSection("Authentication:Facebook");
       options.AppId = FBAuthNSection["AppId"];
       options.AppSecret = FBAuthNSection["AppSecret"];
   })
   .AddOAuth("GitHub", options =>
   {
       options.ClientId = "Ov23liATUhZcWZF5qYqD";
       options.ClientSecret = "228c0278b963e474e7ae35822167593de9a02603";
       options.CallbackPath = new PathString("/signin-github");

       options.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
       options.TokenEndpoint = "https://github.com/login/oauth/access_token";
       options.UserInformationEndpoint = "https://api.github.com/user";

       options.Scope.Add("user:email");

       options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
       options.ClaimActions.MapJsonKey(ClaimTypes.Name, "login");
       options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");

       options.SaveTokens = true;

       options.Events = new OAuthEvents
       {
           OnCreatingTicket = async context =>
           {
               var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
               request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
               request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);

               var response = await context.Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
               response.EnsureSuccessStatusCode();

               var user = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

               context.RunClaimActions(user.RootElement);
           }
       };
   });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseSession();
// ??t tr??c UseRouting



app.UseRouting();

app.UseAuthentication(); ;
app.UseAuthorization();


//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Home}/{action=Index}/{id?}");


//app.UseEndpoints(endpoints =>
//{
//    endpoints.MapControllerRoute(
//      name: "Admin",
//      pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
//    endpoints.MapControllerRoute(
//        name: "Company",
//        pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
//    endpoints.MapControllerRoute(
//      name: "default",
//      pattern: "{controller=Home}/{action=Index}/{id?}");
//    endpoints.MapControllerRoute(
//       name: "search",
//       pattern: "product/search",
//       defaults: new { controller = "Product", action = "Search" },
//       constraints: new { httpMethod = new HttpMethodRouteConstraint("POST") }
//   );
//});

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "AdminArea",
        pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
    );

    endpoints.MapControllerRoute(
        name: "AdminProductSearch",
        pattern: "{area:exists}/{controller=Product}/search",  // ???ng d?n ??n action "Search"
        defaults: new { action = "Search" },
        constraints: new { area = "Admin", httpMethod = new HttpMethodRouteConstraint("POST") }
    );

    endpoints.MapControllerRoute(
       name: "AdminCategorySearch",
       pattern: "{area:exists}/{controller=Product}/searchcategory",  // ???ng d?n ??n action "SearchCategory"
       defaults: new { controller = "Product", action = "SearchCategory" },  // Th�m action "SearchCategory" ? ?�y
       constraints: new { area = "Admin", httpMethod = new HttpMethodRouteConstraint("POST") }
   );

    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}"
    );

});



app.MapRazorPages();
app.Run();