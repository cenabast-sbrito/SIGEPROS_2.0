namespace SIGEPROS.Models.DTO
{
    public class ResultadoCargaDto
    {
        public bool Status { get; set; }
        public string Message { get; set; }

        public int Count { get; set; }
        public int CountNoCero { get; set; }

        public int CountRechazos { get; set; }

        public int CountFilasFueraDelPeriodo { get; set; }
        public int CountFilasSinFactura { get; set; }
        public int CountFilasOtroProveedor { get; set; }
        public int CountFilasOtroCanal { get; set; }

        public int CountFacturasAceptadas { get; set; }
        /////public int CountFacturasNOSAP { get; set; }
    }
}