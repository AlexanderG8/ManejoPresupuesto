using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ManejoPresupuesto.Services
{
    public interface IRepositorioTransacciones
    {
        Task Crear(Transaccion transaccion);
        Task<Transaccion> ObtenerPorID(int id, int usuarioId);
        Task Actualizar(Transaccion transaccion, decimal montoAnterior, int cuentaAnterior);
        Task<IEnumerable<Transaccion>> ObtenerPorCuentaId(ObtenerTransaccionesPorCuenta modelo);
        Task Borrar(int id);
    }
    public class RepositorioTransacciones : IRepositorioTransacciones
    {
        private readonly string connectionString;
        public RepositorioTransacciones(IConfiguration configuration)
        {
            this.connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Crear(Transaccion transaccion) 
        {
            using var connection = new SqlConnection(connectionString);
            var id = await connection.QuerySingleAsync<int>("Transacciones_Insertar", 
                                                            new {transaccion.UsuarioId, 
                                                                transaccion.FechaTransaccion, 
                                                                transaccion.Monto, 
                                                                transaccion.CategoriaId, 
                                                                transaccion.CuentaId, 
                                                                transaccion.Nota}, 
                                                            commandType: System.Data.CommandType.StoredProcedure);
            transaccion.Id = id;
        }

        public async Task Actualizar(Transaccion transaccion, decimal montoAnterior, int cuentaAnteriorId) 
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync("Transacciones_Actualizar", 
                                                            new {transaccion.Id, 
                                                                transaccion.FechaTransaccion, 
                                                                transaccion.Monto, 
                                                                montoAnterior,
                                                                transaccion.CuentaId,
                                                                cuentaAnteriorId,
                                                                transaccion.CategoriaId, 
                                                                transaccion.Nota}, 
                                                            commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<Transaccion>> ObtenerPorCuentaId(ObtenerTransaccionesPorCuenta modelo) 
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Transaccion>(@"SELECT t.Id, t.Monto, t.FechaTransaccion, c.Nombre as Categoria,cu.Nombre as Cuenta, c.TipoOperacionId
                                                              FROM Transacciones t
                                                              INNER JOIN Categorias c ON c.Id = t.CategoriaId
                                                              INNER JOIN Cuentas cu ON cu.Id = t.CuentaId
                                                              WHERE t.CuentaId = @CuentaId AND t.UsuarioId = @UsuarioId
                                                              AND t.FechaTransaccion BETWEEN @FechaInicio AND @FechaFin", modelo);
        }

        public async Task<Transaccion> ObtenerPorID(int id, int usuarioId) 
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<Transaccion>(@"SELECT tra.*, cat.TipoOperacionId 
                                                                            FROM Transacciones tra
                                                                            INNER JOIN Categorias cat ON cat.Id = tra.CategoriaId
                                                                            WHERE tra.Id = @Id and tra.UsuarioId = @UsuarioId", new { id, usuarioId});
        }


        public async Task Borrar(int id) 
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync("Transacciones_Borrar", new { id }, commandType: CommandType.StoredProcedure);
        }
    }
}
