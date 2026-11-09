using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using SIGEPROS.Models;
using SIGEPROS.Services;

namespace SIGEPROS.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly DBUsuarioService _dBUsuarioService;
        private readonly DBFacturaService _dBFacturaService;

        public UsuarioController(IHttpContextAccessor httpContextAccessor, DBUsuarioService dBUsuarioService, DBFacturaService dBFacturaService)
        {
            _httpContextAccessor = httpContextAccessor;
            _dBUsuarioService = dBUsuarioService;
            _dBFacturaService = dBFacturaService;
        }
        public IActionResult Index()
        {
            return View();
        }
        /* valido para mantenedor */
        public JsonResult getUsuarios()
        {

            //var rolSeleccionado = _httpContextAccessor.HttpContext?.User?.FindFirst("rolSeleccionado")?.Value;
            //int programa = (rolSeleccionado == "ADM") ? 0 :
            //               (rolSeleccionado == "MIN") ? 0 :
            //               (rolSeleccionado == "MIP") ? 1 :
            //               (rolSeleccionado == "ADP") ? 1 :
            //               (rolSeleccionado == "CON") ? 0 :
            //               (rolSeleccionado == "COP") ? 1 : 1;

            var list = _dBUsuarioService.GetUsuarios();
            return Json(list);
        }
        public JsonResult GetUsuario(string rut)
        {
            UsuarioModel usuario = new UsuarioModel();
            usuario = _dBUsuarioService.GetUsuario(rut);
            return Json(usuario);
        }
        //public JsonResult postUsuario(
        //    string Rut,
        //    string Nombre,
        //    string Apellido,
        //    string Organismo,
        //    bool Estado,
        //    string Clave
        //    )
        //{
        //    try
        //    {
        //        if (Rut.IsNullOrEmpty()) { return Json("Debe indicar el rut del usuario "); }
        //        if (Nombre.IsNullOrEmpty()) { return Json("Debe indicar el nombre del usuario "); }
        //        if (Organismo.IsNullOrEmpty()) { return Json("Debe indicar el organismo "); }
        //        ///if (Clave.IsNullOrEmpty()) { return Json("Debe indicar una clave "); }
        //        int resultado = _dBUsuarioService.PostUsuario(Rut, Nombre, Apellido, Organismo, Estado, Clave);

        //        return Json(resultado);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(ex.Message);
        //    }
        //}
        public JsonResult postUsuario(
            string rut,
            string nombre,
            string apellido,
            string organismo,
            string estado,
            string usuarioLDAP,
            string clave
        )
        {
            if (rut.IsNullOrEmpty()) { return Json("Debe indicar el rut del usuario "); }
            if (nombre.IsNullOrEmpty()) { return Json("Debe indicar el nombre del usuario "); }
            if (organismo.IsNullOrEmpty()) { return Json("Debe indicar el organismo "); }
            try
            {
                int resultado = _dBUsuarioService.PostUsuario(rut, nombre, apellido, organismo, estado, usuarioLDAP, clave);
                return resultado > 0
                    ? Json(new { status = true, message = "Usuario agregado correctamente." })
                    : Json(new { status = false, message = "No se guardó ningún registro." });
            }
            catch (SqlException ex)
            {
                return Json(new { status = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = "Ocurrió un error al guardar el usuario." });
            }
        }

        public JsonResult putUsuario(
            string rut,
            string nombre,
            string apellido,
            string organismo,
            string estado,
            string usuarioLDAP,
            string clave
        )
        {
            if (rut.IsNullOrEmpty()) { return Json("Debe indicar el rut del usuario "); }
            if (nombre.IsNullOrEmpty()) { return Json("Debe indicar el nombre del usuario "); }
            if (organismo.IsNullOrEmpty()) { return Json("Debe indicar el organismo "); }
            try
            {
                int resultado = _dBUsuarioService.PutUsuario(rut, nombre, apellido, organismo, estado, usuarioLDAP, clave);
                return resultado > 0
                    ? Json(new { status = true, message = "Usuario actualizado correctamente." })
                    : Json(new { status = false, message = "No se actualizó ningún registro." });
            }
            catch (SqlException ex)
            {
                return Json(new { status = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = "Ocurrió un error al actualizar el usuario." });
            }
        }

        public JsonResult setUsuario(
string rut,
string nombre,
string apellido,
string organismo,
string estado,
string usuarioLDAP
)
        {
            if (rut.IsNullOrEmpty()) { return Json("Debe indicar el rut del usuario "); }
            if (nombre.IsNullOrEmpty()) { return Json("Debe indicar el nombre del usuario "); }
            if (organismo.IsNullOrEmpty()) { return Json("Debe indicar el organismo "); }
            try
            {
                int resultado = _dBUsuarioService.PostUsuario(rut, nombre, apellido, organismo, estado, usuarioLDAP);
                return resultado > 0
                    ? Json(new { status = true, message = "Usuario actualizado correctamente." })
                    : Json(new { status = false, message = "No se actualizó ningún registro." });
            }
            catch (SqlException ex)
            {
                // Los THROW 5000x del SP caen aquí con Number personalizado
                return Json(new { status = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = "Ocurrió un error al actualizar el usuario." });
            }
        }
        public JsonResult ToggleUsuario(UsuarioModel usuario)
        {
            try
            {
                var rows = _dBUsuarioService.toggleUsuario(usuario); // el método corregido
                return rows > 0
                    ? Json(new { status = true, message = "Usuario actualizado correctamente." })
                    : Json(new { status = false, message = "No se actualizó ningún registro." });
            }
            catch (SqlException ex)
            {
                // Los THROW 5000x del SP caen aquí con Number personalizado
                return Json(new { status = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = "Ocurrió un error al actualizar el usuario." });
            }
        }

    }





}
