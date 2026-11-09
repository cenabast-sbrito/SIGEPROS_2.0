namespace SIGEPROS.Models
{
    public class ProveedorModel
    {
        public int? RutProveedor { get; set; }
        public bool? Estado { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        public int? CPrograma { get; set; }
        public string Nombre { get; set; }
        public string Direccion { get; set; }
        public string TPrograma { get; set; }
    }
}
