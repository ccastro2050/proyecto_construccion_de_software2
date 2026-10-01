// ============================================================
// RutaReemplazo — la PETICION del PUT (reemplazo COMPLETO).
//
// Exige TODOS los campos. Un PUT con body parcial muere en 422
// ANTES de llegar al controlador — esa es la semantica de PUT, y
// queda escrita aqui, no en un comentario.
//
// La llave va en la RUTA, no en el body.
// ============================================================

using System.ComponentModel.DataAnnotations;

namespace ApiFacturas.Peticiones;

public class RutaReemplazo
{
    [Required(ErrorMessage = "El campo ruta es obligatorio.")]
    [StringLength(100, MinimumLength = 1,
        ErrorMessage = "El campo ruta debe tener entre 1 y 100 caracteres.")]
    public string? RutaTexto { get; set; }

    [Required(ErrorMessage = "El campo descripcion es obligatorio.")]
    [StringLength(200, MinimumLength = 1,
        ErrorMessage = "El campo descripcion debe tener entre 1 y 200 caracteres.")]
    public string? Descripcion { get; set; }
}
