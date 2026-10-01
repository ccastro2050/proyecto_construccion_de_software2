// ============================================================
// Programa.cs — Prueba de capas (criterio 10 de la v2).
//
// Verifica que ServicioProducto funciona con un repositorio
// FALSO en memoria que implementa IRepositorioProducto — sin
// PostgreSQL corriendo.
//
// Si esto pasa, las capas quedaron bien cortadas (polimorfismo +
// inversión de dependencias): el servicio depende de la INTERFAZ,
// no del motor.
//
// Este archivo usa "instrucciones de nivel superior" de C#: el
// programa se escribe directo, sin envolverlo en una clase Main.
// Regla del lenguaje: esas instrucciones van PRIMERO y las clases
// de apoyo (el repositorio falso) se declaran AL FINAL del archivo.
// ============================================================

using ApiFacturas.Excepciones;
using ApiFacturas.Modelos;
using ApiFacturas.Repositorios;
using ApiFacturas.Servicios;

// ------------------------------------------------------------
// La prueba: el MISMO ServicioProducto de la API, pero armado con
// el repositorio falso (polimorfismo). Un guion de verificaciones.
// ------------------------------------------------------------

var servicio = new ServicioProducto(new RepositorioFalsoEnMemoria());

// El ciclo completo contra el repositorio falso. Las lecturas
// devuelven OBJETOS Producto: se pregunta por sus propiedades.
await servicio.CrearAsync(new Producto { Codigo = "T1", Nombre = "Test", Stock = 5, Valorunitario = 100m });
Verificar((await servicio.ListarAsync(10))[0].Codigo == "T1", "crear + listar");
Verificar((await servicio.ObtenerAsync("T1")).Nombre == "Test", "obtener por código");
Verificar(await servicio.ActualizarAsync("T1", new() { ["stock"] = 9 }) == 1, "actualizar");
Verificar((await servicio.ObtenerAsync("T1")).Stock == 9, "el stock quedó en 9");
Verificar(await servicio.EliminarAsync("T1") == 1, "eliminar");

// Las excepciones de negocio también funcionan sin BD:
try { await servicio.ObtenerAsync("NOEXISTE"); Verificar(false, "debió lanzar NoEncontradoExcepcion"); }
catch (NoEncontradoExcepcion) { /* esperado */ }

try { await servicio.ActualizarAsync("T1", new()); Verificar(false, "debió lanzar ArgumentException"); }
catch (ArgumentException) { /* esperado */ }

try { await servicio.ListarAsync(0); Verificar(false, "debió lanzar ArgumentException"); }
catch (ArgumentException) { /* esperado */ }

// ------------------------------------------------------------
// La REGRESION de la v1: el mismo guion sobre PERSONA, que es uno
// de los seis recursos sin clave foránea. Si el molde de la v1 se
// replicó bien, esto pasa sin sorpresas.
// ------------------------------------------------------------

var servicioPersona = new ServicioPersona(new RepositorioPersonaFalsoEnMemoria());

await servicioPersona.CrearAsync(new Persona { Codigo = "T1", Nombre = "Test", Email = "t@t.co", Telefono = "300" });
Verificar((await servicioPersona.ListarAsync(10))[0].Codigo == "T1", "persona: crear + listar");
Verificar((await servicioPersona.ObtenerAsync("T1")).Nombre == "Test", "persona: obtener por código");
Verificar(await servicioPersona.ActualizarAsync("T1", new() { ["telefono"] = "301" }) == 1, "persona: actualizar");
Verificar((await servicioPersona.ObtenerAsync("T1")).Telefono == "301", "persona: el teléfono quedó en 301");
Verificar(await servicioPersona.EliminarAsync("T1") == 1, "persona: eliminar");

try { await servicioPersona.ObtenerAsync("NOEXISTE"); Verificar(false, "persona: debió lanzar NoEncontradoExcepcion"); }
catch (NoEncontradoExcepcion) { /* esperado */ }

try { await servicioPersona.ActualizarAsync("T1", new()); Verificar(false, "persona: debió lanzar ArgumentException"); }
catch (ArgumentException) { /* esperado */ }

