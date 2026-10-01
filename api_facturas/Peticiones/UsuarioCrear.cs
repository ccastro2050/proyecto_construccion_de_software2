// ============================================================
// UsuarioCrear — la PETICION del POST (el body para crear).
//
// Esto NO es un modelo: es la FRONTERA DE ENTRADA. ASP.NET valida
// el body contra estas anotaciones ANTES de que el controlador lo
// toque; si algo no cumple, responde 422 y el dato malo jamas llega
// al servicio ni a la base.
// ============================================================

using System.ComponentModel.DataAnnotations;

namespace ApiFacturas.Peticiones;

public class UsuarioCrear
{
    [Required(ErrorMessage = "El campo email es obligatorio.")]
    [StringLength(100, MinimumLength = 1,
        ErrorMessage = "El campo email debe tener entre 1 y 100 caracteres.")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "El campo contrasena es obligatorio.")]
    [StringLength(200, MinimumLength = 1,
        ErrorMessage = "El campo contrasena debe tener entre 1 y 200 caracteres.")]
    public string? Contrasena { get; set; }
}
