// ============================================================
// ErroresPostgres — el unico sitio que traduce los codigos del motor.
//
// PostgreSQL no lanza «ya existe» ni «no existe»: lanza un SQLSTATE, que es un
// codigo de cinco caracteres del estandar SQL. Los dos que importan aqui:
//
//   23503  foreign_key_violation  la fila a la que apunta la FK no existe,
//                                 o alguien quiere borrar una fila de la que
//                                 otra depende
//   23505  unique_violation       una clave repetida: el mismo codigo, o la
//                                 misma pareja en una tabla puente
//
// LOS DOS SON 409, Y NO 422. Y conviene decir por que, porque la tentacion es
// el 422: el dato que llego tiene la FORMA correcta -«P099» es un texto de la
// longitud permitida-. Lo que se rompe es el ESTADO de la base: esa fila no
// esta, o ya esta. El 422 se reserva para lo que la peticion puede rechazar
// sin consultar nada.
//
// POR QUE ESTA CLASE EXISTE, en un proyecto que evita las abstracciones de mas:
// porque sin ella la misma traduccion estaria copiada en treinta sitios, y el
// dia que haya que agregar un SQLSTATE habria que acordarse de los treinta.
//
// Y POR QUE ESTA EN EL REPOSITORIO y no en el controlador: porque esta capa es
// la que sabe de PostgreSQL. Arriba de aqui nadie conoce PostgresException —el
// servicio ve una ConflictoExcepcion, que es del dominio— y eso es lo que
// permite que manana el motor sea otro sin tocar el servicio.
// ============================================================

using ApiFacturas.Excepciones;
using Npgsql;

namespace ApiFacturas.Repositorios;

public static class ErroresPostgres
{
    /// <summary>Ejecuta la operacion y traduce los errores de integridad del
    /// motor a excepciones del dominio. Lo que no reconoce, lo deja subir tal
    /// cual: un 500 con el mensaje del motor es mejor que un 409 inventado.</summary>
    public static async Task<T> TraducirAsync<T>(Func<Task<T>> operacion)
    {
        try
        {
            return await operacion();
        }
        catch (PostgresException e) when (e.SqlState == "23503")
        {
            // El mensaje del motor viaja completo: trae el NOMBRE de la
            // restriccion -«fk_cliente_persona»-, que es la pista util.
            throw new ConflictoExcepcion(
                "La operacion rompe una relacion de la base de datos: "
                + e.MessageText);
        }
        catch (PostgresException e) when (e.SqlState == "23505")
        {
            throw new ConflictoExcepcion(
                "Ese registro ya existe: " + e.MessageText);
        }
    }
}
