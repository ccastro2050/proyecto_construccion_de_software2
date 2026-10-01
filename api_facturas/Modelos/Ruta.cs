// ============================================================
// Ruta — el MODELO de la tabla `ruta` (la clase entidad).
//
// Modelo = clase ENTIDAD: una por tabla. Los body de los verbos NO
// son modelos: son PETICIONES y viven en Peticiones/.
//
// Mismo patron que Producto.cs — y eso es el punto: la v1 son SEIS
// rebanadas verticales identicas salvo los campos.
// ============================================================

namespace ApiFacturas.Modelos;

public class Ruta
{
    /// <summary>Id (la llave primaria).
    ///
    /// NO lleva `required`, y eso es deliberado: la columna es SERIAL y la
    /// genera la BASE. Si fuera obligatorio, el POST tendria que inventarle
    /// un id — que es justo lo que no debe hacer.</summary>
    public int Id { get; set; }

    /// <summary>Ruta.</summary>
    public required string RutaTexto { get; set; }

    /// <summary>Descripcion.</summary>
    public required string Descripcion { get; set; }
}
