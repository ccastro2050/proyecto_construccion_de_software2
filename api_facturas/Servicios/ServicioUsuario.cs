// ============================================================
// ServicioUsuario — la capa de NEGOCIO de `usuario`.
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

public class ServicioUsuario : IServicioUsuario
{
    private readonly IRepositorioUsuario _repositorio;

    public ServicioUsuario(IRepositorioUsuario repositorio)
    {
        _repositorio = repositorio;
    }

    private static string ValidarClave(string email)
    {
        email = email.Trim();
        if (email == "")
        {
            throw new ArgumentException("El campo email no puede estar vacio.");
        }
        return email;
    }

    public async Task<List<Usuario>> ListarAsync(int limite)
    {
        // El contrato dice 400 (no 422) para limites invalidos: es una
        // REGLA DE NEGOCIO, no un problema de forma del body.
        if (limite <= 0)
        {
            throw new ArgumentException("El limite debe ser un entero mayor que cero.");
        }
        return await _repositorio.ObtenerTodosAsync(limite);
    }

    public async Task<Usuario> ObtenerAsync(string email)
    {
        email = ValidarClave(email);
        var entidad = await _repositorio.ObtenerPorClaveAsync(email);
        if (entidad == null)
        {
            throw new NoEncontradoExcepcion($"No existe el usuario con email = {email}");
        }
        return entidad;
    }

    public async Task CrearAsync(string email, string contrasena)
    {
        // El body ya paso por la peticion (tipos y rangos): aqui solo se
        // delega. Si la base rechaza —clave duplicada—, la excepcion sube
        // tal cual y el controlador la convierte en 500.
        await _repositorio.CrearAsync(email, contrasena);
    }

    public async Task<int> ActualizarAsync(string email, Dictionary<string, object> datos)
    {
        email = ValidarClave(email);
        // Un PATCH con body {} paso la validacion de la peticion… pero no
        // tiene sentido de negocio: no hay nada que actualizar -> 400.
        if (datos.Count == 0)
        {
            throw new ArgumentException("No se envio ningun campo para actualizar.");
        }
        var filasAfectadas = await _repositorio.ActualizarAsync(email, datos);
        if (filasAfectadas == 0)
        {
            throw new NoEncontradoExcepcion($"No existe el usuario con email = {email}");
        }
        return filasAfectadas;
    }

    public async Task<int> EliminarAsync(string email)
    {
        email = ValidarClave(email);
        var filasEliminadas = await _repositorio.EliminarAsync(email);
        if (filasEliminadas == 0)
        {
            throw new NoEncontradoExcepcion($"No existe el usuario con email = {email}");
        }
        return filasEliminadas;
    }
}
