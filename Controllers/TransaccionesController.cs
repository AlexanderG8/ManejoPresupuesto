using AutoMapper;
using ClosedXML.Excel;
using ManejoPresupuesto.Models;
using ManejoPresupuesto.Models.View;
using ManejoPresupuesto.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;
using System.Reflection;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ManejoPresupuesto.Controllers
{
    public class TransaccionesController : Controller
    {
        private readonly IServicioUsuario servicioUsuario;
        private readonly IRepositorioCuentas repositorioCuentas;
        private readonly IRepositorioCategorias repositorioCategorias;
        private readonly IRepositorioTransacciones repositorioTransacciones;
        private readonly IMapper mapper;
        private readonly IServicioReportes servicioReportes;

        public TransaccionesController(IServicioUsuario servicioUsuario, 
                                       IRepositorioCuentas repositorioCuentas, 
                                       IRepositorioCategorias repositorioCategorias, 
                                       IRepositorioTransacciones repositorioTransacciones,
                                       IMapper mapper,
                                       IServicioReportes servicioReportes)
        {
            this.servicioUsuario = servicioUsuario;
            this.repositorioCuentas = repositorioCuentas;
            this.repositorioCategorias = repositorioCategorias;
            this.repositorioTransacciones = repositorioTransacciones;
            this.mapper = mapper;
            this.servicioReportes = servicioReportes;
        }
        public async Task<IActionResult> Index(int mes, int ano)
        {
            var usuarioId = servicioUsuario.ObtenerUsuarioId();
            var modelo = await servicioReportes.ObtenerReporteTransaccionesDetalladas(usuarioId, mes, ano, ViewBag);

            return View(modelo);
        }

        public async Task<IActionResult> Semanal(int mes, int ano) 
        {
            var usuarioId = servicioUsuario.ObtenerUsuarioId();
            IEnumerable<ResultadoObtenerPorSemana> transaccionesPorSemana = await servicioReportes.ObtenerReporteSemanal(usuarioId, mes, ano, ViewBag);

            var agrupado = transaccionesPorSemana.GroupBy(x => x.Semana).Select(x =>
            new ResultadoObtenerPorSemana()
            {
                Semana = x.Key,
                Ingresos = x.Where(x => x.TipoOperacionId == TipoOperacion.Ingreso).Select(x => x.Monto).FirstOrDefault(),
                Gastos = x.Where(x => x.TipoOperacionId == TipoOperacion.Gasto).Select(x => x.Monto).FirstOrDefault()
            }).ToList();

            if (ano == 0 || mes == 0) 
            {
                var hoy = DateTime.Today;
                ano = hoy.Year;
                mes = hoy.Month;
            }

            var fechaReferencia = new DateTime(ano, mes, 1);
            // Obtener el número de días del mes para poder segmentar las semanas correctamente. Se obtiene el último día del mes restando un día al primer día del siguiente mes.
            var diasDelMes = Enumerable.Range(1, fechaReferencia.AddMonths(1).AddDays(-1).Day);
            // Segmentar los días del mes en grupos de 7 para representar las semanas. Esto es útil para mostrar el reporte semanal con los días correspondientes a cada semana.
            // El metodo chunk es una extensión de Linq que permite segmentar una colección en grupos de un tamaño específico.
            // En este caso, se segmentan los días del mes en grupos de 7 para representar las semanas.
            var diasSegmentados = diasDelMes.Chunk(7).ToList();

            for (int i = 0; i < diasSegmentados.Count; i++)
            {
                var semana = i + 1;
                var fechaInicio = new DateTime(ano, mes, diasSegmentados[i].First());
                var fechaFin = new DateTime(ano, mes, diasSegmentados[i].Last());
                var grupoSemana = agrupado.FirstOrDefault(x => x.Semana == semana);

                if (grupoSemana is null)
                {
                    agrupado.Add(new ResultadoObtenerPorSemana
                    {
                        Semana = semana,
                        FechaInicio = fechaInicio,
                        FechaFin = fechaFin
                    });
                }
                else 
                {
                    grupoSemana.FechaInicio = fechaInicio;
                    grupoSemana.FechaFin = fechaFin;
                }
            }

            agrupado = agrupado.OrderByDescending(x => x.Semana).ToList();

            var modelo = new ReporteSemanalViewModel();
            modelo.TransaccionesPorSemana = agrupado;
            modelo.FechaReferencia = fechaReferencia;

            return View(modelo);
        }
        public async Task<IActionResult> Mensual(int ano)
        {
            var usuarioId = servicioUsuario.ObtenerUsuarioId();
            if (ano == 0) 
            {
                ano = DateTime.Today.Year;
            }

            var transaccionesPorMes = await repositorioTransacciones.ObtenerPorMes(usuarioId, ano);

            var transaccionesAgrupadas = transaccionesPorMes.GroupBy(x => x.Mes).Select(x => new ResultadoObtenerPorMes()
            {
                Mes = x.Key,
                Ingreso = x.Where(x => x.TipoOperacionId == TipoOperacion.Ingreso).Select(x => x.Monto).FirstOrDefault(),
                Gasto = x.Where(x => x.TipoOperacionId == TipoOperacion.Gasto).Select(x => x.Monto).FirstOrDefault(),
            }).ToList();

            for (int mes = 1; mes <= 12; mes++) 
            {
                var transaccion = transaccionesAgrupadas.FirstOrDefault(x => x.Mes == mes);
                var fechaReferencia = new DateTime(ano, mes, 1);

                if (transaccion is null) 
                {
                    transaccionesAgrupadas.Add(new ResultadoObtenerPorMes 
                    {
                        Mes = mes,
                        FechaReferencia = fechaReferencia
                    });
                }
                else
                {
                    transaccion.FechaReferencia = fechaReferencia;
                }
            }

            transaccionesAgrupadas = transaccionesAgrupadas.OrderByDescending(x => x.Mes).ToList();

            var modelo = new ReporteMensualViewModel();
            modelo.Ano = ano;
            modelo.TransaccionesPorMes = transaccionesAgrupadas;

            return View(modelo);
        }
        public IActionResult ExcelReporte()
        {
            return View();
        }

        [HttpGet]
        public async Task<FileResult> ExportarExcelPorMes(int mes, int ano) 
        {
            var fechaInicio = new DateTime(ano, mes, 1);
            var fechaFin = fechaInicio.AddMonths(1).AddDays(-1);
            var usuarioId = servicioUsuario.ObtenerUsuarioId();

            var transacciones = await repositorioTransacciones.ObtenerPorUsuarioId(new ParametroObtenerTransaccionesPorUsuario
            {
                UsuarioId = usuarioId,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin
            });

            var nombreArchivo = $"Manejo Presupuesto - {fechaInicio.ToString("MMMM yyyy")}.xlsx";

            return GenerarExcel(nombreArchivo, transacciones);
        }

        [HttpGet]
        public async Task<FileResult> ExportarExcelPorAno(int ano)
        {
            var fechaInicio = new DateTime(ano, 1, 1);
            var fechaFin = fechaInicio.AddYears(1).AddDays(-1);
            var usuarioId = servicioUsuario.ObtenerUsuarioId();

            var transacciones = await repositorioTransacciones.ObtenerPorUsuarioId(new ParametroObtenerTransaccionesPorUsuario
            {
                UsuarioId = usuarioId,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin
            });

            var nombreArchivo = $"Manejo Presupuesto - {fechaInicio.ToString("yyyy")}.xlsx";

            return GenerarExcel(nombreArchivo, transacciones);
        }

        [HttpGet]
        public async Task<FileResult> ExportarExcelTodo()
        {
            var fechaInicio = DateTime.Today.AddYears(-100);
            var fechaFin = DateTime.Today.AddYears(1000);
            var usuarioId = servicioUsuario.ObtenerUsuarioId();

            var transacciones = await repositorioTransacciones.ObtenerPorUsuarioId(new ParametroObtenerTransaccionesPorUsuario
            {
                UsuarioId = usuarioId,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin
            });

            var nombreArchivo = $"Manejo Presupuesto - {DateTime.Today.ToString("dd-MM-yyyy")}.xlsx";

            return GenerarExcel(nombreArchivo, transacciones);
        }

        private FileResult GenerarExcel(string nombreArchivo, IEnumerable<Transaccion> transacciones) 
        {
            // Creamos un DataTable para almacenar las transacciones que se exportarán a Excel.
            // El DataTable es una estructura de datos en memoria que representa una tabla con filas y columnas,
            // similar a una hoja de cálculo de Excel. Se le asigna el nombre "Transacciones" y
            // se definen las columnas que se incluirán en el reporte: Fecha, Cuenta, Categoria, Nota, Monto e Ingreso/Gasto.
            DataTable dataTable = new DataTable("Transacciones");
            dataTable.Columns.AddRange(new DataColumn[] 
            {
                new DataColumn("Fecha"),
                new DataColumn("Cuenta"),
                new DataColumn("Categoria"),
                new DataColumn("Nota"),
                new DataColumn("Monto"),
                new DataColumn("Ingreso/Gasto")
            });
            // Se recorren las transacciones obtenidas de la base de datos y se agregan al DataTable como filas.
            // Cada fila representa una transacción con sus respectivas columnas.
            foreach (var transaccion in transacciones) 
            {
                dataTable.Rows.Add(transaccion.FechaTransaccion,
                    transaccion.Cuenta,
                    transaccion.Categoria,
                    transaccion.Nota,
                    transaccion.Monto,
                    transaccion.TipoOperacionId);
            }
            // Se utiliza la biblioteca ClosedXML para generar el archivo Excel a partir del DataTable.
            using (XLWorkbook wb = new XLWorkbook()) 
            {
                // Se agrega el DataTable como una nueva hoja de cálculo en el libro de Excel.
                wb.Worksheets.Add(dataTable);
                // Se guarda el libro de Excel en un MemoryStream para luego convertirlo a un arreglo de bytes que se retornará como un archivo descargable.
                using (MemoryStream stream = new MemoryStream()) 
                {
                    // Se guarda el libro de Excel en el MemoryStream.
                    wb.SaveAs(stream);
                    // Se retorna el archivo Excel como un FileResult, especificando el tipo de contenido y el nombre del archivo.
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", nombreArchivo);
                }
            }
        }

        public IActionResult Calendario()
        {
            return View();
        }

        public async Task<JsonResult> ObtenerTransaccionesCalendario(DateTime start, DateTime end) 
        {
            var usuarioId = servicioUsuario.ObtenerUsuarioId();
            var transacciones = await repositorioTransacciones.ObtenerPorUsuarioId(new ParametroObtenerTransaccionesPorUsuario
            {
                UsuarioId = usuarioId,
                FechaInicio = start,
                FechaFin = end
            });

            var eventosCalendario = transacciones.Select(transaccion => new EventoCalendario()
            {
                Title = transaccion.Monto.ToString("N"),
                Start = transaccion.FechaTransaccion.ToString("yyyy-MM-dd"),
                End = transaccion.FechaTransaccion.ToString("yyyy-MM-dd"),
                Color = transaccion.TipoOperacionId == TipoOperacion.Ingreso ? "green" : "red"
            });

            return Json(eventosCalendario);
        }

        public async Task<JsonResult> ObtenerTransaccionesPorFecha(DateTime fecha) 
        {
            var usuarioId = servicioUsuario.ObtenerUsuarioId();
            var transacciones = await repositorioTransacciones.ObtenerPorUsuarioId(new ParametroObtenerTransaccionesPorUsuario
            {
                UsuarioId = usuarioId,
                FechaInicio = fecha,
                FechaFin = fecha
            });

            return Json(transacciones);
        }

        public async Task<IActionResult> Crear()
        {
            var usuarioId = servicioUsuario.ObtenerUsuarioId();
            var modelo = new TransaccionCreacionViewModel();
            modelo.Cuentas = await ObtenerCuentas(usuarioId);
            modelo.Categorias = await ObtenerCategorias(usuarioId, modelo.TipoOperacionId);
            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Crear(TransaccionCreacionViewModel modelo)
        {
            var usuarioId = servicioUsuario.ObtenerUsuarioId();
            if (!ModelState.IsValid) 
            {
                modelo.Cuentas = await ObtenerCuentas(usuarioId);
                modelo.Categorias = await ObtenerCategorias(usuarioId, modelo.TipoOperacionId);
                return View(modelo);
            }

            var cuenta = await repositorioCuentas.ObtenerPorId(modelo.CuentaId, usuarioId);
            if (cuenta is null) 
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            var categoria = await repositorioCategorias.ObtenerPorId(modelo.CategoriaId, usuarioId);
            if (categoria is null) 
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            modelo.UsuarioId = usuarioId;

            if (modelo.TipoOperacionId == TipoOperacion.Gasto) 
            {
                modelo.Monto *= -1;
            }

            await repositorioTransacciones.Crear(modelo);
            return RedirectToAction("Index");
        }
        [HttpGet]
        public async Task<IActionResult> Editar(int id, string urlRetorno = null)
        {
            var usuarioId = servicioUsuario.ObtenerUsuarioId();
            var transaccion = await repositorioTransacciones.ObtenerPorID(id, usuarioId);
            if (transaccion is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            var modelo = mapper.Map<TransaccionActualizacionViewModel>(transaccion);
            // En caso sea ingreso el monto será un valor positivo 
            modelo.MontoAnterior = modelo.Monto;
            // En caso sea un gasto el monto se guarda como negativo en la base de datos.
            if (modelo.TipoOperacionId == TipoOperacion.Gasto) 
            {
                modelo.MontoAnterior = modelo.Monto * -1;
            }

            modelo.CuentaAnteriorId = transaccion.CuentaId;
            modelo.Categorias = await ObtenerCategorias(usuarioId, transaccion.TipoOperacionId);
            modelo.Cuentas = await ObtenerCuentas(usuarioId);
            modelo.UrlRetorno = urlRetorno;
            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(TransaccionActualizacionViewModel modelo)
        {
            var usuarioId = servicioUsuario.ObtenerUsuarioId();
            // Validación del modelo. Si no es válido, se vuelven a cargar las cuentas y categorías para mostrar el formulario con los datos ingresados por el usuario.
            if (!ModelState.IsValid)
            {
                modelo.Cuentas = await ObtenerCuentas(usuarioId);
                modelo.Categorias = await ObtenerCategorias(usuarioId, modelo.TipoOperacionId);
                return View(modelo);
            }
            // Validación de que la cuenta y categoría seleccionadas por el usuario existan en la base de datos y pertenezcan al usuario. Si no es así, se redirige a una página de error.
            var cuenta = await repositorioCuentas.ObtenerPorId(modelo.CuentaId, usuarioId);
            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            // Validación de que la categoría seleccionada por el usuario exista en la base de datos y pertenezca al usuario. Si no es así, se redirige a una página de error.
            var categoria = await repositorioCategorias.ObtenerPorId(modelo.CategoriaId, usuarioId);
            if (categoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            // Mapear el modelo de vista a la entidad Transaccion para poder actualizarla en la base de datos.
            var transaccion = mapper.Map<Transaccion>(modelo);

            // Si es un gasto , el monto se guarda como negativo en la base de datos, por lo que al actualizarlo, si el tipo de operación es gasto, se multiplica por -1 para mantener esa lógica.
            if (modelo.TipoOperacionId == TipoOperacion.Gasto)
            {
                modelo.Monto *= -1;
            }
            // Actualizar la transacción en la base de datos. Se le pasan el monto anterior y el id de la cuenta anterior para que el repositorio pueda ajustar los saldos de las cuentas correctamente en caso de que el usuario haya cambiado el monto
            await repositorioTransacciones.Actualizar(transaccion, modelo.MontoAnterior, modelo.CuentaAnteriorId);

            if (string.IsNullOrEmpty(modelo.UrlRetorno))
            {
                return RedirectToAction("Index");
            }
            else 
            {
                return LocalRedirect(modelo.UrlRetorno);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Borrar(int id, string urlRetorno = null) 
        {
            var usuarioId = servicioUsuario.ObtenerUsuarioId();
            var transaccion = await repositorioTransacciones.ObtenerPorID(id, usuarioId);
            if (transaccion is null) 
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            await repositorioTransacciones.Borrar(id);
            if (string.IsNullOrEmpty(urlRetorno))
            {
                return RedirectToAction("Index");
            }
            else
            {
                return LocalRedirect(urlRetorno);
            }
        }

        private async Task<IEnumerable<SelectListItem>> ObtenerCuentas(int usuarioId)
        {
            var cuentasUsuario = await repositorioCuentas.Buscar(usuarioId);
            return cuentasUsuario.Select(x => new SelectListItem(x.Nombre, x.Id.ToString()));
        }

        private async Task<IEnumerable<SelectListItem>> ObtenerCategorias(int usuarioId, TipoOperacion tipoOperacion) 
        {
            var categorias = await repositorioCategorias.Obtener(usuarioId, tipoOperacion);
            // Se crea una lista de SelectListItem a partir de las categorías obtenidas de la base de datos.
            // Cada SelectListItem representa una categoría con su nombre como texto y su id como valor.
            // Y con Tolist convierte en una lista para poder agregar un elemento por defecto
            // al inicio de la lista que permita al usuario seleccionar una categoría.
            var resultado = categorias.Select(x => new SelectListItem(x.Nombre, x.Id.ToString())).ToList();
            // Se crea un SelectListItem que representa la opción por defecto para seleccionar una categoría.
            // Esta opción tiene el texto "-- Seleccionar Categoría --", un valor de "0" y se marca como seleccionada por defecto.
            var opcionPorDefecto = new SelectListItem("-- Selecionar Categoría --", "0", true);
            // Se inserta la opción por defecto al inicio de la lista de categorías para
            // que sea la primera opción que el usuario vea al desplegar el menú de selección de categorías.
            resultado.Insert(0,opcionPorDefecto);
            return resultado;
        }

        [HttpPost]
        public async Task<IActionResult> ObtenerCategorias([FromBody] TipoOperacion tipoOperacion) 
        {
            var usuarioId = servicioUsuario.ObtenerUsuarioId();
            var categorias = await ObtenerCategorias(usuarioId, tipoOperacion);
            return Ok(categorias);
        }
    }
}
