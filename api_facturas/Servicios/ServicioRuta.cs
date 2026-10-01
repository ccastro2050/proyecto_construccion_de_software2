// ============================================================
// ServicioRuta — la capa de NEGOCIO de `ruta`.
//
// Recibe POR CONSTRUCTOR la interfaz del repositorio: no sabe si
// detras hay PostgreSQL o un falso en memoria — y asi debe ser.
//
// No conoce HTTP: comunica los problemas con excepciones de negocio
// que el controlador traduce a codigos.
// ============================================================

using ApiFacturas.Excepciones;
using ApiFacturas.Modelos;
using ApiFacturas.Repositorios;

namespace ApiFacturas.Servicios;

public class ServicioRuta : IServicioRuta
{
    private readonly IRepositorioRuta _repositorio;

    public ServicioRuta(IRepositorioRuta repositorio)
    {
        _repositorio = repositorio;
    }

    private static int ValidarClave(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentException("El campo id debe ser un entero mayor que 0.");
        }
        return id;
    }

    public async Task<List<Ruta>> ListarAsync(int limite)
    {
        // El contrato dice 400 (no 422) para limites invalidos: es una
        // REGLA DE NEGOCIO, no un problema de forma del body.
        if (limite <= 0)
        {
            throw new ArgumentException("El limite debe ser un entero mayor que cero.");
        }
        return await _repositorio.ObtenerTodosAsync(limite);
    }

    public async Task<Ruta> ObtenerAsync(int id)
    {
        id = ValidarClave(id);
        var entidad = await _repositorio.ObtenerPorClaveAsync(id);
        if (entidad == null)
        {
            throw new NoEncontradoExcepcion($"No existe la ruta con id = {id}");
        }
        return entidad;
    }

    public async Task CrearAsync(Ruta entidad)
    {
        // El body ya paso por la peticion (tipos y rangos): aqui solo se
        // delega. Si la base rechaza —clave duplicada—, la excepcion sube
        // tal cual y el controlador la convierte en 500.
        await _repositorio.CrearAsync(entidad);
    }

    public async Task<int> ActualizarAsync(int id, Dictionary<string, object> datos)
    {
        id = ValidarClave(id);
        // Un PATCH con body {} paso la validacion de la peticion… pero no
        // tiene sentido de negocio: no hay nada que actualizar -> 400.
        if (datos.Count == 0)
        {
            throw new ArgumentException("No se envio ningun campo para actualizar.");
        }
        var filasAfectadas = await _repositorio.ActualizarAsync(id, datos);
        if (filasAfectadas == 0)
        {
            throw new NoEncontradoExcepcion($"No existe la ruta con id = {id}");
        }
        return filasAfectadas;
    }

    public async Task<int> EliminarAsync(int id)
    {
        id = ValidarClave(id);
        var filasEliminadas = await _repositorio.EliminarAsync(id);
        if (filasEliminadas == 0)
        {
            throw new NoEncontradoExcepcion($"No existe la ruta con id = {id}");
        }
        return filasEliminadas;
    }
}
