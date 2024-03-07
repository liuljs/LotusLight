using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace LotusLight
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        //void Application_BeginRequest(object sender, EventArgs e)
        //{
        //    if (Code.Helper.RequireSSL)
        //        if (!Request.Url.Scheme.ToLower().Equals("https"))
        //            Response.Redirect(Request.Url.AbsoluteUri.Replace("http://", "https://"));
        //}

        /// <summary>
        /// 是否強制轉SSL
        /// </summary>
        //public static bool RequireSSL
        //{
        //    get
        //    {
        //        string requireSSL = ConfigurationManager.AppSettings["RequireSSL"];
        //        if (String.IsNullOrEmpty(requireSSL) || requireSSL.ToLower().Equals("true"))
        //            return true;
        //        else
        //            return false;
        //    }
        //}

    }
}
