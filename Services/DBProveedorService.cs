using Microsoft.Data.SqlClient;
using SIGEPROS.Models;
using System.Data;
using System.Data.Common;

namespace SIGEPROS.Services
{
    public class DBProveedorService
    {
        private readonly DBService _dbservice;
        private static IConfiguration? _configuracion;
        private static IConfiguration? _config;
        SqlConnection? _connection = null;
        SqlCommand? _command = null;
        SqlCommand? _commandIdentity = null;

        public DBProveedorService(DBService dbservice, IConfiguration configuracion)
        {
            _dbservice = dbservice;
            _configuracion = configuracion;
        }
        public UsuarioModel GetUsuarioByToken(string token)
        {
            var usuario = new UsuarioModel { autenticado = false };

            // Validaciones rápidas
            if (string.IsNullOrWhiteSpace(token))
                return usuario;

            try
            {
                using (var connection = new SqlConnection(_dbservice.GetConnectionString()))
                using (var cmd = new SqlCommand("[dbo].[proc_Usuario_Sistema_Authenticate]", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Parámetro de entrada @Id
                    cmd.Parameters.Add("@Id", SqlDbType.Char, 72).Value = token;

                    // Parámetros de salida
                    var pAut = cmd.Parameters.Add("@Autenticado", SqlDbType.Bit);
                    pAut.Direction = ParameterDirection.Output;

                    var pNombre = cmd.Parameters.Add("@Nombre", SqlDbType.VarChar, 60);
                    pNombre.Direction = ParameterDirection.Output;

                    var pUsuario = cmd.Parameters.Add("@Usuario", SqlDbType.VarChar, 60);
                    pUsuario.Direction = ParameterDirection.Output;

                    connection.Open();
                    cmd.ExecuteNonQuery();

                    bool autenticado =
                        pAut.Value != DBNull.Value &&
                        Convert.ToBoolean(pAut.Value);

                    usuario.autenticado = autenticado;

                    if (autenticado)
                    {
                        string nombreOut =
                            pNombre.Value == DBNull.Value
                                ? null
                                : pNombre.Value.ToString();

                        string usuarioOut =
                            pUsuario.Value == DBNull.Value
                                ? null
                                : pUsuario.Value.ToString();

                        usuario.Nombre = nombreOut;

                        // Si rutUsuario es string
                        usuario.Rut = usuarioOut;

                        // Si rutUsuario es double, usar:
                        // if (double.TryParse(usuarioOut, out double rut))
                        //     usuario.rutUsuario = rut;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error autenticando por token: {ex.Message}");
            }

            return usuario;
        }

        public UsuarioModel GetUsuarioByTokenOLD( string token)
        {
            var usuario = new UsuarioModel { autenticado = false };

            // Validaciones rápidas
            if (string.IsNullOrWhiteSpace(token))
                return usuario;

            try
            {
                using (var connection = new SqlConnection(_dbservice.GetConnectionString()))
                using (var cmd = new SqlCommand("[dbo].[proc_Usuario_Sistema_Authenticate]", connection)) /* Procedimiento en PZ_SISTEMA_TRANS */
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Parámetro de entrada @Id
                    var pId = cmd.Parameters.Add("@Id", SqlDbType.Char, 72);
                    pId.Value = token;

                    // Parámetros de salida
                    var pAut = cmd.Parameters.Add("@Autenticado", SqlDbType.Bit);
                    pAut.Direction = ParameterDirection.Output;

                    var pNombre = cmd.Parameters.Add("@Nombre", SqlDbType.VarChar, 60);
                    pNombre.Direction = ParameterDirection.Output;


                    connection.Open();
                    cmd.ExecuteNonQuery();

                    // Lee los outputs
                    bool autenticado = (pAut.Value != DBNull.Value) && Convert.ToBoolean(pAut.Value);
                    usuario.autenticado = autenticado;

                    if (autenticado)
                    {
                        string nombreOut = pNombre.Value == DBNull.Value ? null : (string)pNombre.Value;

                        if (!string.IsNullOrWhiteSpace(nombreOut) && double.TryParse(nombreOut, out double rutNumeric))
                            usuario.rutUsuario = rutNumeric;

                        usuario.Nombre = nombreOut;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error autenticando por token: {ex.Message}");
            }

            return usuario;
        }
        public Proveedor GetProveedor(string Rut)
        {

            Proveedor usuario = new Proveedor();
            try
            {
                using (_connection = new SqlConnection(_dbservice.GetConnectionString()))
                {
                    _command = _connection.CreateCommand();
                    _command.CommandType = CommandType.StoredProcedure;
                    _command.CommandText = "proc_Proveedor_GET"; //// Nuevo Procedimiento
                    _command.Parameters.AddWithValue("@rutProveedor", Rut);

                    _connection.Open();
                    SqlDataReader dr = _command.ExecuteReader();

                    if (dr.Read())
                    {
                        usuario = new Proveedor();
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
        public List<EstadoModel> GetlistaEstados(int cEstado, string organismo)
        {
            EstadoModel dato;
            List<EstadoModel> datos = new List<EstadoModel>();
            try
            {

                using (_connection = new SqlConnection(_dbservice.GetConnectionString()))
                {
                    string textoAdicional = "";
                    if (organismo == "M" ) /// MINSAL
                    {
                        textoAdicional = " WHERE CESTADO IN (5,6,7,8,9,10) ";
                    }

                    _command = _connection.CreateCommand();
                    _command.CommandType = CommandType.Text;
                    _command.CommandText = "SELECT CEstado,TEstado,TMensaje,TEstadoTiempoPresente,IconoSugerido FROM [SIGEPROS].[dbo].[Estado] "+textoAdicional;
                   /// _command.Parameters.AddWithValue("@CESTADO", cEstado);

                    _connection.Open();
                    SqlDataReader dr = _command.ExecuteReader();

                    while (dr.Read())
                    {
                        dato = new EstadoModel();

                        dato.CEstado = dr["CEstado"] != DBNull.Value
                            ? Convert.ToInt32(dr["CEstado"])
                            : 0;

                        dato.TEstado = dr["TEstadoTiempoPresente"]?.ToString() ?? string.Empty;


                        datos.Add(dato);
                    }
                    dr.Close();
                    _connection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return datos;
        }

        public Proveedor getProveedorPedidoCompra(string pedidoCompra)
        {
            SqlConnection con = new SqlConnection(_dbservice.GetConnectionString());
            SqlCommand cmd = new SqlCommand("proc_ProveedorSAP_Obtener @pedidoCompra", con);
            cmd.Parameters.AddWithValue("@pedidoCompra", pedidoCompra);

            SqlDataReader reader;
            Proveedor prov = new Proveedor(); // <-- SIEMPRE instanciado

            prov.RutProveedor = 0;
            prov.Nombre = "";

            con.Open();
            try
            {
                reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    prov.RutProveedor = int.Parse(reader["rutProveedor"].ToString());
                    prov.Nombre = reader["Nombre"].ToString();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.ToString());
                // prov sigue en 0,0 y "", correcto
            }
            finally
            {
                con.Close();
            }

            return prov;  // <-- nunca será null
        }
        public int getFacturaDV(string documentoVenta)
        {
            SqlConnection con = new SqlConnection(_dbservice.GetConnectionString());
            SqlCommand cmd = new SqlCommand("proc_FacturaDV_Obtener @documentoVenta", con);
            cmd.Parameters.AddWithValue("@documentoVenta", documentoVenta);

            SqlDataReader reader;

            var nFactura = 0;
            con.Open();
            try
            {
                reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    nFactura = int.Parse(reader["VBELN_VF"].ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.ToString());
            }
            finally
            {
                con.Close();
            }

            return nFactura;  
        }

        private Proveedor _set(SqlDataReader rd)
        {


            Proveedor proveedor = new Proveedor();
            try
            {

                proveedor.RutProveedor = int.Parse(rd["RutProveedor"].ToString());
                proveedor.Nombre = rd["Nombre"].ToString();
                proveedor.Direccion = rd["Direccion"].ToString();
                proveedor.Estado = Boolean.Parse(rd["Estado"].ToString()) ? 1 : 0;
                proveedor.CPrograma = int.Parse(rd["CPrograma"].ToString()); ///// rd["Rol"] == DBNull.Value ? "" : rd["Rol"].ToString().Trim().ToUpperInvariant();




                /*
                user.Organismo = rd["Organismo"].ToString();
                user.email = rd["Email"] == DBNull.Value ? "" : rd["Email"].ToString().Trim();

                //var encryptedBase64 = rd["Clave"]?.ToString();

                //if (!string.IsNullOrWhiteSpace(encryptedBase64))
                //{
                //    user.Clave = CryptoHelper.DecryptFromBase64(encryptedBase64);
                //}

                user.acceso_PNACPACAM = int.Parse(rd["Acceso_PNACPACAM"].ToString());
                user.acceso_PNI = int.Parse(rd["Acceso_PNI"].ToString());
                user.esGestor = int.Parse(rd["esGestor"].ToString());

                return user;*/
                return proveedor;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return proveedor;

        }

        public int PostProveedor(string RutProveedor, string Nombre , string Direccion, string Estado, string CPrograma )
        {
            using (SqlConnection connection = new SqlConnection(_dbservice.GetConnectionString()))
            using (SqlCommand command = connection.CreateCommand())
            {

                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "proc_Proveedor_Agregar";
                command.Parameters.AddWithValue("@RutProveedor", RutProveedor);
                command.Parameters.AddWithValue("@Nombre", Nombre);
                command.Parameters.AddWithValue("@Direccion", Direccion);
                command.Parameters.AddWithValue("@Estado", Estado);
                command.Parameters.AddWithValue("@CPrograma", CPrograma);

                connection.Open();
                return command.ExecuteNonQuery();

            }
        }

        public int PutProveedor(string RutProveedor, string Nombre, string Direccion, string Estado, string CPrograma)
        {
            using (SqlConnection connection = new SqlConnection(_dbservice.GetConnectionString()))
            using (SqlCommand command = connection.CreateCommand())
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "proc_Proveedor_Actualizar";
                command.Parameters.AddWithValue("@RutProveedor", RutProveedor);
                command.Parameters.AddWithValue("@Nombre", Nombre);
                command.Parameters.AddWithValue("@Direccion", Direccion);
                command.Parameters.AddWithValue("@Estado", Estado);
                command.Parameters.AddWithValue("@CPrograma", CPrograma);

                connection.Open();

                return command.ExecuteNonQuery();
            }
        }

        public int UpdateProveedor( string RutProveedor, int Estado)
        {
            using (SqlConnection connection = new SqlConnection(_dbservice.GetConnectionString()))
            using (SqlCommand command = connection.CreateCommand())
            {
                command.CommandType = CommandType.Text;
                command.CommandText = "UPDATE PROVEEDOR SET ESTADO = @Estado WHERE RUTPROVEEDOR = @RutProveedor";
                command.Parameters.AddWithValue("@RutProveedor", RutProveedor);
                command.Parameters.AddWithValue("@Estado", Estado);

                connection.Open();

                return command.ExecuteNonQuery();
            }
        }

        public List<ProveedorModel> GetlistaProveedores(int programa)
        {
            ProveedorModel dato;
            List<ProveedorModel> datos = new List<ProveedorModel>();
            try
            {

                using (_connection = new SqlConnection(_dbservice.GetConnectionString()))
                {
                    _command = _connection.CreateCommand();
                    _command.CommandType = CommandType.StoredProcedure;
                    _command.CommandText = "[dbo].[proc_Proveedor_Obtener]";
                    _command.Parameters.AddWithValue("@PROGRAMA", programa);

                    _connection.Open();
                    SqlDataReader dr = _command.ExecuteReader();

                    while (dr.Read())
                    {
                        dato = new ProveedorModel();

                        dato.RutProveedor = dr["RutProveedor"] != DBNull.Value
                            ? Convert.ToInt32(dr["RutProveedor"])
                            : null;

                        dato.Nombre = dr["Nombre"]?.ToString() ?? string.Empty;

                        dato.Direccion = dr["Direccion"]?.ToString() ?? string.Empty;

                        dato.Estado = dr["Estado"] != DBNull.Value
                            ? Convert.ToBoolean(dr["Estado"])
                            : null;

                        dato.FechaCreacion = dr["FechaCreacion"] != DBNull.Value
                            ? Convert.ToDateTime(dr["FechaCreacion"])
                            : null;

                        dato.FechaActualizacion = dr["FechaActualizacion"] != DBNull.Value
                            ? Convert.ToDateTime(dr["FechaActualizacion"])
                            : null;

                        dato.CPrograma = dr["CPrograma"] != DBNull.Value
                            ? Convert.ToInt32(dr["CPrograma"])
                            : null;

                        dato.TPrograma = dr["TPrograma"]?.ToString() ?? string.Empty;

                        datos.Add(dato);
                    }
                    dr.Close();
                    _connection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return datos;
        }

    }

}
