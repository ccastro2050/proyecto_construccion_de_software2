// ============================================================
// UsuarioController — la capa HTTP de `usuario`.
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
[Route("api/usuario")]
public class UsuarioController : ControllerBase
{
    private readonly IServicioUsuario _servicio;

    public UsuarioController(IServicioUsuario servicio)
    {
        _servicio = servicio;
    }

    // ------------------------------------------------------------
    // GET /api/usuario[?limite=N]  ->  listar
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
                tabla = "usuario",
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
    // GET /api/usuario/{email}  ->  obtener uno
    // ------------------------------------------------------------
    [HttpGet("{email}")]
    public async Task<IActionResult> Obtener(string email)
    {
        try
        {
            return Ok(await _servicio.ObtenerAsync(email));
        }
        catch (ArgumentException e)
        {
            return StatusCode(400, new { estado = 400, mensaje = "Parametros invalidos.", detalle = e.Message });
        }
        catch (NoEncontradoExcepcion e)
        {
            return StatusCode(404, new { estado = 404, mensaje = "Usuario no encontrado.", detalle = e.Message });
        }
        catch (Exception e)
        {
            return StatusCode(500, new { estado = 500, mensaje = "Error interno.", detalle = e.Message });
        }
    }

    // ------------------------------------------------------------
    // POST /api/usuario  ->  crear
    // ------------------------------------------------------------
    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] UsuarioCrear body)
    {
        try
        {
            // Los dos valores sueltos: el modelo Usuario ya no tiene
            // contrasena, justamente para que no pueda volver en una
            // respuesta.
            await _servicio.CrearAsync(body.Email!, body.Contrasena!);
            return Ok(new { estado = 200, mensaje = "Usuario creado exitosamente." });
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
    // PUT /api/usuario/{email}  ->  reemplazo COMPLETO
    // ------------------------------------------------------------
    // La peticion UsuarioReemplazo exige TODOS los campos: un PUT con body
    // parcial muere en 422 ANTES de llegar aqui.
    [HttpPut("{email}")]
    public async Task<IActionResult> Reemplazar(string email, [FromBody] UsuarioReemplazo body)
    {
        try
        {
            var datos = new Dictionary<string, object>
            {
                ["contrasena"] = body.Contrasena!,
            };
            var filas = await _servicio.ActualizarAsync(email, datos);
            return Ok(new { estado = 200, mensaje = "Usuario reemplazado exitosamente.", filasAfectadas = filas });
        }
        catch (ArgumentException e)
        {
            return StatusCode(400, new { estado = 400, mensaje = "Parametros invalidos.", detalle = e.Message });
        }
        catch (NoEncontradoExcepcion e)
        {
            return StatusCode(404, new { estado = 404, mensaje = "Usuario no encontrado.", detalle = e.Message });
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
    // PATCH /api/usuario/{email}  ->  actualizacion PARCIAL
    // ------------------------------------------------------------
    // UsuarioActualizar no exige campos: valida SOLO los que llegaron. El
    // MISMO body que en PUT da 422, aqui pasa.
    [HttpPatch("{email}")]
    public async Task<IActionResult> Actualizar(string email, [FromBody] UsuarioActualizar body)
    {
        try
        {
            // La lista blanca: solo estas columnas pueden viajar al SQL.
            var datos = new Dictionary<string, object>();
            if (body.Contrasena != null) { datos["contrasena"] = body.Contrasena; }

            var filas = await _servicio.ActualizarAsync(email, datos);
            return Ok(new { estado = 200, mensaje = "Usuario actualizado exitosamente.", filasAfectadas = filas });
        }
        catch (ArgumentException e)
        {
            return StatusCode(400, new { estado = 400, mensaje = "Parametros invalidos.", detalle = e.Message });
        }
        catch (NoEncontradoExcepcion e)
        {
            return StatusCode(404, new { estado = 404, mensaje = "Usuario no encontrado.", detalle = e.Message });
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
    // DELETE /api/usuario/{email}  ->  eliminar
    // ------------------------------------------------------------
    [HttpDelete("{email}")]
    public async Task<IActionResult> Eliminar(string email)
    {
        try
        {
            var filas = await _servicio.EliminarAsync(email);
            return Ok(new { estado = 200, mensaje = "Usuario eliminado exitosamente.", filasEliminadas = filas });
        }
        catch (ArgumentException e)
        {
            return StatusCode(400, new { estado = 400, mensaje = "Parametros invalidos.", detalle = e.Message });
        }
        catch (NoEncontradoExcepcion e)
        {
            return StatusCode(404, new { estado = 404, mensaje = "Usuario no encontrado.", detalle = e.Message });
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
