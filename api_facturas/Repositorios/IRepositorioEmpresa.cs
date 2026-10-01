// ============================================================
// IRepositorioEmpresa — el CONTRATO de la capa de datos.
//
// Define QUE operaciones existen sobre `empresa`, sin decir COMO ni
// CONTRA QUE motor. El servicio depende de ESTA interfaz, nunca de
// una clase concreta (inversion de dependencias).
// ============================================================

using ApiFacturas.Modelos;

namespace ApiFacturas.Repositorios;

public interface IRepositorioEmpresa
{
    Task<List<Empresa>> ObtenerTodosAsync(int limite);

    Task<Empresa?> ObtenerPorClaveAsync(string codigo);

    Task CrearAsync(Empresa entidad);

    /// <summary>Escribe los campos del diccionario (los usan PUT y PATCH).
    /// Devuelve filas afectadas (0 = no existe).</summary>
    Task<int> ActualizarAsync(string codigo, Dictionary<string, object> datos);

    Task<int> EliminarAsync(string codigo);
}
