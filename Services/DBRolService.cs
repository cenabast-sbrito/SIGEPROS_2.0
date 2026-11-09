using Microsoft.Data.SqlClient;
using SIGEPROS.Models;
using SIGEPROS.Static;
using System.Data;

namespace SIGEPROS.Services
{
    public class DBRolService
    {
        private static IConfiguration? _configuracion;
        private readonly DBService _dbservice;
        SqlConnection? _connection = null;
        SqlCommand? _command = null;
        SqlCommand? _commandIdentity = null;

        public DBRolService(DBService dbservice, IConfiguration configuracion)
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
                 Programa       = rd.GetStringSafe("TPrograma_breve"),
                 Vigente        = rd.GetBoolSafe("Vigente")
            };
        }
        public int toggleRol(UsuarioModel usuario)
        {
            using (var con = new SqlConnection(_dbservice.GetConnectionString()))
            using (var cmd = new SqlCommand("sp_ToggleRolPrograma", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Rut", usuario.Rut);
                cmd.Parameters.AddWithValue("@Programa", usuario.programa);
                cmd.Parameters.AddWithValue("@EsAdmin", usuario.esAdmin);
                cmd.Parameters.AddWithValue("@Activo", usuario.activo);
                con.Open();
                return cmd.ExecuteNonQuery(); // filas afectadas
            }
        }













        public void ActualizarRoles(
    string rut,
    string roles)
        {
            using SqlConnection cn =
                new SqlConnection(_dbservice.GetConnectionString());

            cn.Open();

            using SqlTransaction tx = cn.BeginTransaction();

            try
            {
                var delete = new SqlCommand(
                    @"DELETE UsuarioRol
              WHERE Rut = @Rut",
                    cn,
                    tx);

                delete.Parameters.AddWithValue("@Rut", rut);

                delete.ExecuteNonQuery();

                if (!string.IsNullOrWhiteSpace(roles))
                {
                    foreach (var rol in roles.Split(','))
                    {
                        var insert = new SqlCommand(
                            @"INSERT INTO UsuarioRol
                      (
                          Rut,
                          CRol, Vigente, FActualizacion, FCreacion
                      )
                      VALUES
                      (
                          @Rut,
                          @Rol, 1, GETDATE(), GETDATE()
                      )",
                            cn,
                            tx);

                        insert.Parameters.AddWithValue("@Rut", rut);
                        insert.Parameters.AddWithValue("@Rol", rol);

                        insert.ExecuteNonQuery();
                    }
                }

                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }









        public void ActualizarRolesOLD(
            string rut,
            string roles)
        {
            var listaRoles = roles
                .Split(',', StringSplitOptions.RemoveEmptyEntries);


            using SqlConnection cn =
                new SqlConnection(_dbservice.GetConnectionString());

            cn.Open();

            SqlTransaction tx =
                cn.BeginTransaction();

            try
            {
                SqlCommand delete =
                    new SqlCommand(
                        @"DELETE UsuarioRol
                  WHERE Rut=@Rut",
                        cn,
                        tx);

                delete.Parameters.AddWithValue(
                    "@Rut",
                    rut);

                delete.ExecuteNonQuery();

                foreach (var rol in listaRoles)
                {
                    SqlCommand insert =
                        new SqlCommand(
                            @"INSERT INTO UsuarioRol
                      (
                        Rut,
                        CRol
                      )
                      VALUES
                      (
                        @Rut,
                        @Rol
                      )",
                            cn,
                            tx);

                    insert.Parameters.AddWithValue(
                        "@Rut",
                        rut);

                    insert.Parameters.AddWithValue(
                        "@Rol",
                        rol);

                    insert.ExecuteNonQuery();
                }

                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }



    }
}
