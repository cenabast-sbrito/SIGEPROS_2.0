using System.Data;
using Microsoft.Data.SqlClient;
using SIGEPROS.Models;

namespace SIGEPROS.Services
{
    public class DBAppService
    {
        private readonly DBService _dbservice;
        private static IConfiguration? _configuracion;
        private static IConfiguration? _config;

        SqlConnection? _connection = null;
        SqlCommand? _command = null;
        SqlCommand? _commandIdentity = null;

        public DBAppService(DBService dbservice, IConfiguration configuracion)
        {
            _dbservice = dbservice;
            _configuracion = configuracion;
        }



    }
}
