using Aspose.Pdf.Facades;
using SIGEPROS;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace SIGEPROS.Models
{
    public class Archivos
    {
        public string Url { get; set; }
        public string UrlDA { get; set; }
        public List<Archivo> Nombres { get; set; }
    }
    public class Archivo
    {
        public string Nombre { get; set; }
        //public List<Archivo> GetArchivos(string rut, string doc)
        //{

        //    try
        //    {
        //        string url = ConfigurationManager.AppSettings["urlCargaSapPro"].ToString();
        //        var urlBaseCedibles = ConfigurationManager.AppSettings["urlBaseCedibles"].ToString();
        //        string linkCedible = urlBaseCedibles + int.Parse(rut) + "/procesados/" + doc + ".pdf";
        //        string folder = urlBaseCedibles + int.Parse(rut) + "/procesados/";
        //        folder = url + int.Parse(rut) + "/procesados/";

        //        Archivo archivo = new Archivo();
        //        List<Archivo> archivos = new List<Archivo>();

        //        string file = doc + ".pdf";
        //        linkCedible = folder + file;

        //        var archivosLista = Directory.GetFiles(folder, file)
        //            .Select(Path.GetFileName)
        //            .OrderBy(f => f)
        //            .Select(f => new Archivo { Nombre = f })
        //            .ToList();

        //        var modelo = new Archivos
        //        {
        //            Url = folder,         
        //            Nombres = archivosLista
        //        };

        //        return modelo.Nombres;

        //    }
        //    catch (Exception)
        //    {
        //        return null;
        //    }
        //}
    }
}