using System.Web;
using System.Web.Mvc;
using Proyecto_Diseno_Desarrollo_Grupo5.Filters;

namespace Proyecto_Diseno_Desarrollo_Grupo5
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new BitacoraAuditAttribute());
        }
    }
}
