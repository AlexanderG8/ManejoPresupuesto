using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Services
{
    public interface IRepositorioCategorias
    {
        Task Crear(Categoria categoria);
        Task<IEnumerable<Categoria>> Obtener(int usuarioId, PaginacionViewModel paginacion);
        Task<IEnumerable<Categoria>> Obtener(int usuarioId, TipoOperacion tipoOperacionId);
        Task<Categoria> ObtenerPorId(int id, int usuarioId);
        Task Actualizar(Categoria categoria);
        Task Borrar(int id);
        Task<int> Contar(int usuarioId);
    }
    public class RepositorioCategorias : IRepositorioCategorias
    {
        private readonly string connectionString;
        public RepositorioCategorias(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Crear(Categoria categoria)
        {
            using var connection = new SqlConnection(connectionString);
            var id = await connection.QuerySingleAsync<int>(
                @"INSERT INTO Categorias (Nombre, TipoOperacionId, UsuarioId)
                  VALUES (@Nombre, @TipoOperacionId, @UsuarioId);
                  SELECT SCOPE_IDENTITY();", categoria);
            categoria.Id = id;
        }

        public async Task<IEnumerable<Categoria>> Obtener(int usuarioId, PaginacionViewModel paginacion)
        {
            using var connection = new SqlConnection(connectionString);
            // OFFSET y FETCH NEXT se utilizan para paginar los resultados.
            // OFFSET indica cuántos registros se deben saltar, y
            // FETCH NEXT indica cuántos registros se deben recuperar después de saltar los anteriores.
            return await connection.QueryAsync<Categoria>(
                @$"SELECT *
                  FROM Categorias
                  WHERE UsuarioId = @UsuarioId
                  ORDER BY Nombre
                  OFFSET {paginacion.RecordsASaltar} 
                  ROWS FETCH NEXT {paginacion.RecordsPorPagina} 
                  ROWS ONLY", new { usuarioId });
        }

        public async Task<int> Contar(int usuarioId) 
        {
            using var connection = new SqlConnection(connectionString);
            // ExecuteScalarAsync se utiliza para ejecutar una consulta que devuelve un solo valor,
            // en este caso el conteo de categorías para un usuario específico.
            return await connection.ExecuteScalarAsync<int>(
                @"SELECT COUNT(*) 
                  FROM Categorias
                  WHERE UsuarioId = @UsuarioId", new { usuarioId });
        }

        public async Task<IEnumerable<Categoria>> Obtener(int usuarioId, TipoOperacion tipoOperacionId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Categoria>(
                @"SELECT *
                  FROM Categorias
                  WHERE UsuarioId = @UsuarioId AND TipoOperacionId = @tipoOperacionId", new { usuarioId, tipoOperacionId });
        }

        public async Task<Categoria> ObtenerPorId(int id, int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<Categoria>(
                @"SELECT *
                  FROM Categorias
                  WHERE Id = @Id AND UsuarioId = @UsuarioId", new { id, usuarioId });
        }

        public async Task Actualizar(Categoria categoria)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(
                @"UPDATE Categorias
                  SET Nombre = @Nombre, TipoOperacionId = @TipoOperacionId
                  WHERE Id = @Id AND UsuarioId = @UsuarioId", categoria);
        }

        public async Task Borrar(int id) 
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(
                @"DELETE FROM Categorias WHERE Id = @Id", new { id });
        }
    }
}
