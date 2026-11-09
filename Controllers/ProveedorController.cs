using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using SIGEPROS.Models;
using SIGEPROS.Services;

namespace SIGEPROS.Controllers
{
    public class ProveedorController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly DBProveedorService _dBProveedorService;
        private readonly DBFacturaService _dBFacturaService;

        public ProveedorController( IHttpContextAccessor httpContextAccessor, DBProveedorService dBProveedorService, DBFacturaService dBFacturaService)
        {
            _httpContextAccessor = httpContextAccessor;
            _dBProveedorService = dBProveedorService;
            _dBFacturaService = dBFacturaService;
        }

        public IActionResult Index()
        {
            return View();
        }
        public JsonResult GetProveedor(string rut)
        {
            Proveedor usuario = new Proveedor();
            usuario = _dBProveedorService.GetProveedor(rut);
            return Json(usuario);
        }
        public JsonResult getProveedores(string tipoConsulta)
        {

            var rolSeleccionado = _httpContextAccessor.HttpContext?.User?.FindFirst("rolSeleccionado")?.Value;
            int programa = ( rolSeleccionado=="ADM" ) ? 0 :
                           ( rolSeleccionado=="MIN" ) ? 0 :
                           ( rolSeleccionado=="MIP" ) ? 1 :
                           ( rolSeleccionado=="ADP" ) ? 1 :
                           ( rolSeleccionado=="CON" ) ? 0 :
                           ( rolSeleccionado=="COP" ) ? 1 : 1;

            var list = _dBProveedorService.GetlistaProveedores(programa);
            return Json(list);
        }
        public JsonResult getListFacturas(string rutproveedor)
        {
            var list = _dBFacturaService.getListFactura(rutproveedor);

            return Json(list);
        }
        public JsonResult setProveedor(
            string RutProveedor,
            string Nombre,
            string Direccion,
            string Estado,
            string CPrograma
            )
        {
            if (RutProveedor.IsNullOrEmpty()) { return Json("Debe indicar el rut del usuario "); }
            if (Nombre.IsNullOrEmpty()) { return Json("Debe indicar el nombre del usuario "); }
            if (CPrograma.IsNullOrEmpty()) { return Json("Debe indicar el programa "); }
            try
            {
                int resultado = _dBProveedorService.PostProveedor(RutProveedor, Nombre, Direccion, Estado, CPrograma);
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
        public JsonResult putProveedor(            string RutProveedor,            string Nombre,            string Direccion,            string Estado,            string CPrograma            )
        {
            if (RutProveedor.IsNullOrEmpty()) { return Json("Debe indicar el rut del usuario "); }
            if (Nombre.IsNullOrEmpty()) { return Json("Debe indicar el nombre del usuario "); }
            if (CPrograma.IsNullOrEmpty()) { return Json("Debe indicar el programa "); }
            try
            {
                int resultado = _dBProveedorService.PutProveedor(RutProveedor, Nombre, Direccion, Estado, CPrograma);
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
        public JsonResult updateProveedor(    string RutProveedor,    int Estado    )
        {
            if (RutProveedor.IsNullOrEmpty()) { return Json("Debe indicar el rut del usuario "); }
            try
            {
                int resultado = _dBProveedorService.UpdateProveedor(RutProveedor, Estado);
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
    
    
    
    
    
    
    }
}
