using Microsoft.Data.SqlClient;
using SIGEPROS.Models;
using System.Net;
using Aspose.Pdf.Facades;

namespace SIGEPROS.Services
{
    public class DBExpedienteService
    {
        private readonly DBService _dbservice;
        private readonly DBDespachosService _dbdespachosservice;
        private static IConfiguration? _configuracion;
        private static IConfiguration? _config;
        private readonly ConfiguracionArchivos _cfg;
        private readonly DBExpedienteJobService _jobService;
        SqlConnection? _connection = null;
        SqlCommand? _command = null;
        SqlCommand? _commandIdentity = null;


        public DBExpedienteService(DBService dbservice, IConfiguration configuracion, DBDespachosService dbdespachosservice, DBExpedienteJobService jobservice)
        {
            _dbservice = dbservice;
            _configuracion = configuracion;
            _cfg = ConfiguracionArchivos.Crear(configuracion);
            _dbdespachosservice = dbdespachosservice;
            _jobService = jobservice;
        }

        public int existeExpediente(string rut, string factura, Boolean full)
        {

            try
            {
                string ambiente = _cfg.Ambiente;
                string urlBaseDestino = _cfg.UrlBaseDestino;
                string rutaBaseFisicaExpedientes = _cfg.RutaBaseFisicaExpedientes;
                string rutaBaseFisicaComision = _cfg.RutaBaseFisicaComision;
                string rutaBaseFisicaDocumentoAdjuntoBase = _cfg.RutaBaseFisicaDocumentoAdjuntoBase;
                string urlBaseDestinoDocumentoAdjuntoBase = _cfg.RutaBaseFisicaDocumentoAdjuntoBase;
                string urlBaseDestinoComision = _cfg.RutaBaseFisicaComision;
                string prefijo = _cfg.Prefijo;

                //return 1; 
                //                var urlBase = ConfigurationManager.AppSettings["urlBaseIOExpedientes"].ToString();
                var urlBase = rutaBaseFisicaExpedientes;
                var nombreExpediente = (full) ? rut + "_" + factura + "_FULL.pdf" : rut + "_" + factura + ".pdf";
                var archivoFull = urlBase + nombreExpediente;
                if (System.IO.File.Exists(archivoFull)) return 1; else return 0;
            }
            catch
            {
                return 0;
            }

        }
        public string crearExpediente(long idJob, string rut, string factura)
        {
            string rutaBaseFisicaExpedientes = _cfg.RutaBaseFisicaExpedientes;
            string urlCargaSapPro = _cfg.UrlCargaSapPro;
            string urlBaseCedibles = _cfg.UrlBaseCedibles;

            string backupDir;
            string sourceDir;
            string bckupDirFull;
            string doc_venta;
            string fileName;
            string NombreExpediente;

            var listaDespachos = _dbdespachosservice.getDespachosByFactura(rut, factura, true);

            List<string> archivosValidos = new List<string>();

            int total = listaDespachos.Count;
            int procesados = 0;
            int copiados = 0;
            int noEncontrados = 0;
            int corruptos = 0;
            int erroresDescarga = 0;
            int erroresGuardado = 0;

            try
            {
                _jobService.ActualizarEstado(
                    idJob,
                    "COPIANDO",
                    1,
                    "Iniciando generación de expediente");

                foreach (var despa in listaDespachos)
                {
                    procesados++;

                    doc_venta = despa.DocumentoVenta;

                    int porcentaje = Math.Max(
                        1,
                        (int)((procesados * 60.0) / Math.Max(total, 1))
                    );

                    _jobService.ActualizarEstado(
                        idJob,
                        "COPIANDO",
                        porcentaje,
                        $"Procesando documento {procesados} de {total}: {doc_venta}");

                    string carpetaRemota =
                        Path.Combine(urlCargaSapPro, rut, "procesados") + "\\";

                    if (!Directory.Exists(carpetaRemota))
                    {
                        noEncontrados++;

                        _jobService.RegistrarError(
                            idJob,
                            doc_venta,
                            "CARPETA_NO_EXISTE",
                            carpetaRemota);

                        continue;
                    }

                    string[] archivos =
                        Directory.GetFiles(carpetaRemota, doc_venta + "*.pdf");

                    if (archivos.Length == 0)
                    {
                        noEncontrados++;

                        _jobService.RegistrarError(
                            idJob,
                            doc_venta,
                            "PDF_NO_ENCONTRADO",
                            $"No existe PDF para {doc_venta}");

                        continue;
                    }

                    string archivoSeleccionado = archivos
                        .Select(a =>
                        {
                            string nombre =
                                Path.GetFileNameWithoutExtension(a);

                            string resto =
                                nombre.Substring(doc_venta.Length);

                            int n = 0;

                            int.TryParse(resto, out n);

                            return new
                            {
                                archivo = a,
                                num = n
                            };
                        })
                        .OrderBy(x => x.num)
                        .Last()
                        .archivo;

                    string nombreArchivoSeleccionado =
                        Path.GetFileName(archivoSeleccionado);

                    fileName = $"{rut}_{doc_venta}.pdf";

                    backupDir =
                        Path.Combine(rutaBaseFisicaExpedientes, fileName);

                    sourceDir =
                        $"{urlBaseCedibles}{rut}/procesados/{nombreArchivoSeleccionado}";

                    byte[] fileData;

                    try
                    {
                        using (WebClient client = new WebClient())
                        {
                            ServicePointManager.SecurityProtocol =
                                SecurityProtocolType.Tls12;

                            fileData = client.DownloadData(sourceDir);
                        }
                    }
                    catch (WebException ex)
                    {
                        erroresDescarga++;

                        _jobService.RegistrarError(
                            idJob,
                            doc_venta,
                            "ERROR_DESCARGA",
                            ex.Message);

                        continue;
                    }

                    try
                    {
                        using (FileStream fileStream =
                               new FileStream(backupDir, FileMode.Create))
                        {
                            fileStream.Write(fileData, 0, fileData.Length);
                            fileStream.Flush();
                        }

                        copiados++;

                        archivosValidos.Add(backupDir);
                    }
                    catch (Exception ex)
                    {
                        erroresGuardado++;

                        _jobService.RegistrarError(
                            idJob,
                            doc_venta,
                            "ERROR_GUARDADO",
                            ex.ToString());

                        continue;
                    }
                }

                //----------------------------------------------------
                // VALIDACIÓN PDFs
                //----------------------------------------------------

                _jobService.ActualizarEstado(
                    idJob,
                    "VALIDANDO",
                    65,
                    "Validando archivos PDF");

                List<string> pdfsFusionables =
                    new List<string>();

                foreach (string file in archivosValidos)
                {
                    try
                    {
                        if (!File.Exists(file))
                        {
                            corruptos++;

                            _jobService.RegistrarError(
                                idJob,
                                Path.GetFileName(file),
                                "ARCHIVO_NO_EXISTE",
                                file);

                            continue;
                        }

                        FileInfo info = new FileInfo(file);

                        if (info.Length < 1000)
                        {
                            corruptos++;

                            _jobService.RegistrarError(
                                idJob,
                                Path.GetFileName(file),
                                "ARCHIVO_VACIO",
                                file);

                            continue;
                        }

                        using (var pdfDoc =
                            new Aspose.Pdf.Document(file))
                        {
                            // válido
                        }

                        pdfsFusionables.Add(file);
                    }
                    catch (Exception ex)
                    {
                        corruptos++;

                        _jobService.RegistrarError(
                            idJob,
                            Path.GetFileName(file),
                            "PDF_CORRUPTO",
                            ex.Message);

                        continue;
                    }
                }

                //----------------------------------------------------
                // NO HAY PDFs VÁLIDOS
                //----------------------------------------------------

                if (!pdfsFusionables.Any())
                {
                    _jobService.FinalizarJob(
                        idJob,
                        "ERROR",
                        "");

                    return "No se encontraron PDFs válidos para generar el expediente.";
                }

                //----------------------------------------------------
                // CONCATENACIÓN
                //----------------------------------------------------

                _jobService.ActualizarEstado(
                    idJob,
                    "FUSIONANDO",
                    80,
                    "Generando expediente");

                NombreExpediente =
                    $"{rut}_{factura}_FULL.pdf";

                bckupDirFull =
                    Path.Combine(
                        rutaBaseFisicaExpedientes,
                        NombreExpediente);

                try
                {
                    PdfFileEditor pdfEditor =
                        new PdfFileEditor();

                    pdfEditor.Concatenate(
                        pdfsFusionables.ToArray(),
                        bckupDirFull);
                }
                catch (Exception ex)
                {
                    _jobService.RegistrarError(
                        idJob,
                        factura,
                        "ERROR_CONCATENACION",
                        ex.Message);

                    _jobService.FinalizarJob(
                        idJob,
                        "ERROR",
                        "");

                    return ex.Message;
                }

                //----------------------------------------------------
                // LIMPIEZA
                //----------------------------------------------------

                _jobService.ActualizarEstado(
                    idJob,
                    "ELIMINANDO_TEMPORALES",
                    95,
                    "Eliminando archivos temporales");

                foreach (string archivo in pdfsFusionables)
                {
                    try
                    {
                        if (File.Exists(archivo))
                        {
                            File.Delete(archivo);
                        }
                    }
                    catch (Exception ex)
                    {
                        _jobService.RegistrarError(
                            idJob,
                            Path.GetFileName(archivo),
                            "ERROR_ELIMINACION",
                            ex.Message);
                    }
                }

                //----------------------------------------------------
                // FINALIZACIÓN
                //----------------------------------------------------

                string estadoFinal =
                    (
                        noEncontrados > 0 ||
                        corruptos > 0 ||
                        erroresDescarga > 0 ||
                        erroresGuardado > 0
                    )
                    ? "FINALIZADO_CON_OBSERVACIONES"
                    : "FINALIZADO";

                _jobService.FinalizarJob(
                    idJob,
                    estadoFinal,
                    bckupDirFull);

                return estadoFinal;
            }
            catch (Exception ex)
            {
                _jobService.RegistrarError(
                    idJob,
                    factura,
                    "ERROR_GENERAL",
                    ex.ToString());

                _jobService.FinalizarJob(
                    idJob,
                    "ERROR",
                    "");

                return ex.Message;
            }
        }

