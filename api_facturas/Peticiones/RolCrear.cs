// ============================================================
// RolCrear — la PETICION del POST (el body para crear).
//
// Esto NO es un modelo: es la FRONTERA DE ENTRADA. ASP.NET valida
// el body contra estas anotaciones ANTES de que el controlador lo
// toque; si algo no cumple, responde 422 y el dato malo jamas llega
// al servicio ni a la base.
// ============================================================

using System.ComponentModel.DataAnnotations;

namespace ApiFacturas.Peticiones;

public class RolCrear
{
    // La llave `id` NO va en el body: la genera la base (SERIAL).

    [Required(ErrorMessage = "El campo nombre es obligatorio.")]
    [StringLength(50, MinimumLength = 1,
        ErrorMessage = "El campo nombre debe tener entre 1 y 50 caracteres.")]
    public string? Nombre { get; set; }
}
