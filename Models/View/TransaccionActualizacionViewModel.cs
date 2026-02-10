namespace ManejoPresupuesto.Models.View
{
    public class TransaccionActualizacionViewModel : TransaccionCreacionViewModel
    {
        public int CuentaAnteriorId { get; set; }
        public decimal MontoAnterior { get; set; }
    }
}
