using Microsoft.Data.SqlClient;
using SIGEPROS.Models;
using System.Data;

namespace SIGEPROS.Services
{
    public class DBDespachosService
    {
        private readonly DBService _dbservice;
        private static IConfiguration? _configuracion;
        private static IConfiguration? _config;
        private readonly ConfiguracionArchivos _cfg;

        SqlConnection? _connection = null;
        SqlCommand? _command = null;
        SqlCommand? _commandIdentity = null;

        public DBDespachosService(DBService dbservice, IConfiguration configuracion)
        {
            _dbservice = dbservice;
            _configuracion = configuracion;
            _cfg = ConfiguracionArchivos.Crear(configuracion);
        }

        public List<DespachoModel> getDespachosByFactura(string rut, string nFactura, Boolean incluye)
        {
            DespachoModel despacho = new DespachoModel();
            List<DespachoModel> despachos = new List<DespachoModel>();
            try
            {
                string urlBaseCedibles = _cfg.UrlBaseCedibles;

                int programa = 0; ///temporalmente
                using (_connection = new SqlConnection(_dbservice.GetConnectionString()))
                {
                    _command = _connection.CreateCommand();
                    _command.CommandType = CommandType.StoredProcedure;
                    _command.CommandText = "proc_Guias_Obtener";
                    _command.Parameters.AddWithValue("@nFactura", nFactura);
                    _command.Parameters.AddWithValue("@rutProveedor", rut);
                    _command.Parameters.AddWithValue("@excluyeSS", incluye ? 1 : 0); // excluir el servicio de salud mejora el tiempo de ejecución de la query Guías

                    _connection.Open();
                    SqlDataReader dr = _command.ExecuteReader();
                    UsuarioModel user = null;
                    while (dr.Read())
                    {
                        despacho = new DespachoModel();
                        despacho.LIFNR = dr["LIFNR"].ToString();
                        despacho.NFactura = dr["NFactura"].ToString();
                        despacho.DocumentoVenta = dr["DocumentoVenta"].ToString();
                        despacho.PedidoCompra = dr["PedidoCompra"].ToString();
                        despacho.GrupoArticulos = dr["GrupoArticulos"].ToString();
                        despacho.Canal = dr["Canal"].ToString();
                        despacho.DescripcionCanal = dr["DescripcionCanal"].ToString();
                        despacho.GuiaDespacho = dr["GuiaDespacho"].ToString();
                        despacho.CodigoMaterial = dr["CodigoMaterial"].ToString();
                        despacho.Denominacion = dr["Denominacion"].ToString();
                        despacho.Lote = dr["Lote"].ToString();
                        despacho.CantidadDocumento = dr["CantidadDocumento"].ToString();
                        despacho.CantidadInformada = dr["CantidadInformada"].ToString();
                        despacho.ValorNeto = dr["ValorNeto"].ToString();
                        ///despacho.ZZIDPRO_E = dr["ZZIDPRO_E"].ToString();
                        despacho.ZZIDMER_E = dr["ZZIDMER_E"].ToString();
                        despacho.RutEstablecimiento = dr["RutEstablecimiento"].ToString();
                        despacho.Destinatario = dr["Destinatario"].ToString();
                        despacho.ServicioSalud = dr["ServicioSalud"].ToString();

                        if (despacho != null &&
                            !string.IsNullOrWhiteSpace(urlBaseCedibles) &&
                            !string.IsNullOrWhiteSpace(despacho.LIFNR) &&
                            int.TryParse(despacho.LIFNR, out var lifnr) &&
                            !string.IsNullOrWhiteSpace(despacho.DocumentoVenta))
                        {
                            despacho.linkCedible = urlBaseCedibles + int.Parse(despacho.LIFNR) + "/procesados/" + despacho.DocumentoVenta + ".pdf";
                        }
                        else
                        {
                            despacho.linkCedible = "no url"; // o string.Empty
                        }
                        despachos.Add(despacho);
                    }
                    dr.Close();
                    _connection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return despachos;

        }
    }
}
