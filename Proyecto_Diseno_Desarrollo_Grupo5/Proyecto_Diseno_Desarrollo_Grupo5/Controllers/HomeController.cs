using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using Proyecto_Diseno_Desarrollo_Grupo5.EF;
using Proyecto_Diseno_Desarrollo_Grupo5.Filters;

namespace Proyecto_Diseno_Desarrollo_Grupo5.Controllers
{
    public class HomeController : Controller
    {
        private readonly DBGRUPO5Entities db = new DBGRUPO5Entities();

        public ActionResult Index()
        {
            return View();
        }

        [RolAuthorize(1)]
        public ActionResult Bitacora(string q = "", string accion = "", string sort = "fecha", string dir = "desc", int page = 1, int pageSize = 10)
        {
            var query = db.BITACORA.Include(x => x.USUARIOS).AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                query = query.Where(x => x.DESCRIPCION.Contains(q) || x.ACCION.Contains(q));
            }

            if (!string.IsNullOrWhiteSpace(accion))
            {
                query = query.Where(x => x.ACCION == accion);
            }

            var asc = string.Equals(dir, "asc", StringComparison.OrdinalIgnoreCase);
            switch ((sort ?? "").ToLower())
            {
                case "usuario":
                    query = asc ? query.OrderBy(x => x.USUARIOS.NOMBRE) : query.OrderByDescending(x => x.USUARIOS.NOMBRE);
                    break;
                case "accion":
                    query = asc ? query.OrderBy(x => x.ACCION) : query.OrderByDescending(x => x.ACCION);
                    break;
                case "descripcion":
                    query = asc ? query.OrderBy(x => x.DESCRIPCION) : query.OrderByDescending(x => x.DESCRIPCION);
                    break;
                default:
                    sort = "fecha";
                    query = asc ? query.OrderBy(x => x.FECHA) : query.OrderByDescending(x => x.FECHA);
                    break;
            }

            var total = query.Count();
            var skip = (Math.Max(page, 1) - 1) * pageSize;
            var data = query.Skip(skip).Take(pageSize).ToList();

            ViewBag.Q = q;
            ViewBag.Accion = accion;
            ViewBag.Sort = sort;
            ViewBag.Dir = asc ? "asc" : "desc";
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = total;
            ViewBag.TotalPages = pageSize <= 0 ? 0 : (int)Math.Ceiling((double)total / pageSize);
            ViewBag.Acciones = db.BITACORA.Select(x => x.ACCION).Distinct().OrderBy(x => x).ToList();

            return View(data);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}