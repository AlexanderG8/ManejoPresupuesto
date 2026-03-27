using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Services
{
    public interface IRepositorioUsuarios
    {
        Task<int> CrearUsuario(Usuario usuario);
        Task<Usuario> BuscarUsuarioPorEmail(string emailNormalizado);
        Task Actualizar(Usuario usuario);
    }
    public class RepositorioUsuarios : IRepositorioUsuarios
    {
        private readonly string connectionString;
        public RepositorioUsuarios(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // Método para crear usuario
        public async Task<int> CrearUsuario(Usuario usuario) 
        {
            using var connection = new SqlConnection(connectionString);
            var id = await connection.QuerySingleAsync<int>(
                @"INSERT INTO Usuarios (Email, EmailNormalizado, PasswordHash)
                  VALUES (@Email, @EmailNormalizado, @PasswordHash);
                  SELECT SCOPE_IDENTITY();", usuario);

            await connection.ExecuteAsync("CrearDatosUsuarioNuevo", new { UsuarioId = id }, commandType: System.Data.CommandType.StoredProcedure);

            return id;
        }

        public async Task<Usuario> BuscarUsuarioPorEmail(string emailNormalizado) 
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<Usuario>(
                "SELECT * FROM Usuarios WHERE EmailNormalizado = @EmailNormalizado", 
                new { EmailNormalizado = emailNormalizado });
        }

        public async Task Actualizar(Usuario usuario) 
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(@"
            UPDATE Usuarios
            SET PasswordHash = @PasswordHash
            WHERE Id = @Id", usuario);
        }
    }
}
