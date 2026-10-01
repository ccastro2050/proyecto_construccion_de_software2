// ============================================================
// IRepositorioRuta — el CONTRATO de la capa de datos.
//
// Define QUE operaciones existen sobre `ruta`, sin decir COMO ni
// CONTRA QUE motor. El servicio depende de ESTA interfaz, nunca de
// una clase concreta (inversion de dependencias).
// ============================================================

using ApiFacturas.Modelos;

namespace ApiFacturas.Repositorios;

public interface IRepositorioRuta
{
    Task<List<Ruta>> ObtenerTodosAsync(int limite);

    Task<Ruta?> ObtenerPorClaveAsync(int id);

    Task CrearAsync(Ruta entidad);

    /// <summary>Escribe los campos del diccionario (los usan PUT y PATCH).
    /// Devuelve filas afectadas (0 = no existe).</summary>
    Task<int> ActualizarAsync(int id, Dictionary<string, object> datos);

    Task<int> EliminarAsync(int id);
}
