// ============================================================
// RolReemplazo — la PETICION del PUT (reemplazo COMPLETO).
//
// Exige TODOS los campos. Un PUT con body parcial muere en 422
// ANTES de llegar al controlador — esa es la semantica de PUT, y
// queda escrita aqui, no en un comentario.
//
// La llave va en la RUTA, no en el body.
// ============================================================

using System.ComponentModel.DataAnnotations;

namespace ApiFacturas.Peticiones;

public class RolReemplazo
{
    [Required(ErrorMessage = "El campo nombre es obligatorio.")]
    [StringLength(50, MinimumLength = 1,
        ErrorMessage = "El campo nombre debe tener entre 1 y 50 caracteres.")]
    public string? Nombre { get; set; }
}
