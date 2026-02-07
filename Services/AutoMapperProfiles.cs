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
        }
    }
}
