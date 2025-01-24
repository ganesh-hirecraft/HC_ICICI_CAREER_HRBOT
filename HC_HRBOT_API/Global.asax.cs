using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace HC_HRBOT_API
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            //System.Diagnostics.Debugger.Break();
            beHC_HR_BOT.Common.Connection.ConStr = ConfigurationManager.ConnectionStrings["DBCS"].ToString();

            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Application_BeginRequest(object sender, EventArgs e)
        {

            //var application = sender as HttpApplication;
            //if (application != null && application.Context != null)
            //{
            //    application.Context.Response.Headers.Remove("Server");
            //}

            if (HttpContext.Current.Request.HttpMethod == "OPTIONS")
            {
                HttpContext.Current.Response.AddHeader("Cache-Control", "no-cache");
                HttpContext.Current.Response.AddHeader("Access-Control-Allow-Methods", "GET, POST");
                HttpContext.Current.Response.AddHeader("Access-Control-Allow-Headers", "Content-Type, Accept");
                HttpContext.Current.Response.AddHeader("Access-Control-Max-Age", "1728000");
                HttpContext.Current.Response.End();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        protected void Application_PreSendRequestHeaders()
        {
           // Response.Headers.Set("Server", "My httpd server");
            Response.Headers.Set("X-SourceFiles", "My Scource Files");
            Response.Headers.Set("Cache-Control", "no-cache");
            Response.Headers.Set("Pragma", "no-cache");
            Response.Headers.Set("Expires", "-1");
            Response.Headers.Set("X-Frame-Options", "SAMEORIGIN");
            Response.Headers.Set("X-Content-Type-Options", "nosniff");
            Response.Headers.Set("X-XSS-Protection", "1; mode=block");
            //Response.Headers.Set("Strict-Transport-Security", "max-age=300");
            Response.Headers.Set("X-Powered-By", "NA");
            Response.Headers.Set("Allow", "NA");
            Response.Headers.Set("X-Custom-Name", "NA");
            Response.Headers.Remove("X-AspNet-Version");
            Response.Headers.Remove("X-AspNetMvc-Version");
            Response.Headers.Remove("X-Powered-By");
            Response.Headers.Remove("ETag");
            Response.Headers.Set("Access-Control-Allow-Origin", "*");
            //Response.Headers.Set("Origin", "*");

        }
    }
}
