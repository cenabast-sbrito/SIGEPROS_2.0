namespace SIGEPROS.Static
{
    using Microsoft.Data.SqlClient;

    public static class SqlDataReaderExtensions
    {
        private static bool HasColumn(SqlDataReader dr, string columnName)
        {
            for (int i = 0; i < dr.FieldCount; i++)
            {
                if (string.Equals(
                    dr.GetName(i),
                    columnName,
                    StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        public static string GetStringSafe(this SqlDataReader dr, string columnName)
        {
            if (!HasColumn(dr, columnName))
                return string.Empty;

            int pos = dr.GetOrdinal(columnName);

            return dr.IsDBNull(pos)
                ? string.Empty
                : Convert.ToString(dr.GetValue(pos)) ?? string.Empty;
        }

        public static int? GetIntSafe(this SqlDataReader dr, string columnName)
        {
            if (!HasColumn(dr, columnName))
                return null;

            int pos = dr.GetOrdinal(columnName);

            return dr.IsDBNull(pos)
                ? null
                : Convert.ToInt32(dr.GetValue(pos));
        }

        public static long? GetLongSafe(this SqlDataReader dr, string columnName)
        {
            if (!HasColumn(dr, columnName))
                return null;

            int pos = dr.GetOrdinal(columnName);

            return dr.IsDBNull(pos)
                ? null
                : Convert.ToInt64(dr.GetValue(pos));
        }

        public static bool? GetBoolSafe(this SqlDataReader dr, string columnName)
        {
            if (!HasColumn(dr, columnName))
                return null;

            int pos = dr.GetOrdinal(columnName);

            return dr.IsDBNull(pos)
                ? null
                : Convert.ToBoolean(dr.GetValue(pos));
        }

        public static decimal? GetDecimalSafe(this SqlDataReader dr, string columnName)
        {
            if (!HasColumn(dr, columnName))
                return null;

            int pos = dr.GetOrdinal(columnName);

            return dr.IsDBNull(pos)
                ? null
                : Convert.ToDecimal(dr.GetValue(pos));
        }

        public static DateTime? GetDateTimeSafe(this SqlDataReader dr, string columnName)
        {
            if (!HasColumn(dr, columnName))
                return null;

            int pos = dr.GetOrdinal(columnName);

            return dr.IsDBNull(pos)
                ? null
                : Convert.ToDateTime(dr.GetValue(pos));
        }
    }
}