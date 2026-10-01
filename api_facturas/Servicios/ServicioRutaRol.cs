// ServicioRutaRol — la capa de NEGOCIO del puente rutarol (v3).
// Las búsquedas sin resultados son 404 (el recurso preguntado no
// tiene asignaciones) — lo decide el negocio, no el repositorio.

using ApiFacturas.Excepciones;
using ApiFacturas.Modelos;
using ApiFacturas.Repositorios;

namespace ApiFacturas.Servicios;

public class ServicioRutaRol : IServicioRutaRol
{
    private readonly IRepositorioRutaRol _repositorio;

    public ServicioRutaRol(IRepositorioRutaRol repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<List<RutaRol>> ListarAsync(int limite)
    {
        if (limite <= 0)
        {
            throw new ArgumentException("El límite debe ser un entero mayor que cero.");
        }
        return await _repositorio.ObtenerTodosAsync(limite);
    }

    public async Task<List<RutaRol>> ObtenerPorRutaAsync(int fkidruta)
    {
        if (fkidruta <= 0)
        {
            throw new ArgumentException("El id de ruta debe ser un entero mayor que cero.");
        }
        var lista = await _repositorio.ObtenerPorRutaAsync(fkidruta);
        if (lista.Count == 0)
        {
            throw new NoEncontradoExcepcion($"No hay asignaciones en rutarol para ruta = {fkidruta}");
        }
        return lista;
    }

    public async Task<List<RutaRol>> ObtenerPorRolAsync(int fkidrol)
    {
        if (fkidrol <= 0)
        {
            throw new ArgumentException("El id de rol debe ser un entero mayor que cero.");
        }
        var lista = await _repositorio.ObtenerPorRolAsync(fkidrol);
        if (lista.Count == 0)
        {
            throw new NoEncontradoExcepcion($"No hay asignaciones en rutarol para rol = {fkidrol}");
        }
        return lista;
    }

    public async Task CrearAsync(RutaRol asignacion)
    {
        await _repositorio.CrearAsync(asignacion);
    }

    public async Task<int> EliminarAsync(int fkidruta, int fkidrol)
    {
        if (fkidruta <= 0)
        {
            throw new ArgumentException("El id de ruta debe ser un entero mayor que cero.");
        }
        var filas = await _repositorio.EliminarAsync(fkidruta, fkidrol);
        if (filas == 0)
        {
            throw new NoEncontradoExcepcion($"No existe la pareja ({fkidruta}, {fkidrol}) en rutarol");
        }
        return filas;
    }
}
