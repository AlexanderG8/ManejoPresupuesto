namespace ManejoPresupuesto.Services
{
    public interface IServicioUsuario
    {
        int ObtenerUsuarioId();
    }
    public class ServicioUsuarios : IServicioUsuario
    {
        public int ObtenerUsuarioId()
        {
            return 1;
        }
    }
}
