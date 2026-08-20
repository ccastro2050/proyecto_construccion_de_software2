// ============================================================
// IServicioFactura — el CONTRATO de negocio de factura.
//
// Corto a propósito: la lógica pesada (subtotales, total, stock,
// anulación) vive en la BD. El servicio valida argumentos y
// delega — RNF2 de la v2: si un número sale mal, el bug se busca
// en la BD, no aquí.
//
// Excepciones: ArgumentException → 400 · NoEncontradoExcepcion →
// 404 · ConflictoExcepcion → 409 · PostgresException y demás → 500.
// ============================================================

using ApiFacturas.Modelos;

namespace ApiFacturas.Servicios;

public interface IServicioFactura
{
    /// <summary>Todas las facturas con su detalle (vía SP).</summary>
    Task<List<Factura>> ListarAsync();

    /// <summary>UNA factura completa. ArgumentException si numero &lt;= 0 ·
    /// NoEncontradoExcepcion si no existe.</summary>
    Task<Factura> ConsultarAsync(int numero);

    /// <summary>Crea la factura maestro-detalle. 'productosJson' llega
    /// armado por el controlador desde la petición ya validada
    /// ([{"codigo":…,"cantidad":…}]). Devuelve la factura creada con
    /// los valores que CALCULÓ la BD.</summary>
    Task<Factura> CrearAsync(int fkidcliente, int fkidvendedor, string productosJson);

    /// <summary>Anula (borrado lógico). Devuelve el JSON del SP.
    /// ConflictoExcepcion si ya estaba anulada.</summary>
    Task<string> AnularAsync(int numero);
}
