
using System;
using System.Collections.Generic;
using System.Linq;

public static class OrganismoCanalesHelper
{
    // Diccionario compatible con .NET Framework 4.7.2
    private static readonly Dictionary<string, string[]> CanalesPorOrganismo =
        new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            { "PNACPACAM", new[] { "36", "37" } },
            { "PNI",      new[] { "17" } },
            { "ADM", new[] { "36", "37" } },
            { "ADP", new[] { "17" } },
        };

    /// <summary>
    /// Obtiene los canales permitidos para el organismo
    /// </summary>
    public static IList<string> ObtenerCanalesPermitidos(string organismo)
    {
        if (string.IsNullOrWhiteSpace(organismo))
            return new List<string>();

        string[] canales;
        if (CanalesPorOrganismo.TryGetValue(organismo.Trim(), out canales))
            return new List<string>(canales);

        return new List<string>();
    }

    /// <summary>
    /// Valida si un canal es permitido para el organismo
    /// </summary>
    public static bool EsCanalValido(string programa, string canal)
    {
         if (string.IsNullOrWhiteSpace(programa) || string.IsNullOrWhiteSpace(canal))
            return false;

        var permitidos = ObtenerCanalesPermitidos(programa);
        return permitidos.Contains(canal.Trim());
    }

    /// <summary>
    /// Resuelve el canal:
    /// - Si viene y es válido → se usa
    /// - Si no viene y hay uno solo → se asigna
    /// - Si CENABAST sin canal → usa 36 por defecto
    /// </summary>
    public static bool TryResolverCanal(string organismo, string canalEntrada, out string canalResuelto)
    {
        canalResuelto = null;

        var permitidos = ObtenerCanalesPermitidos(organismo);
        if (permitidos.Count == 0)
            return false;

        // 1. Canal ingresado y válido
        if (!string.IsNullOrWhiteSpace(canalEntrada) &&
            permitidos.Contains(canalEntrada.Trim()))
        {
            canalResuelto = canalEntrada.Trim();
            return true;
        }

        // 2. Solo un canal posible (PNI)
        if (permitidos.Count == 1)
        {
            canalResuelto = permitidos[0];
            return true;
        }

        // 3. Default negocio para CENABAST
        if (organismo.Equals("PNACPACAM", StringComparison.OrdinalIgnoreCase))
        {
            canalResuelto = "36"; // default definido por negocio
            return true;
        }

        return false;
    }
}
