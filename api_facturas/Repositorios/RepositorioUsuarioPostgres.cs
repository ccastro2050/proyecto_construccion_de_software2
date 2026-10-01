// ============================================================
// RepositorioUsuarioPostgres — la capa de DATOS de `usuario`.
//
// SQL escrito A MANO y SIEMPRE parametrizado; Dapper como
// micro-ejecutor. Sin Entity Framework: nada genera SQL por
// nosotros (constitucion, Art. 2).
// ============================================================

using ApiFacturas.Modelos;
using Dapper;
using Npgsql;

namespace ApiFacturas.Repositorios;

public class RepositorioUsuarioPostgres : IRepositorioUsuario
{
    private readonly string _cadenaConexion;

    public RepositorioUsuarioPostgres(string cadenaConexion)
    {
        _cadenaConexion = cadenaConexion;
    }

    private NpgsqlConnection CrearConexion() => new(_cadenaConexion);

    public async Task<List<Usuario>> ObtenerTodosAsync(int limite)
    {
        // NO proyecta `contrasena`, y es deliberado: lo que no se lee
        // no se puede devolver por accidente.
        const string sql = @"SELECT email
                             FROM usuario ORDER BY email LIMIT @limite";
        await using var conexion = CrearConexion();
        var filas = await conexion.QueryAsync<Usuario>(sql, new { limite });
        return filas.ToList();
    }

    public async Task<Usuario?> ObtenerPorClaveAsync(string email)
    {
        const string sql = @"SELECT email
                             FROM usuario WHERE email = @email";
        await using var conexion = CrearConexion();
        // Cero filas -> null. El SERVICIO decide que significa ese null:
        // aqui solo hay hechos.
        return await conexion.QueryFirstOrDefaultAsync<Usuario>(sql, new { email });
    }

    /// <summary>Recibe los dos valores SUELTOS y no el modelo, porque el
    /// modelo ya no tiene contrasena. La firma dice la verdad: para crear
    /// hacen falta las dos cosas; para leer, solo vuelve una.</summary>
    public async Task CrearAsync(string email, string contrasena)
    {
        const string sql = @"INSERT INTO usuario (email, contrasena)
                             VALUES (@Email, @Contrasena)";
        await using var conexion = CrearConexion();
        await ErroresPostgres.TraducirAsync(
            () => conexion.ExecuteAsync(sql, new { email, contrasena }));
    }

    public async Task<int> ActualizarAsync(string email, Dictionary<string, object> datos)
    {
        // SET dinamico SOLO con las columnas que llegaron. Los NOMBRES
        // salen de las PETICIONES (lista blanca) — jamas del cliente; los
        // VALORES van parametrizados.
        var asignaciones = string.Join(", ", datos.Keys.Select(c => $"{c} = @{c}"));
        var sql = $"UPDATE usuario SET {asignaciones} WHERE email = @clave";
        var parametros = new DynamicParameters(datos);
        parametros.Add("clave", email);
        await using var conexion = CrearConexion();
        return await ErroresPostgres.TraducirAsync(
            () => conexion.ExecuteAsync(sql, parametros));
    }

    public async Task<int> EliminarAsync(string email)
    {
        // Si otras tablas lo referencian, la FK del motor rechaza -> 500.
        const string sql = "DELETE FROM usuario WHERE email = @email";
        await using var conexion = CrearConexion();
        return await ErroresPostgres.TraducirAsync(
            () => conexion.ExecuteAsync(sql, new { email }));
    }
}
