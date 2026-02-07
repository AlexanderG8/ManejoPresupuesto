using AutoMapper;
using ManejoPresupuesto.Models;
using ManejoPresupuesto.Models.View;
using ManejoPresupuesto.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;

namespace ManejoPresupuesto.Controllers
{
    public class CuentasController : Controller
    {
        private readonly IRepositorioTiposCuentas repositorioTiposCuentas;
        private readonly IServicioUsuario servicioUsuario;
        private readonly IRepositorioCuentas repositorioCuentas;
        private readonly IMapper mapper;

        public CuentasController(IRepositorioTiposCuentas repositorioTiposCuentas, IServicioUsuario servicioUsuario, IRepositorioCuentas repositorioCuentas, IMapper mapper)
        {
            this.repositorioTiposCuentas = repositorioTiposCuentas;
            this.servicioUsuario = servicioUsuario;
            this.repositorioCuentas = repositorioCuentas;
            this.mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            var usuarioId = servicioUsuario.ObtenerUsuarioId();
            var cuentasConTipoCuenta = await repositorioCuentas.Buscar(usuarioId);

            /*
             * Aquí estoy agrupando los valores por Tipo Cuenta
             * - grupo.Key me devuelve el valor del tipo de cuenta
             * - Cuentas me devuelve la lista de cuentas que corresponden a ese tipo de cuenta
             */
            var modelo = cuentasConTipoCuenta.GroupBy(x => x.TipoCuenta).Select(grupo => new IndiceCuentasViewModel
            {
                TipoCuenta = grupo.Key,
                Cuentas = grupo.AsEnumerable()
            }).ToList();
            return View(modelo);
        }

        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            var usuarioId = servicioUsuario.ObtenerUsuarioId();
            var tiposCuentas = await repositorioTiposCuentas.Obtener(usuarioId);
            var modelo = new CuentaCreacionViewModel();
            modelo.TiposCuentas = tiposCuentas.Select(x => new SelectListItem(x.Nombre, x.Id.ToString()));
            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Crear(CuentaCreacionViewModel cuenta)
        {
            var usuarioId = servicioUsuario.ObtenerUsuarioId();
            var tipoCuenta = await ObtenerTiposCuentas(usuarioId);
            // Validar que el tipo de cuenta seleccionado exista para el usuario
            if (tipoCuenta is null) 
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            // Validar el modelo
            if (!ModelState.IsValid)
            {
                cuenta.TiposCuentas = await ObtenerTiposCuentas(usuarioId);
                return View(cuenta);
            }
            // Crear la cuenta
            await repositorioCuentas.Crear(cuenta); 
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Editar(int id)
        {
            var usuarioId = servicioUsuario.ObtenerUsuarioId();
            var cuenta = await repositorioCuentas.ObtenerPorId(id, usuarioId);
            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            //var modelo = new CuentaCreacionViewModel
            //{
            //    Id = cuenta.Id,
            //    Nombre = cuenta.Nombre,
            //    Descripcion = cuenta.Descripcion,
            //    TipoCuentaId = cuenta.TipoCuentaId,
            //    Balance = cuenta.Balance
            //};
            /*
             * Con Autompper me evito de escribir código para asignar cada propiedad del modelo, ya que el mapeo se hace automáticamente
             */
            var modelo = mapper.Map<CuentaCreacionViewModel>(cuenta);
            modelo.TiposCuentas = await ObtenerTiposCuentas(usuarioId);
            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(CuentaCreacionViewModel cuentaEditar) 
        {
            var usuarioId = servicioUsuario.ObtenerUsuarioId();
            var cuenta = await repositorioCuentas.ObtenerPorId(cuentaEditar.Id, usuarioId);
            // Validar que el tipo de cuenta seleccionado exista para el usuario
            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            var tipoCuenta = await repositorioTiposCuentas.ObtenerPorId(cuentaEditar.TipoCuentaId, usuarioId);
            if (tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            // Editar la cuenta
            await repositorioCuentas.Actualizar(cuentaEditar);
            return RedirectToAction("Index");
        }
        [HttpGet]
        public async Task<IActionResult> Borrar(int id) 
        {
                var usuarioId = servicioUsuario.ObtenerUsuarioId();
                var cuenta = await repositorioCuentas.ObtenerPorId(id, usuarioId);
                if (cuenta is null)
                {
                    return RedirectToAction("NoEncontrado", "Home");
                }
                return View(cuenta);
        }

        [HttpPost]
        public async Task<IActionResult> BorrarCuenta(int id)
        {
            var usuarioId = servicioUsuario.ObtenerUsuarioId();
            var cuenta = await repositorioCuentas.ObtenerPorId(id, usuarioId);
            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            await repositorioCuentas.Borrar(id);
            return RedirectToAction("Index");
        }

        private async Task<IEnumerable<SelectListItem>> ObtenerTiposCuentas(int usuarioId)
        {
            var tiposCuentas = await repositorioTiposCuentas.Obtener(usuarioId);
            return tiposCuentas.Select(x => new SelectListItem(x.Nombre, x.Id.ToString()));
        }
    }
}
