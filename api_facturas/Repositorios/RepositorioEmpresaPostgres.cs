// ============================================================
// RepositorioEmpresaPostgres — la capa de DATOS de `empresa`.
//
// SQL escrito A MANO y SIEMPRE parametrizado; Dapper como
// micro-ejecutor. Sin Entity Framework: nada genera SQL por
// nosotros (constitucion, Art. 2).
// ============================================================

using ApiFacturas.Modelos;
using Dapper;
using Npgsql;

namespace ApiFacturas.Repositorios;

public class RepositorioEmpresaPostgres : IRepositorioEmpresa
{
    private readonly string _cadenaConexion;

    public RepositorioEmpresaPostgres(string cadenaConexion)
    {
        _cadenaConexion = cadenaConexion;
    }

    private NpgsqlConnection CrearConexion() => new(_cadenaConexion);

    public async Task<List<Empresa>> ObtenerTodosAsync(int limite)
    {
        const string sql = @"SELECT codigo, nombre
                             FROM empresa ORDER BY codigo LIMIT @limite";
        await using var conexion = CrearConexion();
        var filas = await conexion.QueryAsync<Empresa>(sql, new { limite });
        return filas.ToList();
    }

    public async Task<Empresa?> ObtenerPorClaveAsync(string codigo)
    {
        const string sql = @"SELECT codigo, nombre
                             FROM empresa WHERE codigo = @codigo";
        await using var conexion = CrearConexion();
        // Cero filas -> null. El SERVICIO decide que significa ese null:
        // aqui solo hay hechos.
        return await conexion.QueryFirstOrDefaultAsync<Empresa>(sql, new { codigo });
    }

    public async Task CrearAsync(Empresa entidad)
    {
        const string sql = @"INSERT INTO empresa (codigo, nombre)
                             VALUES (@Codigo, @Nombre)";
        await using var conexion = CrearConexion();
        await ErroresPostgres.TraducirAsync(
            () => conexion.ExecuteAsync(sql, entidad));
    }

    public async Task<int> ActualizarAsync(string codigo, Dictionary<string, object> datos)
    {
        // SET dinamico SOLO con las columnas que llegaron. Los NOMBRES
        // salen de las PETICIONES (lista blanca) — jamas del cliente; los
        // VALORES van parametrizados.
        var asignaciones = string.Join(", ", datos.Keys.Select(c => $"{c} = @{c}"));
        var sql = $"UPDATE empresa SET {asignaciones} WHERE codigo = @clave";
        var parametros = new DynamicParameters(datos);
        parametros.Add("clave", codigo);
        await using var conexion = CrearConexion();
        return await ErroresPostgres.TraducirAsync(
            () => conexion.ExecuteAsync(sql, parametros));
    }

    public async Task<int> EliminarAsync(string codigo)
    {
        // Si otras tablas lo referencian, la FK del motor rechaza -> 500.
        const string sql = "DELETE FROM empresa WHERE codigo = @codigo";
        await using var conexion = CrearConexion();
        return await ErroresPostgres.TraducirAsync(
            () => conexion.ExecuteAsync(sql, new { codigo }));
    }
}
