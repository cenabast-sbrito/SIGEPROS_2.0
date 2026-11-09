namespace SIGEPROS.Static
{
    public static class Configuracion
    {
        public static string GetAmbiente(IConfiguration configuration)
            => configuration["Configuracion:Ambiente"];

        public static bool EsProduccion(IConfiguration configuration)
            => GetAmbiente(configuration) == "Production";

        public static string GetValor(IConfiguration configuration, string key)
            => configuration[$"Configuracion:{key}"];

        public static string GetVersion(IConfiguration configuration)
    => configuration["Configuracion:version"];

        public static string GetValorPorAmbiente(IConfiguration configuration, string key)
        {
            return EsProduccion(configuration)
                ? configuration[$"Configuracion:{key}"]
                : configuration[$"Configuracion:{key}_TEST"];
        }

    }
}