using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FrontFacturas.Modelos;

namespace FrontFacturas.Servicios;

// ============================================================
// ServicioUsuarioConRoles — el servicio del formulario INTEGRADO.
//
//   ListarAsync      GET    /api/usuario-con-roles
//   CrearAsync       POST   /api/usuario-con-roles
//   ReemplazarAsync  PUT    /api/usuario-con-roles/{email}
//   EliminarAsync    DELETE /api/usuario-con-roles/{email}
//
// Por que no se usa ServicioUsuario + ServicioRolUsuario en dos llamadas: si
// la segunda falla, queda un usuario sin ningun rol -que no puede hacer nada y
// nadie sabe que esta ahi-. El procedimiento almacenado hace las dos cosas en
// UNA transaccion: o entra todo, o no entra nada.
// ============================================================
public class ServicioUsuarioConRoles(HttpClient cliente)
{
    private const string Ruta = "api/usuario-con-roles";

    private static readonly JsonSerializerOptions Opciones =
        new() { PropertyNameCaseInsensitive = true };

    public record Resultado<T>(bool Bien, T? Dato, string? Mensaje)
    {
        public static Resultado<T> Ok(T dato) => new(true, dato, null);
        public static Resultado<T> Falla(string mensaje) => new(false, default, mensaje);
    }

    public async Task<Resultado<List<UsuarioConRoles>>> ListarAsync()
    {
        try
        {
            var r = await cliente.GetAsync(Ruta);
            if (r.StatusCode == HttpStatusCode.NoContent)
                return Resultado<List<UsuarioConRoles>>.Ok([]);
            if (!r.IsSuccessStatusCode)
                return Resultado<List<UsuarioConRoles>>.Falla(await MensajeDe(r));
            var sobre = await r.Content.ReadFromJsonAsync<JsonElement>();
            var datos = sobre.GetProperty("datos")
                             .Deserialize<List<UsuarioConRoles>>(Opciones) ?? [];
            return Resultado<List<UsuarioConRoles>>.Ok(datos);
        }
        catch (Exception)
        {
            return Resultado<List<UsuarioConRoles>>.Falla(
                "No se pudo conectar con el servicio. Intente de nuevo en un momento.");
        }
    }

    public async Task<Resultado<bool>> CrearAsync(UsuarioConRolesEnvio e)
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

    /// <summary>Reemplaza los roles. La contrasena VACIA significa «dejela como
    /// esta»: se manda igual, y el procedimiento no la toca.</summary>
    public async Task<Resultado<bool>> ReemplazarAsync(string email, string? contrasena, List<int> roles)
    {
        try
        {
            var r = await cliente.PutAsJsonAsync($"{Ruta}/{email}",
                new { contrasena, roles });
            if (!r.IsSuccessStatusCode)
                return Resultado<bool>.Falla(await MensajeDe(r));
            return Resultado<bool>.Ok(true);
        }
        catch (Exception)
        {
            return Resultado<bool>.Falla("No se pudo conectar con el servicio.");
        }
    }

    public async Task<Resultado<bool>> EliminarAsync(string email)
    {
        try
        {
            var r = await cliente.DeleteAsync($"{Ruta}/{email}");
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
            HttpStatusCode.NotFound => "No se encontro ese usuario.",
            HttpStatusCode.UnprocessableEntity => "Faltan datos: el correo, la contrasena y al menos un rol.",
            HttpStatusCode.BadRequest => "Los datos enviados no son validos.",
            HttpStatusCode.Conflict => "Ese correo ya esta registrado, o uno de los roles ya no existe.",
            _ => "El servicio respondio con un problema. Intente de nuevo."
        };
    }
}
