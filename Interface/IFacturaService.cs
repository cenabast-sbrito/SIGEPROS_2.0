using SIGEPROS.Models;

namespace SIGEPROS.Interface
{
    public interface IFacturaService
    {
        List<FacturaModel> GetFacturas(
            string rol,
            string rutProveedor,
            string nFactura,
            string ano,
            string cEstado);
    }
}
