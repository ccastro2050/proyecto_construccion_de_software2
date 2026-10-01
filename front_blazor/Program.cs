using FrontFacturas.Components;
using FrontFacturas.Servicios;

var builder = WebApplication.CreateBuilder(args);

// Blazor Server: el componente se renderiza en el servidor y el navegador
// recibe el HTML ya armado, manteniendo una conexión para los eventos.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// ============================================================
// DE DÓNDE SALEN LOS DATOS
//
// De la API, por HTTP, y de ningún otro sitio. La dirección viene de la
// configuración: fuera de Docker vale lo de appsettings.json; dentro, el
// compose la sobreescribe con el NOMBRE del servicio —`api-facturas`—, porque
// `localhost` dentro de un contenedor es el contenedor mismo.
// ============================================================
var urlApi = builder.Configuration["UrlApi"] ?? "http://localhost:8043";

builder.Services.AddHttpClient<ServicioProducto>(cliente =>
{
    cliente.BaseAddress = new Uri(urlApi);
    cliente.Timeout = TimeSpan.FromSeconds(10);
});

// ============================================================
// UN SERVICIO POR RECURSO
//
// Hoy hay uno porque la v1 construye una tabla como ejemplo trabajado. Cuando
// haya doce recursos habrá doce líneas aquí — no una que sirva para cualquier
// tabla. Un `ApiService` genérico con el nombre de la tabla como parámetro es
// más corto y es justo lo que no se hace.
// ============================================================

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();
