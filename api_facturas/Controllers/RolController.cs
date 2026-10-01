// ============================================================
// RolController — la capa HTTP de `rol`.
//
// Su unico trabajo: recibir la peticion (ASP.NET ya valido el body
// contra la PETICION del verbo -> 422 automatico), delegar al
// servicio, y responder JSON con el codigo correcto.
// Aqui NO hay SQL ni reglas de negocio.
//
// Traduccion a codigos (6_contracts.md):
//   body con errores de forma -> 422 (lo arma Program.cs)
//   ArgumentException         -> 400
//   NoEncontradoExcepcion     -> 404
//   las demas                 -> 500
// ============================================================

using ApiFacturas.Excepciones;
using ApiFacturas.Modelos;
using ApiFacturas.Peticiones;
using ApiFacturas.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace ApiFacturas.Controllers;

[ApiController]
[Route("api/rol")]
public class RolController : ControllerBase
{
    private readonly IServicioRol _servicio;

    public RolController(IServicioRol servicio)
    {
        _servicio = servicio;
    }

    // ------------------------------------------------------------
    // GET /api/rol[?limite=N]  ->  listar
    // ------------------------------------------------------------
    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] int limite = 1000)
    {
        try
        {
            var lista = await _servicio.ListarAsync(limite);
            if (lista.Count == 0)
            {
                return NoContent();   // 204: exito SIN contenido
            }
            return Ok(new
            {
                tabla = "rol",
                limite,
                total = lista.Count,
                datos = lista,
            });
        }
        catch (ArgumentException e)
        {
            return StatusCode(400, new { estado = 400, mensaje = "Parametros invalidos.", detalle = e.Message });
        }
        catch (Exception e)
        {
            return StatusCode(500, new { estado = 500, mensaje = "Error interno.", detalle = e.Message });
        }
    }

    // ------------------------------------------------------------
    // GET /api/rol/{id}  ->  obtener uno
    // ------------------------------------------------------------
    [HttpGet("{id}")]
    public async Task<IActionResult> Obtener(int id)
    {
        try
        {
            return Ok(await _servicio.ObtenerAsync(id));
        }
        catch (ArgumentException e)
        {
            return StatusCode(400, new { estado = 400, mensaje = "Parametros invalidos.", detalle = e.Message });
        }
        catch (NoEncontradoExcepcion e)
        {
            return StatusCode(404, new { estado = 404, mensaje = "Rol no encontrado.", detalle = e.Message });
        }
        catch (Exception e)
        {
            return StatusCode(500, new { estado = 500, mensaje = "Error interno.", detalle = e.Message });
        }
    }

    // ------------------------------------------------------------
    // POST /api/rol  ->  crear
    // ------------------------------------------------------------
    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] RolCrear body)
    {
        try
        {
            var entidad = new Rol
            {
                Nombre = body.Nombre!,
            };
            await _servicio.CrearAsync(entidad);
            return Ok(new { estado = 200, mensaje = "Rol creado exitosamente." });
        }
        catch (Exception e)
        {
            return StatusCode(500, new { estado = 500, mensaje = "Error interno.", detalle = e.Message });
        }
    }

    // ------------------------------------------------------------
    // PUT /api/rol/{id}  ->  reemplazo COMPLETO
    // ------------------------------------------------------------
    // La peticion RolReemplazo exige TODOS los campos: un PUT con body
    // parcial muere en 422 ANTES de llegar aqui.
    [HttpPut("{id}")]
    public async Task<IActionResult> Reemplazar(int id, [FromBody] RolReemplazo body)
    {
        try
        {
            var datos = new Dictionary<string, object>
            {
                ["nombre"] = body.Nombre!,
            };
            var filas = await _servicio.ActualizarAsync(id, datos);
            return Ok(new { estado = 200, mensaje = "Rol reemplazado exitosamente.", filasAfectadas = filas });
        }
        catch (ArgumentException e)
        {
            return StatusCode(400, new { estado = 400, mensaje = "Parametros invalidos.", detalle = e.Message });
        }
        catch (NoEncontradoExcepcion e)
        {
            return StatusCode(404, new { estado = 404, mensaje = "Rol no encontrado.", detalle = e.Message });
        }
        catch (Exception e)
        {
            return StatusCode(500, new { estado = 500, mensaje = "Error interno.", detalle = e.Message });
        }
    }

    // ------------------------------------------------------------
    // PATCH /api/rol/{id}  ->  actualizacion PARCIAL
    // ------------------------------------------------------------
    // RolActualizar no exige campos: valida SOLO los que llegaron. El
    // MISMO body que en PUT da 422, aqui pasa.
    [HttpPatch("{id}")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] RolActualizar body)
    {
        try
        {
            // La lista blanca: solo estas columnas pueden viajar al SQL.
            var datos = new Dictionary<string, object>();
            if (body.Nombre != null) { datos["nombre"] = body.Nombre; }

            var filas = await _servicio.ActualizarAsync(id, datos);
            return Ok(new { estado = 200, mensaje = "Rol actualizado exitosamente.", filasAfectadas = filas });
        }
        catch (ArgumentException e)
        {
            return StatusCode(400, new { estado = 400, mensaje = "Parametros invalidos.", detalle = e.Message });
        }
        catch (NoEncontradoExcepcion e)
        {
            return StatusCode(404, new { estado = 404, mensaje = "Rol no encontrado.", detalle = e.Message });
        }
        catch (Exception e)
        {
            return StatusCode(500, new { estado = 500, mensaje = "Error interno.", detalle = e.Message });
        }
    }

    // ------------------------------------------------------------
    // DELETE /api/rol/{id}  ->  eliminar
    // ------------------------------------------------------------
    [HttpDelete("{id}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        try
        {
            var filas = await _servicio.EliminarAsync(id);
            return Ok(new { estado = 200, mensaje = "Rol eliminado exitosamente.", filasEliminadas = filas });
        }
        catch (ArgumentException e)
        {
            return StatusCode(400, new { estado = 400, mensaje = "Parametros invalidos.", detalle = e.Message });
        }
        catch (NoEncontradoExcepcion e)
        {
            return StatusCode(404, new { estado = 404, mensaje = "Rol no encontrado.", detalle = e.Message });
        }
        catch (Exception e)
        {
            return StatusCode(500, new { estado = 500, mensaje = "Error interno.", detalle = e.Message });
        }
    }
}
