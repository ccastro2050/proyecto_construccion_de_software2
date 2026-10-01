// ============================================================
// RutaActualizar — la PETICION del PATCH (parcial).
//
// NINGUN campo es obligatorio: el que llegue SI se valida. El
// contraste con RutaReemplazo es la leccion del verbo — el MISMO
// body falla en PUT (le faltan campos) y pasa en PATCH.
//
// Si el body llega vacio ({}), eso NO es problema de forma sino de
// negocio: lo decide el servicio con un 400.
// ============================================================

using System.ComponentModel.DataAnnotations;

namespace ApiFacturas.Peticiones;

public class RutaActualizar
{
    [StringLength(100, MinimumLength = 1,
        ErrorMessage = "El campo ruta debe tener entre 1 y 100 caracteres.")]
    public string? RutaTexto { get; set; }

    [StringLength(200, MinimumLength = 1,
        ErrorMessage = "El campo descripcion debe tener entre 1 y 200 caracteres.")]
    public string? Descripcion { get; set; }
}
