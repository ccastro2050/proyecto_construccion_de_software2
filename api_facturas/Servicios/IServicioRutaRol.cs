// IServicioRutaRol — contrato de negocio del puente rutarol (v3).

using ApiFacturas.Modelos;

namespace ApiFacturas.Servicios;

public interface IServicioRutaRol
{
    Task<List<RutaRol>> ListarAsync(int limite);
    Task<List<RutaRol>> ObtenerPorRutaAsync(int fkidruta);
    Task<List<RutaRol>> ObtenerPorRolAsync(int fkidrol);
    Task CrearAsync(RutaRol asignacion);
    Task<int> EliminarAsync(int fkidruta, int fkidrol);
}
