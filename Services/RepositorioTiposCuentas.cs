using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Services
{
    public interface IRepositorioTiposCuentas
    {
        Task Crear(TipoCuenta tipoCuenta);
        Task<bool> Existe (string nombre, int usuarioId);
        Task<IEnumerable<TipoCuenta>> Obtener(int usuarioId);
        Task Actualizar(TipoCuenta tipoCuenta);
        Task<TipoCuenta> ObtenerPorId(int id, int usuarioId);
        Task Borrar(int id);
    }
    public class RepositorioTiposCuentas : IRepositorioTiposCuentas
    {
        private readonly string connectionString;
        public RepositorioTiposCuentas(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Crear(TipoCuenta tipoCuenta)
        {
            using var connection = new SqlConnection(connectionString);
            /*QuerySingleAsync => Permite hacer un post */
            var id = await connection.QuerySingleAsync<int>(
                @"INSERT INTO TiposCuentas (Nombre, UsuarioId, Orden)
                  VALUES (@Nombre, @UsuarioId, @Orden);
                  SELECT SCOPE_IDENTITY();",
                tipoCuenta);
            tipoCuenta.Id = id;
        }

        public async Task<bool> Existe(string nombre, int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            /*QueryFirstOrDefaultAsync => Permite hacer una consulta y me traer el primer resultado o el resultado por default*/
            var existe = await connection.QueryFirstOrDefaultAsync<int>(
                @"SELECT 1 FROM TiposCuentas WHERE Nombre = @Nombre AND UsuarioId = @UsuarioId;",
                new { nombre, usuarioId });
            /*Si existe retornará 1 entonces sera True sino sera False*/
            return existe == 1;
        }

        public async Task<IEnumerable<TipoCuenta>> Obtener(int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            /*QueryAsync => Permite hacer una consulta*/
            return await connection.QueryAsync<TipoCuenta>(@"SELECT Id, Nombre, Orden FROM TiposCuentas WHERE UsuarioId = @UsuarioId", new { usuarioId });
        }

        public async Task Actualizar(TipoCuenta tipoCuenta)
        {
            using var connection = new SqlConnection(connectionString);
            /*ExecuteAsync => Te permite ejecutar una query sin necesidad de esperar un valor*/
            await connection.ExecuteAsync(
                @"UPDATE TiposCuentas SET Nombre = @Nombre
                  WHERE Id = @Id AND UsuarioId = @UsuarioId;",
                tipoCuenta);
        }

        public async Task<TipoCuenta> ObtenerPorId(int id, int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            /*QueryFirstOrDefaultAsync => Permite hacer una consulta y me traer el primer resultado o el resultado por default*/
            return await connection.QueryFirstOrDefaultAsync<TipoCuenta>(
                @"SELECT Id, Nombre, Orden FROM TiposCuentas WHERE Id = @Id AND UsuarioId = @UsuarioId;",
                new { id, usuarioId });
        }

        public async Task Borrar(int id) 
        {
            using var connection = new SqlConnection(connectionString);
            /*ExecuteAsync => Te permite ejecutar una query sin necesidad de esperar un valor*/
            await connection.ExecuteAsync(
                @"DELETE FROM TiposCuentas WHERE Id = @Id;",
                new { id });

        }
    }
}
