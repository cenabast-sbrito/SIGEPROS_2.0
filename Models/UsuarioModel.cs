namespace SIGEPROS.Models
{
    public class UsuarioModel
    {


        public string usuarioLDAP { get; set; }
        public string Rut { get; set; }
        public bool Estado { get; set; }
        public string Clave { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Organismo { get; set; }
        public String FechaCreacion { get; set; }
        public String email { get; set; }
        public string Rol { get; set; }
        public int acceso_PNACPACAM { get; set; }
        public int acceso_PNI { get; set; }
        public int esGestor { get; set; }
        public int esAdmin { get; set; }
        public int programa { get; set; }
        public int activo { get; set; }
        public String FechaActualizacion { get; set; }














        public double? rutUsuario { get; set; }  // nullable por si no se autentica
///            public string nombre { get; set; }
            public bool autenticado { get; set; }


        public string QryUsuario = "SELECT rut, Pwd clave, nombre, apellido, email, estado, rol FROM usuarios ";
        public string QryUsuarios = "SELECT rut, Pwd clave, nombre, apellido, email, estado, rol FROM usuarios where rut = @Rut;";
        public string QryAuthorization = "SELECT rut FROM usuarios where rut = @Rut and estado=1 and rut exist (SELECT [Rut] FROM [PNACPACAM].[dbo].[UsuarioRol] where rol = @rol) ;";
        public string QryAuthenticate = "SELECT rut from usuarios where rut = @Rut and @Pass =  Pwd ;";
    }
}
