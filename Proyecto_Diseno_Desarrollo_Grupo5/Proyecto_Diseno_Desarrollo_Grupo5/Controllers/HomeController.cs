using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Proyecto_Diseno_Desarrollo_Grupo5.EF;
using Proyecto_Diseno_Desarrollo_Grupo5.Filters;
using Proyecto_Diseno_Desarrollo_Grupo5.Models;

namespace Proyecto_Diseno_Desarrollo_Grupo5.Controllers
{
    [RolAuthorize(1,2,3)]
    public class HomeController : Controller
    {
        private readonly DBGRUPO5Entities db = new DBGRUPO5Entities();

        public ActionResult Index()
        {
            return View();
        }

        [RolAuthorize(3)]
        public ActionResult Productos()
        {
            var estadoActivoId = db.ESTADO
                .Where(e => e.NOMBRE == "Activo")
                .Select(e => e.ID_ESTADO)
                .FirstOrDefault();

            var query = db.PRODUCTOS.AsQueryable();

            if (estadoActivoId > 0)
            {
                query = query.Where(p => p.ID_ESTADO == estadoActivoId);
            }

            var productosCliente = query
                .OrderBy(p => p.NOMBRE)
                .Take(80)
                .Select(p => new ProductoFilaVM
                {
                    ID_PRODUCTO = p.ID_PRODUCTO,
                    NOMBRE = p.NOMBRE,
                    PRECIO_VENTA = p.PRECIO_VENTA,
                    CATEGORIA = p.CATEGORIAS != null ? p.CATEGORIAS.NOMBRE : "Sin categoria"
                })
                .ToList();

            return View("ClienteCatalogo", productosCliente);
        }

        [RolAuthorize(1)]
        public ActionResult Bitacora(string q = "", string accion = "", string modulo = "", string resultado = "", string sort = "fecha", string dir = "desc", int page = 1, int pageSize = 10)
        {
            var source = db.Database.SqlQuery<BitacoraItemVM>(@"
                SELECT
                    b.ID_BITACORA,
                    b.ID_USUARIO,
                    ISNULL(u.NOMBRE, 'Usuario #' + CAST(b.ID_USUARIO AS VARCHAR(20))) AS USUARIO_NOMBRE,
                    b.ACCION,
                    b.DESCRIPCION,
                    b.FECHA,
                    b.MODULO,
                    b.ID_REGISTRO_AFECTADO,
                    b.RESULTADO
                FROM dbo.BITACORA b
                LEFT JOIN dbo.USUARIOS u ON u.ID_USUARIO = b.ID_USUARIO
            ").ToList();

            var query = source.AsQueryable();
            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();
                query = query.Where(x =>
                    (x.DESCRIPCION ?? "").Contains(term) ||
                    (x.ACCION ?? "").Contains(term) ||
                    (x.MODULO ?? "").Contains(term) ||
                    (x.USUARIO_NOMBRE ?? "").Contains(term));
            }

            if (!string.IsNullOrWhiteSpace(accion))
            {
                query = query.Where(x => x.ACCION == accion);
            }

            if (!string.IsNullOrWhiteSpace(modulo))
            {
                query = query.Where(x => x.MODULO == modulo);
            }

            if (!string.IsNullOrWhiteSpace(resultado))
            {
                query = query.Where(x => x.RESULTADO == resultado);
            }

            var asc = string.Equals(dir, "asc", StringComparison.OrdinalIgnoreCase);
            switch ((sort ?? "").ToLower())
            {
                case "usuario":
                    query = asc ? query.OrderBy(x => x.USUARIO_NOMBRE) : query.OrderByDescending(x => x.USUARIO_NOMBRE);
                    break;
                case "accion":
                    query = asc ? query.OrderBy(x => x.ACCION) : query.OrderByDescending(x => x.ACCION);
                    break;
                case "modulo":
                    query = asc ? query.OrderBy(x => x.MODULO) : query.OrderByDescending(x => x.MODULO);
                    break;
                case "resultado":
                    query = asc ? query.OrderBy(x => x.RESULTADO) : query.OrderByDescending(x => x.RESULTADO);
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
            var paged = query.Skip(skip).Take(pageSize).ToList();

            ViewBag.Q = q;
            ViewBag.Accion = accion;
            ViewBag.Modulo = modulo;
            ViewBag.Resultado = resultado;
            ViewBag.Sort = sort;
            ViewBag.Dir = asc ? "asc" : "desc";
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = total;
            ViewBag.TotalPages = pageSize <= 0 ? 0 : (int)Math.Ceiling((double)total / pageSize);
            ViewBag.Acciones = source.Select(x => x.ACCION).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().OrderBy(x => x).ToList();
            ViewBag.Modulos = source.Select(x => x.MODULO).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().OrderBy(x => x).ToList();

            return View(paged);
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