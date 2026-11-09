namespace SIGEPROS.Models
{
    public class FacturaModel
    {
        public long? pedidoCompra { get; set; }

        public long? cMaterial { get; set; }
        public string denominacion { get; set; }

        public long? rutProveedor { get; set; }
        public string nombreProveedor { get; set; }

        public long? nFactura { get; set; }
        public int? ano { get; set; }
        public int? total { get; set; }

        public DateTime? expediente { get; set; }

        public string expedienteTexto =>
            expediente?.ToString("dd-MM-yyyy") ?? ""; public int? cEstado { get; set; }

        public int? estadoOK { get; set; }
        public int? cEstadoOKObservaciones { get; set; }
        public int? estadoNOOK { get; set; }

        public string tEstado { get; set; }
        public string tEstadoTiempoPresente { get; set; }
        public string tObservacion { get; set; }

        public string tEstadoOK { get; set; }
        public string tEstadoOKObservaciones { get; set; }
        public string tEstadoNOOK { get; set; }

        public bool? puedeEjecutarOK { get; set; }
        public bool? puedeEjecutarNOOK { get; set; }
        public bool? puedeVerOK { get; set; }
        public bool? puedeVerNOOK { get; set; }
        public string linkExpedientes { get; set; }
        public bool ExisteExpediente { get; set; }

        public string linkComision { get; set; }
        public bool ExisteComision { get; set; }

        public string linkDocumentoAdjunto { get; set; }
        public bool ExisteDocumentoAdjunto { get; set; }
    }
    public class FacturaLista
    {
        public string factura { get; set; }
    }
}
