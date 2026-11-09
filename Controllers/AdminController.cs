using Microsoft.AspNetCore.Mvc;

namespace SIGEPROS.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult _viewCargaPlanilla()
        {

            //return View("../PerfilAdministrador/_CargaPlanilla");
            return View("../PerfilAdministrador/_cargasExternas");
        }
        public IActionResult _viewCargaDocumentos()
        {

            //return View("../PerfilAdministrador/_CargaPlanilla");
            return View("../PerfilAdministrador/_cargasExternas2");
        }
        public IActionResult _viewCargaUsuarios()
        {
            return View("../PerfilAdministrador/_usuario");
        }
        public IActionResult _viewCargaProveedores()
        {
            return View("../PerfilAdministrador/_proveedorMaster");
        }
        public IActionResult _viewHistorial()
        {
            return View("../Shared/_historial");
        }

    }
}
