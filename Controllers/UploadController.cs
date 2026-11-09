using Microsoft.AspNetCore.Mvc;
using SIGEPROS.Models;
using SIGEPROS.Models.DTO;
using SIGEPROS.Services;
using SIGEPROS.Static;

namespace SIGEPROS.Controllers
{
    //    [Authorize]
    //    [OutputCache(NoStore = true, Duration = 0)]
    public class UploadController : Controller
    {
        private readonly IPlanillaZdis022Service _planillaservice;
        private readonly DBDocumentosService _documentosservice;
        private readonly DBFacturaService _dbfacturaservice;
        private readonly ConfiguracionArchivos _cfg;

        public UploadController(IPlanillaZdis022Service planilla, DBDocumentosService documentosservice, IConfiguration configuracion, DBFacturaService dbfacturaservice)
        {
            _planillaservice = planilla;
            _documentosservice = documentosservice;
            _cfg = ConfiguracionArchivos.Crear(configuracion);
            _dbfacturaservice = dbfacturaservice;
        }



        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile(
                        IFormFile archivoExcel,
                        string rutProveedorCarga
                        )
        {
            var resultado =
                _planillaservice.Procesar(
                    archivoExcel.OpenReadStream(),
                    new ParametrosCargaDto
                    {
                        RutProveedorCarga = rutProveedorCarga
                    });

            return Ok(resultado);
        }

        public JsonResult getZDis022(string topx)
        {

            PlanillaZDIS022 despacho = new PlanillaZDIS022();

            var despachos = _planillaservice.getZDis022("Despachos_TEMP");

            return Json(despachos);

        }

        public JsonResult consolidarUploadFile()
        {

            var resp = _planillaservice.consolidar();
            if (resp == "OK") return Json(new { message = "Los datos han sido consolidados exitosamente", status = true });
            else return Json(new { message = resp, status = false });

        }

        [HttpPost]
        public async Task<JsonResult> subirDocumento( IFormFile archivoPDF, string _rutProveedorCarga, string _facturaLista, string _tipoDocumentoCarga)
        {
            try
            {
                // Validar archivo
                if (archivoPDF == null || archivoPDF.Length == 0)
                {
                    return Json(new
                    {
                        status = false,
                        message = "Debe seleccionar un archivo PDF."
                    });
                }

                // Validar extensión
                string extension = Path.GetExtension(archivoPDF.FileName);

                if (!string.Equals(extension, ".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    return Json(new
                    {
                        status = false,
                        message = "Sólo se permiten archivos PDF."
                    });
                }

                // Validar content-type
                var tiposPermitidos = new[]
                {
            "application/pdf",
            "application/octet-stream"
        };

                if (!tiposPermitidos.Contains(archivoPDF.ContentType, StringComparer.OrdinalIgnoreCase))
                {
                    return Json(new
                    {
                        status = false,
                        message = "Tipo de archivo no permitido."
                    });
                }

                // Ruta destino
                string rutaDestino = Path.Combine(
                    _cfg.UrlBaseIODocumentos,
                    _rutProveedorCarga);

                if (!Directory.Exists(rutaDestino))
                {
                    Directory.CreateDirectory(rutaDestino);
                }

                // Nombre original saneado
                string nombreOriginal = Path.GetFileName(archivoPDF.FileName);

                foreach (char c in Path.GetInvalidFileNameChars())
                {
                    nombreOriginal = nombreOriginal.Replace(c, '_');
                }

                // Generar nombre final
                string nombreArchivo;

                if (_tipoDocumentoCarga == "FC")
                {
                    nombreArchivo =
                        $"factura_comision_{_rutProveedorCarga}_{_facturaLista}.pdf";
                }
                else
                {
                    nombreArchivo =
                        $"{_tipoDocumentoCarga}_{_facturaLista}_{nombreOriginal}";
                }

                // Ruta completa
                string rutaCompleta = Path.Combine(
                    rutaDestino,
                    nombreArchivo);

                // Guardar archivo
                await using (var stream = new FileStream(
                    rutaCompleta,
                    FileMode.Create,
                    FileAccess.Write))
                {
                    await archivoPDF.CopyToAsync(stream);
                }

                return Json(new
                {
                    status = true,
                    message = "Archivo subido correctamente.",
                    archivo = nombreArchivo,
                    ruta = rutaCompleta
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    status = false,
                    message = ex.Message
                });
            }
        }

