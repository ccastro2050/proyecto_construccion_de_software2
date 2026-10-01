// ============================================================
// Usuario — el MODELO de la entidad CON UN SECRETO.
//
// Fíjese en lo que NO tiene: ninguna propiedad de contraseña.
//
// Y no es un olvido: es la regla. **Lo que no está en el modelo de lectura NO
// PUEDE filtrarse a una respuesta HTTP.** Ni la contraseña ni su hash viajan
// jamás. El secreto vive solo en la base, con hash, y lo maneja el
// repositorio.
//
// POR QUE IMPORTA AUNQUE SEA UN HASH Y NO LA CLAVE: porque un hash expuesto se
// puede romper SIN PRISA y FUERA DEL SISTEMA. Ya no hay que probar contraseñas
// contra la API —que es lento y se puede detectar— sino contra el hash, en una
// máquina propia, millones por segundo. bcrypt lo hace caro, no imposible.
//
// Y antes que la razón de seguridad hay una de diseño: una respuesta no debe
// traer datos que nadie le pidió.
// ============================================================

namespace ApiFacturas.Modelos;

public class Usuario
{
    /// <summary>El correo, que es la llave primaria — y lo ÚNICO que se lee.</summary>
    public required string Email { get; set; }
}
