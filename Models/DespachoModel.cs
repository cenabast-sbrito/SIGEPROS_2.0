namespace SIGEPROS.Models
{
    public class DespachoModel
    {
        public string LIFNR { get; set; }
        public string NFactura { get; set; }
        public string DocumentoVenta { get; set; }
        public string PedidoCompra { get; set; }
        public string GrupoArticulos { get; set; }
        public string Canal { get; set; }
        public string DescripcionCanal { get; set; }
        public string GuiaDespacho { get; set; }
        public string CodigoMaterial { get; set; }
        public string ValorNeto { get; set; }
        public string Denominacion { get; set; }
        public string Lote { get; set; }
        public string CantidadDocumento { get; set; }
        public string CantidadInformada { get; set; }
        public string ZZIDPRO_E { get; set; }
        public string ZZIDMER_E { get; set; }
        public string RutEstablecimiento { get; set; }
        public string Destinatario { get; set; }
        public string ServicioSalud { get; set; }
        public string linkCedible { get; set; }
    }
}
