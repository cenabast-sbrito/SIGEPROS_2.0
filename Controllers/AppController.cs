using Aspose.Pdf.Operators;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SIGEPROS.Interface;
using SIGEPROS.Models;
using SIGEPROS.Services;

namespace SIGEPROS.Controllers
{
    public class AppController : Controller
    {
        
        /// /private readonly DBAppService _dbappservice;
        
        private readonly DBProveedorService _dbproveedorservice;
        private readonly DBDespachosService _dbdespachosservice;
        private readonly DBFacturaService _dbfacturaservice;
        private readonly DBExpedienteService _dbexpedienteservice;
        private readonly DBExpedienteJobService _expedientejobservice;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AppController( DBProveedorService dbproveedorservice, DBFacturaService dbfacturaservice, DBExpedienteService dbexpedienteservice, DBDespachosService dbdespachosservice, DBExpedienteJobService expedientejobservice, IHttpContextAccessor httpContextAccessor)
        {
            ////_dbappservice = dbappservice;
            _dbproveedorservice = dbproveedorservice;
            _dbfacturaservice = dbfacturaservice;
            _dbexpedienteservice = dbexpedienteservice;
            _dbdespachosservice = dbdespachosservice;
            _expedientejobservice = expedientejobservice;
            _httpContextAccessor = httpContextAccessor;

        }

        [HttpGet]
        public JsonResult GetFacturas( string rol, string rutProveedor,  string nFactura, string ano, string cEstado)
        {
            var rolSeleccionado = _httpContextAccessor.HttpContext?.User
?.FindFirst("rolSeleccionado")?.Value;

            try
            {
                List<FacturaModel> facturas =
                    _dbfacturaservice.GetFacturas(
                        rolSeleccionado,
                        rutProveedor,
                        nFactura,
                        ano,
                        cEstado);

                if (facturas == null || !facturas.Any())
                {
                    return Json(new
                    {
                        resultado = new List<FacturaModel>(),
                        message = "No se encontraron facturas.",
                        status = false
                    });
                }

                return Json(new
                {
                    resultado = facturas,
                    message = "Consulta realizada correctamente.",
                    status = true
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    resultado = new List<FacturaModel>(),
                    message = ex.Message,
                    status = false
                });
            }
        }
        [HttpGet]
        public JsonResult GetFacturasHistoricas(string rol,string rutProveedor,string nFactura,string ano,string cEstado)
        {
            var rolSeleccionado = _httpContextAccessor.HttpContext?.User
?.FindFirst("rolSeleccionado")?.Value;

            try
            {
                List<FacturaModel> facturas =
                    _dbfacturaservice.GetFacturasHistoricas(
                        rol,
                        rutProveedor,
                        nFactura,
                        ano,
                        cEstado);

                if (facturas == null || !facturas.Any())
                {
                    return Json(new
                    {
                        resultado = new List<FacturaModel>(),
                        message = "No se encontraron facturas.",
                        status = false
                    });
                }

                return Json(new
                {
                    resultado = facturas,
                    message = "Consulta realizada correctamente.",
                    status = true
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    resultado = new List<FacturaModel>(),
                    message = ex.Message,
                    status = false
                });
            }
        }

