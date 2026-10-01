// IServicioRolUsuario — contrato de negocio del puente rol_usuario (v3).

using ApiFacturas.Modelos;

namespace ApiFacturas.Servicios;

public interface IServicioRolUsuario
{
    Task<List<RolUsuario>> ListarAsync(int limite);
    Task<List<RolUsuario>> ObtenerPorUsuarioAsync(string fkemail);
    Task<List<RolUsuario>> ObtenerPorRolAsync(int fkidrol);
    Task CrearAsync(RolUsuario asignacion);
    Task<int> EliminarAsync(string fkemail, int fkidrol);
}
