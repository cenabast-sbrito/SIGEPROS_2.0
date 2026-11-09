using DocumentFormat.OpenXml.Office.Word;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Data.SqlClient;
using SIGEPROS.Models;
using SIGEPROS.Static;
using System.Data;

namespace SIGEPROS.Services
{
    public class DBUsuarioService
    {
        private static IConfiguration? _configuracion;
        private readonly DBService _dbservice;
        SqlConnection? _connection = null;
        SqlCommand? _command = null;
        SqlCommand? _commandIdentity = null;

        public DBUsuarioService(DBService dbservice, IConfiguration configuracion)
        {
            _dbservice = dbservice;
            _configuracion = configuracion;
        }
        /* valido */
        public UsuarioModel GetUsuario(string Rut)
        {
            // Sanitizar por si viene con formato antes de consultar
            ///string rutLimpio = LimpiarRut(Rut);
            ///
            string rutLimpio = (Rut); 
            UsuarioModel usuario = new UsuarioModel();
            try
            {
                using (_connection = new SqlConnection(_dbservice.GetConnectionString()))
                {
                    _command = _connection.CreateCommand();
                    _command.CommandType = CommandType.StoredProcedure;
                    _command.CommandText = "proc_Usuarios_Obtener";
                    _command.Parameters.AddWithValue("@rut", rutLimpio);

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
        /* valido para mantenedor */
        public List<UsuarioModel> GetUsuarios()
        {

            List<UsuarioModel> usuarios = new List<UsuarioModel>();
            try
            {
                using (_connection = new SqlConnection(_dbservice.GetConnectionString()))
                {
                    _command = _connection.CreateCommand();
                    _command.CommandType = CommandType.StoredProcedure;
                    _command.CommandText = "proc_Usuarios_Obtener";

                    _connection.Open();
                    SqlDataReader dr = _command.ExecuteReader();
                    UsuarioModel usuario = new UsuarioModel();

                    while (dr.Read())
                    {
                        usuario = new UsuarioModel();
                        usuario = _set(dr);
                        usuarios.Add(usuario);
                    }
                    dr.Close();
                    _connection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return usuarios;
        }
        private UsuarioModel _set(SqlDataReader rd)
        {
            UsuarioModel user = new UsuarioModel();
            try
            {
                user.Rut = rd["Rut"].ToString();
                user.Estado = bool.Parse(rd["Estado"].ToString());
                user.Nombre = rd["Nombre"].ToString();
                user.Apellido = rd["Apellido"].ToString();
                user.Rol = rd["Rol"] == DBNull.Value ? "" : rd["Rol"].ToString().Trim().ToUpperInvariant();
                user.Organismo = rd["Organismo"].ToString();
                user.email = rd["Email"] == DBNull.Value ? "" : rd["Email"].ToString().Trim();
                user.usuarioLDAP = rd["usuarioLDAP"] == DBNull.Value ? "" : rd["UsuarioLDAP"].ToString().Trim();

                if (rd.HasColumn("Clave") && rd["Clave"] != DBNull.Value)
                {
                    var encryptedBase64 = rd["Clave"]?.ToString();
                    if (!string.IsNullOrWhiteSpace(encryptedBase64))
                    {
                        try
                        {
                            user.Clave = CryptoHelper.DecryptFromBase64(encryptedBase64);
                        }
                        catch
                        {
                            user.Clave = encryptedBase64; // fallback si estaba en texto plano
                        }
                    }
                }

                user.acceso_PNACPACAM = rd.HasColumn("Acceso_PNACPACAM") && rd["Acceso_PNACPACAM"] != DBNull.Value ? int.Parse(rd["Acceso_PNACPACAM"].ToString()) : 0;
                user.acceso_PNI = rd.HasColumn("Acceso_PNI") && rd["Acceso_PNI"] != DBNull.Value ? int.Parse(rd["Acceso_PNI"].ToString()) : 0;
                user.esGestor = rd.HasColumn("esGestor") && rd["esGestor"] != DBNull.Value ? int.Parse(rd["esGestor"].ToString()) : 0;

                return user;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return user;
        }

        public int PostUsuario(
            string Rut,
            string Nombre,
            string Apellido,
            string Organismo,
            string Estado, 
            string UsuarioLDAP,
            string? Clave = null
            )
        {
            ////string rutLimpio = LimpiarRut(Rut);
            string rutLimpio = Rut;

            using (SqlConnection connection = new SqlConnection(_dbservice.GetConnectionString()))
            using (SqlCommand command = connection.CreateCommand())
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "proc_Usuarios_Agregar";
                command.Parameters.AddWithValue("@Rut", rutLimpio);
                command.Parameters.AddWithValue("@Estado", Estado);
                command.Parameters.AddWithValue("@Nombre", Nombre);
                command.Parameters.AddWithValue("@Apellido", Apellido);
                command.Parameters.AddWithValue("@Organismo", Organismo);
                command.Parameters.AddWithValue("@UsuarioLDAP", UsuarioLDAP ?? "");

                if (!string.IsNullOrWhiteSpace(Clave))
                {
                    command.Parameters.AddWithValue("@Clave", CryptoHelper.EncryptToBase64(Clave));
                }
                else
                {
                    command.Parameters.AddWithValue("@Clave", DBNull.Value);
                }

                connection.Open();
                return command.ExecuteNonQuery();
            }
        }

        public int PutUsuario(
            string Rut,
            string Nombre,
            string Apellido,
            string Organismo,
            string Estado, 
            string usuarioLDAP,
            string? Clave = null
            )
        {
            ///string rutLimpio = LimpiarRut(Rut);

            string rutLimpio = Rut;
            using (SqlConnection connection = new SqlConnection(_dbservice.GetConnectionString()))
            using (SqlCommand command = connection.CreateCommand())
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "proc_Usuarios_Actualizar";
                command.Parameters.AddWithValue("@Rut", rutLimpio);
                command.Parameters.AddWithValue("@Estado", Estado);
                command.Parameters.AddWithValue("@Nombre", Nombre);
                command.Parameters.AddWithValue("@Apellido", Apellido);
                command.Parameters.AddWithValue("@Organismo", Organismo);
                command.Parameters.AddWithValue("@UsuarioLDAP", usuarioLDAP ?? "");

                if (!string.IsNullOrWhiteSpace(Clave))
                {
                    command.Parameters.AddWithValue("@Clave", CryptoHelper.EncryptToBase64(Clave));
                }
                else
                {
                    command.Parameters.AddWithValue("@Clave", DBNull.Value);
                }

                connection.Open();

                return command.ExecuteNonQuery();
            }
        }

        public int toggleUsuario(UsuarioModel usuario)
        {
            using (var con = new SqlConnection(_dbservice.GetConnectionString()))
            using (var cmd = new SqlCommand("proc_Usuarios_ActivarDesactivar", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                ////var rut = LimpiarRut(usuario?.Rut ?? "");
                ///
                var rut = usuario?.Rut;
                if (string.IsNullOrEmpty(rut))
                    throw new ArgumentException("El Rut del usuario es obligatorio.");

                int estado = (usuario?.Estado == true) ? 1 : 0;

                cmd.Parameters.Add("@Rut", SqlDbType.VarChar, 30).Value = rut;
                cmd.Parameters.Add("@Estado", SqlDbType.Int).Value = estado;

                con.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        public static string LimpiarRut_ELIMINAR(string rut)
        {
            if (string.IsNullOrWhiteSpace(rut)) return string.Empty;

            // Remueve puntos, guiones y espacios
            string limpio = rut.Replace(".", "").Replace("-", "").Trim();

            // Si contiene el dígito verificador al final (ejemplo 12345678-9 o 123456789)
            // Se comprueba si la longitud excede la parte numérica esperada y se remueve el DV
            if (limpio.Length >= 8)
            {
                // Remover el último caracter (dígito verificador) si viene con él
                limpio = limpio.Substring(0, limpio.Length - 1);
            }

            return limpio;
        }
    }

    public static class SqlDataReaderExtensions
    {
        public static bool HasColumn(this SqlDataReader reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }
    }
}