        public JsonResult GetListaProveedores(string parametro1, int programa)
        {
            try
            {
                var proveedores = _dbproveedorservice.GetlistaProveedores(programa);

                if (proveedores == null || !proveedores.Any())
                {
                    return Json(new
                    {
                        resultado = new List<ProveedorModel>(),
                        message = "No se encontraron proveedores.",
                        status = false
                    });
                }

                return Json(new
                {
                    resultado = proveedores,
                    message = "Consulta realizada correctamente.",
                    status = true
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    resultado = new List<ProveedorModel>(),
                    message = ex.Message,
                    status = false
                });
            }
        }
        public JsonResult GetListaEstados(int cEstado)
        {
            try
            {

                var rolSeleccionado = _httpContextAccessor.HttpContext?.User?.FindFirst("rolSeleccionado")?.Value;
                string organismo = (rolSeleccionado == "ADM") ? "C" :
                                   (rolSeleccionado == "MIN") ? "M" :
                                   (rolSeleccionado == "MIP") ? "M" :
                                   (rolSeleccionado == "ADP") ? "C" :
                                   (rolSeleccionado == "CON") ? "C" :
                                   (rolSeleccionado == "COP") ? "C" : "C";

                var estados = _dbproveedorservice.GetlistaEstados(cEstado, organismo);

                if (estados == null || !estados.Any())
                {
                    return Json(new
                    {
                        resultado = new List<EstadoModel>(),
                        message = "No se encontraron Estados.",
                        status = false
                    });
                }

                return Json(new
                {
                    resultado = estados,
                    message = "Consulta realizada correctamente.",
                    status = true
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    resultado = new List<EstadoModel>(),
                    message = ex.Message,
                    status = false
                });
            }
        }
        public Boolean existeExpediente(string rutProveedor, string factura)
        {
            if (_dbexpedienteservice.existeExpediente(rutProveedor, factura, true) == 1)
            {
                return true;
            }
            return false;

        }
        [HttpPost]
        public IActionResult CrearExpediente(string rutProveedor, string factura)
        {
            try
            {
                string usuario =
                    User.Identity?.Name ?? "SISTEMA";

                long idJob =
                    _expedientejobservice.CrearJob(usuario);

                var resultado =
                    _dbexpedienteservice.crearExpediente(
                        idJob,
                        rutProveedor,
                        factura);

                if (resultado == "FINALIZADO" ||
                    resultado == "FINALIZADO_CON_OBSERVACIONES")
                {
                    if (_dbexpedienteservice.existeExpediente(
                            rutProveedor,
                            factura,
                            true) == 1)
                    {
                        _dbfacturaservice.putEstado(
                            "",
                            rutProveedor,
                            factura,
                            "Expediente",
                            "");
                    }

                    return Json(new
                    {
                        state = true,
                        idJob = idJob,
                        estado = resultado,
                        rutProveedor = rutProveedor,
                        factura = factura,
                        linkExpediente =
                            _dbexpedienteservice.getLinkExpediente(
                                rutProveedor,
                                factura)
                    });
                }

                return Json(new
                {
                    state = false,
                    idJob = idJob,
                    message = resultado
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    state = false,
                    message = ex.Message
                });
            }
        }

        public JsonResult getDespachosByFactura(string rutProveedor, string factura, Boolean incluye)
        {
            DespachoModel despacho = new DespachoModel();
            List<DespachoModel> despachos = new List<DespachoModel>();
            try
            {
                despachos = _dbdespachosservice.getDespachosByFactura(
                    rutProveedor,
                    factura,
                    incluye
                    );

                if (despachos == null)
                    return Json(new
                    {
                        state = false,
                        message = " Lista vacía, solicite a TI revisar tabla SAP VBPA ",
                        resultado = despachos
                    });

                return Json(new
                {
                    state = true,
                    message = "OK.",
                    resultado = despachos
                });
            }
            catch (SqlException ex)
            {
                string mensaje = ex.Number == -2
                    ? "Se agotó el tiempo de espera en la consulta. La Base de Datos No Respondió. Intente más tarde."
                    : "Error en la base de datos.";

                return Json(new
                {
                    state = false,
                    message = mensaje,
                    resultado = (object)null
                });

            }
            catch (Exception ex)
            {
                return Json(new
                {
                    state = false,
                    message = "Sesión Finalizada",
                    resultado = despacho
                });

            }
        }

        [HttpPost]
        public JsonResult GenerarExpediente(string rut, string factura)
        {
            try
            {
                string usuario = User.Identity?.Name ?? "SISTEMA";

                long idJob = _expedientejobservice.CrearJob(usuario);

                _dbexpedienteservice.crearExpediente(
                    idJob,
                    rut,
                    factura);

                return Json(new
                {
                    Ok = true,
                    IdJob = idJob
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Ok = false,
                    Mensaje = ex.Message
                });
            }
        }

        [HttpGet]
        public IActionResult GetEstadoJob(long idJob)
        {
            try
            {
                var job = _expedientejobservice.ObtenerEstado(idJob);

                if (job == null)
                {
                    return Json(new
                    {
                        state = false,
                        message = "Job no encontrado"
                    });
                }

                return Json(new
                {
                    state = true,
                    idJob = job.IdJob,
                    estado = job.Estado,
                    porcentaje = job.Porcentaje,
                    mensaje = job.Mensaje,
                    rutaExpediente = job.RutaExpediente
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    state = false,
                    message = ex.Message
                });
            }
        }

    }
}
