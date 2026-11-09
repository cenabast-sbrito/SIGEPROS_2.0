using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SIGEPROS.Models
{
    public class FuncionarioModel
    {
        public int id_funcionario { get; set; }
        public int id_programa { get; set; }

        [Display(Name = "Nombre de Usuario")]
        [Required(ErrorMessage = "Por favor, ingrese su nombre de usuario")]
        public string nombre_usuario { get; set; }

        [Display(Name = "Nombre de Programa")]
        [Required(ErrorMessage = "Por favor, ingrese el Programa")]
        public string nombre_programa { get; set; }

        [Display(Name = "Contraseña")]
        [Required(ErrorMessage = "Por favor, ingrese su contraseña")]
        public string password { get; set; }
        public string? nombre { get; set; }
        public string? apellido { get; set; }
        public string? email { get; set; }
        public string? anexo { get; set; }
        public string? departamento { get; set; }
        public string? subdepartamento { get; set; }
        public string? unidad { get; set; }
        public string? cargo { get; set; }
    }
}
