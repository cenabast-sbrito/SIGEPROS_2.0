using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SIGEPROS.Models;
using SIGEPROS.Static;
using System.Data;
using System.Data.Common;

namespace SIGEPROS.Services
{
    public class DBFacturaService
    {
        

        private readonly DBService _dbservice;
        private static IConfiguration? _configuracion;
        private static IConfiguration? _config;
        private readonly ConfiguracionArchivos _cfg;
        private readonly IHttpContextAccessor _httpContextAccessor;

        SqlConnection? _connection = null;
        SqlCommand? _command = null;
        SqlCommand? _commandIdentity = null;

        public DBFacturaService(DBService dbservice, IConfiguration configuracion, IHttpContextAccessor httpContextAccessor)
        {
            _dbservice = dbservice;
            _configuracion = configuracion;
            _cfg = ConfiguracionArchivos.Crear(configuracion);
            _httpContextAccessor = httpContextAccessor;
        }
        public List<FacturaModel> GetFacturas(
            string rol,
            string rutProveedor,
            string nFactura,
            string ano,
            string cEstado)
        {

            string ambiente = _cfg.Ambiente;
            string urlBaseDestino = _cfg.UrlBaseDestino;
            string rutaBaseFisicaExpedientes = _cfg.RutaBaseFisicaExpedientes;
            string rutaBaseFisicaComision = _cfg.RutaBaseFisicaComision;
            string rutaBaseFisicaDocumentoAdjuntoBase = _cfg.RutaBaseFisicaDocumentoAdjuntoBase;
            string urlBaseDestinoDocumentoAdjuntoBase = _cfg.RutaBaseFisicaDocumentoAdjuntoBase;
            string urlBaseDestinoComision = _cfg.RutaBaseFisicaComision;
            string prefijo = _cfg.Prefijo;

            string urlBaseLink = _cfg.UrlBaseLink;

            List<FacturaModel> datos = new List<FacturaModel>();
            try
            {
                using (_connection = new SqlConnection(_dbservice.GetConnectionString()))
                {

                    var rolSeleccionado = _httpContextAccessor.HttpContext?.User
                    ?.FindFirst("rolSeleccionado")?.Value;

                    _command = _connection.CreateCommand();

                    _command.CommandType = CommandType.StoredProcedure;
                    _command.CommandText = "[dbo].[proc_Facturas_Obtener]";
                    _command.Parameters.AddWithValue("@rol", rolSeleccionado ?? "");
                    _command.Parameters.AddWithValue("@rutProveedor", rutProveedor ?? "");
                    _command.Parameters.AddWithValue("@NFactura", nFactura ?? "");
                    _command.Parameters.AddWithValue("@Ano", ano ?? "");
                    _command.Parameters.AddWithValue("@CEstado", cEstado);

                    _connection.Open();

                    SqlDataReader dr = _command.ExecuteReader();

                    while (dr.Read())
                    {
                        FacturaModel factura = new FacturaModel();
                        factura = _setFactura(dr);

                        // ===================== EXPEDIENTE =====================
                        string nombreExpediente = $"{factura.rutProveedor}_{factura.nFactura}_FULL.pdf";
                        string rutaExpediente = Path.Combine(rutaBaseFisicaExpedientes, nombreExpediente);

                        factura.ExisteExpediente = File.Exists(rutaExpediente);
                        factura.linkExpedientes = factura.ExisteExpediente
                            ? urlBaseDestino + nombreExpediente
                            : null;

                        // ===================== COMISION =====================
                        string nombreComision = $"{prefijo}{factura.rutProveedor}_{factura.nFactura}.pdf";
                        string rutaComision = Path.Combine(rutaBaseFisicaComision, nombreComision);

                        factura.ExisteComision = File.Exists(rutaComision);
                        factura.linkComision = factura.ExisteComision
                            ? urlBaseDestinoComision + nombreComision
                            : null;

                        factura.linkComision = urlBaseLink + "FacturasComision/" + nombreComision;

                        // ===================== DOCUMENTO ADJUNTO (FIX ✅) =====================
                        factura.ExisteDocumentoAdjunto = false;
                        factura.linkDocumentoAdjunto = null;

                        var rutaFisicaAdjunto = Path.Combine(rutaBaseFisicaDocumentoAdjuntoBase, factura.rutProveedor.ToString());
                        var urlAdjunto = urlBaseDestinoDocumentoAdjuntoBase + factura.rutProveedor.ToString() + "/";

                        if (!string.IsNullOrWhiteSpace(factura.rutProveedor.ToString()) &&
                            Directory.Exists(rutaFisicaAdjunto))
                        {
                            var archivoAdjunto = Directory
                                .EnumerateFiles(
                                    rutaFisicaAdjunto,
                                    $"*_{factura.nFactura}_*.pdf"
                                )
                                .Select(Path.GetFileName)
                                .FirstOrDefault();

                            factura.ExisteDocumentoAdjunto = archivoAdjunto != null;
                            factura.linkDocumentoAdjunto = factura.ExisteDocumentoAdjunto
                                ? urlAdjunto + archivoAdjunto
                                : null;
                        }
                        datos.Add(factura);
                    }
                    dr.Close();
                    _connection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return datos;
        }
        public List<FacturaModel> GetFacturasHistoricas(
                    string rol,
                    string rutProveedor,
                    string nFactura,
                    string ano,
                    string cEstado)
        {

            string ambiente = _cfg.Ambiente;
            string urlBaseDestino                     = _cfg.UrlBaseDestino;
            string rutaBaseFisicaExpedientes          = _cfg.RutaBaseFisicaExpedientes;
            string rutaBaseFisicaComision             = _cfg.RutaBaseFisicaComision;
            string rutaBaseFisicaDocumentoAdjuntoBase = _cfg.RutaBaseFisicaDocumentoAdjuntoBase;
            string urlBaseDestinoDocumentoAdjuntoBase = _cfg.RutaBaseFisicaDocumentoAdjuntoBase;
            string urlBaseDestinoComision             = _cfg.RutaBaseFisicaComision;
            string prefijo                            = _cfg.Prefijo;

            string urlBaseLink = _cfg.UrlBaseLink;

            List<FacturaModel> datos = new List<FacturaModel>();
            try
            {
                using (_connection = new SqlConnection(_dbservice.GetConnectionString()))
                {

                    var rolSeleccionado = _httpContextAccessor.HttpContext?.User
                    ?.FindFirst("rolSeleccionado")?.Value;

                    _command = _connection.CreateCommand();

                    _command.CommandType = CommandType.StoredProcedure;
                    _command.CommandText = "[dbo].[proc_FacturasHisto_Obtener]";
                    _command.Parameters.AddWithValue("@rol", rolSeleccionado ?? "");
                    _command.Parameters.AddWithValue("@rutProveedor", rutProveedor ?? "");
                    _command.Parameters.AddWithValue("@NFactura", nFactura ?? "");
                    _command.Parameters.AddWithValue("@Ano", ano ?? "");
                    _command.Parameters.AddWithValue("@CEstado", cEstado );

                    _connection.Open();

                    SqlDataReader dr = _command.ExecuteReader();

                    while (dr.Read())
                    {
                        FacturaModel factura = new FacturaModel();
                        factura = _setFactura(dr);

                        // ===================== EXPEDIENTE =====================
                        string nombreExpediente = $"{factura.rutProveedor}_{factura.nFactura}_FULL.pdf";
                        string rutaExpediente = Path.Combine(rutaBaseFisicaExpedientes, nombreExpediente);

                        factura.ExisteExpediente = File.Exists(rutaExpediente);
                        factura.linkExpedientes = factura.ExisteExpediente
                            ? urlBaseDestino + nombreExpediente
                            : null;

                        // ===================== COMISION =====================
                        string nombreComision = $"{prefijo}{factura.rutProveedor}_{factura.nFactura}.pdf";
                        string rutaComision = Path.Combine(rutaBaseFisicaComision, nombreComision);

                        factura.ExisteComision = File.Exists(rutaComision);
                        factura.linkComision = factura.ExisteComision
                            ? urlBaseDestinoComision + nombreComision
                            : null;

                        factura.linkComision = urlBaseLink + "FacturasComision/" + nombreComision;

                        // ===================== DOCUMENTO ADJUNTO (FIX ✅) =====================
                        factura.ExisteDocumentoAdjunto = false;
                        factura.linkDocumentoAdjunto = null;

                        var rutaFisicaAdjunto = Path.Combine(rutaBaseFisicaDocumentoAdjuntoBase, factura.rutProveedor.ToString());
                        var urlAdjunto = urlBaseDestinoDocumentoAdjuntoBase + factura.rutProveedor.ToString() + "/";

                        if (!string.IsNullOrWhiteSpace(factura.rutProveedor.ToString()) &&
                            Directory.Exists(rutaFisicaAdjunto))
                        {
                            var archivoAdjunto = Directory
                                .EnumerateFiles(
                                    rutaFisicaAdjunto,
                                    $"*_{factura.nFactura}_*.pdf"
                                )
                                .Select(Path.GetFileName)
                                .FirstOrDefault();

                            factura.ExisteDocumentoAdjunto = archivoAdjunto != null;
                            factura.linkDocumentoAdjunto = factura.ExisteDocumentoAdjunto
                                ? urlAdjunto + archivoAdjunto
                                : null;
                        }
                        datos.Add(factura);
                    }
                    dr.Close();
                    _connection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return datos;
        }
        public FacturaModel GetFactura(
            string rutProveedor,
            string nFactura,
            string ano,
            string cEstado)
        {

            string ambiente = _cfg.Ambiente;
            string urlBaseDestino = _cfg.UrlBaseDestino;
            string rutaBaseFisicaExpedientes = _cfg.RutaBaseFisicaExpedientes;
            string rutaBaseFisicaComision = _cfg.RutaBaseFisicaComision;
            string rutaBaseFisicaDocumentoAdjuntoBase = _cfg.RutaBaseFisicaDocumentoAdjuntoBase;
            string urlBaseDestinoDocumentoAdjuntoBase = _cfg.RutaBaseFisicaDocumentoAdjuntoBase;
            string urlBaseDestinoComision = _cfg.RutaBaseFisicaComision;
            string prefijo = _cfg.Prefijo;

            string urlBaseLink = _cfg.UrlBaseLink;

            FacturaModel factura = new FacturaModel();
            try
            {
                using (_connection = new SqlConnection(_dbservice.GetConnectionString()))
                {
                    ///// se requiere saber el programa para poder filtrar los datos
                    var rolSeleccionado = _httpContextAccessor.HttpContext?.User
                    ?.FindFirst("rolSeleccionado")?.Value;

                    _command = _connection.CreateCommand();

                    _command.CommandType = CommandType.StoredProcedure;
                    _command.CommandText = "[dbo].[proc_Facturas_Obtener]";
                    _command.Parameters.AddWithValue("@rol", rolSeleccionado ?? "");
                    _command.Parameters.AddWithValue("@rutProveedor", rutProveedor ?? "");
                    _command.Parameters.AddWithValue("@NFactura", nFactura ?? "");
                    _command.Parameters.AddWithValue("@Ano", ano ?? "");
                    _command.Parameters.AddWithValue("@CEstado", cEstado );

                    _connection.Open();

                    SqlDataReader dr = _command.ExecuteReader();
                    if (!dr.HasRows)
                    {
                        throw new Exception(
                            $"No se encontró factura. Rut={rutProveedor} Factura={nFactura}"
                        );
                    }
                    if (dr.Read())
                    {
                        factura = _setFactura(dr);

                        // ===================== EXPEDIENTE =====================
                        string nombreExpediente = $"{factura.rutProveedor}_{factura.nFactura}_FULL.pdf";
                        string rutaExpediente = Path.Combine(rutaBaseFisicaExpedientes, nombreExpediente);

                        factura.ExisteExpediente = File.Exists(rutaExpediente);
                        factura.linkExpedientes = factura.ExisteExpediente
                            ? urlBaseDestino + nombreExpediente
                            : null;

                        // ===================== COMISION =====================
                        string nombreComision = $"{prefijo}{factura.rutProveedor}_{factura.nFactura}.pdf";
                        string rutaComision = Path.Combine(rutaBaseFisicaComision, nombreComision);

                        factura.ExisteComision = File.Exists(rutaComision);
                        factura.linkComision = factura.ExisteComision
                            ? urlBaseDestinoComision + nombreComision
                            : null;

                        factura.linkComision = urlBaseLink + "FacturasComision/" + nombreComision;

                        // ===================== DOCUMENTO ADJUNTO (FIX ✅) =====================
                        factura.ExisteDocumentoAdjunto = false;
                        factura.linkDocumentoAdjunto = null;

                        // ✅ usar el rut REAL desde la BD

                        var rutaFisicaAdjunto = Path.Combine(rutaBaseFisicaDocumentoAdjuntoBase, factura.rutProveedor.ToString());
                        var urlAdjunto = urlBaseDestinoDocumentoAdjuntoBase + factura.rutProveedor.ToString() + "/";

                        if (!string.IsNullOrWhiteSpace(factura.rutProveedor.ToString()) &&
                            Directory.Exists(rutaFisicaAdjunto))
                        {
                            var archivoAdjunto = Directory
                                .EnumerateFiles(
                                    rutaFisicaAdjunto,
                                    $"*_{factura.nFactura}_*.pdf"
                                )
                                .Select(Path.GetFileName)
                                .FirstOrDefault();

                            factura.ExisteDocumentoAdjunto = archivoAdjunto != null;
                            factura.linkDocumentoAdjunto = factura.ExisteDocumentoAdjunto
                                ? urlAdjunto + archivoAdjunto
                                : null;
                        }
                    }
                    dr.Close();
                    _connection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return factura;
        }

        private FacturaModel _setFactura(SqlDataReader dr)
        {
            return new FacturaModel
            {
                pedidoCompra = dr.GetLongSafe("PedidoCompra"),
                cMaterial = dr.GetLongSafe("CMaterial"),
                denominacion = dr.GetStringSafe("Denominacion"),
                rutProveedor = dr.GetLongSafe("RutProveedor"),
                nombreProveedor = dr.GetStringSafe("NombreProveedor"),
                nFactura = dr.GetLongSafe("NFactura"),
                ano = dr.GetIntSafe("Ano"),
                total = dr.GetIntSafe("Total"),
                expediente = dr.GetDateTimeSafe("Expediente"),
                cEstado = dr.GetIntSafe("CEstado"),
                estadoOK = dr.GetIntSafe("Estado_OK"),
                cEstadoOKObservaciones = dr.GetIntSafe("CEstadoOK_Observaciones"),
                estadoNOOK = dr.GetIntSafe("Estado_NOOK"),
                tEstado = dr.GetStringSafe("TEstado"),
                tEstadoTiempoPresente = dr.GetStringSafe("TEstadoTiempoPresente"),
                tObservacion = dr.GetStringSafe("TObservacion"),
                tEstadoOK = dr.GetStringSafe("TEstadoOK"),
                tEstadoOKObservaciones = dr.GetStringSafe("TEstadoOK_Observaciones"),
                tEstadoNOOK = dr.GetStringSafe("tEstadoNOOK"),
                puedeEjecutarOK = dr.GetBoolSafe("PuedeEjecutarOK"),
                puedeEjecutarNOOK = dr.GetBoolSafe("PuedeEjecutarNOOK"),
                puedeVerOK = dr.GetBoolSafe("PuedeVerOK"),
                puedeVerNOOK = dr.GetBoolSafe("PuedeVerNOOK")
            };
        }

        public EstadoFacturaModel putEstado(string rutusuario, string rutProveedor, string NFactura, string campo, string obs)
        {
            string mje = "";
            string updateCampo = "";
            string insertCampo = "";

            switch (campo)
            {
                case "1": insertCampo = "[EnRevisionCenabast]"; updateCampo = "[EnRevisionCenabast] = CONVERT(varchar,GETDATE(),126) "; break;
                case "2": insertCampo = "[DevueltoContrato]"; updateCampo = "[DevueltoContrato] = CONVERT(varchar,GETDATE(),126) "; break;
                case "3": insertCampo = "[EnviarContrato]"; updateCampo = "[EnviarContrato] = CONVERT(varchar,GETDATE(),126) "; break;
                case "4": insertCampo = "[EnviarMinsal]"; updateCampo = "[EnviarMinsal] = CONVERT(varchar,GETDATE(),126) "; break;
                case "5": insertCampo = "[EnRevisionMinsal]"; updateCampo = "[EnRevisionMinsal] = CONVERT(varchar,GETDATE(),126) "; break;
                case "6": insertCampo = "[Rechazado]"; updateCampo = "[Rechazado] = CONVERT(varchar,GETDATE(),126) "; break;
                case "7": insertCampo = "[Aceptado]"; updateCampo = "[Aceptado] = CONVERT(varchar,GETDATE(),126) "; break;
                case "8": insertCampo = "[AceptadoObservacion]"; updateCampo = "[AceptadoObservacion] = CONVERT(varchar,GETDATE(),126) "; break;
                case "9": insertCampo = "[EnGestionPago]"; updateCampo = "[EnGestionPago] = CONVERT(varchar,GETDATE(),126) "; break;
                case "10": insertCampo = "[Cerrado]"; updateCampo = "[Cerrado] = CONVERT(varchar,GETDATE(),126) "; break;
                case "11": insertCampo = "[OKContrato]"; updateCampo = "[OKContrato] = CONVERT(varchar,GETDATE(),126) "; break;
                case "Expediente": insertCampo = "[Expediente]"; updateCampo = "[Expediente] = CONVERT(varchar,GETDATE(),126) "; break;
                default: insertCampo = "[EnRevisionCenabast]"; updateCampo = "[EnRevisionCenabast] = CONVERT(varchar,GETDATE(),126) "; break;
            }

            string updateCampoEstado = campo != "Expediente" ? ", NEstado = @NEstado " : "";

            // 🔹 Un solo bloque SQL que maneja inserción o actualización
            string sql = $@"
                IF EXISTS (SELECT 1 FROM EstadoFactura WHERE RutProveedor = @RutProveedor AND NFactura = @NFactura)
                BEGIN
                    UPDATE EstadoFactura
                    SET FEstado = CONVERT(varchar,GETDATE(),126),
                        TObservacion = @Obs
                        {updateCampoEstado},
                        {updateCampo}
                    WHERE RutProveedor = @RutProveedor AND NFactura = @NFactura;
                END
                ELSE
                BEGIN
                    INSERT INTO EstadoFactura (RutProveedor, NFactura, {(campo != "Expediente" ? "NEstado," : "")} {insertCampo}, FEstado, TObservacion)
                    VALUES (@RutProveedor, @NFactura, {(campo != "Expediente" ? "@NEstado," : "")} CONVERT(varchar,GETDATE(),126), CONVERT(varchar,GETDATE(),126), @Obs);
                END";

            int i = 0;

            using (SqlConnection con = new SqlConnection(_dbservice.GetConnectionString()))
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@RutProveedor", rutProveedor);
                cmd.Parameters.AddWithValue("@NFactura", NFactura);
                cmd.Parameters.AddWithValue("@Obs", obs ?? "");
                if (campo != "Expediente") cmd.Parameters.AddWithValue("@NEstado", campo);

                con.Open();
                i = cmd.ExecuteNonQuery();
            }
            return null;
        }


        public List<FacturaLista> getListFactura(string rutproveedor)
        {

            SqlConnection con = new SqlConnection(_dbservice.GetConnectionString());

            SqlCommand cmd = new SqlCommand("proc_FacturasProveedor_Obtener @rutProveedor", con);

            cmd.Parameters.Add("@rutProveedor", SqlDbType.Char, 10)
              .Value = rutproveedor.Trim().PadLeft(10, '0');

            SqlDataReader reader;

            List<FacturaLista> facturas = new List<FacturaLista>();
            con.Open();
            try
            {
                reader = cmd.ExecuteReader();
                // if (reader.HasRows)  
                // {
                while (reader.Read())
                {
                    FacturaLista factura = new FacturaLista();
                    factura.factura = (reader["NFactura"].ToString());

                    facturas.Add(factura);
                }
            }
            catch (SqlException sex)
            {
                Console.WriteLine("Error: " + sex.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.ToString());
            }
            finally
            {
                con.Close();
            }
            return facturas;
        }

    }
}
