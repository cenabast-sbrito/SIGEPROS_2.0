using System.Security.Claims;

namespace SIGEPROS.Static
{
    public static class SeguridadHelper
    {
        public static bool PuedeAdministrarPrograma(
            ClaimsPrincipal user,
            int programa)
        {
            return user.Claims.Any(c =>
                c.Type == $"Programa_{programa}" &&
                c.Value == "Administra");
        }

        public static bool PuedeConsultarPrograma(
            ClaimsPrincipal user,
            int programa)
        {
            return user.Claims.Any(c =>
                c.Type == $"Programa_{programa}");
        }
    }
}
