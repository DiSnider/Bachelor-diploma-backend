using System.Web.Http;
using System.Web.Http.Cors;

namespace Diploma_backend.API
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            var cors = new EnableCorsAttribute("*", "*", "*");

            config.EnableCors(cors);

            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "{controller}/{action}"
            );

            config.Filters.Add(new ExceptionHandlingAttribute());
        }
    }
}
