// ============================================================
// IServicioRuta — el CONTRATO de la capa de negocio.
//
// El controlador depende de esta interfaz. Los problemas se
// comunican con excepciones de NEGOCIO que el controlador traduce:
//   ArgumentException     -> 400
//   NoEncontradoExcepcion -> 404
//   las demas             -> 500
// ============================================================

using ApiFacturas.Modelos;

namespace ApiFacturas.Servicios;

public interface IServicioRuta
{
    Task<List<Ruta>> ListarAsync(int limite);

    Task<Ruta> ObtenerAsync(int id);

    Task CrearAsync(Ruta entidad);

    Task<int> ActualizarAsync(int id, Dictionary<string, object> datos);

    Task<int> EliminarAsync(int id);
}
