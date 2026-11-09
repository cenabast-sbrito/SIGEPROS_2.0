using ClosedXML.Excel;
using Microsoft.Data.SqlClient;
using SIGEPROS.Models;
using SIGEPROS.Models.DTO;
using SIGEPROS.Services;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Text;

public class PlanillaZdis022Service : IPlanillaZdis022Service
{
    private readonly DBService _dbservice;
    private readonly DBProveedorService _dbproveedorservice;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PlanillaZdis022Service(DBService dbservice, IHttpContextAccessor httpContextAccessor, DBProveedorService dbproveedorservice)
    {
        _dbservice = dbservice;
        _httpContextAccessor = httpContextAccessor;
        _dbproveedorservice = dbproveedorservice;
    }
    public ResultadoCargaDto Procesar(
        Stream stream,
        ParametrosCargaDto parametros)
    {

        var resultado = new ResultadoCargaDto();

        try
        {
            using var workbook = new XLWorkbook(stream);

            var worksheet = workbook.Worksheet(1);

            var rows = worksheet.RangeUsed().RowsUsed();

            if (!ValidarEstructura(worksheet,out string mensaje))
            {
                resultado.Status = false;
                resultado.Message = mensaje;

                return resultado;
            }

            if (rows.Count() <= 1)
            {
                resultado.Status = false;
                resultado.Message = "El archivo no contiene datos.";

                return resultado;
            }

            var despacho = new PlanillaZDIS022();

            truncDespachos("Despachos_TEMP");

            foreach (var row in rows.Skip(1))
            {
                ProcesarFila(
                    row,
                    parametros,
                    resultado);
            }

            resultado.CountRechazos =
                resultado.Count -
                resultado.CountFacturasAceptadas;

            resultado.CountNoCero =
                resultado.CountFacturasAceptadas;

            resultado.Status = true;
            resultado.Message = "OK";

            return resultado;
        }
        catch (Exception ex)
        {
            resultado.Status = false;
            resultado.Message = ex.Message;
            return resultado;
        }
    }

    private void ProcesarFila(
    IXLRangeRow row,
    ParametrosCargaDto parametros,
    ResultadoCargaDto resultado)
    {
        try
        {
            var documentoVenta =
                row.Cell(1).GetString();

            if (string.IsNullOrWhiteSpace(documentoVenta))
                return;

            if (documentoVenta == "Documento de venta")
                return;

            resultado.Count++;

            string canal =
                row.Cell(4).GetString();
            var rolSeleccionado = _httpContextAccessor.HttpContext?.User?.FindFirst("rolSeleccionado")?.Value;
            if (!OrganismoCanalesHelper.EsCanalValido(
                    /////parametros.ProgramaDefinido,
                    rolSeleccionado, canal))
            {
                resultado.CountFilasOtroCanal++;
                return;
            }

            string periodoFila =
                ObtenerPeriodo(row.Cell(7));

            if (parametros.FiltroPeriodo &&
                periodoFila != parametros.FechaCarga)
            {
                resultado.CountFilasFueraDelPeriodo++;
                return;
            }

            int numeroFactura =
                ObtenerFactura(row.Cell(13));

            var nFacturaSAP = _dbproveedorservice.getFacturaDV(documentoVenta);

            if (numeroFactura <= 0)
            {
                resultado.CountFilasSinFactura++;
                return;
            }
            //if(nFacturaSAP != numeroFactura)
            //{
            //    resultado.CountFacturasNOSAP++;
            //    return;
            //}

            string pedidoCompra =
                row.Cell(2).GetString();

            var proveedor =
                _dbproveedorservice.getProveedorPedidoCompra(
                    pedidoCompra);

            if (!int.TryParse(
                    parametros.RutProveedorCarga,
                    out int rutProveedor))
            {
                resultado.CountFilasOtroProveedor++;
                return;
            }

            if (proveedor.RutProveedor != rutProveedor)
            {
                resultado.CountFilasOtroProveedor++;
                return;
            }

            var despacho = new PlanillaZDIS022();

            despacho = despacho._setRow(row);

            setDespachos(
                despacho,
                "Despachos_TEMP");

            resultado.CountFacturasAceptadas++;
        }
        catch
        {
            resultado.CountRechazos++;
        }
    }


