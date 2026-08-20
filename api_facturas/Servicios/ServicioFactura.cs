// ============================================================
// ServicioFactura — la capa de NEGOCIO de factura.
//
// Note lo DELGADO que es comparado con ServicioProducto: aquí no
// hay cálculos porque la regla RNF2 de la v2 lo prohíbe — los
// subtotales, el total y el stock son de la BD (trigger + SPs).
// El servicio solo cuida los argumentos y delega por la interfaz.
// ============================================================

using ApiFacturas.Modelos;
using ApiFacturas.Repositorios;

namespace ApiFacturas.Servicios;

public class ServicioFactura : IServicioFactura
{
    private readonly IRepositorioFactura _repositorio;

    public ServicioFactura(IRepositorioFactura repositorio)
    {
        _repositorio = repositorio;
    }

    private static void ValidarNumero(int numero)
    {
        if (numero <= 0)
        {
            throw new ArgumentException("El número de factura debe ser un entero mayor que cero.");
        }
    }

    public async Task<List<Factura>> ListarAsync()
    {
        return await _repositorio.ListarAsync();
    }

    public async Task<Factura> ConsultarAsync(int numero)
    {
        ValidarNumero(numero);
        // Si no existe, el REPOSITORIO ya lanza NoEncontradoExcepcion
        // (tradujo el RAISE EXCEPTION del SP) — aquí no hay nada más que hacer:
        return await _repositorio.ConsultarAsync(numero);
    }

    public async Task<Factura> CrearAsync(int fkidcliente, int fkidvendedor, string productosJson)
    {
        // El body ya pasó por la petición FacturaCrear (ids presentes,
        // lista con mínimo 1 renglón, cantidades >= 1). Si el cliente o
        // el vendedor no existen, la FK de la BD rechaza (500); si falta
        // stock, el TRIGGER rechaza con su mensaje (500). Delegar:
        return await _repositorio.CrearAsync(fkidcliente, fkidvendedor, productosJson);
    }

    public async Task<string> AnularAsync(int numero)
    {
        ValidarNumero(numero);
        return await _repositorio.AnularAsync(numero);
    }
}
