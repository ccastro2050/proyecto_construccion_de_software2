// ============================================================
// IServicioRol — el CONTRATO de la capa de negocio.
//
// El controlador depende de esta interfaz. Los problemas se
// comunican con excepciones de NEGOCIO que el controlador traduce:
//   ArgumentException     -> 400
//   NoEncontradoExcepcion -> 404
//   las demas             -> 500
// ============================================================

using ApiFacturas.Modelos;

namespace ApiFacturas.Servicios;

public interface IServicioRol
{
    Task<List<Rol>> ListarAsync(int limite);

    Task<Rol> ObtenerAsync(int id);

    Task CrearAsync(Rol entidad);

    Task<int> ActualizarAsync(int id, Dictionary<string, object> datos);

    Task<int> EliminarAsync(int id);
}
