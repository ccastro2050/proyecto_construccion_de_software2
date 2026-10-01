// ============================================================
// EmpresaController — la capa HTTP de `empresa`.
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
[Route("api/empresa")]
public class EmpresaController : ControllerBase
{
    private readonly IServicioEmpresa _servicio;

    public EmpresaController(IServicioEmpresa servicio)
    {
        _servicio = servicio;
    }

    // ------------------------------------------------------------
    // GET /api/empresa[?limite=N]  ->  listar
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
                tabla = "empresa",
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
    // GET /api/empresa/{codigo}  ->  obtener uno
    // ------------------------------------------------------------
    [HttpGet("{codigo}")]
    public async Task<IActionResult> Obtener(string codigo)
    {
        try
        {
            return Ok(await _servicio.ObtenerAsync(codigo));
        }
        catch (ArgumentException e)
        {
            return StatusCode(400, new { estado = 400, mensaje = "Parametros invalidos.", detalle = e.Message });
        }
        catch (NoEncontradoExcepcion e)
        {
            return StatusCode(404, new { estado = 404, mensaje = "Empresa no encontrado.", detalle = e.Message });
        }
        catch (Exception e)
        {
            return StatusCode(500, new { estado = 500, mensaje = "Error interno.", detalle = e.Message });
        }
    }

    // ------------------------------------------------------------
    // POST /api/empresa  ->  crear
    // ------------------------------------------------------------
    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] EmpresaCrear body)
    {
        try
        {
            var entidad = new Empresa
            {
                Codigo = body.Codigo!,
                Nombre = body.Nombre!,
            };
            await _servicio.CrearAsync(entidad);
            return Ok(new { estado = 200, mensaje = "Empresa creado exitosamente." });
        }
        catch (Exception e)
        {
            return StatusCode(500, new { estado = 500, mensaje = "Error interno.", detalle = e.Message });
        }
    }

    // ------------------------------------------------------------
    // PUT /api/empresa/{codigo}  ->  reemplazo COMPLETO
    // ------------------------------------------------------------
    // La peticion EmpresaReemplazo exige TODOS los campos: un PUT con body
    // parcial muere en 422 ANTES de llegar aqui.
    [HttpPut("{codigo}")]
    public async Task<IActionResult> Reemplazar(string codigo, [FromBody] EmpresaReemplazo body)
    {
        try
        {
            var datos = new Dictionary<string, object>
            {
                ["nombre"] = body.Nombre!,
            };
            var filas = await _servicio.ActualizarAsync(codigo, datos);
            return Ok(new { estado = 200, mensaje = "Empresa reemplazado exitosamente.", filasAfectadas = filas });
        }
        catch (ArgumentException e)
        {
            return StatusCode(400, new { estado = 400, mensaje = "Parametros invalidos.", detalle = e.Message });
        }
        catch (NoEncontradoExcepcion e)
        {
            return StatusCode(404, new { estado = 404, mensaje = "Empresa no encontrado.", detalle = e.Message });
        }
        catch (Exception e)
        {
            return StatusCode(500, new { estado = 500, mensaje = "Error interno.", detalle = e.Message });
        }
    }

    // ------------------------------------------------------------
    // PATCH /api/empresa/{codigo}  ->  actualizacion PARCIAL
    // ------------------------------------------------------------
    // EmpresaActualizar no exige campos: valida SOLO los que llegaron. El
    // MISMO body que en PUT da 422, aqui pasa.
    [HttpPatch("{codigo}")]
    public async Task<IActionResult> Actualizar(string codigo, [FromBody] EmpresaActualizar body)
    {
        try
        {
            // La lista blanca: solo estas columnas pueden viajar al SQL.
            var datos = new Dictionary<string, object>();
            if (body.Nombre != null) { datos["nombre"] = body.Nombre; }

            var filas = await _servicio.ActualizarAsync(codigo, datos);
            return Ok(new { estado = 200, mensaje = "Empresa actualizado exitosamente.", filasAfectadas = filas });
        }
        catch (ArgumentException e)
        {
            return StatusCode(400, new { estado = 400, mensaje = "Parametros invalidos.", detalle = e.Message });
        }
        catch (NoEncontradoExcepcion e)
        {
            return StatusCode(404, new { estado = 404, mensaje = "Empresa no encontrado.", detalle = e.Message });
        }
        catch (Exception e)
        {
            return StatusCode(500, new { estado = 500, mensaje = "Error interno.", detalle = e.Message });
        }
    }

    // ------------------------------------------------------------
    // DELETE /api/empresa/{codigo}  ->  eliminar
    // ------------------------------------------------------------
    [HttpDelete("{codigo}")]
    public async Task<IActionResult> Eliminar(string codigo)
    {
        try
        {
            var filas = await _servicio.EliminarAsync(codigo);
            return Ok(new { estado = 200, mensaje = "Empresa eliminado exitosamente.", filasEliminadas = filas });
        }
        catch (ArgumentException e)
        {
            return StatusCode(400, new { estado = 400, mensaje = "Parametros invalidos.", detalle = e.Message });
        }
        catch (NoEncontradoExcepcion e)
        {
            return StatusCode(404, new { estado = 404, mensaje = "Empresa no encontrado.", detalle = e.Message });
        }
        catch (Exception e)
        {
            return StatusCode(500, new { estado = 500, mensaje = "Error interno.", detalle = e.Message });
        }
    }
}
