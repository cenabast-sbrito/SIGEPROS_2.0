using Aspose.Pdf.Operators;
using DocumentFormat.OpenXml.Office.Word;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Data.SqlClient;
using SIGEPROS.Models;
using System.Data;

namespace SIGEPROS.Services
{
    public class DBSeguridadService
    {
        private readonly DBService _dbservice;


        SqlConnection? _connection = null;
        SqlCommand? _command = null;
        SqlCommand? _commandIdentity = null;

        public DBSeguridadService(DBService dbservice, IConfiguration configuracion)
        {
            _dbservice = dbservice;
        }
        public bool AuthorizationLDAP(string usuario, string programa)
        {

            bool autorizado;

            using (SqlConnection cn = new SqlConnection(_dbservice.GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand("proc_Usuario_AuthorizationLDAP", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Parámetros de entrada
                    cmd.Parameters.AddWithValue("@usuario", usuario);
                    cmd.Parameters.AddWithValue("@Programa", programa);

                    // Parámetro OUTPUT
                    SqlParameter pAutorizado = new SqlParameter("@Autorizado", SqlDbType.Bit);
                    pAutorizado.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(pAutorizado);

                    cn.Open();
                    cmd.ExecuteNonQuery();

                    autorizado = Convert.ToBoolean(pAutorizado.Value);
                }
            }
            return autorizado;

        }
        public bool AuthorizationRUT(string rut, string programa)
        {

            bool autorizado;

            using (SqlConnection cn = new SqlConnection(_dbservice.GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand("proc_Usuario_Authorization", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Parámetros de entrada
                    cmd.Parameters.AddWithValue("@Rut", rut);
                    cmd.Parameters.AddWithValue("@Programa", programa);

                    // Parámetro OUTPUT
                    SqlParameter pAutorizado = new SqlParameter("@Autorizado", SqlDbType.Bit);
                    pAutorizado.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(pAutorizado);

                    cn.Open();
                    cmd.ExecuteNonQuery();

                    autorizado = Convert.ToBoolean(pAutorizado.Value);
                }
            }
            return autorizado;

        }
        public bool AuthenticateRUT(string Rut, string Pass)
        {
            bool autenticado;

            using (SqlConnection cn = new SqlConnection(_dbservice.GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand("proc_Usuario_Authenticate", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Parámetros de entrada
                    cmd.Parameters.AddWithValue("@Rut", Rut);
                    var encryptedBase64 = CryptoHelper.EncryptToBase64(Pass);
                    cmd.Parameters.AddWithValue("@Clave", encryptedBase64);

                    // Parámetro OUTPUT
                    SqlParameter pAutenticado = new SqlParameter("@Autenticado", SqlDbType.Bit);
                    pAutenticado.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(pAutenticado);

                    cn.Open();
                    cmd.ExecuteNonQuery();

                    autenticado = Convert.ToBoolean(pAutenticado.Value);
                }
            }
            return autenticado;

        }
        public bool AuthenticateLDAP(string Usuario, string Pass)
        {
            bool autenticado;

            using (SqlConnection cn = new SqlConnection(_dbservice.GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand("proc_Usuario_AuthenticateLDAP", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Parámetros de entrada
                    cmd.Parameters.AddWithValue("@Usuario", Usuario);
                    var encryptedBase64 = CryptoHelper.EncryptToBase64(Pass);
                    cmd.Parameters.AddWithValue("@Clave", encryptedBase64);

                    // Parámetro OUTPUT
                    SqlParameter pAutenticado = new SqlParameter("@Autenticado", SqlDbType.Bit);
                    pAutenticado.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(pAutenticado);

                    cn.Open();
                    cmd.ExecuteNonQuery();

                    autenticado = Convert.ToBoolean(pAutenticado.Value);
                }
            }
            return autenticado;

        }
        //public UsuarioModel GetUsuario(string Rut, string programa)
        //{
           
        //    UsuarioModel usuario = null;
        //    try
        //    {
        //        using (_connection = new SqlConnection(_dbservice.GetConnectionString()))
        //        {
        //            _command = _connection.CreateCommand();
        //            _command.CommandType = CommandType.StoredProcedure;
        //            _command.CommandText = "proc_Usuarios_Obtener_new";
        //            _command.Parameters.AddWithValue("@rut", Rut);
        //            _command.Parameters.AddWithValue("@programa", programa);

        //            _connection.Open();
        //            SqlDataReader dr = _command.ExecuteReader();

        //            if (dr.Read())
        //            {

        //                usuario = new UsuarioModel();
        //                usuario = _set(dr);
        //            }
        //            dr.Close();
        //            _connection.Close();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.Message);
        //    }
        //    return usuario;
        //}

        /* valido */
        public UsuarioModel GetUsuarioLDAP(string Usuario, string programa)
        {

            UsuarioModel usuario = null;
            try
            {
                using (_connection = new SqlConnection(_dbservice.GetConnectionString()))
                {
                    _command = _connection.CreateCommand();
                    _command.CommandType = CommandType.StoredProcedure;
                    _command.CommandText = "proc_Usuarios_Obtener_ldap";
                    _command.Parameters.AddWithValue("@Usuario", Usuario);
                    _command.Parameters.AddWithValue("@Programa", programa);

                    _connection.Open();
                    SqlDataReader dr = _command.ExecuteReader();

                    if (dr.Read())
                    {

                        usuario = new UsuarioModel();
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
        /* valido */
        public UsuarioModel GetUsuarioByToken(string token)
        {
            UsuarioModel usuario = null;

            try
            {
                using (_connection = new SqlConnection(_dbservice.GetConnectionString()))
                {
                    _command = _connection.CreateCommand();
                    _command.CommandType = CommandType.Text;
            //        _command.CommandText = @"
            //    UPDATE [PZ_SISTEMA_APP].[dbo].[AMBIENTECLIENTES_ACCESO_SISTEMASARQUITECTURA]
            //       SET Activo = 0
            //    OUTPUT inserted.RutUsuario, inserted.idSistema
            //     WHERE IdIngreso = @Id AND Activo in (0,1);
            //";
                    _command.CommandText = @"

                            SELECT RutUsuario, idSistema
                              FROM [PZ_SISTEMA_APP].[dbo].[AMBIENTECLIENTES_ACCESO_SISTEMASARQUITECTURA]
                             WHERE IdIngreso = @Id AND Activo = 1
							 
                        ";

                    _command.Parameters.AddWithValue("@Id", token);

                    _connection.Open();

                    using (SqlDataReader dr = _command.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            usuario = new UsuarioModel
                            {
                                Rut = dr["RutUsuario"].ToString(),
                                programa = Convert.ToInt32(dr["idSistema"])
                            };
                        }
                    }

                    _connection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return usuario;
        }
        public UsuarioModel GetUsuarioByTokenOLD(string token)
        {
            UsuarioModel usuario = null;
            try
            {
                using (_connection = new SqlConnection(_dbservice.GetConnectionString()))
                {
                    _command = _connection.CreateCommand();
                    _command.CommandType = CommandType.Text;
                    _command.CommandText = @"

                            SELECT RutUsuario, idSistema
                              FROM [PZ_SISTEMA_APP].[dbo].[AMBIENTECLIENTES_ACCESO_SISTEMASARQUITECTURA]
                             WHERE IdIngreso = @Id AND Activo in (0,1)
                        ";
                    _command.Parameters.AddWithValue("@Id", token);
                    _connection.Open();
                    SqlDataReader dr = _command.ExecuteReader();
                    if (dr.Read())
                    {
                        usuario = new UsuarioModel();
                        usuario.Rut = dr["RutUsuario"].ToString();
                        usuario.programa = int.Parse( dr["idSistema"].ToString());
                        ////                        usuario = _set(dr);
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
        private UsuarioModel _set(SqlDataReader rd)
        {
            UsuarioModel user = new UsuarioModel();
            user.Rut = rd["Rut"].ToString();
            user.Estado = bool.Parse(rd["Estado"].ToString());
            user.Nombre = rd["Nombre"].ToString();
            user.Apellido = rd["Apellido"].ToString();
            user.Rol = rd["Rol"].ToString();
            user.Organismo = rd["Organismo"].ToString();
            return user;
        }
    }
}
