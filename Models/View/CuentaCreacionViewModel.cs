using Microsoft.AspNetCore.Mvc.Rendering;

namespace ManejoPresupuesto.Models.View
{
    public class CuentaCreacionViewModel : Cuenta
    {
        public IEnumerable<SelectListItem> TiposCuentas { get; set; }
    }
}
