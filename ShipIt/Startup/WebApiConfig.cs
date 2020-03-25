using System.Web.Http;
using ShipIt.Filters;

namespace ShipIt
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.Routes.MapHttpRoute(
                "ApiRoot",
                "Test/{id}",
                new {controller = "Product", id = RouteParameter.Optional}
            );
            config.Routes.MapHttpRoute(
                "DefaultApi",
                "{controller}/{id}",
                new {id = RouteParameter.Optional}
            );
            config.Routes.MapHttpRoute(
                "AppLaunch",
                "",
                new {controller = "Status"}
            );
            config.Filters.Add(new ShipItExceptionFilterAttribute());
            config.Formatters.XmlFormatter.UseXmlSerializer = true;
        }
    }
}