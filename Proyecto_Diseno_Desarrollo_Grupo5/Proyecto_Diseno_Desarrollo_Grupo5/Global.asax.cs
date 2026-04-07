using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Proyecto_Diseno_Desarrollo_Grupo5.EF;
using System.Data.SqlClient;

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
        public static void Registrar(int? idUsuario, string accion, string descripcion, string modulo = null, string idRegistroAfectado = null, string resultado = "OK")
        {
            if (!idUsuario.HasValue || idUsuario.Value <= 0) return;

            try
            {
                using (var db = new DBGRUPO5Entities())
                {
                    db.Database.ExecuteSqlCommand(
                        "INSERT INTO dbo.BITACORA (ID_USUARIO, ACCION, DESCRIPCION, FECHA, MODULO, ID_REGISTRO_AFECTADO, RESULTADO) VALUES (@ID_USUARIO, @ACCION, @DESCRIPCION, @FECHA, @MODULO, @ID_REGISTRO_AFECTADO, @RESULTADO)",
                        new SqlParameter("@ID_USUARIO", idUsuario.Value),
                        new SqlParameter("@ACCION", (object)((accion ?? string.Empty).Trim()) ?? DBNull.Value),
                        new SqlParameter("@DESCRIPCION", (object)((descripcion ?? string.Empty).Trim()) ?? DBNull.Value),
                        new SqlParameter("@FECHA", DateTime.Now),
                        new SqlParameter("@MODULO", (object)(string.IsNullOrWhiteSpace(modulo) ? (object)DBNull.Value : modulo.Trim().ToUpperInvariant())),
                        new SqlParameter("@ID_REGISTRO_AFECTADO", (object)(string.IsNullOrWhiteSpace(idRegistroAfectado) ? (object)DBNull.Value : idRegistroAfectado.Trim())),
                        new SqlParameter("@RESULTADO", (object)(string.IsNullOrWhiteSpace(resultado) ? "OK" : resultado.Trim().ToUpperInvariant()))
                    );
                }
            }
            catch
            {
            }
        }
    }
}
