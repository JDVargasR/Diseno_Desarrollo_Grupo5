using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Proyecto_Diseno_Desarrollo_Grupo5.EF;

namespace Proyecto_Diseno_Desarrollo_Grupo5
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
    }

    public static class BitacoraHelper
    {
        public static void Registrar(int? idUsuario, string accion, string descripcion)
        {
            if (!idUsuario.HasValue || idUsuario.Value <= 0) return;

            try
            {
                using (var db = new DBGRUPO5Entities())
                {
                    db.BITACORA.Add(new BITACORA
                    {
                        ID_USUARIO = idUsuario.Value,
                        ACCION = (accion ?? string.Empty).Trim(),
                        DESCRIPCION = (descripcion ?? string.Empty).Trim(),
                        FECHA = DateTime.Now
                    });

                    db.SaveChanges();
                }
            }
            catch
            {
            }
        }
    }
}
