using Microsoft.Data.SqlClient;
using SIGEPROS.Models;
using SIGEPROS.Static;
using System.Data;

namespace SIGEPROS.Services
{
    public class DBUploadService
    {
        private static IConfiguration? _configuracion;
        private readonly DBService _dbservice;
        SqlConnection? _connection = null;
        SqlCommand? _command = null;
        SqlCommand? _commandIdentity = null;

        public DBUploadService(DBService dbservice, IConfiguration configuracion)
        {
            _dbservice = dbservice;
            _configuracion = configuracion;
        }
        public List<RolModel> GetRoles(string Rut)
        {

            List<RolModel> roles = new List<RolModel>();
            RolModel rol = null;
            try
            {
                using (_connection = new SqlConnection(_dbservice.GetConnectionString()))
                {
                    _command = _connection.CreateCommand();
                    _command.CommandType = CommandType.StoredProcedure;
                    _command.CommandText = "proc_Roles_Obtener";
                    _command.Parameters.AddWithValue("@rut", Rut);

                    _connection.Open();
                    SqlDataReader dr = _command.ExecuteReader();

                    while (dr.Read())
                    {
                        rol = new RolModel();

                        rol = new RolModel();
                        rol = _set(dr);
                        roles.Add(rol);
                    }
                    dr.Close();
                    _connection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return roles;
        }
        public int PostRol( string rut, string rol )
        {
            using (SqlConnection connection = new SqlConnection(_dbservice.GetConnectionString()))
            using (SqlCommand _command = connection.CreateCommand())
            {
                _command.CommandType = CommandType.StoredProcedure;
                _command.CommandText = "proc_Rol_Asignar";
                _command.Parameters.AddWithValue("@Rol", rol);
                _command.Parameters.AddWithValue("@Rut", rut);


                connection.Open();

                return _command.ExecuteNonQuery();
            }
        }
        public int DelRol(string rut, string rol)
        {
            using (SqlConnection connection = new SqlConnection(_dbservice.GetConnectionString()))
            using (SqlCommand _command = connection.CreateCommand())
            {
                _command.CommandType = CommandType.StoredProcedure;
                _command.CommandText = "proc_Rol_Quitar";
                _command.Parameters.AddWithValue("@Rol", rol);
                _command.Parameters.AddWithValue("@Rut", rut);


                connection.Open();

                return _command.ExecuteNonQuery();
            }
        }
        private RolModel _set(SqlDataReader rd)
        {
            return new RolModel
            {
                 Rut            = rd.GetStringSafe("Rut"),
                 CRol           = rd.GetStringSafe("CRol"),
                 TRol           = rd.GetStringSafe("TRol"),
                 Organismo      = rd.GetStringSafe("Organismo"),
                 Programa       = rd.GetStringSafe("TPrograma_breve")
            };
        }

    }
}