    private string ObtenerPeriodo(
    IXLCell cell)
    {
        try
        {
            var fecha = cell.GetDateTime();

            return fecha.ToString("yyyy-MM");
        }
        catch
        {
            if (DateTime.TryParse(
                cell.GetString(),
                out DateTime fecha))
            {
                return fecha.ToString("yyyy-MM");
            }

            return string.Empty;
        }
    }

    private int ObtenerFactura(
    IXLCell cell)
    {
        if (int.TryParse(
            cell.GetString(),
            out int numero))
        {
            return numero;
        }

        return 0;
    }

    public int truncDespachos(string tabla)
    {

        int i = 0;
        string sql = " TRUNCATE TABLE [dbo].[" + tabla + "] ";

        SqlConnection con = new SqlConnection(_dbservice.GetConnectionString());
        SqlCommand cmd = new SqlCommand(sql, con);
        con.Open();
        try
        {
            i = cmd.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.ToString());
            i = 0;
        }
        finally
        {
            con.Close();
        }

        return i;
    }
    public int setDespachos(PlanillaZDIS022 despacho, string tabla)
    {

        int i = 0;
        string sql = "INSERT INTO [dbo].[Despachos_TEMP] " +
            "(  [Documento de venta]       " +
                               " ,[Pedido de Compra]	     " +
                               " ,[Grupo de articulos]	     " +
                               " ,[Canal]				     " +
                               " ,[Código Material]		     " +
                               " ,[Fecha de entrega al]	     " +
                               " ,[Fecha entrega al OL]	     " +
                               " ,[Cantidad documento d]     " +
                               " ,[Cantidad informada]	     " +
                               " ,[Lote]				     " +
                               " ,[Fecha entrega al ol(]     " +
                               " ,[OS]					     " +
                               " ,[NFactura]			     " +
                               " ,[Guía de despacho]	     " +
                               " ,[Observaciones]		     " +
                               " ,[Suspensión de entreg]     " +
                               " ,[Autoriza suspensión]	     " +
                               " ,[Fecha de carga de in]     " +
                               " ,[Salida mercancía]	     " +
                               " ,[Documento de entrega]     " +
                               " ,[Status carga cedible]     " +
                               " ,[Fecha Sal# mercancía]     " +
                               " ,[Fact# comisiones pro]     " +
                               " ,[Fecha fact# comision]     " +
                               " ,[Fecha recepción conf]     " +
                               " ,[Fecha entrega cedibl]     " +
                               " ,[Centro]				     " +
                               " ,[Valor neto]			     " +
                               " ,[Bultos]				     " +
                               " ,[Volumen]				     " +
                               " ,[Peso]				     " +
                               " ,[Fecha vencimiento]	     " +
                               " ,[GTIN]				     " +
                               " ,[Agrupador]			     " +
                               " ,[Motivo Rechazo]		     " +
                               " ,[OC]					     " +
                               " ,[DocVenta]			     )  VALUES " +
                "( " +
                "  @Documento_de_venta    " +
                " ,@Pedido_de_Compra      " +
                " ,@Grupo_de_articulos    " +
                " ,@Canal                 " +
                " ,@Código_Material       " +
                " ,@Fecha_de_entrega_al   " +
                " ,@Fecha_entrega_al_OL   " +
                " ,@Cantidad_documento_d  " +
                " ,@Cantidad_informada    " +
                " ,@Lote                  " +
                " ,@Fecha_entrega_al_ol_  " +
                " ,@OS                    " +
                " ,@NFactura              " +
                " ,@Guía_de_despacho      " +
                " ,@Observaciones         " +
                " ,@Suspensión_de_entreg  " +
                " ,@Autoriza_suspensión   " +
                " ,@Fecha_de_carga_de_in  " +
                " ,@Salida_mercancía      " +
                " ,@Documento_de_entrega  " +
                " ,@Status_carga_cedible  " +
                " ,@Fecha_Sal_mercancía   " +
                " ,@Fact_comisiones_pro   " +
                " ,@Fecha_fact_comision   " +
                " ,@Fecha_recepción_conf  " +
                " ,@Fecha_entrega_cedibl  " +
                " ,@Centro                " +
                " ,@Valor_neto            " +
                " ,@Bultos                " +
                " ,@Volumen               " +
                " ,@Peso                  " +
                " ,@Fecha_vencimiento     " +
                " ,@GTIN                  " +
                " ,@Agrupador             " +
                " ,@Motivo_Rechazo        " +
                " ,@OC                    " +
                " ,@DocVenta              " +
                " )";

        SqlConnection con = new SqlConnection(_dbservice.GetConnectionString());
        SqlCommand cmd = new SqlCommand(sql, con);
        con.Open();
        try
        {

            cmd.Parameters.AddWithValue("Documento_de_venta", despacho.Documento_de_venta);
            cmd.Parameters.AddWithValue("Pedido_de_Compra", despacho.Pedido_de_Compra);
            cmd.Parameters.AddWithValue("Grupo_de_articulos", despacho.Grupo_de_articulos);
            cmd.Parameters.AddWithValue("Canal", despacho.Canal);
            cmd.Parameters.AddWithValue("Código_Material", despacho.Código_Material);
            cmd.Parameters.AddWithValue("Fecha_de_entrega_al", despacho.Fecha_de_entrega_al);
            cmd.Parameters.AddWithValue("Fecha_entrega_al_OL", despacho.Fecha_entrega_al_OL);
            cmd.Parameters.AddWithValue("Cantidad_documento_d", despacho.Cantidad_documento_d);
            cmd.Parameters.AddWithValue("Cantidad_informada", despacho.Cantidad_informada);
            cmd.Parameters.AddWithValue("Lote", despacho.Lote);
            cmd.Parameters.AddWithValue("Fecha_entrega_al_ol_", despacho.Fecha_entrega_al_ol_);
            cmd.Parameters.AddWithValue("OS", despacho.OS);
            cmd.Parameters.AddWithValue("NFactura", despacho.NFactura);
            cmd.Parameters.AddWithValue("Guía_de_despacho", despacho.Guía_de_despacho);
            cmd.Parameters.AddWithValue("Observaciones", despacho.Observaciones);
            cmd.Parameters.AddWithValue("Suspensión_de_entreg", despacho.Suspensión_de_entreg);
            cmd.Parameters.AddWithValue("Autoriza_suspensión", despacho.Autoriza_suspensión);
            cmd.Parameters.AddWithValue("Fecha_de_carga_de_in", despacho.Fecha_de_carga_de_in);
            cmd.Parameters.AddWithValue("Salida_mercancía", despacho.Salida_mercancía);
            cmd.Parameters.AddWithValue("Documento_de_entrega", despacho.Documento_de_entrega);
            cmd.Parameters.AddWithValue("Status_carga_cedible", despacho.Status_carga_cedible);
            cmd.Parameters.AddWithValue("Fecha_Sal_mercancía", despacho.Fecha_Sal_mercancía);
            cmd.Parameters.AddWithValue("Fact_comisiones_pro", despacho.Fact_comisiones_pro);
            cmd.Parameters.AddWithValue("Fecha_fact_comision", despacho.Fecha_fact_comision);
            cmd.Parameters.AddWithValue("Fecha_recepción_conf", despacho.Fecha_recepción_conf);
            cmd.Parameters.AddWithValue("Fecha_entrega_cedibl", despacho.Fecha_entrega_cedibl);

            /* código correcto */
            cmd.Parameters.Add("@Centro", SqlDbType.Int)
          .Value = string.IsNullOrWhiteSpace(despacho.Centro)
                   ? (object)DBNull.Value
                   : int.Parse(despacho.Centro);

            cmd.Parameters.Add("@Valor_neto", SqlDbType.Float)
                          .Value = string.IsNullOrWhiteSpace(despacho.Valor_neto)
                                   ? (object)DBNull.Value
                                   : double.Parse(despacho.Valor_neto, CultureInfo.InvariantCulture);

            cmd.Parameters.Add("@Bultos", SqlDbType.Float)
                          .Value = string.IsNullOrWhiteSpace(despacho.Bultos)
                                   ? (object)DBNull.Value
                                   : double.Parse(despacho.Bultos, CultureInfo.InvariantCulture);

            cmd.Parameters.Add("@Volumen", SqlDbType.Float)
                          .Value = string.IsNullOrWhiteSpace(despacho.Volumen)
                                   ? (object)DBNull.Value
                                   : double.Parse(despacho.Volumen, CultureInfo.InvariantCulture);

            cmd.Parameters.Add("@Peso", SqlDbType.Float)
                          .Value = string.IsNullOrWhiteSpace(despacho.Peso)
                                   ? (object)DBNull.Value
                                   : double.Parse(despacho.Peso, CultureInfo.InvariantCulture);

            cmd.Parameters.AddWithValue("Fecha_vencimiento", despacho.Fecha_vencimiento);
            cmd.Parameters.AddWithValue("GTIN", despacho.GTIN);
            cmd.Parameters.AddWithValue("Agrupador", despacho.Agrupador);
            cmd.Parameters.AddWithValue("Motivo_Rechazo", despacho.Motivo_Rechazo);
            cmd.Parameters.AddWithValue("OC", despacho.OC);
            cmd.Parameters.AddWithValue("DocVenta", despacho.DocVenta);
            foreach (SqlParameter p in cmd.Parameters)
            {
                Debug.WriteLine($"{p.ParameterName} = {p.Value} ({p.SqlDbType})");
            }

            i = cmd.ExecuteNonQuery();
        }
        catch (SqlException ex)
        {
            Console.WriteLine("Mensaje : " + ex.Message);
            Console.WriteLine("Número  : " + ex.Number);
            Console.WriteLine("Estado  : " + ex.State);
            Console.WriteLine("Proced. : " + ex.Procedure);
            Console.WriteLine("Línea   : " + ex.LineNumber);

            throw;
        }
        finally
        {
            con.Close();
        }

        return i;
    }

    /* VALIDACIONES INCIALES */
    private bool ValidarEstructura(IXLWorksheet worksheet, out string mensajefinal)
    {
        if (!ValidarEstructura1(
        worksheet,
        out string mensaje1))
        {
            mensajefinal = mensaje1;
            return false;
        }
        if (!ValidarEstructura2(
        worksheet,
        out string mensaje2))
        {
            mensajefinal = mensaje2;
            return false;
        }
        mensajefinal = "OK";
        return true;
    }

    /// <summary>
    /// el numero de columnas es 37
    /// </summary>
    /// <param name="worksheet"></param>
    /// <param name="mensaje"></param>
    /// <returns></returns>
    private bool ValidarEstructura1(IXLWorksheet worksheet, out string mensaje)
    {
        mensaje = string.Empty;

        var header = worksheet.FirstRowUsed();

        if (header == null)
        {
            mensaje = "La planilla no contiene encabezados.";
            return false;
        }
        var cantidadColumnas = header.CellsUsed().Count();
        if (cantidadColumnas < 35)
        {
            mensaje = $"La planilla posee {cantidadColumnas} columnas y se esperaban al menos 35.";
            return false;
        }

        return true;
    }
    private readonly Dictionary<int, string> ColumnasObligatorias =
