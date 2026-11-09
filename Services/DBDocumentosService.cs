using Microsoft.Data.SqlClient;
using SIGEPROS.Models;
using System.Data;

namespace SIGEPROS.Services
{
    public class DBDocumentosService
    {
        private readonly DBService _dbservice;
        private readonly ConfiguracionArchivos _cfg;
        public DBDocumentosService(DBService dbservice, IConfiguration configuracion)
        {
            _dbservice = dbservice;
            _cfg = ConfiguracionArchivos.Crear(configuracion);
        }

        public List<TipoDocumentoModel> GetTiposDocumento()
        {
            var listado = new List<TipoDocumentoModel>();

            try
            {
                using (var connection = new SqlConnection(_dbservice.GetConnectionString()))
                using (var command = connection.CreateCommand())
                {
                    command.CommandType = CommandType.Text;
                    command.CommandText = "SELECT Codigo, Nombre, Activo FROM TipoDocumento WHERE Activo = 1 ORDER BY Nombre ASC";

                    connection.Open();
                    using (var dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            listado.Add(new TipoDocumentoModel
                            {
                                Codigo = dr["Codigo"]?.ToString()?.Trim() ?? string.Empty,
                                Nombre = dr["Nombre"]?.ToString()?.Trim() ?? string.Empty,
                                Activo = dr["Activo"] != DBNull.Value && Convert.ToBoolean(dr["Activo"])
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al consultar TipoDocumento: {ex.Message}");
            }

            return listado;
        }

        public int ExisteDocumentoAdjunto(string rut, string factura)
        {

            string ambiente = _cfg.Ambiente;
            string urlBaseDestino = _cfg.UrlBaseDestino;
            string rutaBaseFisicaExpedientes = _cfg.RutaBaseFisicaExpedientes;
            string rutaBaseFisicaComision = _cfg.RutaBaseFisicaComision;
            string rutaBaseFisicaDocumentoAdjuntoBase = _cfg.RutaBaseFisicaDocumentoAdjuntoBase;
            string urlBaseDestinoDocumentoAdjuntoBase = _cfg.RutaBaseFisicaDocumentoAdjuntoBase;
            string urlBaseDestinoComision = _cfg.RutaBaseFisicaComision;

            // Retorna 1 si existe al menos un PDF que matchee con la factura; 0 en caso contrario.
            // Si hay un error controlado (p.ej. carpeta no existe) retorna 0.

            // Sanitiza RUT: deja solo números (si necesitas conservar DV, ajusta aquí).
            string rutSanitizado = new string((rut ?? string.Empty).Where(char.IsDigit).ToArray());
            if (string.IsNullOrWhiteSpace(rutSanitizado) || string.IsNullOrWhiteSpace(factura))
                return 0;

            // Lee ambiente y paths
            string basePath = rutaBaseFisicaDocumentoAdjuntoBase;

            if (string.IsNullOrWhiteSpace(basePath))
                return 0;

            try
            {
                // Normaliza la carpeta del proveedor: <base>/<rutSanitizado>/
                string proveedorFolder = Path.Combine(basePath, rutSanitizado);

                // Si la carpeta no existe, no hay archivos
                if (!Directory.Exists(proveedorFolder))
                    return 0;

                // Patrón de búsqueda: PDFs que contengan el número de factura
                // Si tu convención es distinta (ej. "NC_{factura}_*.pdf" o "factura_comision_{rut}_{factura}.pdf"),
                // ajusta el patrón en consecuencia:
                string searchPattern = $"*{factura}*.pdf";

                // Enumeración eficiente (no carga todo a memoria)
                var existe = Directory.EnumerateFiles(proveedorFolder, searchPattern, SearchOption.TopDirectoryOnly)
                                      .Any();

                return existe ? 1 : 0;
            }
            catch (UnauthorizedAccessException)
            {
                // Sin permisos para leer la carpeta → lo tratamos como "no existe" (o loguea si quieres)
                return 0;
            }
        }
    }
}
