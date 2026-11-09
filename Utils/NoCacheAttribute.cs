using Microsoft.AspNetCore.Mvc.Filters;

namespace SIGEPROS.Utils
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class NoCacheAttribute : ActionFilterAttribute
    {
        public override void OnResultExecuting(ResultExecutingContext context)
        {
            var response = context.HttpContext.Response;
            response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate, max-age=0";
            response.Headers["Pragma"] = "no-cache";
            response.Headers["Expires"] = "-1";

            base.OnResultExecuting(context);
        }
    }
}