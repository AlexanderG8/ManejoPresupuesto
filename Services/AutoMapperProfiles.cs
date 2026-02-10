using AutoMapper;
using ManejoPresupuesto.Models;
using ManejoPresupuesto.Models.View;

namespace ManejoPresupuesto.Services
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<Cuenta, CuentaCreacionViewModel>();
            // ReverseMap () permite mapear en ambos sentidos, es decir, de Transacción a TransaccionActualizacionViewModel y viceversa
            CreateMap<TransaccionActualizacionViewModel, Transaccion>().ReverseMap();
        }
    }
}
