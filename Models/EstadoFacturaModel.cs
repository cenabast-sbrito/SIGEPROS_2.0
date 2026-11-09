namespace SIGEPROS.Models
{
    public class EstadoFacturaModel
    {
        public string RutProveedor { get; set; }
        public string NFactura { get; set; }
        public string EnRevisionCenabast { get; set; }
        public string DerivadoAContratos { get; set; }
        public string ListoParaRevisionMinsal { get; set; }
        public string EnRevisionMinsal { get; set; }
        public string Rechazado { get; set; }
        public string Aceptado { get; set; }
        public string AceptadoConObservacion { get; set; }
        public string EnGestionPago { get; set; }
        public string Expediente { get; set; }
        public string Certificado { get; set; }
        public string FacturaAdjunta { get; set; }
    }

}
