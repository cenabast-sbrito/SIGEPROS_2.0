using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace SIGEPROS.Utils
{
    public static class PathHelper
    {

        public static string GetPath(string relativePath, HttpContext httpContext)
        {
            var environment = EnvironmentHelper.GetEnvironment();
            string pathPrefix = string.Empty;

            switch (environment)
            {
                case "Development":
                    pathPrefix = "";
                    break;
                case "Staging":
                    pathPrefix = "/ProyectosTI/{nombreapp}";
                    break;
                case "Production":
                    pathPrefix = "/apps/{nombreapp}";
                    break;
            }

            if (relativePath != null)
            {
                if (relativePath.StartsWith("~/"))
                {
                    relativePath = relativePath.Substring(2);
                }
                else if (relativePath.StartsWith("/"))
                {
                    relativePath = relativePath.Substring(1);
                }
            }
            var domain = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}";
            return $"{domain}{pathPrefix}/{relativePath}";
        }
    }
}
