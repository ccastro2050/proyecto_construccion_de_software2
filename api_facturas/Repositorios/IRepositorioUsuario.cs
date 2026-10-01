// ============================================================
// IRepositorioUsuario — el CONTRATO de la capa de datos.
//
// Define QUE operaciones existen sobre `usuario`, sin decir COMO ni
// CONTRA QUE motor. El servicio depende de ESTA interfaz, nunca de
// una clase concreta (inversion de dependencias).
// ============================================================

using ApiFacturas.Modelos;

namespace ApiFacturas.Repositorios;

public interface IRepositorioUsuario
{
    Task<List<Usuario>> ObtenerTodosAsync(int limite);

    Task<Usuario?> ObtenerPorClaveAsync(string email);

    Task CrearAsync(string email, string contrasena);

    /// <summary>Escribe los campos del diccionario (los usan PUT y PATCH).
    /// Devuelve filas afectadas (0 = no existe).</summary>
    Task<int> ActualizarAsync(string email, Dictionary<string, object> datos);

    Task<int> EliminarAsync(string email);
}
