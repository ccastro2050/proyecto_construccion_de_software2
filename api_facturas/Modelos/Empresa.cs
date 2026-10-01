// ============================================================
// Empresa — el MODELO de la tabla `empresa` (la clase entidad).
//
// Modelo = clase ENTIDAD: una por tabla. Los body de los verbos NO
// son modelos: son PETICIONES y viven en Peticiones/.
//
// Mismo patron que Producto.cs — y eso es el punto: la v1 son SEIS
// rebanadas verticales identicas salvo los campos.
// ============================================================

namespace ApiFacturas.Modelos;

public class Empresa
{
    /// <summary>Codigo (la llave primaria).</summary>
    public required string Codigo { get; set; }

    /// <summary>Nombre.</summary>
    public required string Nombre { get; set; }
}
