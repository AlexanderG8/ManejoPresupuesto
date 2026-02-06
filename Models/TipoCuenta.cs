using System.ComponentModel.DataAnnotations;
using ManejoPresupuesto.Models.Validations;
using Microsoft.AspNetCore.Mvc;

namespace ManejoPresupuesto.Models
{
    /*Validación desde el Modelo*/
    //public class TipoCuenta : IValidatableObject
    public class TipoCuenta
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [PrimeraLetraMayuscula]
        [Remote(action: "VerificarExisteTipoCuenta", controller: "TiposCuentas")]
        public string Nombre { get; set; }
        public int UsuarioId { get; set; }
        public int Orden { get; set; }

        /*Validación desde el Modelo*/
        //public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        //{
        //    if (Nombre != null && Nombre.Length > 0)
        //    {
        //        var primeraLetra = Nombre[0].ToString();
        //        if (primeraLetra != primeraLetra.ToUpper())
        //        {
        //            /*Aquí estoy indicando que retorne un mensaje de error en el campo Nombre*/
        //            yield return new ValidationResult("La primera letra debe ser mayúscula", new[] { nameof(Nombre) });

        //            //yield return new ValidationResult("La primera letra debe ser mayúscula");
        //            /*Si en el caso no agrego el campo específico para que se muestre el error, esto lo toma como un error al nivel del modelo.
        //             Los errores a nivel del modelo son muy útiles cuando tienes un error que no tiene que ver exactamente 
        //            con un campo específico, sino que tiene que ver con algún tipo de acción general, como por ejemplo
        //            que quizás el usuario no tenga permiso para hacer una acción o algo por el estilo. */
        //        }
        //    }
        //}
    }
}
