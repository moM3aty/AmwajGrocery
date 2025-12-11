using AmwajGrocery.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
builder.Services.AddLocalization();
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Admin/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(1);
    });

var app = builder.Build();


var arabicCulture = new CultureInfo("ar");
arabicCulture.NumberFormat.NumberDecimalSeparator = ".";
arabicCulture.NumberFormat.CurrencyDecimalSeparator = ".";

var supportedCultures = new[] { arabicCulture, new CultureInfo("en") };

var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(arabicCulture),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
};

localizationOptions.RequestCultureProviders.Clear();
localizationOptions.RequestCultureProviders.Add(new QueryStringRequestCultureProvider());
localizationOptions.RequestCultureProviders.Add(new CookieRequestCultureProvider());

app.UseRequestLocalization(localizationOptions);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();