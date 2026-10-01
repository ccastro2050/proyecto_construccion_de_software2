using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FrontFacturas.Modelos;

namespace FrontFacturas.Servicios;

// ============================================================
// ServicioVendedor — CRUD completo con UNA clave foranea obligatoria.
// ============================================================
public class ServicioVendedor(HttpClient cliente)
{
    private const string Ruta = "api/vendedor";

    private static readonly JsonSerializerOptions Opciones =
        new() { PropertyNameCaseInsensitive = true };

    /// <summary>O salio bien, o hay un mensaje para mostrarle a la persona.
    /// Nunca una excepcion suelta que tumbe la interfaz grafica.</summary>
    public record Resultado<T>(bool Bien, T? Dato, string? Mensaje)
    {
        public static Resultado<T> Ok(T dato) => new(true, dato, null);
        public static Resultado<T> Falla(string mensaje) => new(false, default, mensaje);
    }

    public async Task<Resultado<List<Vendedor>>> ListarAsync()
    {
        try
        {
            var r = await cliente.GetAsync(Ruta);
            if (r.StatusCode == HttpStatusCode.NoContent)
                return Resultado<List<Vendedor>>.Ok([]);
            if (!r.IsSuccessStatusCode)
                return Resultado<List<Vendedor>>.Falla(await MensajeDe(r));
            var sobre = await r.Content.ReadFromJsonAsync<JsonElement>();
            var datos = sobre.GetProperty("datos")
                             .Deserialize<List<Vendedor>>(Opciones) ?? [];
            return Resultado<List<Vendedor>>.Ok(datos);
        }
        catch (Exception)
        {
            return Resultado<List<Vendedor>>.Falla(
                "No se pudo conectar con el servicio. Intente de nuevo en un momento.");
        }
    }

    public async Task<Resultado<Vendedor>> CrearAsync(Vendedor e)
    {
        try
        {
            var r = await cliente.PostAsJsonAsync(Ruta, e);
            if (!r.IsSuccessStatusCode)
                return Resultado<Vendedor>.Falla(await MensajeDe(r));
            return Resultado<Vendedor>.Ok(e);
        }
        catch (Exception)
        {
            return Resultado<Vendedor>.Falla("No se pudo conectar con el servicio.");
        }
    }

    public async Task<Resultado<Vendedor>> ReemplazarAsync(int clave, Vendedor e)
    {
        try
        {
            var r = await cliente.PutAsJsonAsync($"{Ruta}/{clave}", e);
            if (!r.IsSuccessStatusCode)
                return Resultado<Vendedor>.Falla(await MensajeDe(r));
            return Resultado<Vendedor>.Ok(e);
        }
        catch (Exception)
        {
            return Resultado<Vendedor>.Falla("No se pudo conectar con el servicio.");
        }
    }

    public async Task<Resultado<bool>> ActualizarAsync(int clave, object parcial)
    {
        try
        {
            var r = await cliente.PatchAsJsonAsync($"{Ruta}/{clave}", parcial);
            if (!r.IsSuccessStatusCode)
                return Resultado<bool>.Falla(await MensajeDe(r));
            return Resultado<bool>.Ok(true);
        }
        catch (Exception)
        {
            return Resultado<bool>.Falla("No se pudo conectar con el servicio.");
        }
    }

    public async Task<Resultado<bool>> EliminarAsync(int clave)
    {
        try
        {
            var r = await cliente.DeleteAsync($"{Ruta}/{clave}");
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
            HttpStatusCode.NotFound => "No se encontro ese vendedor.",
            HttpStatusCode.UnprocessableEntity => "Faltan datos obligatorios.",
            HttpStatusCode.BadRequest => "Los datos enviados no son validos.",
            HttpStatusCode.Conflict => "La persona que eligio ya no existe.",
            _ => "El servicio respondio con un problema. Intente de nuevo."
        };
    }
}
