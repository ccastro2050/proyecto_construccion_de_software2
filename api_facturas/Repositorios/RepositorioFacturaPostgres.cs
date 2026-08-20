// ============================================================
// RepositorioFacturaPostgres — la capa de DATOS de factura (v2).
//
// La API como TRADUCTORA: este repositorio no escribe SQL de
// tablas — llama los PROCEDIMIENTOS ALMACENADOS que viven en
// db/bdfacturas_postgres.sql desde el día 1, recibe su resultado
// como JSON por el parámetro INOUT p_resultado, y lo deserializa
// a los modelos (System.Text.Json).
//
// Las dos mecánicas propias del dialecto PostgreSQL:
//   1. Los SPs son PROCEDURE con INOUT p_resultado JSON: se
//      invocan con un CALL de texto y el CALL devuelve los INOUT
//      como UNA FILA de resultado → ExecuteScalarAsync() la lee.
//   2. Los errores de negocio son RAISE EXCEPTION sin número:
//      todos salen con SQLSTATE 'P0001', y la traducción filtra
//      por P0001 + el patrón del mensaje ("no existe" → 404,
//      "ya está anulada" → 409). Arriba de aquí nadie conoce
//      PostgresException — esa es la frontera.
// ============================================================

using System.Text.Json;
using System.Text.Json.Serialization;
using ApiFacturas.Excepciones;
using ApiFacturas.Modelos;
using Npgsql;

namespace ApiFacturas.Repositorios;

public class RepositorioFacturaPostgres : IRepositorioFactura
{
    private readonly string _cadenaConexion;

    private static readonly JsonSerializerOptions _opcionesJson = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public RepositorioFacturaPostgres(string cadenaConexion)
    {
        _cadenaConexion = cadenaConexion;
    }

    // ------------------------------------------------------------
    // El ayudante central: ejecutar un CALL y devolver su JSON
    // ------------------------------------------------------------

    /// <summary>Ejecuta el CALL indicado (texto SQL con sus @parámetros;
    /// el último argumento del CALL es NULL: el INOUT p_resultado) y
    /// devuelve el JSON que el SP dejó ahí. Traduce los RAISE EXCEPTION
    /// (P0001) a excepciones de negocio; el resto sube tal cual (500).</summary>
    private async Task<string> EjecutarSpAsync(string sqlCall, Action<NpgsqlParameterCollection> configurar)
    {
        await using var conexion = new NpgsqlConnection(_cadenaConexion);
        await using var comando = new NpgsqlCommand(sqlCall, conexion);
        configurar(comando.Parameters);

        try
        {
            await conexion.OpenAsync();
            // El CALL devuelve una fila con los INOUT; la única columna
            // es p_resultado (el JSON del SP):
            var resultado = await comando.ExecuteScalarAsync();
            return resultado is null or DBNull ? "null" : (string)resultado;
        }
        // 'P0001' = raise_exception (los errores DE NEGOCIO de los SPs).
        // El patrón del mensaje decide cuál excepción — mismos textos
        // que valida el quickstart: "no existe" y "ya está anulada":
        catch (PostgresException e) when (e.SqlState == "P0001"
                                          && e.MessageText.Contains("no existe"))
        {
            throw new NoEncontradoExcepcion(e.MessageText);  // → 404
        }
        catch (PostgresException e) when (e.SqlState == "P0001"
                                          && e.MessageText.Contains("anulada"))
        {
            throw new ConflictoExcepcion(e.MessageText);     // → 409
        }
        // Lo demás (stock insuficiente del trigger, FK inexistente, el
        // mínimo de renglones) sube tal cual → 500.
    }

    // El mismo sobre {"factura":{…},"productos":[…]} que en PostgreSQL
    // (los SPs de ambos motores devuelven el MISMO JSON — a propósito):
    private class RespuestaFacturaSp
    {
        [JsonPropertyName("factura")]
        public Factura? Factura { get; set; }

        [JsonPropertyName("productos")]
        public List<ProductoDeFactura>? Productos { get; set; }
    }

    private static Factura ArmarFactura(string json)
    {
        var respuesta = JsonSerializer.Deserialize<RespuestaFacturaSp>(json, _opcionesJson)!;
        var factura = respuesta.Factura!;
        factura.Productos = respuesta.Productos ?? new List<ProductoDeFactura>();
        return factura;
    }

    // ------------------------------------------------------------
    // Los 4 métodos del contrato (mismos SPs, mismo JSON)
    // ------------------------------------------------------------

    public async Task<List<Factura>> ListarAsync()
    {
        // El NULL es el INOUT p_resultado (PostgreSQL exige pasarlo):
        var json = await EjecutarSpAsync(
            "CALL sp_listar_facturas_y_productosporfactura(NULL)", _ => { });
        return JsonSerializer.Deserialize<List<Factura>>(json, _opcionesJson) ?? new List<Factura>();
    }

    public async Task<Factura> ConsultarAsync(int numero)
    {
        var json = await EjecutarSpAsync(
            "CALL sp_consultar_factura_y_productosporfactura(@p_numero, NULL)",
            parametros => parametros.AddWithValue("@p_numero", numero));
        return ArmarFactura(json);
    }

    public async Task<Factura> CrearAsync(int fkidcliente, int fkidvendedor, string productosJson)
    {
        // El detalle viaja como texto y el ::json lo tipa para el SP —
        // el mismo "un solo viaje, UNA transacción" de siempre:
        var json = await EjecutarSpAsync(
            "CALL sp_insertar_factura_y_productosporfactura(@p_fkidcliente, @p_fkidvendedor, @p_productos::json, 1, NULL)",
            parametros =>
            {
                parametros.AddWithValue("@p_fkidcliente", fkidcliente);
                parametros.AddWithValue("@p_fkidvendedor", fkidvendedor);
                parametros.AddWithValue("@p_productos", productosJson);
            });
        return ArmarFactura(json);
    }

    public async Task<string> AnularAsync(int numero)
    {
        return await EjecutarSpAsync(
            "CALL sp_anular_factura(@p_numero, NULL)",
            parametros => parametros.AddWithValue("@p_numero", numero));
    }
}
