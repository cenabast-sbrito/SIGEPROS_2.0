using System.Data;
using Microsoft.Extensions.Configuration;

namespace SIGEPROS.Services
{
    public class DBService
    {
        private readonly IDbConnection _dbConnection;
        private static IConfiguration? _configuracion;

        public DBService(IConfiguration configuracion, IDbConnection dbConnection)
        {
            _configuracion = configuracion;
            _dbConnection = dbConnection;
        }
        public string GetConnectionString()
        {
            return _configuracion.GetConnectionString("dbAppSIGEPROS");
        }
        public string GetConnectionString(string db)
        {
            return (db == "NUEVOMODELO") ? _configuracion.GetConnectionString("dbNuevoModelo") : _configuracion.GetConnectionString("dbAppSIGEPROS");
        }
    }
}
