namespace SIGEPROS.Models
{

    public class Proveedor
    {
        public int RutProveedor { get; set; }
        public string Nombre { get; set; }
        public string Direccion { get; set; }
        public int Estado { get; set; }
        public string FechaCreacion { get; set; }
        public string FechaActualizacion { get; set; }
        public int CPrograma { get; set; }
        public string TPrograma { get; set; }

    }

}