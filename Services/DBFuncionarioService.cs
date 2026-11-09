using Microsoft.Data.SqlClient;
using SIGEPROS.Models;
using SIGEPROS.Static;
using System.Data;

namespace SIGEPROS.Services
{
    public class DBFuncionarioService
    {
        private static IConfiguration? _configuracion;
        private readonly DBService _dbservice;
        SqlConnection? _connection = null;
        SqlCommand? _command = null;
        SqlCommand? _commandIdentity = null;

        public DBFuncionarioService(DBService dbservice, IConfiguration configuracion)
        {
            _dbservice = dbservice;
            _configuracion = configuracion;
        }
        public FuncionarioModel GetUsuario(string Rut, string programa)
        {

            FuncionarioModel usuario = new FuncionarioModel();
            try
            {
                using (_connection = new SqlConnection(_dbservice.GetConnectionString()))
                {
                    _command = _connection.CreateCommand();
                    _command.CommandType = CommandType.StoredProcedure;
                    _command.CommandText = "proc_Funcionarios_Obtener";
                    _command.Parameters.AddWithValue("@rut", Rut);

                    _connection.Open();
                    SqlDataReader dr = _command.ExecuteReader();

                    if (dr.Read())
                    {
                        usuario = new FuncionarioModel();
                        usuario = _set(dr);
                    }
                    dr.Close();
                    _connection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return usuario;
        }
        private FuncionarioModel _set(SqlDataReader rd)
        {
            return new FuncionarioModel
            {
/*
                Rut       = SqlDataReaderExtensions.GetStringSafe(rd, "Rut"),
                Estado    = (bool)SqlDataReaderExtensions.GetBoolSafe(rd, "Estado"),
                Nombre    = SqlDataReaderExtensions.GetStringSafe(rd, "Nombre"),
                Apellido  = SqlDataReaderExtensions.GetStringSafe(rd, "Apellido"),
                Rol       = SqlDataReaderExtensions.GetStringSafe(rd, "Rol"),
                Organismo = SqlDataReaderExtensions.GetStringSafe(rd, "Organismo")*/
            };
        }
    }
}