// ------------------------------------------------------------
// v2 — AQUI ESTA LO NUEVO: un recurso CON CLAVE FORANEA.
//
// `cliente` tiene dos: fkcodpersona y fkcodempresa. Y el repositorio
// falso NO LAS COMPRUEBA — ni siquiera sabe que existen—, así que esta
// prueba crea un cliente que apunta a una persona inventada y PASA.
//
// Eso no es un hueco de la prueba: es exactamente lo que hay que ver.
// La integridad referencial vive EN LA BASE, no en el servicio. El
// servicio no la puede comprobar ni le toca, y por eso la misma
// operación contra PostgreSQL responde 409 (criterio 3) y aquí no.
//
// Si el servicio validara la FK, esta prueba fallaría — y querría decir
// que la regla está duplicada en dos sitios.
// ------------------------------------------------------------

var servicioCliente = new ServicioCliente(new RepositorioClienteFalsoEnMemoria());

await servicioCliente.CrearAsync(new Cliente
{
    Credito = 500000m,
    Fkcodpersona = "PERSONA_QUE_NO_EXISTE",   // el falso no tiene integridad referencial
    Fkcodempresa = null,                       // y el opcional es opcional de verdad
});
Verificar((await servicioCliente.ListarAsync(10)).Count == 1, "cliente: crear + listar");
Verificar((await servicioCliente.ListarAsync(10))[0].Fkcodempresa is null, "cliente: la empresa quedó en null");
Verificar(await servicioCliente.ActualizarAsync(1, new() { ["credito"] = 900000m }) == 1, "cliente: actualizar");
Verificar((await servicioCliente.ObtenerAsync(1)).Credito == 900000m, "cliente: el crédito quedó en 900000");
Verificar(await servicioCliente.EliminarAsync(1) == 1, "cliente: eliminar");

try { await servicioCliente.ObtenerAsync(999); Verificar(false, "cliente: debió lanzar NoEncontradoExcepcion"); }
catch (NoEncontradoExcepcion) { /* esperado */ }

try { await servicioCliente.ActualizarAsync(1, new()); Verificar(false, "cliente: debió lanzar ArgumentException"); }
catch (ArgumentException) { /* esperado */ }

Console.WriteLine("CRITERIO 10 OK: producto, persona y cliente funcionan con repositorios falsos, sin PostgreSQL");

// Mini-verificador (función local): si la condición es falsa, reporta
// y sale con error (terminar con 0 = pasó; con 1 = falló).
static void Verificar(bool condicion, string descripcion)
{
    if (!condicion)
    {
        Console.Error.WriteLine($"FALLÓ: {descripcion}");
        Environment.Exit(1);
    }
}

// ------------------------------------------------------------
// El REPOSITORIO FALSO: cumple el mismo contrato que el de SQL
// Server, pero guarda los Producto en un diccionario en memoria —
// cero SQL, cero red. Como el servicio depende de la INTERFAZ,
// no nota la diferencia.
// ------------------------------------------------------------
class RepositorioFalsoEnMemoria : IRepositorioProducto
{
    // El "almacén": código → Producto.
    private readonly Dictionary<string, Producto> _datos = new();

    // Los métodos del contrato son async (devuelven Task); como aquí
    // no hay nada que esperar, se responde con Task.FromResult
    // ("promesa ya cumplida con este valor"):

    public Task<List<Producto>> ObtenerTodosAsync(int limite)
    {
        // Ordenar por código y tomar los primeros 'limite' (como el TOP):
        var lista = _datos.Values.OrderBy(p => p.Codigo).Take(limite).ToList();
        return Task.FromResult(lista);
    }

    public Task<Producto?> ObtenerPorCodigoAsync(string codigo)
    {
        // TryGetValue busca la llave; si no está, producto queda null:
        _datos.TryGetValue(codigo, out var producto);
        return Task.FromResult(producto);
    }

    public Task CrearAsync(Producto producto)
    {
        _datos[producto.Codigo] = producto;
        return Task.CompletedTask;   // promesa cumplida, sin valor
    }

    public Task<int> ActualizarAsync(string codigo, Dictionary<string, object> datos)
    {
        if (!_datos.TryGetValue(codigo, out var producto))
        {
            return Task.FromResult(0);   // 0 filas = no existía
        }
        // Se escriben SOLO los campos que llegaron, con los setters
        // de las propiedades del modelo:
        if (datos.TryGetValue("nombre", out var nombre)) { producto.Nombre = (string)nombre; }
        if (datos.TryGetValue("stock", out var stock)) { producto.Stock = (int)stock; }
        if (datos.TryGetValue("valorunitario", out var valor)) { producto.Valorunitario = (decimal)valor; }
        return Task.FromResult(1);
    }

