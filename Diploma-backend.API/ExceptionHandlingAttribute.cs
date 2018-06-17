using System.Web.Http.Filters;
using NLog;

namespace Diploma_backend.API
{
    public class ExceptionHandlingAttribute : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext context)
        {
            LogManager.GetCurrentClassLogger().Error(context.Exception);
        }
    }
}