using Microsoft.Data.SqlClient;
using SIGEPROS.Models;
using System.Data;

namespace SIGEPROS.Services
{
    public class DBExpedienteJobService
    {
        private readonly DBService _dbservice;

        public DBExpedienteJobService(DBService dbservice)
        {
            _dbservice = dbservice;
        }

        /// <summary>
        /// Crea un Job y retorna el Id generado.
        /// </summary>
        public long CrearJob(string usuario)
        {
            using SqlConnection connection =
                new SqlConnection(_dbservice.GetConnectionString());

            using SqlCommand command =
                connection.CreateCommand();

            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "sp_Expediente_CrearJob";

            command.Parameters.AddWithValue("@Usuario", usuario);

            connection.Open();

            object result = command.ExecuteScalar();

            return Convert.ToInt64(result);
        }

        /// <summary>
        /// Actualiza progreso y estado.
        /// </summary>
        public int ActualizarEstado(
            long idJob,
            string estado,
            int porcentaje,
            string mensaje)
        {
            using SqlConnection connection =
                new SqlConnection(_dbservice.GetConnectionString());

            using SqlCommand command =
                connection.CreateCommand();

            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "sp_Expediente_ActualizarEstado";

            command.Parameters.AddWithValue("@IdJob", idJob);
            command.Parameters.AddWithValue("@Estado", estado);
            command.Parameters.AddWithValue("@Porcentaje", porcentaje);
            command.Parameters.AddWithValue("@Mensaje", mensaje);

            connection.Open();

            return command.ExecuteNonQuery();
        }

        /// <summary>
        /// Registra error asociado a un documento.
        /// </summary>
        public int RegistrarError(
            long idJob,
            string documentoVenta,
            string tipoError,
            string mensaje)
        {
            using SqlConnection connection =
                new SqlConnection(_dbservice.GetConnectionString());

            using SqlCommand command =
                connection.CreateCommand();

            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "sp_Expediente_RegistrarError";

            command.Parameters.AddWithValue("@IdJob", idJob);
            command.Parameters.AddWithValue("@DocumentoVenta", documentoVenta);
            command.Parameters.AddWithValue("@TipoError", tipoError);
            command.Parameters.AddWithValue("@Mensaje", mensaje);

            connection.Open();

            return command.ExecuteNonQuery();
        }

        /// <summary>
        /// Finaliza el Job.
        /// </summary>
        public int FinalizarJob(
            long idJob,
            string estado,
            string rutaExpediente)
        {
            using SqlConnection connection =
                new SqlConnection(_dbservice.GetConnectionString());

            using SqlCommand command =
                connection.CreateCommand();

            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "sp_Expediente_FinalizarJob";

            command.Parameters.AddWithValue("@IdJob", idJob);
            command.Parameters.AddWithValue("@Estado", estado);
            command.Parameters.AddWithValue("@RutaExpediente",
                string.IsNullOrEmpty(rutaExpediente)
                ? DBNull.Value
                : rutaExpediente);

            connection.Open();

            return command.ExecuteNonQuery();
        }

        /// <summary>
        /// Obtiene el estado actual del Job.
        /// </summary>
        public ExpedienteJobModel ObtenerEstado(long idJob)
        {
            ExpedienteJobModel model = null;

            using SqlConnection connection =
                new SqlConnection(_dbservice.GetConnectionString());

            using SqlCommand command =
                connection.CreateCommand();

            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "sp_Expediente_ConsultarEstado";

            command.Parameters.AddWithValue("@IdJob", idJob);

            connection.Open();

            using SqlDataReader rd = command.ExecuteReader();

            if (rd.Read())
            {
                model = new ExpedienteJobModel
                {
                    IdJob = Convert.ToInt64(rd["IdJob"]),
                    Estado = rd["Estado"].ToString(),
                    Porcentaje = Convert.ToInt32(rd["Porcentaje"]),
                    Mensaje = rd["Mensaje"].ToString(),
                    RutaExpediente = rd["RutaExpediente"]?.ToString()
                };
            }

            return model;
        }
    }
}