    public Task<int> EliminarAsync(string codigo)
    {
        // Remove devuelve true si la llave existía; se traduce a 1/0 filas:
        return Task.FromResult(_datos.Remove(codigo) ? 1 : 0);
    }
}

// ------------------------------------------------------------
// v2 — el repositorio falso de PERSONA: el gemelo del de arriba,
// calcado igual que se calcó toda la rebanada. Cumple
// IRepositorioPersona con un diccionario en memoria.
// ------------------------------------------------------------
class RepositorioPersonaFalsoEnMemoria : IRepositorioPersona
{
    private readonly Dictionary<string, Persona> _datos = new();

    public Task<List<Persona>> ObtenerTodasAsync(int limite)
    {
        var lista = _datos.Values.OrderBy(p => p.Codigo).Take(limite).ToList();
        return Task.FromResult(lista);
    }

    public Task<Persona?> ObtenerPorCodigoAsync(string codigo)
    {
        _datos.TryGetValue(codigo, out var persona);
        return Task.FromResult(persona);
    }

    public Task CrearAsync(Persona persona)
    {
        _datos[persona.Codigo] = persona;
        return Task.CompletedTask;
    }

    public Task<int> ActualizarAsync(string codigo, Dictionary<string, object> datos)
    {
        if (!_datos.TryGetValue(codigo, out var persona))
        {
            return Task.FromResult(0);
        }
        if (datos.TryGetValue("nombre", out var nombre)) { persona.Nombre = (string)nombre; }
        if (datos.TryGetValue("email", out var email)) { persona.Email = (string)email; }
        if (datos.TryGetValue("telefono", out var telefono)) { persona.Telefono = (string)telefono; }
        return Task.FromResult(1);
    }

    public Task<int> EliminarAsync(string codigo)
    {
        return Task.FromResult(_datos.Remove(codigo) ? 1 : 0);
    }
}

// ------------------------------------------------------------
// v2 — el repositorio falso de CLIENTE.
//
// Dos cosas lo diferencian de los de arriba, y las dos son de la v2:
//
//   1. LA CLAVE LA GENERA EL REPOSITORIO, no quien crea. En PostgreSQL
//      la pone el SERIAL; aquí, un contador. El servicio no la fija en
//      ninguno de los dos casos — y por eso no nota la diferencia.
//
//   2. NO HAY INTEGRIDAD REFERENCIAL. `fkcodpersona` es un texto y nada
//      más: nadie comprueba que esa persona exista. Contra PostgreSQL,
//      la misma operación responde 409. La regla vive en la BASE.
// ------------------------------------------------------------
class RepositorioClienteFalsoEnMemoria : IRepositorioCliente
{
    private readonly Dictionary<int, Cliente> _datos = new();
    private int _siguienteId = 1;

    public Task<List<Cliente>> ObtenerTodosAsync(int limite)
    {
        var lista = _datos.Values.OrderBy(c => c.Id).Take(limite).ToList();
        return Task.FromResult(lista);
    }

    public Task<Cliente?> ObtenerPorIdAsync(int id)
    {
        _datos.TryGetValue(id, out var cliente);
        return Task.FromResult(cliente);
    }

    public Task CrearAsync(Cliente entidad)
    {
        // El SERIAL, imitado: la clave la pone quien guarda, no quien pide.
        entidad.Id = _siguienteId++;
        _datos[entidad.Id] = entidad;
        return Task.CompletedTask;
    }

    public Task<int> ActualizarAsync(int id, Dictionary<string, object> datos)
    {
        if (!_datos.TryGetValue(id, out var cliente))
        {
            return Task.FromResult(0);
        }
        if (datos.TryGetValue("credito", out var credito)) { cliente.Credito = (decimal)credito; }
        if (datos.TryGetValue("fkcodpersona", out var p)) { cliente.Fkcodpersona = (string)p; }
        if (datos.TryGetValue("fkcodempresa", out var e))
        {
            // DBNull es lo que el controlador manda para decir «ponga NULL».
            cliente.Fkcodempresa = e is DBNull ? null : (string?)e;
        }
        return Task.FromResult(1);
    }

    public Task<int> EliminarAsync(int id)
    {
        return Task.FromResult(_datos.Remove(id) ? 1 : 0);
    }
}