        public string crearExpedienteOLD(string rut, string factura)
        {

            string ambiente = _cfg.Ambiente;
            string urlBaseDestino = _cfg.UrlBaseDestino;
            string rutaBaseFisicaExpedientes = _cfg.RutaBaseFisicaExpedientes;
            string rutaBaseFisicaComision = _cfg.RutaBaseFisicaComision;
            string rutaBaseFisicaDocumentoAdjuntoBase = _cfg.RutaBaseFisicaDocumentoAdjuntoBase;
            string urlBaseDestinoDocumentoAdjuntoBase = _cfg.RutaBaseFisicaDocumentoAdjuntoBase;
            string urlBaseDestinoComision = _cfg.RutaBaseFisicaComision;
            string urlCargaSapPro = _cfg.UrlCargaSapPro;
            string urlBaseCedibles = _cfg.UrlBaseCedibles;
            string prefijo = _cfg.Prefijo;

            string backupDir, sourceDir, bckupDirFull, doc_venta, fileName, NombreExpediente;
            DespachoModel despacho = new DespachoModel();

            var listaDespachos = _dbdespachosservice.getDespachosByFactura(rut, factura, true);
            string[] dire = new string[listaDespachos.Count];
            int ind = 0;
            String res = "";

            foreach (var despa in listaDespachos)
            {
                doc_venta = despa.DocumentoVenta;

                /*
                 * seleccionar el ultimo documento correcto cargado
                 */

                // -------------------------------------------
                // 1) RUTA UNC DEL DIRECTORIO "procesados"
                // -------------------------------------------
                string carpetaRemota = Path.Combine(urlCargaSapPro, rut, "procesados") + "\\";

                if (!Directory.Exists(carpetaRemota))
                {
                    return $"Error: No existe la carpeta remota: {carpetaRemota}";
                }

                // -------------------------------------------
                // 2) ENCONTRAR PDF CORRECTO (doc_venta + mayor sufijo)
                // -------------------------------------------
                string[] archivos = Directory.GetFiles(carpetaRemota, doc_venta + "*.pdf");

                if (archivos.Length == 0)
                {
                    return $"Error: No se encontraron archivos PDF para {doc_venta} en {carpetaRemota}";
                }

                string archivoSeleccionado = archivos
                    .Select(a =>
                    {
                        string nombre = Path.GetFileNameWithoutExtension(a);
                        string resto = nombre.Substring(doc_venta.Length);

                        int n = 0;
                        int.TryParse(resto, out n);
                        return new { archivo = a, num = n };
                    })
                    .OrderBy(x => x.num)
                    .Last()
                    .archivo;

                // Archivo PDF real
                sourceDir = archivoSeleccionado;

                string nombreArchivoSeleccionado = Path.GetFileName(archivoSeleccionado);

                fileName = rut + "_" + doc_venta + ".pdf";

                backupDir = rutaBaseFisicaExpedientes + fileName;
                sourceDir = @urlBaseCedibles + rut + "/procesados/" + doc_venta + ".pdf";
                sourceDir = @urlBaseCedibles + rut + "/procesados/" + nombreArchivoSeleccionado;

                dire[ind] = backupDir;
                ind++;

                byte[] fileData;
                try
                {
                    using (WebClient client = new WebClient())
                    {
                        System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                        fileData = client.DownloadData(sourceDir);
                    }
                }
                catch (WebException e)
                {
                    ind = 0;
                    return "... creando expediente de factura " + factura +
                           ", Ocurrió un error al descargar PDF desde: " + sourceDir +
                           ", detalle: " + e.Message;
                }

                try
                {
                    using (FileStream fileStream = new FileStream(backupDir, FileMode.Create))
                    {
                        fileStream.Write(fileData, 0, fileData.Length);
                        fileStream.Flush();
                    }
                }
                catch (Exception e)
                {
                    ind = 0;
                    return "... creando expediente de factura " + factura +
                           ", error al guardar PDF " + backupDir +
                           ", detalle: " + e.Message;
                }

                res = "OK";
            }


            // -------------------------------
            // VALIDACIÓN PREVIA DE ARCHIVOS PDF
            // -------------------------------
            for (int i = 0; i < dire.Length; i++)
            {
                string file = dire[i];

                // Validación 1: existe el archivo
                if (!File.Exists(file))
                {
                    return $"Error: El archivo PDF no existe: {file}";
                }

                // Validación 2: tamaño mínimo razonable
                var info = new FileInfo(file);
                if (info.Length < 1000)  // < 1 KB = muy sospechoso
                {
                    return $"Error: El PDF está vacío o corrupto: {file}";
                }

                // Validación 3: intentar abrir con Aspose
                try
                {
                    using (var pdfDoc = new Aspose.Pdf.Document(file))
                    {
                        // Si abre aquí, el PDF es válido
                    }
                }
                catch (Exception ex)
                {
                    return $"Error: El archivo PDF es inválido o está dañado: {file}. Detalle: {ex.Message}";
                }
            }


            // -------------------------------
            // CONCATENACIÓN DE PDFS
            // -------------------------------
            try
            {
                if (ind > 0)
                {
                    NombreExpediente = rut + "_" + factura + "_FULL.pdf";
                    bckupDirFull = rutaBaseFisicaExpedientes + NombreExpediente;

                    PdfFileEditor pdfEditor = new PdfFileEditor();

                    // Aquí ya estamos seguros de que los PDFs son válidos
                    pdfEditor.Concatenate(dire, bckupDirFull);

                    // Borrar PDFs individuales
                    foreach (var despa in listaDespachos)
                    {
                        doc_venta = despa.DocumentoVenta;
                        fileName = rut + "_" + doc_venta + ".pdf";
                        backupDir = rutaBaseFisicaExpedientes + fileName;

                        if (File.Exists(backupDir))
                        {
                            File.Delete(backupDir);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return "... creando expediente de factura " + factura +
                       ", ocurrió un error en la concatenación: " + e.Message;
            }

            return "OK";
        }
        public string getLinkExpediente(string rut, string factura)
        {
            string urlBaseExpedientes = _cfg.UrlBaseLinkExpedientes;

            return $"{urlBaseExpedientes}/{rut}_{factura}_FULL.pdf";
        }

    }
}
