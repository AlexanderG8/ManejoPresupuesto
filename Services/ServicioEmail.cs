using System.Net;
using System.Net.Mail;

namespace ManejoPresupuesto.Services
{
    public interface IServicioEmail
    {
        Task EnviarEmailCambioPassword(string receptor, string enlace);
    }

    public class ServicioEmail : IServicioEmail
    {
        private readonly IConfiguration configuration;
        public ServicioEmail(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public async Task EnviarEmailCambioPassword(string receptor, string enlace)
        {
            var email = configuration.GetValue<string>("CONFIGURACIONES_EMAIL:EMAIL");
            var password = configuration.GetValue<string>("CONFIGURACIONES_EMAIL:PASSWORD");
            var host = configuration.GetValue<string>("CONFIGURACIONES_EMAIL:HOST");
            var puerto = configuration.GetValue<int>("CONFIGURACIONES_EMAIL:PUERTO");

            var cliente = new SmtpClient(host, puerto);
            // Indicamos que tenemos que utilizar un canal seguro de comunicación
            cliente.EnableSsl = true;
            /*Con esto indico que no utilizaré las credenciales por defecto ya que, enviaré mis credenciales con las variables emailEmisor y password*/
            cliente.UseDefaultCredentials = false;

            cliente.Credentials = new NetworkCredential(email, password);
            var emisor = email;
            var subject = "Cambio de contraseña";
            var contenidoHTML = @$"Saludos,
                                Se ha solicitado un cambio de contraseña para tu cuenta.
                                Para restablecer tu contraseña, haz clic en el siguiente enlace:
                                {enlace}
                                Si no solicitaste este cambio, puedes ignorar este correo electrónico.
                                Atte. Equipo Manejo Presupuesto.";
            var mensaje = new MailMessage(emisor, receptor, subject, contenidoHTML);
            await cliente.SendMailAsync(mensaje);
        }
    }
}
