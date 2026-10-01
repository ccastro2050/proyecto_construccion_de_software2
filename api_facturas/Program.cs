// ============================================================
// Program.cs — el PUNTO DE ENTRADA de la API (el "main" de .NET).
//
// Aquí se arma la aplicación: se registran los servicios (el
// ENSAMBLADOR de las capas), se configura cómo responder cuando
// una petición no valida (422), y se encienden las rutas.
//
// El recorrido completo de una petición está explicado en
// docs/FLUJO_DE_UNA_PETICION.md.
// ============================================================

// "using" trae tipos de otros espacios de nombres para poder usarlos:
using ApiFacturas.Repositorios;
using ApiFacturas.Servicios;
using Microsoft.AspNetCore.Mvc;

// El "builder" es el constructor de la aplicación: a él se le
// registra TODO antes de arrancar.
var builder = WebApplication.CreateBuilder(args);

// ------------------------------------------------------------
// 1. EL ENSAMBLADOR — el único lugar que conoce clases concretas
// ------------------------------------------------------------
// Aquí se le dice al contenedor de dependencias de .NET qué clase
// concreta entregar cuando alguien pida una INTERFAZ:
//   - pide IRepositorioProducto → recibe RepositorioProductoPostgres
//   - pide IServicioProducto    → recibe ServicioProducto
// El controlador y el servicio JAMÁS hacen "new" de clases concretas:
// las reciben por constructor (inyección de dependencias).
// Cuando la v5 agregue otro motor, SOLO estas líneas cambiarán.

// La cadena de conexión: viene de appsettings.json, y en Docker la
// sobreescribe la variable de entorno ConnectionStrings__Postgres.
var cadenaConexion = builder.Configuration.GetConnectionString("Postgres")
    ?? throw new InvalidOperationException("Falta la cadena de conexión 'Postgres'.");

// AddScoped = "una instancia por peticion HTTP" (cada request estrena la suya).
// Es el ensamblador, y crece de a una linea por recurso. Esa lista larga es
// deliberada: cuando llegue el SEGUNDO MOTOR -la v5- es el argumento de la
// fabrica, y el dolor de hoy es lo que la justifica.

// ------------------------------------------------------------
// LA v1 — las SEIS tablas SIN clave foranea
// ------------------------------------------------------------
// El criterio de la v1 es ese y no otro: ninguna de estas seis depende de
// otra fila para existir, asi que se pueden construir en cualquier orden.
builder.Services.AddScoped<IRepositorioProducto>(
    _ => new RepositorioProductoPostgres(cadenaConexion));
builder.Services.AddScoped<IServicioProducto, ServicioProducto>();
builder.Services.AddScoped<IRepositorioEmpresa>(
    _ => new RepositorioEmpresaPostgres(cadenaConexion));
builder.Services.AddScoped<IServicioEmpresa, ServicioEmpresa>();
builder.Services.AddScoped<IRepositorioPersona>(
    _ => new RepositorioPersonaPostgres(cadenaConexion));
builder.Services.AddScoped<IServicioPersona, ServicioPersona>();
builder.Services.AddScoped<IRepositorioRol>(
    _ => new RepositorioRolPostgres(cadenaConexion));
builder.Services.AddScoped<IServicioRol, ServicioRol>();
builder.Services.AddScoped<IRepositorioRuta>(
    _ => new RepositorioRutaPostgres(cadenaConexion));
builder.Services.AddScoped<IServicioRuta, ServicioRuta>();
builder.Services.AddScoped<IRepositorioUsuario>(
    _ => new RepositorioUsuarioPostgres(cadenaConexion));
builder.Services.AddScoped<IServicioUsuario, ServicioUsuario>();

// ------------------------------------------------------------
// LA v2 — las SEIS tablas CON clave foranea, y con ellas estan las 12
// ------------------------------------------------------------
// La v2 INCLUYE la v1: no se reinicia nada. Lo de arriba sigue en pie y esto
// se le suma.
//
// Aqui aparecen los procedimientos almacenados y el trigger: factura no se
// arma con INSERT sueltos, y usuario-con-roles tampoco.
builder.Services.AddScoped<IRepositorioCliente>(
    _ => new RepositorioClientePostgres(cadenaConexion));
