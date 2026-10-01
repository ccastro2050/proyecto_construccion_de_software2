// ============================================================
// IServicioEmpresa — el CONTRATO de la capa de negocio.
//
// El controlador depende de esta interfaz. Los problemas se
// comunican con excepciones de NEGOCIO que el controlador traduce:
//   ArgumentException     -> 400
//   NoEncontradoExcepcion -> 404
//   las demas             -> 500
// ============================================================

using ApiFacturas.Modelos;

namespace ApiFacturas.Servicios;

public interface IServicioEmpresa
{
    Task<List<Empresa>> ListarAsync(int limite);

    Task<Empresa> ObtenerAsync(string codigo);

    Task CrearAsync(Empresa entidad);

    Task<int> ActualizarAsync(string codigo, Dictionary<string, object> datos);

    Task<int> EliminarAsync(string codigo);
}
