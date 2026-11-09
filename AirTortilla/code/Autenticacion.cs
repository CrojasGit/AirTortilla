using MySqlConnector;

namespace AirTortilla.code
{
    public class Autenticacion
    {
        public static async Task<Usuario?> IniciarSesion(
            string connectionString,
            string documento,
            string contraseña)
        {
            await using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            string sql = @"
                SELECT documento, nombre, apellido1, apellido2,
                       password_hash, tipo_usuario
                FROM Usuario
                WHERE documento = @documento
                  AND activo = TRUE;
            ";

            await using var command = new MySqlCommand(sql, connection);

            command.Parameters.AddWithValue("@documento", documento);

            await using var reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                return null;
            }

            string passwordHash = reader.GetString("password_hash");

            if (!BCrypt.Net.BCrypt.Verify(contraseña, passwordHash))
            {
                return null;
            }

            string documentoUsuario = reader.GetString("documento");
            string nombre = reader.GetString("nombre");
            string apellido1 = reader.GetString("apellido1");

            string? apellido2 = reader.IsDBNull(reader.GetOrdinal("apellido2"))
                ? null
                : reader.GetString("apellido2");

            string tipoUsuario = reader.GetString("tipo_usuario");

            return new Usuario(
                documentoUsuario,
                nombre,
                apellido1,
                apellido2,
                tipoUsuario
            );
        }
    }
}
