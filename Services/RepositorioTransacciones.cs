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
