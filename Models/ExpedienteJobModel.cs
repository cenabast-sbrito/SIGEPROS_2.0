namespace SIGEPROS.Models
{
    public class ExpedienteJobModel
    {
        public long IdJob { get; set; }
        public string Estado { get; set; }
        public int Porcentaje { get; set; }
        public string Mensaje { get; set; }
        public string RutaExpediente { get; set; }
    }
}