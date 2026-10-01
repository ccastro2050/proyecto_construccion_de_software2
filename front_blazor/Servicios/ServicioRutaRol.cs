using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FrontFacturas.Modelos;

namespace FrontFacturas.Servicios;

// ============================================================
// ServicioRutaRol — la segunda tabla puente. Mismo trato que RolUsuario.
//
// Aqui la ruta SI es de una palabra: `api/rutarol`, sin guion. No es un
// descuido: es el nombre de la tabla, y la tabla no lleva subrayado. Vale la
// pena mirar el [Route] del controlador antes de escribirla.
// ============================================================
public class ServicioRutaRol(HttpClient cliente)
{
    private const string Ruta = "api/rutarol";

    private static readonly JsonSerializerOptions Opciones =
        new() { PropertyNameCaseInsensitive = true };

    /// <summary>O salio bien, o hay un mensaje para mostrarle a la persona.
    /// Nunca una excepcion suelta que tumbe la interfaz grafica.</summary>
    public record Resultado<T>(bool Bien, T? Dato, string? Mensaje)
    {
        public static Resultado<T> Ok(T dato) => new(true, dato, null);
        public static Resultado<T> Falla(string mensaje) => new(false, default, mensaje);
    }

    public async Task<Resultado<List<RutaRol>>> ListarAsync()
    {
        try
        {
            var r = await cliente.GetAsync(Ruta);
            if (r.StatusCode == HttpStatusCode.NoContent)
                return Resultado<List<RutaRol>>.Ok([]);
            if (!r.IsSuccessStatusCode)
                return Resultado<List<RutaRol>>.Falla(await MensajeDe(r));
            var sobre = await r.Content.ReadFromJsonAsync<JsonElement>();
            var datos = sobre.GetProperty("datos")
                             .Deserialize<List<RutaRol>>(Opciones) ?? [];
            return Resultado<List<RutaRol>>.Ok(datos);
        }
        catch (Exception)
        {
            return Resultado<List<RutaRol>>.Falla(
                "No se pudo conectar con el servicio. Intente de nuevo en un momento.");
        }
    }

    public async Task<Resultado<bool>> CrearAsync(RutaRol e)
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

    public async Task<Resultado<bool>> EliminarAsync(int idruta, int idrol)
    {
        try
        {
            var r = await cliente.DeleteAsync($"{Ruta}/{idruta}/{idrol}");
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
            HttpStatusCode.NotFound => "No se encontro ese permiso.",
            HttpStatusCode.UnprocessableEntity => "Elija una interfaz y un rol.",
            HttpStatusCode.BadRequest => "Los datos enviados no son validos.",
            HttpStatusCode.Conflict => "Ese rol ya tiene ese permiso, o uno de los dos ya no existe.",
            _ => "El servicio respondio con un problema. Intente de nuevo."
        };
    }
}
