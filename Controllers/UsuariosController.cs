using System.Text;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Spreadsheet;
using ManejoPresupuesto.Models;
using ManejoPresupuesto.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace ManejoPresupuesto.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly UserManager<Usuario> userManager;
        private readonly SignInManager<Usuario> signInManager;
        private readonly IServicioEmail servicioEmail;

        public UsuariosController(UserManager<Usuario> userManager, SignInManager<Usuario> signInManager, IServicioEmail servicioEmail)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.servicioEmail = servicioEmail;
        }
        public IActionResult Index()
        {
            return View();
        }
        [AllowAnonymous]
        public async Task<IActionResult> Registro(RegistroViewModel modelo)
        {
            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            var usuario = new Usuario { Email = modelo.Email };

            var resultado = await userManager.CreateAsync(usuario, password: modelo.Password);

            if (resultado.Succeeded)
            {
                // Esto indica que el usuario se ha creado correctamente, por lo que se inicia sesión automáticamente
                await signInManager.SignInAsync(usuario, isPersistent: true);
                return RedirectToAction("Index", "Transacciones");
            }
            else
            {
                foreach (var error in resultado.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View(modelo);
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel modelo)
        {
            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            // Esto indica que el usuario se ha creado correctamente, por lo que se inicia sesión automáticamente
            // Ahora el lockoutOnFailure: false indica que no se bloqueará la cuenta después de varios intentos fallidos de inicio de sesión
            var resultado = await signInManager.PasswordSignInAsync(modelo.Email, modelo.Password, modelo.Recuerdame, lockoutOnFailure: false);

            if (resultado.Succeeded)
            {
                return RedirectToAction("Index", "Transacciones");
            }
            else
            {
                ModelState.AddModelError(String.Empty, "Nombre de usuario o password incorrecto");
                return View(modelo);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            return RedirectToAction("Login");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult OlvideMiPassword(string mensaje = "")
        {
            ViewBag.Mensaje = mensaje;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> OlvideMiPassword(OlvideMiPasswordViewModel modelo)
        {
            // Este mensaje se muestra para evitar que un atacante
            // pueda averiguar si un email dado se corresponde
            // con uno de nuestros usuarios o no, ya que el mensaje es el mismo en ambos casos

            if (modelo.Email is null) 
            {
                return RedirectToAction("OlvideMiPassword", new { mensaje = "Favor de ingresar su email" });
            }

            var mensaje = "Proceso concluido. Si el email dado se corresponde con uno de nuestros usuarios, entonces podrá encontrar las instrucciones para recuperar su cuenta en la bandeja de entrada de dicho email.";
            ViewBag.Mensaje = mensaje;
            ModelState.Clear();

            var usuario = await userManager.FindByEmailAsync(modelo.Email);

            if (usuario is null)
            {
                return View();
            }

            // El código que se genera a continuación es un código
            // de un solo uso, que se asocia a un usuario específico y que tiene una duración limitada.
            var codigo = await userManager.GeneratePasswordResetTokenAsync(usuario);
            // Para que el código pueda ser enviado a través de una URL, es necesario codificarlo en Base64
            var codigoBase64 = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(codigo));
            // El enlace que se genera a continuación es el enlace que el usuario deberá pulsar para recuperar su contraseña.
            // En dicho enlace se incluye el código generado anteriormente como un parámetro de la URL
            var enlace = Url.Action("RecuperarPassword", "Usuarios", new { codigo = codigoBase64 }, protocol: Request.Scheme);
            // Enviamos el correo electrónico al usuario con el enlace para recuperar su contraseña
            await servicioEmail.EnviarEmailCambioPassword(modelo.Email, enlace);

            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult RecuperarPassword(string codigo = null) 
        {
            if (codigo is null) 
            {
                var mensaje = "Código no encontrado";
                return RedirectToAction("OlvideMiPassword", new { mensaje });
            }

            var modelo = new RecuperarPasswordViewModel();
            modelo.CodigoReseteo = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(codigo));

            return View(modelo);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> RecuperarPassword(RecuperarPasswordViewModel modelo) 
        {
            var usuario = await userManager.FindByEmailAsync(modelo.Email);
            if (usuario is null) 
            {
                return RedirectToAction("PasswordCambiado");
            }

            var resultado = await userManager.ResetPasswordAsync(usuario, modelo.CodigoReseteo, modelo.Password);

            return RedirectToAction("PasswordCambiado");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult PasswordCambiado() 
        {
            return View();
        }


    }
}
