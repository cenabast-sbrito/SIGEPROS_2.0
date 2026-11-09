namespace SIGEPROS.Models
{
    public class ResultadoProcesoPdf
    {
        public bool Exitoso { get; set; }

        public string DocumentoVenta { get; set; }

        public string Mensaje { get; set; }

        public string RutaArchivo { get; set; }
    }
}
