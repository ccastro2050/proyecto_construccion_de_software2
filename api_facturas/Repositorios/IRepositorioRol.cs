// ============================================================
// IRepositorioRol — el CONTRATO de la capa de datos.
//
// Define QUE operaciones existen sobre `rol`, sin decir COMO ni
// CONTRA QUE motor. El servicio depende de ESTA interfaz, nunca de
// una clase concreta (inversion de dependencias).
// ============================================================

using ApiFacturas.Modelos;

namespace ApiFacturas.Repositorios;

public interface IRepositorioRol
{
    Task<List<Rol>> ObtenerTodosAsync(int limite);

    Task<Rol?> ObtenerPorClaveAsync(int id);

    Task CrearAsync(Rol entidad);

    /// <summary>Escribe los campos del diccionario (los usan PUT y PATCH).
    /// Devuelve filas afectadas (0 = no existe).</summary>
    Task<int> ActualizarAsync(int id, Dictionary<string, object> datos);

    Task<int> EliminarAsync(int id);
}
