using System.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Data.SqlClient;
using SIGEPROS.Services;
using SIGEPROS.Utils;


var builder = WebApplication.CreateBuilder(args);

// Configuración para local con User Secrets
var configuration = new ConfigurationBuilder()
.SetBasePath(Directory.GetCurrentDirectory())
.AddJsonFile("appsettings.json", optional: true)
.AddUserSecrets<Program>()
.AddEnvironmentVariables()
.Build();

builder.Services.AddSingleton<IDbConnection>(sp =>
    new SqlConnection(configuration.GetConnectionString("dbAppSIGEPROS")));

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<SIGEPROS.Utils.NoCacheAttribute>();
});

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddScoped<DBService>();
builder.Services.AddScoped<DBAppService>();
builder.Services.AddScoped<DBProveedorService>();
builder.Services.AddScoped<DBFacturaService>();
builder.Services.AddScoped<DBDespachosService>();
builder.Services.AddScoped<DBUsuarioService>();
builder.Services.AddScoped<DBRolService>();
builder.Services.AddScoped<DBExpedienteService>();
builder.Services.AddScoped<DBSeguridadService>();
builder.Services.AddScoped<DBDocumentosService>();
builder.Services.AddScoped<DBExpedienteService>();
builder.Services.AddScoped<DBExpedienteJobService>();

///builder.Services.AddScoped<PlanillaZdis022Service>();
builder.Services.AddScoped<IPlanillaZdis022Service, PlanillaZdis022Service>();


builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = ".SIGEPROS.Auth";
        options.Cookie.Path = "/";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.LoginPath = "/Home/Index";
        options.AccessDeniedPath = "/Home/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = false;
    });
builder.Services.AddAuthorization();

var app = builder.Build();
EnvironmentHelper.Initialize(app.Configuration);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.Use((context, next) =>
{
    context.Response.Headers["X-Powered-By"] = string.Empty;
    context.Response.Headers["X-AspNet-Version"] = string.Empty;
    context.Response.Headers["X-AspNetMvc-Version"] = string.Empty;
    context.Response.Headers["X-SourceFiles"] = string.Empty;
    context.Response.Headers["Server"] = string.Empty;

    return next();
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.Use(async (context, next) =>
{
    context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate, max-age=0";
    context.Response.Headers["Pragma"] = "no-cache";
    context.Response.Headers["Expires"] = "0";
    await next();
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
