// ============================================================
// Usuario — el MODELO de la tabla `usuario` (la clase entidad).
//
// Modelo = clase ENTIDAD: una por tabla. Los body de los verbos NO
// son modelos: son PETICIONES y viven en Peticiones/.
//
// Mismo patron que Producto.cs — y eso es el punto: la v1 son SEIS
// rebanadas verticales identicas salvo los campos.
// ============================================================

namespace ApiFacturas.Modelos;

public class Usuario
{
    /// <summary>Email (la llave primaria).</summary>
    public required string Email { get; set; }

    /// <summary>Contrasena.</summary>
    public required string Contrasena { get; set; }
}
