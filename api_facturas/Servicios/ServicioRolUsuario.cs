// ServicioRolUsuario — la capa de NEGOCIO del puente rol_usuario (v3).
// Las búsquedas sin resultados son 404 (el recurso preguntado no
// tiene asignaciones) — lo decide el negocio, no el repositorio.

using ApiFacturas.Excepciones;
using ApiFacturas.Modelos;
using ApiFacturas.Repositorios;

namespace ApiFacturas.Servicios;

public class ServicioRolUsuario : IServicioRolUsuario
{
    private readonly IRepositorioRolUsuario _repositorio;

    public ServicioRolUsuario(IRepositorioRolUsuario repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<List<RolUsuario>> ListarAsync(int limite)
    {
        if (limite <= 0)
        {
            throw new ArgumentException("El límite debe ser un entero mayor que cero.");
        }
        return await _repositorio.ObtenerTodosAsync(limite);
    }

    public async Task<List<RolUsuario>> ObtenerPorUsuarioAsync(string fkemail)
    {
        fkemail = fkemail.Trim();
        if (fkemail == "")
        {
            throw new ArgumentException("El usuario no puede estar vacío.");
        }
        var lista = await _repositorio.ObtenerPorUsuarioAsync(fkemail);
        if (lista.Count == 0)
        {
            throw new NoEncontradoExcepcion($"No hay asignaciones en rol_usuario para usuario = {fkemail}");
        }
        return lista;
    }

    public async Task<List<RolUsuario>> ObtenerPorRolAsync(int fkidrol)
    {
        if (fkidrol <= 0)
        {
            throw new ArgumentException("El id de rol debe ser un entero mayor que cero.");
        }
        var lista = await _repositorio.ObtenerPorRolAsync(fkidrol);
        if (lista.Count == 0)
        {
            throw new NoEncontradoExcepcion($"No hay asignaciones en rol_usuario para rol = {fkidrol}");
        }
        return lista;
    }

    public async Task CrearAsync(RolUsuario asignacion)
    {
        await _repositorio.CrearAsync(asignacion);
    }

    public async Task<int> EliminarAsync(string fkemail, int fkidrol)
    {
        fkemail = fkemail.Trim();
        if (fkemail == "")
        {
            throw new ArgumentException("El usuario no puede estar vacío.");
        }
        var filas = await _repositorio.EliminarAsync(fkemail, fkidrol);
        if (filas == 0)
        {
            throw new NoEncontradoExcepcion($"No existe la pareja ({fkemail}, {fkidrol}) en rol_usuario");
        }
        return filas;
    }
}
