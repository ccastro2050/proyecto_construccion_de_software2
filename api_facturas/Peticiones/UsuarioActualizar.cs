// ============================================================
// UsuarioActualizar — la PETICION del PATCH (parcial).
//
// NINGUN campo es obligatorio: el que llegue SI se valida. El
// contraste con UsuarioReemplazo es la leccion del verbo — el MISMO
// body falla en PUT (le faltan campos) y pasa en PATCH.
//
// Si el body llega vacio ({}), eso NO es problema de forma sino de
// negocio: lo decide el servicio con un 400.
// ============================================================

using System.ComponentModel.DataAnnotations;

namespace ApiFacturas.Peticiones;

public class UsuarioActualizar
{
    [StringLength(200, MinimumLength = 1,
        ErrorMessage = "El campo contrasena debe tener entre 1 y 200 caracteres.")]
    public string? Contrasena { get; set; }
}
