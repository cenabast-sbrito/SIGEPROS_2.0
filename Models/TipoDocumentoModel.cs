namespace SIGEPROS.Models
{
    public class TipoDocumentoModel
    {
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public bool Activo { get; set; }
    }
}