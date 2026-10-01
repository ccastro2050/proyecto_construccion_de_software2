using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FrontFacturas.Modelos;

namespace FrontFacturas.Servicios;

// ============================================================
// ServicioRolUsuario — una tabla PUENTE, y por eso le faltan dos verbos.
//
//   ListarAsync   GET    /api/rol-usuario
//   CrearAsync    POST   /api/rol-usuario
//   EliminarAsync DELETE /api/rol-usuario/{email}/{idrol}   <- DOS claves
//
// No hay ReemplazarAsync ni ActualizarAsync: una asignacion no tiene campos
// que cambiar. Existe o no existe. Cambiar «Ana es vendedora» por «Ana es
// administradora» es quitar una pareja y poner otra.
//
// OJO CON LA RUTA: es `api/rol-usuario` CON GUION, aunque la tabla se llame
// `rol_usuario` con subrayado. Asumir el nombre de la tabla da 404 — paso.
// ============================================================
public class ServicioRolUsuario(HttpClient cliente)
{
    private const string Ruta = "api/rol-usuario";

    private static readonly JsonSerializerOptions Opciones =
        new() { PropertyNameCaseInsensitive = true };

    /// <summary>O salio bien, o hay un mensaje para mostrarle a la persona.
    /// Nunca una excepcion suelta que tumbe la interfaz grafica.</summary>
    public record Resultado<T>(bool Bien, T? Dato, string? Mensaje)
    {
        public static Resultado<T> Ok(T dato) => new(true, dato, null);
        public static Resultado<T> Falla(string mensaje) => new(false, default, mensaje);
    }

    public async Task<Resultado<List<RolUsuario>>> ListarAsync()
    {
        try
        {
            var r = await cliente.GetAsync(Ruta);
            if (r.StatusCode == HttpStatusCode.NoContent)
                return Resultado<List<RolUsuario>>.Ok([]);
            if (!r.IsSuccessStatusCode)
                return Resultado<List<RolUsuario>>.Falla(await MensajeDe(r));
            var sobre = await r.Content.ReadFromJsonAsync<JsonElement>();
            var datos = sobre.GetProperty("datos")
                             .Deserialize<List<RolUsuario>>(Opciones) ?? [];
            return Resultado<List<RolUsuario>>.Ok(datos);
        }
        catch (Exception)
        {
            return Resultado<List<RolUsuario>>.Falla(
                "No se pudo conectar con el servicio. Intente de nuevo en un momento.");
        }
    }

    public async Task<Resultado<bool>> CrearAsync(RolUsuario e)
    {
        try
        {
            var r = await cliente.PostAsJsonAsync(Ruta, e);
            if (!r.IsSuccessStatusCode)
                return Resultado<bool>.Falla(await MensajeDe(r));
            return Resultado<bool>.Ok(true);
        }
        catch (Exception)
        {
            return Resultado<bool>.Falla("No se pudo conectar con el servicio.");
        }
    }

    /// <summary>El borrado necesita LAS DOS claves, porque la PK es compuesta.</summary>
    public async Task<Resultado<bool>> EliminarAsync(string email, int idrol)
    {
        try
        {
            var r = await cliente.DeleteAsync($"{Ruta}/{email}/{idrol}");
            if (!r.IsSuccessStatusCode)
                return Resultado<bool>.Falla(await MensajeDe(r));
            return Resultado<bool>.Ok(true);
        }
        catch (Exception)
        {
            return Resultado<bool>.Falla("No se pudo conectar con el servicio.");
        }
    }

    private static async Task<string> MensajeDe(HttpResponseMessage r)
    {
        try
        {
            var e = await r.Content.ReadFromJsonAsync<ErrorApi>(Opciones);
            if (!string.IsNullOrWhiteSpace(e?.Mensaje)) return e!.Mensaje;
        }
        catch { }

        return r.StatusCode switch
        {
            HttpStatusCode.NotFound => "No se encontro esa asignacion.",
            HttpStatusCode.UnprocessableEntity => "Elija un usuario y un rol.",
            HttpStatusCode.BadRequest => "Los datos enviados no son validos.",
            // 409 aqui tiene DOS causas, y conviene no mentir sobre cual:
            // la pareja ya existe (PK compuesta repetida), o el usuario/rol
            // ya no existe (FK).
            HttpStatusCode.Conflict => "Ese usuario ya tiene ese rol, o uno de los dos ya no existe.",
            _ => "El servicio respondio con un problema. Intente de nuevo."
        };
    }
}
