// IRepositorioRutaRol — contrato del puente rutarol (v3): el patrón
// nuevo — sin Actualizar, búsquedas por cada lado, y el DELETE
// exige LA PAREJA (ambas columnas de la PK compuesta).

using ApiFacturas.Modelos;

namespace ApiFacturas.Repositorios;

public interface IRepositorioRutaRol
{
    Task<List<RutaRol>> ObtenerTodosAsync(int limite);
    Task<List<RutaRol>> ObtenerPorRutaAsync(int fkidruta);
    Task<List<RutaRol>> ObtenerPorRolAsync(int fkidrol);
    Task CrearAsync(RutaRol asignacion);
    Task<int> EliminarAsync(int fkidruta, int fkidrol);   // ¡AMBAS columnas!
}