new()
{
    { 1, "Documento" },
    { 2, "Pedido" },
    { 4, "Canal" },
    { 5, "Material" },
    { 13, "Factura" },
    { 14, "Guía" },
    { 27, "Centro" }
};
    private bool ValidarEstructura2(
    IXLWorksheet worksheet,
    out string mensaje)
    {
        mensaje = "";

        var header = worksheet.FirstRowUsed();

        foreach (var columna in ColumnasObligatorias)
        {
            string valorExcel =
                header.Cell(columna.Key)
                      .GetString()
                      .Trim();

            if (!valorExcel.Contains(
                    columna.Value,
                    StringComparison.OrdinalIgnoreCase))
            {
                mensaje =
                    $"La columna {columna.Key} no parece correcta. " +
                    $"Se esperaba encontrar '{columna.Value}' y se recibió '{valorExcel}'.";

                return false;
            }
        }

        return true;
    }


    private readonly string[] EncabezadosEsperados =
    {
        "Documento de venta",
        "Pedido de Compra",
        "Grupo de articulos",
        "Canal",
        "Codigo Material",
        "Fecha de entrega al",
        "Fecha entrega al OL",
        "Cantidad documento d",
        "Cantidad informada",
        "Lote",
        "Fecha entrega al ol",
        "OS",
        "Factura",
        "Guía de despacho",
        "Observaciones",
        "Suspensión de entreg",
        "Autoriza suspensión",
        "Fecha de carga de in",
        "Salida mercancía",
        "Documento de entrega",
        "Status carga cedible",
        "Fecha Sal mercancía",
        "Fact comisiones pro",
        "Fecha fact comisión",
        "Fecha recepción conf",
        "Fecha entrega cedibl",
        "Centro",
        "Valor neto",
        "Bultos",
        "Volumen",
        "Peso",
        "Fecha vencimiento",
        "GTIN",
        "Agrupador",
        "Motivo Rechazo",
        "OC",
        "DocVenta"
    };
    /// <summary>
    /// las columnas son las esperadas
    /// </summary>
    /// <param name="worksheet"></param>
    /// <param name="mensaje"></param>
    /// <returns></returns>
    private bool ValidarEstructura2_muy_exigente(
    IXLWorksheet worksheet,
    out string mensaje)
    {
        mensaje = string.Empty;

        var header = worksheet.FirstRowUsed();

        for (int i = 0; i < EncabezadosEsperados.Length; i++)
        {
            string valorExcel =
                header.Cell(i + 1).GetString().Trim();

            if (NormalizarTexto(valorExcel) !=
                NormalizarTexto(EncabezadosEsperados[i]))
            {
                mensaje =
                    $"La columna {i + 1} no corresponde. " +
                    $"Se esperaba '{EncabezadosEsperados[i]}' " +
                    $"y se recibió '{valorExcel}'.";

                return false;
            }
        }

        return true;
    }

    private static string NormalizarTexto(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
            return string.Empty;

        string normalizado = texto.Normalize(NormalizationForm.FormD);

        StringBuilder sb = new StringBuilder();

        foreach (char c in normalizado)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(c);
            }
        }

        return sb.ToString()
                 .Normalize(NormalizationForm.FormC)
                 .Trim()
                 .ToUpperInvariant();
    }
    public List<PlanillaZDIS022> getZDis022(string tabla)
    {
        string sql = "SELECT " +

            "  [Documento de venta]    Documento_de_venta   " +
                       " ,[Pedido de Compra]	    Pedido_de_Compra     " +
                       " ,[Grupo de articulos]	    Grupo_de_articulos   " +
                       " ,[Canal]				    Canal                " +
                       " ,[Código Material]		    Código_Material      " +
                       " ,[Fecha de entrega al]	    Fecha_de_entrega_al  " +
                       " ,[Fecha entrega al OL]	    Fecha_entrega_al_OL  " +
                       " ,[Cantidad documento d]    Cantidad_documento_d " +
                       " ,[Cantidad informada]	    Cantidad_informada   " +
                       " ,[Lote]				    Lote                 " +
                       " ,[Fecha entrega al ol(]    Fecha_entrega_al_ol_ " +
                       " ,[OS]					    OS                   " +
                       " ,[NFactura]			    NFactura             " +
                       " ,[Guía de despacho]	    Guía_de_despacho     " +
                       " ,[Observaciones]		    Observaciones        " +
                       " ,[Suspensión de entreg]    Suspensión_de_entreg " +
                       " ,[Autoriza suspensión]	    Autoriza_suspensión  " +
                       " ,[Fecha de carga de in]    Fecha_de_carga_de_in " +
                       " ,[Salida mercancía]	    Salida_mercancía     " +
                       " ,[Documento de entrega]    Documento_de_entrega " +
                       " ,[Status carga cedible]    Status_carga_cedible " +
                       " ,[Fecha Sal# mercancía]    Fecha_Sal_mercancía  " +
                       " ,[Fact# comisiones pro]    Fact_comisiones_pro  " +
                       " ,[Fecha fact# comision]    Fecha_fact_comision  " +
                       " ,[Fecha recepción conf]    Fecha_recepción_conf " +
                       " ,[Fecha entrega cedibl]    Fecha_entrega_cedibl " +
                       " ,[Centro]				    Centro               " +
                       " ,[Valor neto]			    Valor_neto           " +
                       " ,[Bultos]				    Bultos               " +
                       " ,[Volumen]				    Volumen              " +
                       " ,[Peso]				    Peso                 " +
                       " ,[Fecha vencimiento]	    Fecha_vencimiento    " +
                       " ,[GTIN]				    GTIN                 " +
                       " ,[Agrupador]			    Agrupador            " +
                       " ,[Motivo Rechazo]		    Motivo_Rechazo       " +
                       " ,[OC]					    OC                   " +
                       " ,[DocVenta]			    DocVenta             "

        + " FROM [dbo].[" + tabla + "] ";
        SqlConnection con = new SqlConnection(_dbservice.GetConnectionString());
        SqlCommand cmd = new SqlCommand(sql, con);
        SqlDataReader reader;
        PlanillaZDIS022 despacho = null;
        List<PlanillaZDIS022> despachos = new List<PlanillaZDIS022>();
        con.Open();
        try
        {
            reader = cmd.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    despacho = new PlanillaZDIS022();
                    despacho = _get(despacho, reader);
                    despachos.Add(despacho);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.ToString());
        }
        finally
        {
            con.Close();
        }

        return despachos;
    }
    private PlanillaZDIS022 _get(PlanillaZDIS022 despacho, SqlDataReader reader)
    {
        despacho.Documento_de_venta = reader["Documento_de_venta"].ToString();
        despacho.Pedido_de_Compra = reader["Pedido_de_Compra"].ToString();
        despacho.Grupo_de_articulos = reader["Grupo_de_articulos"].ToString();
        despacho.Canal = reader["Canal"].ToString();
        despacho.Código_Material = reader["Código_Material"].ToString();
        despacho.Fecha_de_entrega_al = reader["Fecha_de_entrega_al"].ToString();
        despacho.Fecha_entrega_al_OL = reader["Fecha_entrega_al_OL"].ToString();
        despacho.Cantidad_documento_d = reader["Cantidad_documento_d"].ToString();
        despacho.Cantidad_informada = reader["Cantidad_informada"].ToString();
        despacho.Lote = reader["Lote"].ToString();
        despacho.Fecha_entrega_al_ol_ = reader["Fecha_entrega_al_ol_"].ToString();
        despacho.OS = reader["OS"].ToString();
        despacho.NFactura = reader["NFactura"].ToString();
        despacho.Guía_de_despacho = reader["Guía_de_despacho"].ToString();
        despacho.Observaciones = reader["Observaciones"].ToString();
        despacho.Suspensión_de_entreg = reader["Suspensión_de_entreg"].ToString();
        despacho.Autoriza_suspensión = reader["Autoriza_suspensión"].ToString();
        despacho.Fecha_de_carga_de_in = reader["Fecha_de_carga_de_in"].ToString();
        despacho.Salida_mercancía = reader["Salida_mercancía"].ToString();
        despacho.Documento_de_entrega = reader["Documento_de_entrega"].ToString();
        despacho.Status_carga_cedible = reader["Status_carga_cedible"].ToString();
        despacho.Fecha_Sal_mercancía = reader["Fecha_Sal_mercancía"].ToString();
        despacho.Fact_comisiones_pro = reader["Fact_comisiones_pro"].ToString();
        despacho.Fecha_fact_comision = reader["Fecha_fact_comision"].ToString();
        despacho.Fecha_recepción_conf = reader["Fecha_recepción_conf"].ToString();
        despacho.Fecha_entrega_cedibl = reader["Fecha_entrega_cedibl"].ToString();
        despacho.Centro = reader["Centro"].ToString();
        despacho.Valor_neto = reader["Valor_neto"].ToString();
        despacho.Bultos = reader["Bultos"].ToString();
        despacho.Volumen = reader["Volumen"].ToString();
        despacho.Peso = reader["Peso"].ToString();
        despacho.Fecha_vencimiento = reader["Fecha_vencimiento"].ToString();
        despacho.GTIN = reader["GTIN"].ToString();
        despacho.Agrupador = reader["Agrupador"].ToString();
        despacho.Motivo_Rechazo = reader["Motivo_Rechazo"].ToString();
        despacho.OC = reader["OC"].ToString();
        despacho.DocVenta = reader["DocVenta"].ToString();
        return despacho;
    }
    public string consolidar()
    {
        int i = 0;
        string resp = "";

        string sqlConsolidar = "[dbo].[proc_Consolidar]";

        SqlConnection con = new SqlConnection(_dbservice.GetConnectionString());
        SqlCommand cmd = new SqlCommand(sqlConsolidar, con);
        con.Open();
        try
        {
            i = cmd.ExecuteNonQuery();
            if (i != 0)
            {
                resp = "OK";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.ToString());
            resp = "Ha ocurrido algún error ";
            i = 0;
        }
        finally
        {
            con.Close();
        }
        return resp;
    }

}