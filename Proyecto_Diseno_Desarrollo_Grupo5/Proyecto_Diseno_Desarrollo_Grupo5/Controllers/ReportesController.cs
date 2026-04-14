using Proyecto_Diseno_Desarrollo_Grupo5.EF;
using Proyecto_Diseno_Desarrollo_Grupo5.Filters;
using Proyecto_Diseno_Desarrollo_Grupo5.Models.Reportes;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace Proyecto_Diseno_Desarrollo_Grupo5.Controllers
{
    [RolAuthorize(1)]
    public class ReportesController : Controller
    {
        private DBGRUPO5Entities db = new DBGRUPO5Entities();

        // GET: Reportes
        public ActionResult Index()
        {
            return RedirectToAction("Desperdicios");
        }

        // GET: Reportes/Desperdicios
        public ActionResult Desperdicios(DateTime? fechaInicio = null, DateTime? fechaFin = null)
        {
            // Valores por defecto: últimos 30 días
            if (!fechaInicio.HasValue)
                fechaInicio = DateTime.Now.AddDays(-30);
            if (!fechaFin.HasValue)
                fechaFin = DateTime.Now;

            var vm = ObtenerAnalisisDesPerdicio(fechaInicio.Value, fechaFin.Value);
            vm.FechaInicio = fechaInicio.Value;
            vm.FechaFin = fechaFin.Value;

            ViewBag.FechaInicio = fechaInicio.Value.ToString("yyyy-MM-dd");
            ViewBag.FechaFin = fechaFin.Value.ToString("yyyy-MM-dd");

            return View(vm);
        }

        private DesperdicioAnalisisVM ObtenerAnalisisDesPerdicio(DateTime fechaInicio, DateTime fechaFin)
        {
            var vm = new DesperdicioAnalisisVM();

            // Ajustar fechaFin para incluir todo el día (hasta 23:59:59)
            var fechaFinAjustada = fechaFin.AddDays(1).AddSeconds(-1);

            // Obtener todos los desperdicios en el rango de fechas
            var desperdicios = db.DESPERDICIOS_MATERIAL
                .Where(d => d.FECHA >= fechaInicio && d.FECHA <= fechaFinAjustada && d.ORIGEN == "VENTA")
                .Include(d => d.MATERIALES)
                .Include(d => d.PRODUCTOS)
                .ToList();

            if (desperdicios.Count == 0)
                return vm;

            // Totales
            vm.TotalDesperdiciado = desperdicios.Sum(d => d.CANTIDAD_DESPERDICIADA);
            vm.TotalTransacciones = desperdicios.Count;
            vm.PromedioTransaccion = vm.TotalTransacciones > 0 ? vm.TotalDesperdiciado / vm.TotalTransacciones : 0;

            // Desperdicios por Producto
            vm.DesperdiciosPorProducto = desperdicios
                .Where(d => d.ID_PRODUCTO.HasValue && d.PRODUCTOS != null)
                .GroupBy(d => new { d.ID_PRODUCTO, d.PRODUCTOS.NOMBRE })
                .Select(g => new DesperdicioProductoVM
                {
                    IdProducto = g.Key.ID_PRODUCTO.Value,
                    NombreProducto = g.Key.NOMBRE,
                    TotalDesperdiciado = g.Sum(d => d.CANTIDAD_DESPERDICIADA),
                    PorcentajeDesperdicio = g.Count() > 0 ? (g.Sum(d => d.CANTIDAD_DESPERDICIADA) / vm.TotalDesperdiciado * 100) : 0,
                    CantidadTransacciones = g.Count()
                })
                .OrderByDescending(d => d.TotalDesperdiciado)
                .ToList();

            // Desperdicios por Material
            vm.DesperdiciosPorMaterial = desperdicios
                .Where(d => d.MATERIALES != null)
                .GroupBy(d => new { d.ID_MATERIAL, d.MATERIALES.NOMBRE, d.MATERIALES.TIPO })
                .Select(g => new DesperdicioMaterialVM
                {
                    IdMaterial = g.Key.ID_MATERIAL,
                    NombreMaterial = g.Key.NOMBRE,
                    TotalDesperdiciado = g.Sum(d => d.CANTIDAD_DESPERDICIADA),
                    Unidad = g.Key.TIPO,
                    CantidadTransacciones = g.Count()
                })
                .OrderByDescending(d => d.TotalDesperdiciado)
                .ToList();

            // Desperdicios por Día
            vm.DesperdiciosPorDia = desperdicios
                .GroupBy(d => d.FECHA.Date)
                .Select(g => new DesperdicioDiaVM
                {
                    Fecha = g.Key,
                    TotalDesperdiciado = g.Sum(d => d.CANTIDAD_DESPERDICIADA),
                    CantidadTransacciones = g.Count()
                })
                .OrderBy(d => d.Fecha)
                .ToList();

            // Productos con alto desperdicio
            var totalPorProducto = desperdicios
                .Where(d => d.ID_PRODUCTO.HasValue)
                .GroupBy(d => d.ID_PRODUCTO.Value)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.CANTIDAD_DESPERDICIADA));

            vm.ProductosAltoDesperdicio = db.PRODUCTOS
                .Where(p => p.PORC_DESPERDICIO > 5 && p.ID_ESTADO == 1)
                .ToList()
                .Select(p => new ProductoAltoDesperdicioVM
                {
                    IdProducto = p.ID_PRODUCTO,
                    NombreProducto = p.NOMBRE,
                    PorcentajeDesperdicio = p.PORC_DESPERDICIO,
                    TotalDesperdiciado = totalPorProducto.ContainsKey(p.ID_PRODUCTO)
                        ? totalPorProducto[p.ID_PRODUCTO]
                        : 0,
                    Recomendacion = p.PORC_DESPERDICIO > 15
                        ? "⚠️ Alto desperdicio. Revisar procesos de producción."
                        : "Monitorear desperdicio"
                })
                .OrderByDescending(p => p.PorcentajeDesperdicio)
                .ToList();

            return vm;
        }

        // GET: Reportes/DesperdiciosDetalle - Tabla detallada
        public ActionResult DesperdiciosDetalle(DateTime? fechaInicio = null, DateTime? fechaFin = null, int page = 1, int pageSize = 15)
        {
            if (!fechaInicio.HasValue)
                fechaInicio = DateTime.Now.AddDays(-30);
            if (!fechaFin.HasValue)
                fechaFin = DateTime.Now;

            var fechaFinAjustada = fechaFin.Value.Date.AddDays(1).AddSeconds(-1);

            var desperdicios = db.DESPERDICIOS_MATERIAL
                .Where(d => d.FECHA >= fechaInicio && d.FECHA <= fechaFinAjustada && d.ORIGEN == "VENTA")
                .Include(d => d.MATERIALES)
                .Include(d => d.PRODUCTOS)
                .Include(d => d.USUARIOS)
                .OrderByDescending(d => d.FECHA)
                .ToList();

            var lista = desperdicios
                .Select(d => new DesperdicioFilaVM
                {
                    IdDesperdicio = d.ID_DESPERDICIO,
                    Fecha = d.FECHA,
                    Material = d.MATERIALES?.NOMBRE ?? "Sin material",
                    CantidadDesperdiciada = d.CANTIDAD_DESPERDICIADA,
                    Unidad = d.MATERIALES?.TIPO ?? "",
                    Producto = d.PRODUCTOS?.NOMBRE ?? "Sin producto",
                    IdVenta = d.ID_VENTA,
                    Motivo = d.MOTIVO ?? "No especificado",
                    Reutilizable = d.REUTILIZABLE ?? "No",
                    Usuario = d.USUARIOS?.NOMBRE ?? "Sistema"
                })
                .ToList();

            // Paginación
            var total = lista.Count;
            var skip = (Math.Max(page, 1) - 1) * pageSize;
            var paged = lista.Skip(skip).Take(pageSize).ToList();

            ViewBag.FechaInicio = fechaInicio.Value.ToString("yyyy-MM-dd");
            ViewBag.FechaFin = fechaFin.Value.ToString("yyyy-MM-dd");
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = total;
            ViewBag.TotalPages = pageSize <= 0 ? 0 : (int)Math.Ceiling((double)total / pageSize);

            return View(paged);
        }

        // GET: Reportes/DesperdiciosJSON (para gráficos)
        public JsonResult DesperdiciosJSON(DateTime? fechaInicio = null, DateTime? fechaFin = null)
        {
            if (!fechaInicio.HasValue)
                fechaInicio = DateTime.Now.AddDays(-30);
            if (!fechaFin.HasValue)
                fechaFin = DateTime.Now;

            var vm = ObtenerAnalisisDesPerdicio(fechaInicio.Value, fechaFin.Value);

            var result = new
            {
                totalDesperdiciado = vm.TotalDesperdiciado,
                totalTransacciones = vm.TotalTransacciones,
                promedioTransaccion = vm.PromedioTransaccion,
                porProducto = vm.DesperdiciosPorProducto.Select(p => new
                {
                    nombre = p.NombreProducto,
                    cantidad = p.TotalDesperdiciado,
                    porcentaje = Math.Round(p.PorcentajeDesperdicio, 2)
                }),
                porMaterial = vm.DesperdiciosPorMaterial.Select(m => new
                {
                    nombre = m.NombreMaterial,
                    cantidad = m.TotalDesperdiciado,
                    unidad = m.Unidad
                }),
                porDia = vm.DesperdiciosPorDia.Select(d => new
                {
                    fecha = d.Fecha.ToString("yyyy-MM-dd"),
                    cantidad = d.TotalDesperdiciado
                })
            };

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();
            base.Dispose(disposing);
        }
    }
}
