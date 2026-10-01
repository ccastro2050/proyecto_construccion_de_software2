// ============================================================
// RutaController — la capa HTTP de `ruta`.
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
[Route("api/ruta")]
public class RutaController : ControllerBase
{
    private readonly IServicioRuta _servicio;

    public RutaController(IServicioRuta servicio)
    {
        _servicio = servicio;
    }

    // ------------------------------------------------------------
    // GET /api/ruta[?limite=N]  ->  listar
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
                tabla = "ruta",
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
    // GET /api/ruta/{id}  ->  obtener uno
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
            return StatusCode(404, new { estado = 404, mensaje = "Ruta no encontrado.", detalle = e.Message });
        }
        catch (Exception e)
        {
            return StatusCode(500, new { estado = 500, mensaje = "Error interno.", detalle = e.Message });
        }
    }

    // ------------------------------------------------------------
    // POST /api/ruta  ->  crear
    // ------------------------------------------------------------
    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] RutaCrear body)
    {
        try
        {
            var entidad = new Ruta
            {
                RutaTexto = body.RutaTexto!,
                Descripcion = body.Descripcion!,
            };
            await _servicio.CrearAsync(entidad);
            return Ok(new { estado = 200, mensaje = "Ruta creado exitosamente." });
        }
        catch (ConflictoExcepcion e)
        {
            // 409, y NO 422: el dato tiene la forma correcta -lo paso la
            // validacion de la peticion- y lo que se rompe es el ESTADO de la
            // base. Tres causas posibles: la clave foranea apunta a una fila
            // que no existe, la clave ya esta usada, o hay otra fila que
            // depende de esta y el motor no deja borrarla.
            return StatusCode(409, new
            {
                estado = 409,
                mensaje = "La operacion choca con los datos que ya existen.",
                detalle = e.Message,
            });
        }
        catch (Exception e)
        {
            return StatusCode(500, new { estado = 500, mensaje = "Error interno.", detalle = e.Message });
        }
    }

    // ------------------------------------------------------------
    // PUT /api/ruta/{id}  ->  reemplazo COMPLETO
    // ------------------------------------------------------------
    // La peticion RutaReemplazo exige TODOS los campos: un PUT con body
    // parcial muere en 422 ANTES de llegar aqui.
    [HttpPut("{id}")]
    public async Task<IActionResult> Reemplazar(int id, [FromBody] RutaReemplazo body)
    {
        try
        {
            var datos = new Dictionary<string, object>
            {
                ["ruta"] = body.RutaTexto!,
                ["descripcion"] = body.Descripcion!,
            };
            var filas = await _servicio.ActualizarAsync(id, datos);
            return Ok(new { estado = 200, mensaje = "Ruta reemplazado exitosamente.", filasAfectadas = filas });
        }
        catch (ArgumentException e)
        {
            return StatusCode(400, new { estado = 400, mensaje = "Parametros invalidos.", detalle = e.Message });
        }
        catch (NoEncontradoExcepcion e)
        {
            return StatusCode(404, new { estado = 404, mensaje = "Ruta no encontrado.", detalle = e.Message });
        }
        catch (ConflictoExcepcion e)
        {
            // 409, y NO 422: el dato tiene la forma correcta -lo paso la
            // validacion de la peticion- y lo que se rompe es el ESTADO de la
            // base. Tres causas posibles: la clave foranea apunta a una fila
            // que no existe, la clave ya esta usada, o hay otra fila que
            // depende de esta y el motor no deja borrarla.
            return StatusCode(409, new
            {
                estado = 409,
                mensaje = "La operacion choca con los datos que ya existen.",
                detalle = e.Message,
            });
        }
        catch (Exception e)
        {
            return StatusCode(500, new { estado = 500, mensaje = "Error interno.", detalle = e.Message });
        }
    }

    // ------------------------------------------------------------
    // PATCH /api/ruta/{id}  ->  actualizacion PARCIAL
    // ------------------------------------------------------------
    // RutaActualizar no exige campos: valida SOLO los que llegaron. El
    // MISMO body que en PUT da 422, aqui pasa.
    [HttpPatch("{id}")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] RutaActualizar body)
    {
        try
        {
            // La lista blanca: solo estas columnas pueden viajar al SQL.
            var datos = new Dictionary<string, object>();
            if (body.RutaTexto != null) { datos["ruta"] = body.RutaTexto; }
            if (body.Descripcion != null) { datos["descripcion"] = body.Descripcion; }

            var filas = await _servicio.ActualizarAsync(id, datos);
            return Ok(new { estado = 200, mensaje = "Ruta actualizado exitosamente.", filasAfectadas = filas });
        }
        catch (ArgumentException e)
        {
            return StatusCode(400, new { estado = 400, mensaje = "Parametros invalidos.", detalle = e.Message });
        }
        catch (NoEncontradoExcepcion e)
        {
            return StatusCode(404, new { estado = 404, mensaje = "Ruta no encontrado.", detalle = e.Message });
        }
        catch (ConflictoExcepcion e)
        {
            // 409, y NO 422: el dato tiene la forma correcta -lo paso la
            // validacion de la peticion- y lo que se rompe es el ESTADO de la
            // base. Tres causas posibles: la clave foranea apunta a una fila
            // que no existe, la clave ya esta usada, o hay otra fila que
            // depende de esta y el motor no deja borrarla.
            return StatusCode(409, new
            {
                estado = 409,
                mensaje = "La operacion choca con los datos que ya existen.",
                detalle = e.Message,
            });
        }
        catch (Exception e)
        {
            return StatusCode(500, new { estado = 500, mensaje = "Error interno.", detalle = e.Message });
        }
    }

    // ------------------------------------------------------------
    // DELETE /api/ruta/{id}  ->  eliminar
    // ------------------------------------------------------------
    [HttpDelete("{id}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        try
        {
            var filas = await _servicio.EliminarAsync(id);
            return Ok(new { estado = 200, mensaje = "Ruta eliminado exitosamente.", filasEliminadas = filas });
        }
        catch (ArgumentException e)
        {
            return StatusCode(400, new { estado = 400, mensaje = "Parametros invalidos.", detalle = e.Message });
        }
        catch (NoEncontradoExcepcion e)
        {
            return StatusCode(404, new { estado = 404, mensaje = "Ruta no encontrado.", detalle = e.Message });
        }
        catch (ConflictoExcepcion e)
        {
            // 409, y NO 422: el dato tiene la forma correcta -lo paso la
            // validacion de la peticion- y lo que se rompe es el ESTADO de la
            // base. Tres causas posibles: la clave foranea apunta a una fila
            // que no existe, la clave ya esta usada, o hay otra fila que
            // depende de esta y el motor no deja borrarla.
            return StatusCode(409, new
            {
                estado = 409,
                mensaje = "La operacion choca con los datos que ya existen.",
                detalle = e.Message,
            });
        }
        catch (Exception e)
        {
            return StatusCode(500, new { estado = 500, mensaje = "Error interno.", detalle = e.Message });
        }
    }
}
