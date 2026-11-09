using Aspose.Pdf.Operators;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SIGEPROS.Models;
using SIGEPROS.Services;
using System.Reflection;
using System.Security.Claims;

namespace SIGEPROS.Controllers
{
    public class DocumentosController : Controller
    {
        private readonly DBDocumentosService _documentosservice;
        private readonly DBFacturaService _dbfacturaservice;
        private readonly ConfiguracionArchivos _cfg;

        public DocumentosController(DBDocumentosService documentosservice, IConfiguration configuracion, DBFacturaService dbfacturaservice)
        {
            _documentosservice = documentosservice;
            _cfg = ConfiguracionArchivos.Crear(configuracion);
            _dbfacturaservice = dbfacturaservice;
        }

        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public JsonResult getDocumentosAdjuntos(string rut, string factura)
        {

            factura = (factura.IsNullOrEmpty()) ? "*" : factura;
            if (_documentosservice.ExisteDocumentoAdjunto(rut, factura) != 1) return Json(new { state = false, message = "Sin Documentos" });

            try
            {

                ////string folder = _cfg.UrlBaseCedibles + int.Parse(rut) + "/procesados/";

                string folder = _cfg.UrlBaseIODocumentos + int.Parse(rut) + "/";

                Archivo archivo = new Archivo();
                List<Archivo> archivos = new List<Archivo>();

                string fileFind = "???" + factura + "*.pdf";


                var archivosLista = Directory.GetFiles(folder, fileFind)
                        .Select(Path.GetFileName)
                        .OrderBy(f => f)
                        .Select(f => new Archivo { Nombre = f })
                        .ToList();
                ////Url = folder + int.Parse(rut) + "/procesados/",
                var modelo = new Archivos
                {
                    Url = folder + "/procesados/",
                    UrlDA = _cfg.UrlBaseLinkDocumentos + int.Parse(rut) + "/",
                    Nombres = archivosLista
                };

                //return modelo.Nombres;
                return Json(new
                {
                    archivos = modelo,
                    message = "OK",
                    state = true
                });



            }
            catch (Exception e)
            {
                return Json(new
                {
                    archivos = "",
                    message = "NOOK",
                    state = false
                });
            }

        }

        public Boolean existeFacturaComision(string rutProveedor, string factura)
        {
                        return true;
//            Movimiento doc = new Movimiento();
            //if (doc.existeFacturaComision(rutProveedor, factura) == 1)
            //{
            //    return true;
            //}
            //return false;

        }
        public JsonResult putEstado(string rutUsuario, string rutProveedor, string NFactura, string estadoActual, string estadoNuevo, string obs)
        {
            try
            {

                if (estadoNuevo == "4")
                {

                    if (existeDocumento( "Expediente",  rutProveedor,  NFactura)<1) throw new ArgumentException("El Expediente no ha sido creado aún. No es posible enviar a MINSAL");

                }

                // Actualizar estado
                EstadoFacturaModel estado = new EstadoFacturaModel();
                estado = _dbfacturaservice.putEstado(rutUsuario, rutProveedor, NFactura, estadoNuevo, obs);

                // Registrar log
                ///                Log log = new Log();
                ///                log.addLog(rutUsuario, rutProveedor, int.Parse(NFactura), estadoActual, estadoNuevo);

                /////var rol = User.FindFirst(ClaimTypes.Role)?.Value;

                FacturaModel facturas =    _dbfacturaservice.GetFactura( rutProveedor, NFactura, "", "" );

                return Json(new
                {
                    resultado = facturas,
                    message = "OK",
                    status = true
                });


            }
            catch (Exception ex)
            {
                // Capturar cualquier error y devolverlo como JSON
                return Json(new
                {
                    success = false,
                    message = "Error al actualizar el estado: " + ex.Message
                });
            }
        }
        public int existeDocumento(string tipoDocumento, string rut, string factura)
        {

            try
            {
                string urlBase = _cfg.UrlBase;
                string nombreDocumento = "";
                string carpeta = "";

                string prefijo = _cfg.Prefijo;

                 int existe = 0;

                if (tipoDocumento == "Expediente") { carpeta = "Expedientes/"; nombreDocumento = rut + "_" + factura + "_FULL.pdf"; }
                if (tipoDocumento == "Comision") { carpeta = "FacturasComision/factura_comision_"; nombreDocumento = rut + "_" + factura + ".pdf"; }

                var archivoFull = urlBase + carpeta + nombreDocumento;
                if (System.IO.File.Exists(archivoFull)) existe = 1; else existe = 0;

                return existe;
            }
            catch
            {
                return 0;
            }

        }
        public JsonResult ObtenerDocumentos(string rut, string doc)
        {
            string urlBase = _cfg.UrlBase;
            try
            {

                string folder = urlBase + int.Parse(rut) + "/procesados/";
                folder = _cfg.UrlCargaSapPro + int.Parse(rut) + "/procesados/";

                Archivo archivo = new Archivo();
                List<Archivo> archivos = new List<Archivo>();

                string fileFind = doc + "*.pdf";

                var archivosLista = Directory.GetFiles(folder, fileFind)
                    .Select(Path.GetFileName)
                    .OrderBy(f => f)
                    .Select(f => new Archivo { Nombre = f })
                    .ToList();

                var modelo = new Archivos
                {
                    Url = _cfg.UrlBaseCedibles + int.Parse(rut) + "/procesados/",
                    UrlDA = _cfg.UrlBaseLinkDocumentos + int.Parse(rut) + "/",
                    Nombres = archivosLista
                };

                return Json(new { state = true, archivos = modelo } );

            }
            catch (Exception e)
            {
                return Json(new { state = false, message = e.Message } );
            }

        }
        [HttpGet]
        public JsonResult getTiposDocumento()
        {
            try
            {
                var tipos = _documentosservice.GetTiposDocumento();
                return Json(tipos);
            }
            catch (Exception ex)
            {
                return Json(new List<TipoDocumentoModel>());
            }
        }

    }
}
