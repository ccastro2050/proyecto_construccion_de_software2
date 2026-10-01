// ============================================================
// IServicioUsuario — el CONTRATO de la capa de negocio.
//
// El controlador depende de esta interfaz. Los problemas se
// comunican con excepciones de NEGOCIO que el controlador traduce:
//   ArgumentException     -> 400
//   NoEncontradoExcepcion -> 404
//   las demas             -> 500
// ============================================================

using ApiFacturas.Modelos;

namespace ApiFacturas.Servicios;

public interface IServicioUsuario
{
    Task<List<Usuario>> ListarAsync(int limite);

    Task<Usuario> ObtenerAsync(string email);

    Task CrearAsync(string email, string contrasena);

    Task<int> ActualizarAsync(string email, Dictionary<string, object> datos);

    Task<int> EliminarAsync(string email);
}
