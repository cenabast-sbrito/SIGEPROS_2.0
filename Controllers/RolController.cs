using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using SIGEPROS.Models;
using SIGEPROS.Services;

namespace SIGEPROS.Controllers
{
    public class RolController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly DBRolService _dBRolService;

        public RolController(IHttpContextAccessor httpContextAccessor, DBRolService dBRolService)
        {
            _httpContextAccessor = httpContextAccessor;
            _dBRolService = dBRolService;
        }
        public IActionResult Index()
        {
            return View();
        }
        public JsonResult getRoles(string Rut)
        {

            //var rolSeleccionado = _httpContextAccessor.HttpContext?.User?.FindFirst("rolSeleccionado")?.Value;
            //int programa = (rolSeleccionado == "ADM") ? 0 :
            //               (rolSeleccionado == "MIN") ? 0 :
            //               (rolSeleccionado == "MIP") ? 1 :
            //               (rolSeleccionado == "ADP") ? 1 :
            //               (rolSeleccionado == "CON") ? 0 :
            //               (rolSeleccionado == "COP") ? 1 : 1;

            var list = _dBRolService.GetRoles(Rut);
            return Json(list);

        }
        public JsonResult ToggleRol(UsuarioModel usuario)
        {
            try
            {
                var rows = _dBRolService.toggleRol(usuario); // el método corregido
                return rows > 0
                    ? Json(new { status = true, message = "Rol actualizado correctamente." })
                    : Json(new { status = false, message = "No se actualizó ningún registro." });
            }
            catch (SqlException ex)
            {
                return Json(new { status = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = "Ocurrió un error al actualizar el rol." });
            }
        }



        //public JsonResult postRol(string Rol, string Rut)
        //{
        //    try
        //    {

        //        if (Rut.IsNullOrEmpty())
        //            return Json("Debe indicar el rut del usuario");

        //        if (Rol.IsNullOrEmpty())
        //            return Json("Debe indicar el rol que desea asignar");

        //        _dBRolService.PostRol(Rut, Rol);

        //        return Json("OK");

        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(ex.Message);
        //    }
        //}
        //public JsonResult delRol(string Rol, string Rut)
        //{
        //    try
        //    {

        //        if (Rut.IsNullOrEmpty())
        //            return Json("Debe indicar el rut del usuario");

        //        if (Rol.IsNullOrEmpty())
        //            return Json("Debe indicar el rol que desea quitar");

        //        _dBRolService.DelRol(Rut, Rol);

        //        return Json("OK");

        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(ex.Message);
        //    }
        //}



















        public class RolRequest
        {
            public string Rut { get; set; }

            public List<string> Roles { get; set; }
        }
        [HttpPost]
        public IActionResult ActualizarRoles(
    string rut,
    string roles)
        {
            try
            {
                _dBRolService.ActualizarRoles(rut, roles);

                return Json(new
                {
                    ok = true,
                    message = "Roles actualizados correctamente."
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    ok = false,
                    message = ex.Message
                });
            }
        }
        [HttpPost]
        public IActionResult ActualizarRolesOLD(
            string rut,
            string roles)
        {
            try
            {
                _dBRolService.ActualizarRoles(
                    rut,
                    roles);

                return Json(new
                {
                    ok = true,
                    message = "Roles actualizados correctamente."
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    ok = false,
                    message = ex.Message
                });
            }
        }

    }
}
