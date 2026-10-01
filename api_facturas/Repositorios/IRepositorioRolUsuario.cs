// IRepositorioRolUsuario — contrato del puente rol_usuario (v3): el patrón
// nuevo — sin Actualizar, búsquedas por cada lado, y el DELETE
// exige LA PAREJA (ambas columnas de la PK compuesta).

using ApiFacturas.Modelos;

namespace ApiFacturas.Repositorios;

public interface IRepositorioRolUsuario
{
    Task<List<RolUsuario>> ObtenerTodosAsync(int limite);
    Task<List<RolUsuario>> ObtenerPorUsuarioAsync(string fkemail);
    Task<List<RolUsuario>> ObtenerPorRolAsync(int fkidrol);
    Task CrearAsync(RolUsuario asignacion);
    Task<int> EliminarAsync(string fkemail, int fkidrol);   // ¡AMBAS columnas!
}
