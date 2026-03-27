using DocumentFormat.OpenXml.Presentation;
using ManejoPresupuesto.Models;
using ManejoPresupuesto.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Configuración de la política de autorización para requerir usuarios autenticados
var politicaUsuarioAutenticados = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
// Agregar la política de autorización a nivel global para todas las vistas
builder.Services.AddControllersWithViews(opciones => 
{
    opciones.Filters.Add(new AuthorizeFilter(politicaUsuarioAutenticados));
});

builder.Services.AddControllersWithViews();
builder.Services.AddTransient<IRepositorioTiposCuentas, RepositorioTiposCuentas>();
builder.Services.AddTransient<IServicioUsuario, ServicioUsuarios>();
builder.Services.AddTransient<IRepositorioCuentas, RepositorioCuentas>();
builder.Services.AddTransient<IRepositorioCategorias, RepositorioCategorias>();
builder.Services.AddTransient<IRepositorioTransacciones, RepositorioTransacciones>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<IServicioReportes, ServicioReportes>();
builder.Services.AddTransient<IRepositorioUsuarios, RepositorioUsuarios>();
builder.Services.AddTransient<IUserStore<Usuario>, UsuarioStore>();
builder.Services.AddTransient<SignInManager<Usuario>>();
builder.Services.AddIdentityCore<Usuario>(opciones => 
{
    // True = Si || False = No
    // Require Digit: Indica si se requiere al menos un dígito en la contraseña.
    opciones.Password.RequireDigit = false;
    // RequireLowercase: Indica si se requiere al menos una letra minúscula en la contraseña.
    opciones.Password.RequireLowercase = false;
    // RequireUppercase: Indica si se requiere al menos una letra mayúscula en la contraseña.
    opciones.Password.RequireUppercase = false;
    // RequireNonAlphanumeric: Indica si se requiere al menos un carácter no alfanumérico (como símbolos) en la contraseña.
    opciones.Password.RequireNonAlphanumeric = false;

}).AddErrorDescriber<MensajesDeErrorIdentity>()
.AddDefaultTokenProviders(); // Esto permitirá generar tokens para funcionalidades como restablecimiento de contraseña, confirmación de correo electrónico, etc.

// Configuración de autenticación utilizando cookies
builder.Services.AddAuthentication(options => 
{
    options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
    options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignOutScheme = IdentityConstants.ApplicationScheme;
}).AddCookie(IdentityConstants.ApplicationScheme, opciones => 
{
    opciones.LoginPath = "/Usuarios/Login";
});

// Agregamos servicio de envío de correos electrónicos.
builder.Services.AddTransient<IServicioEmail, ServicioEmail>();

builder.Services.AddAutoMapper(typeof(Program));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Transacciones}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
