using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Web; 

namespace SIGEPROS.Models
{
    public class PlanillaZDIS022
    {
        public string Documento_de_venta { get; set; }
        public string Pedido_de_Compra { get; set; }
        public string Grupo_de_articulos { get; set; }
        public string Canal { get; set; }
        public string Código_Material { get; set; }
        public string Fecha_de_entrega_al { get; set; }
        public string Fecha_entrega_al_OL { get; set; }
        public string Cantidad_documento_d { get; set; }
        public string Cantidad_informada { get; set; }
        public string Lote { get; set; }
        public string Fecha_entrega_al_ol_ { get; set; }
        public string OS { get; set; }
        public string NFactura { get; set; }
        public string Guía_de_despacho { get; set; }
        public string Observaciones { get; set; }
        public string Suspensión_de_entreg { get; set; }
        public string Autoriza_suspensión { get; set; }
        public string Fecha_de_carga_de_in { get; set; }
        public string Salida_mercancía { get; set; }
        public string Documento_de_entrega { get; set; }
        public string Status_carga_cedible { get; set; }
        public string Fecha_Sal_mercancía { get; set; }
        public string Fact_comisiones_pro { get; set; }
        public string Fecha_fact_comision { get; set; }
        public string Fecha_recepción_conf { get; set; }
        public string Fecha_entrega_cedibl { get; set; }
        public string Centro { get; set; }
        public string Valor_neto { get; set; }
        public string Bultos { get; set; }
        public string Volumen { get; set; }
        public string Peso { get; set; }
        public string Fecha_vencimiento { get; set; }
        public string GTIN { get; set; }
        public string Agrupador { get; set; }
        public string Motivo_Rechazo { get; set; }
        public string OC { get; set; }
        public string DocVenta { get; set; }
        public string camposInsert = "  [Documento de venta]       " +
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
                               " ,[DocVenta]			     ";
        public string campos = "  [Documento de venta]    Documento_de_venta   " +
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
                       " ,[DocVenta]			    DocVenta             ";

        public PlanillaZDIS022 _setRow(IXLRangeRow row)
        {

            PlanillaZDIS022 fila = new PlanillaZDIS022();
            fila.Documento_de_venta = row.Cell(1).Value.ToString();
            fila.Pedido_de_Compra = row.Cell(2).Value.ToString();
            fila.Grupo_de_articulos = row.Cell(3).Value.ToString();
            fila.Canal = row.Cell(4).Value.ToString();
            fila.Código_Material = row.Cell(5).Value.ToString();
            fila.Fecha_de_entrega_al = row.Cell(6).Value.ToString();
            fila.Fecha_entrega_al_OL = row.Cell(7).Value.ToString();
            fila.Cantidad_documento_d = row.Cell(8).Value.ToString();
            fila.Cantidad_informada = row.Cell(9).Value.ToString();
            fila.Lote = row.Cell(10).Value.ToString();

            fila.NFactura = row.Cell(13).Value.ToString();
            fila.Guía_de_despacho = row.Cell(14).Value.ToString();

            if ((fila.NFactura != "0") && (fila.Guía_de_despacho != "0"))
                fila.Fecha_entrega_al_ol_ = row.Cell(11).Value.ToString();
            else fila.Fecha_entrega_al_ol_ = "";

            fila.OS = row.Cell(12).Value.ToString();

            fila.Observaciones = row.Cell(15).Value.ToString();
            fila.Suspensión_de_entreg = row.Cell(16).Value.ToString();
            fila.Autoriza_suspensión = row.Cell(17).Value.ToString();
            fila.Fecha_de_carga_de_in = row.Cell(18).Value.ToString();
            fila.Salida_mercancía = row.Cell(19).Value.ToString();
            fila.Documento_de_entrega = row.Cell(20).Value.ToString();
            fila.Status_carga_cedible = row.Cell(21).Value.ToString();
            fila.Fecha_Sal_mercancía = row.Cell(22).Value.ToString();
            fila.Fact_comisiones_pro = row.Cell(23).Value.ToString();
            fila.Fecha_fact_comision = row.Cell(24).Value.ToString();
            fila.Fecha_recepción_conf = row.Cell(25).Value.ToString();
            fila.Fecha_entrega_cedibl = row.Cell(26).Value.ToString();
            fila.Centro = row.Cell(27).Value.ToString();
            fila.Valor_neto = row.Cell(28).Value.ToString();
            fila.Bultos = row.Cell(29).Value.ToString();
            fila.Volumen = row.Cell(30).Value.ToString();
            fila.Peso = row.Cell(31).Value.ToString();
            fila.Fecha_vencimiento = row.Cell(32).Value.ToString();
            fila.GTIN = row.Cell(33).Value.ToString();
            fila.Agrupador = row.Cell(34).Value.ToString();
            fila.Motivo_Rechazo = row.Cell(35).Value.ToString();
            fila.OC = row.Cell(36).Value.ToString();
            fila.DocVenta = row.Cell(37).Value.ToString();
            return fila;
        }
        public PlanillaZDIS022 _getRow(IXLRangeRow row)
        {

            PlanillaZDIS022 fila = new PlanillaZDIS022();
            fila.Documento_de_venta = row.Cell(1).Value.ToString();
            fila.Pedido_de_Compra = row.Cell(2).Value.ToString();
            fila.Grupo_de_articulos = row.Cell(3).Value.ToString();
            fila.Canal = row.Cell(4).Value.ToString();
            fila.Código_Material = row.Cell(5).Value.ToString();
            fila.Fecha_de_entrega_al = row.Cell(6).Value.ToString();
            fila.Fecha_entrega_al_OL = row.Cell(7).Value.ToString();
            fila.Cantidad_documento_d = row.Cell(8).Value.ToString();
            fila.Cantidad_informada = row.Cell(9).Value.ToString();
            fila.Lote = row.Cell(10).Value.ToString();
            fila.Fecha_entrega_al_ol_ = row.Cell(11).Value.ToString();
            fila.OS = row.Cell(12).Value.ToString();
            fila.NFactura = row.Cell(13).Value.ToString();
            fila.Guía_de_despacho = row.Cell(14).Value.ToString();
            fila.Observaciones = row.Cell(15).Value.ToString();
            fila.Suspensión_de_entreg = row.Cell(16).Value.ToString();
            fila.Autoriza_suspensión = row.Cell(17).Value.ToString();
            fila.Fecha_de_carga_de_in = row.Cell(18).Value.ToString();
            fila.Salida_mercancía = row.Cell(19).Value.ToString();
            fila.Documento_de_entrega = row.Cell(20).Value.ToString();
            fila.Status_carga_cedible = row.Cell(21).Value.ToString();
            fila.Fecha_Sal_mercancía = row.Cell(22).Value.ToString();
            fila.Fact_comisiones_pro = row.Cell(23).Value.ToString();
            fila.Fecha_fact_comision = row.Cell(24).Value.ToString();
            fila.Fecha_recepción_conf = row.Cell(25).Value.ToString();
            fila.Fecha_entrega_cedibl = row.Cell(26).Value.ToString();
            fila.Centro = row.Cell(27).Value.ToString();
            fila.Valor_neto = row.Cell(28).Value.ToString();
            fila.Bultos = row.Cell(29).Value.ToString();
            fila.Volumen = row.Cell(30).Value.ToString();
            fila.Peso = row.Cell(31).Value.ToString();
            fila.Fecha_vencimiento = row.Cell(32).Value.ToString();
            fila.GTIN = row.Cell(33).Value.ToString();
            fila.Agrupador = row.Cell(34).Value.ToString();
            fila.Motivo_Rechazo = row.Cell(35).Value.ToString();
            fila.OC = row.Cell(36).Value.ToString();
            fila.DocVenta = row.Cell(37).Value.ToString();
            return fila;
        }