        public JsonResult subirDocumento2(IFormFile documentoUpload, string _rutProveedorUpload, string _facturaLista, string _tipoDocumento)
        {
            string Mensaje = string.Empty;
            string rutaDestino = string.Empty;
            string urlBaseDocumentos = string.Empty;
            string nombreArchivo = string.Empty;

            try
            {
                // Requisito: factura debe existir
                //                if (Factura._existeFacturaProveedor(_rutProveedorUpload, _facturaLista) <= 0)
                //                    return Json(new { message = "No ha sido hallada la factura del proveedor; es requisito obligatorio para subir el documento.", status = false });

                if (documentoUpload == null || documentoUpload.Length <= 0)
                    return Json(new { message = "Debe seleccionar un archivo.", status = false });

                // Validación de extensión y content-type (defensa en profundidad)
                var extension = Path.GetExtension(documentoUpload.FileName);
                if (!string.Equals(extension, ".pdf", StringComparison.OrdinalIgnoreCase))
                    return Json(new { message = "Solo se permiten archivos PDF (extensión .pdf).", status = false });

                // Algunos navegadores usan application/octet-stream; validamos lista permitida
                var contentTypesPermitidos = new[] { "application/pdf", "application/octet-stream" };
                if (!contentTypesPermitidos.Contains(documentoUpload.ContentType, StringComparer.OrdinalIgnoreCase))
                    return Json(new { message = "Tipo de contenido no permitido. Solo PDF.", status = false });

                // Resolver ruta base por ambiente y tipo
                string ambiente = _cfg.UrlBase; ////ConfigurationManager.AppSettings["Ambiente"];

                // Asegurar directorio destino
                rutaDestino = urlBaseDocumentos;
                if (!Directory.Exists(rutaDestino))
                    Directory.CreateDirectory(rutaDestino);

                // Sanitizar nombre base original
                string nombreOriginal = Path.GetFileName(documentoUpload.FileName);
                nombreOriginal = string.IsNullOrWhiteSpace(nombreOriginal) ? "documento.pdf" : nombreOriginal;

                // Remover caracteres inválidos
                foreach (var c in Path.GetInvalidFileNameChars())
                    nombreOriginal = nombreOriginal.Replace(c, '_');

                // Definir nombre objetivo
                if (_tipoDocumento != "FC")
                {
                    // Estructura: {TIPO}_{FACTURA}_{original}.pdf
                    nombreArchivo = $"{_tipoDocumento}_{_facturaLista}_{nombreOriginal}";
                }
                else
                {
                    // Estructura para FC: prefijo + rut + _ + factura + .pdf
                    string prefijo = "factura_comision_"; //// ConfigurationManager.AppSettings["prefijoNombreComision"]?.ToString() ?? "FC_";
                    nombreArchivo = $"{prefijo}{_rutProveedorUpload}_{_facturaLista}.pdf";
                }

                string rutaCompleta = Path.Combine(rutaDestino, nombreArchivo);

                // Guardar con sobrescritura:
                // FileMode.Create => crea si no existe o sobrescribe si existe
                using (var fileStream = new FileStream(rutaCompleta, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    documentoUpload.CopyToAsync(fileStream);
                }

                Mensaje = $"Archivo subido correctamente a: {rutaCompleta}";
                return Json(new { message = Mensaje, status = true });
            }
            catch (Exception ex)
            {
                Mensaje = $"Error al subir el archivo: {ex.Message}";
                return Json(new { message = Mensaje, status = false });
            }
        }
    
    }
}