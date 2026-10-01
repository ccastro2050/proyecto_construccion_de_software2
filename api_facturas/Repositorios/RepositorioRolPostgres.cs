// ============================================================
// RepositorioRolPostgres — la capa de DATOS de `rol`.
//
// SQL escrito A MANO y SIEMPRE parametrizado; Dapper como
// micro-ejecutor. Sin Entity Framework: nada genera SQL por
// nosotros (constitucion, Art. 2).
// ============================================================

using ApiFacturas.Modelos;
using Dapper;
using Npgsql;

namespace ApiFacturas.Repositorios;

public class RepositorioRolPostgres : IRepositorioRol
{
    private readonly string _cadenaConexion;

    public RepositorioRolPostgres(string cadenaConexion)
    {
        _cadenaConexion = cadenaConexion;
    }

    private NpgsqlConnection CrearConexion() => new(_cadenaConexion);

    public async Task<List<Rol>> ObtenerTodosAsync(int limite)
    {
        const string sql = @"SELECT id, nombre
                             FROM rol ORDER BY id LIMIT @limite";
        await using var conexion = CrearConexion();
        var filas = await conexion.QueryAsync<Rol>(sql, new { limite });
        return filas.ToList();
    }

    public async Task<Rol?> ObtenerPorClaveAsync(int id)
    {
        const string sql = @"SELECT id, nombre
                             FROM rol WHERE id = @id";
        await using var conexion = CrearConexion();
        // Cero filas -> null. El SERVICIO decide que significa ese null:
        // aqui solo hay hechos.
        return await conexion.QueryFirstOrDefaultAsync<Rol>(sql, new { id });
    }

    public async Task CrearAsync(Rol entidad)
    {
        const string sql = @"INSERT INTO rol (nombre)
                             VALUES (@Nombre)";
        await using var conexion = CrearConexion();
        await conexion.ExecuteAsync(sql, entidad);
    }

    public async Task<int> ActualizarAsync(int id, Dictionary<string, object> datos)
    {
        // SET dinamico SOLO con las columnas que llegaron. Los NOMBRES
        // salen de las PETICIONES (lista blanca) — jamas del cliente; los
        // VALORES van parametrizados.
        var asignaciones = string.Join(", ", datos.Keys.Select(c => $"{c} = @{c}"));
        var sql = $"UPDATE rol SET {asignaciones} WHERE id = @clave";
        var parametros = new DynamicParameters(datos);
        parametros.Add("clave", id);
        await using var conexion = CrearConexion();
        return await conexion.ExecuteAsync(sql, parametros);
    }

    public async Task<int> EliminarAsync(int id)
    {
        // Si otras tablas lo referencian, la FK del motor rechaza -> 500.
        const string sql = "DELETE FROM rol WHERE id = @id";
        await using var conexion = CrearConexion();
        return await conexion.ExecuteAsync(sql, new { id });
    }
}
