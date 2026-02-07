using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Services
{
    public interface IRepositorioCuentas
    {
        Task Actualizar(Cuenta cuenta);
        Task Borrar(int id);
        Task<IEnumerable<Cuenta>> Buscar(int usuarioId);
        Task Crear(Cuenta cuenta);
        Task<Cuenta> ObtenerPorId(int id, int usuarioId);
    }
    public class RepositorioCuentas : IRepositorioCuentas
    {
        private readonly string connectionString;
        public RepositorioCuentas(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        public async Task<IEnumerable<Cuenta>> Buscar(int usuarioId) 
        {
            using var connection = new SqlConnection(connectionString);
            var cuentas = await connection.QueryAsync<Cuenta>(@"SELECT C.Id, C.Nombre, TC.Nombre as TipoCuenta, C.Balance
                                                                FROM Cuentas C
                                                                INNER JOIN TiposCuentas TC ON TC.Id = C.TipoCuentaId
                                                                WHERE TC.UsuarioId = @UsuarioId", new { usuarioId });
            return cuentas;
        }
        public async Task Crear(Cuenta cuenta)
        {
            using var connection = new SqlConnection(connectionString);
            /*QuerySingleAsync => Permite hacer un post */
            var id = await connection.QuerySingleAsync<int>(@"INSERT INTO Cuentas (Nombre, TipoCuentaId, Descripcion, Balance)
                                                         VALUES (@Nombre, @TipoCuentaId, @Descripcion, @Balance);
                                                         SELECT SCOPE_IDENTITY();", cuenta);
            cuenta.Id = id;
        }

        public async Task<Cuenta> ObtenerPorId(int id, int usuarioId) 
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<Cuenta>(@"SELECT C.Id, C.Nombre, C.Balance, C.Descripcion, C.TipoCuentaId
                                                                       FROM Cuentas C
                                                                       INNER JOIN TiposCuentas TC ON TC.Id = C.TipoCuentaId
                                                                       WHERE TC.UsuarioId = @UsuarioId AND C.Id = @Id", new { usuarioId, id});
        }

        public async Task Actualizar(Cuenta cuenta)
        {
            using var connection = new SqlConnection(connectionString);
            /*ExecuteAsync => Realiza la ejecución de la query sin esperar ningun respuesta */
            var id = await connection.ExecuteAsync(@"UPDATE Cuentas
                                                     SET Nombre = @Nombre, 
                                                         Balance = @Balance, 
                                                         Descripcion = @Descripcion, 
                                                         TipoCuentaId = @TipoCuentaId
                                                     WHERE Id = @Id", cuenta);
        }

        public async Task Borrar(int id) 
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(@"DELETE Cuentas WHERE Id = @Id", new { id });
        }
    }
}