        //private PlanillaZDIS022 _get(PlanillaZDIS022 despacho, SqlDataReader reader)
        //{
        //    despacho.Documento_de_venta = reader["Documento_de_venta"].ToString();
        //    despacho.Pedido_de_Compra = reader["Pedido_de_Compra"].ToString();
        //    despacho.Grupo_de_articulos = reader["Grupo_de_articulos"].ToString();
        //    despacho.Canal = reader["Canal"].ToString();
        //    despacho.Código_Material = reader["Código_Material"].ToString();
        //    despacho.Fecha_de_entrega_al = reader["Fecha_de_entrega_al"].ToString();
        //    despacho.Fecha_entrega_al_OL = reader["Fecha_entrega_al_OL"].ToString();
        //    despacho.Cantidad_documento_d = reader["Cantidad_documento_d"].ToString();
        //    despacho.Cantidad_informada = reader["Cantidad_informada"].ToString();
        //    despacho.Lote = reader["Lote"].ToString();
        //    despacho.Fecha_entrega_al_ol_ = reader["Fecha_entrega_al_ol_"].ToString();
        //    despacho.OS = reader["OS"].ToString();
        //    despacho.NFactura = reader["NFactura"].ToString();
        //    despacho.Guía_de_despacho = reader["Guía_de_despacho"].ToString();
        //    despacho.Observaciones = reader["Observaciones"].ToString();
        //    despacho.Suspensión_de_entreg = reader["Suspensión_de_entreg"].ToString();
        //    despacho.Autoriza_suspensión = reader["Autoriza_suspensión"].ToString();
        //    despacho.Fecha_de_carga_de_in = reader["Fecha_de_carga_de_in"].ToString();
        //    despacho.Salida_mercancía = reader["Salida_mercancía"].ToString();
        //    despacho.Documento_de_entrega = reader["Documento_de_entrega"].ToString();
        //    despacho.Status_carga_cedible = reader["Status_carga_cedible"].ToString();
        //    despacho.Fecha_Sal_mercancía = reader["Fecha_Sal_mercancía"].ToString();
        //    despacho.Fact_comisiones_pro = reader["Fact_comisiones_pro"].ToString();
        //    despacho.Fecha_fact_comision = reader["Fecha_fact_comision"].ToString();
        //    despacho.Fecha_recepción_conf = reader["Fecha_recepción_conf"].ToString();
        //    despacho.Fecha_entrega_cedibl = reader["Fecha_entrega_cedibl"].ToString();
        //    despacho.Centro = reader["Centro"].ToString();
        //    despacho.Valor_neto = reader["Valor_neto"].ToString();
        //    despacho.Bultos = reader["Bultos"].ToString();
        //    despacho.Volumen = reader["Volumen"].ToString();
        //    despacho.Peso = reader["Peso"].ToString();
        //    despacho.Fecha_vencimiento = reader["Fecha_vencimiento"].ToString();
        //    despacho.GTIN = reader["GTIN"].ToString();
        //    despacho.Agrupador = reader["Agrupador"].ToString();
        //    despacho.Motivo_Rechazo = reader["Motivo_Rechazo"].ToString();
        //    despacho.OC = reader["OC"].ToString();
        //    despacho.DocVenta = reader["DocVenta"].ToString();
        //    return despacho;
        //}



    }
}