builder.Services.AddScoped<IServicioCliente, ServicioCliente>();
builder.Services.AddScoped<IRepositorioVendedor>(
    _ => new RepositorioVendedorPostgres(cadenaConexion));
builder.Services.AddScoped<IServicioVendedor, ServicioVendedor>();
builder.Services.AddScoped<IRepositorioFactura>(
    _ => new RepositorioFacturaPostgres(cadenaConexion));
builder.Services.AddScoped<IServicioFactura, ServicioFactura>();
builder.Services.AddScoped<IRepositorioRolUsuario>(
    _ => new RepositorioRolUsuarioPostgres(cadenaConexion));
builder.Services.AddScoped<IServicioRolUsuario, ServicioRolUsuario>();
builder.Services.AddScoped<IRepositorioRutaRol>(
    _ => new RepositorioRutaRolPostgres(cadenaConexion));
builder.Services.AddScoped<IServicioRutaRol, ServicioRutaRol>();

// El recurso MAESTRO-DETALLE sobre la tabla puente: el usuario Y sus roles en
// una sola operacion (RF9). No es una tabla mas -son las mismas dos-, es otra
// forma de operarlas, y es la que usa la interfaz grafica.
builder.Services.AddScoped<IRepositorioUsuarioConRoles>(
    _ => new RepositorioUsuarioConRolesPostgres(cadenaConexion));
builder.Services.AddScoped<IServicioUsuarioConRoles, ServicioUsuarioConRoles>();

// ------------------------------------------------------------
// 2. Los controladores y la validación de la petición (el 422)
// ------------------------------------------------------------
// AddControllers activa el sistema de controladores ([ApiController]).
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(opciones =>
    {
        // Cuando un body NO cumple las reglas de su petición (las anotaciones
        // [Required], [Range]... de Peticiones/), ASP.NET arma solo la
        // respuesta de error. Aquí la personalizamos para que sea un
        // 422 con la lista de errores — el formato del contrato:
        opciones.InvalidModelStateResponseFactory = contexto =>
        {
            // Recorrer el ModelState y sacar cada mensaje de error:
            var errores = new List<string>();
            foreach (var campo in contexto.ModelState)
            {
                foreach (var error in campo.Value.Errors)
                {
                    errores.Add(error.ErrorMessage);
                }
            }
            // ObjectResult = "responde este objeto como JSON, con este código":
            return new ObjectResult(new
            {
                estado = 422,
                mensaje = "Datos inválidos.",
                errores
            })
            { StatusCode = 422 };
        };
    });

// ------------------------------------------------------------
// 2b. Swagger — la documentación interactiva de la API
// ------------------------------------------------------------
// Swashbuckle lee los controladores y sus clases de datos y genera una página
// donde se ven TODOS los endpoints y se pueden probar desde el
// navegador (http://localhost:8043/swagger).
builder.Services.AddEndpointsApiExplorer();   // descubre los endpoints
builder.Services.AddSwaggerGen();             // arma el documento OpenAPI

// Construir la aplicación con todo lo registrado:
var app = builder.Build();

// Encender Swagger: el JSON (OpenAPI) y la página interactiva:
app.UseSwagger();
app.UseSwaggerUI();

// ------------------------------------------------------------
// 3. Las rutas
// ------------------------------------------------------------

// GET / — diagnóstico (usable como healthcheck). MapGet registra una
// ruta directa sin necesidad de un controlador:
app.MapGet("/", () => Results.Json(new
{
    mensaje = "API Facturas funcionando",
    version = "v2",
    contratos = "docs/spec_kit/versiones/v2_con_fk/6_contracts.md"
}));

// MapControllers enciende las rutas declaradas con atributos en los
// controladores ([Route], [HttpGet], [HttpPost]...):
app.MapControllers();

// Arrancar y quedarse escuchando (el puerto lo fija ASPNETCORE_URLS
// en el Dockerfile: 8043):
app.Run();
