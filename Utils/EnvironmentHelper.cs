using Microsoft.Extensions.Configuration;

namespace SIGEPROS.Utils
{
    public static class EnvironmentHelper
    {
        private static IConfiguration _configuration;

        public static void Initialize(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public static string GetEnvironment()
        {
            return _configuration["ASPNETCORE_ENVIRONMENT"] ?? "Production";
        }
    }
}
