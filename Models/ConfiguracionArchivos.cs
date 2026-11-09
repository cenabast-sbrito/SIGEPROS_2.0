using SIGEPROS.Static;

namespace SIGEPROS.Models
{
    public class ConfiguracionArchivos
    {
        public string Ambiente { get; set; }
        public string UrlBaseDestino { get; set; }
        public string RutaBaseFisicaExpedientes { get; set; }
        public string RutaBaseFisicaComision { get; set; }
        public string RutaBaseFisicaDocumentoAdjuntoBase { get; set; }
        public string UrlBaseDestinoDocumentoAdjuntoBase { get; set; }
        public string UrlBaseDestinoComision { get; set; }
        public string UrlCargaSapPro { get; set; }
        public string UrlBaseCedibles {  get; set; }
        public string Prefijo { get; set; }
        public string UrlBaseIODocumentos { get; set; }
        public string UrlBaseLinkExpedientes { get; set; }
        public string UrlBaseLinkDocumentos { get;set; }
        public string UrlBase { get; set; }
        public string UrlBaseLink { get; set; }
        public static ConfiguracionArchivos Crear(IConfiguration configuration)
        {
            return new ConfiguracionArchivos
            {
                Ambiente = Configuracion.GetAmbiente(configuration),
                UrlBaseDestino = Configuracion.GetValorPorAmbiente(configuration, "urlBaseLinkExpedientes"),
                RutaBaseFisicaExpedientes = Configuracion.GetValorPorAmbiente(configuration, "urlBaseIOExpedientes"),
                RutaBaseFisicaComision = Configuracion.GetValorPorAmbiente(configuration, "urlBaseIOFacturaComision"),
                RutaBaseFisicaDocumentoAdjuntoBase = Configuracion.GetValorPorAmbiente(configuration, "urlBaseIODocumentos"),
                UrlBaseDestinoDocumentoAdjuntoBase = Configuracion.GetValorPorAmbiente(configuration, "urlBaseLinkDocumentos"),
                UrlBaseDestinoComision = Configuracion.GetValorPorAmbiente(configuration, "urlBaseLinkComision"),
                UrlCargaSapPro = Configuracion.GetValorPorAmbiente(configuration,"urlCargaSapPro"),
                UrlBaseCedibles = Configuracion.GetValorPorAmbiente(configuration, "urlBaseCedibles"),
                UrlBaseLinkExpedientes = Configuracion.GetValorPorAmbiente(configuration, "urlBaseLinkExpedientes"),
                UrlBaseIODocumentos = Configuracion.GetValorPorAmbiente(configuration, "urlBaseIODocumentos"),
                UrlBaseLinkDocumentos = Configuracion.GetValorPorAmbiente(configuration, "urlBaseLinkDocumentos"),
                UrlBase = Configuracion.GetValorPorAmbiente(configuration, "urlBase"),
                UrlBaseLink = Configuracion.GetValorPorAmbiente(configuration, "urlBaseLink"),
                Prefijo = Configuracion.GetValorPorAmbiente(configuration, "prefijoNombreComision")
            };
        }

    }
}
