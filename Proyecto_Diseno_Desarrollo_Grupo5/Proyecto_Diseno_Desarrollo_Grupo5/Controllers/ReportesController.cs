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
                .Where(d => d.FECHA >= fechaInicio && d.FECHA <= fechaFinAjustada)
                .Include(d => d.MATERIALES)
                .Include(d => d.PRODUCTOS)
                .ToList();

            if (desperdicios.Count == 0)
                return vm;

            // Totales generales
            vm.TotalDesperdiciado = desperdicios.Sum(d => d.CANTIDAD_DESPERDICIADA);
            vm.TotalTransacciones = desperdicios.Count;
            vm.PromedioTransaccion = vm.TotalTransacciones > 0 ? vm.TotalDesperdiciado / vm.TotalTransacciones : 0;

            // ===== CAMBIO PRINCIPAL: Análisis por MATERIALES en lugar de PRODUCTOS =====
            // Desperdicios por Material (agrupado)
            vm.DesperdiciosPorMaterial = desperdicios
                .Where(d => d.MATERIALES != null)
                .GroupBy(d => new { d.ID_MATERIAL, d.MATERIALES.NOMBRE, d.MATERIALES.TIPO })
                .Select(g => new DesperdicioMaterialVM
                {
                    IdMaterial = g.Key.ID_MATERIAL,
                    NombreMaterial = g.Key.NOMBRE,
                    TotalDesperdiciado = g.Sum(d => d.CANTIDAD_DESPERDICIADA),
                    Unidad = g.Key.TIPO,
                    CantidadTransacciones = g.Count(),
                    PorcentajeDesperdicio = vm.TotalDesperdiciado > 0 
                        ? (g.Sum(d => d.CANTIDAD_DESPERDICIADA) / vm.TotalDesperdiciado * 100) 
                        : 0
                })
                .OrderByDescending(d => d.TotalDesperdiciado)
                .ToList();

            // Mantener por Producto para compatibilidad (pero secundario)
            vm.DesperdiciosPorProducto = desperdicios
                .Where(d => d.ID_PRODUCTO.HasValue && d.PRODUCTOS != null)
                .GroupBy(d => new { d.ID_PRODUCTO, d.PRODUCTOS.NOMBRE })
                .Select(g => new DesperdicioProductoVM
                {
                    IdProducto = g.Key.ID_PRODUCTO.Value,
                    NombreProducto = g.Key.NOMBRE,
                    TotalDesperdiciado = g.Sum(d => d.CANTIDAD_DESPERDICIADA),
                    PorcentajeDesperdicio = vm.TotalDesperdiciado > 0 
                        ? (g.Sum(d => d.CANTIDAD_DESPERDICIADA) / vm.TotalDesperdiciado * 100) 
                        : 0,
                    CantidadTransacciones = g.Count()
                })
                .OrderByDescending(d => d.TotalDesperdiciado)
                .ToList();

            // Desperdicios por origen de transaccion
            vm.DesperdiciosPorOrigen = desperdicios
                .GroupBy(d => string.IsNullOrWhiteSpace(d.ORIGEN) ? "SIN ORIGEN" : d.ORIGEN.Trim())
                .Select(g => new DesperdicioOrigenVM
                {
                    Origen = g.Key,
                    TotalDesperdiciado = g.Sum(d => d.CANTIDAD_DESPERDICIADA),
                    CantidadTransacciones = g.Count()
                })
                .OrderByDescending(o => o.TotalDesperdiciado)
                .ToList();

            // Desperdicios por Día
            var resumenPorDia = desperdicios
                .GroupBy(d => d.FECHA.Date)
                .Select(g => new DesperdicioDiaVM
                {
                    Fecha = g.Key,
                    TotalDesperdiciado = g.Sum(d => d.CANTIDAD_DESPERDICIADA),
                    CantidadTransacciones = g.Count()
                })
                .ToDictionary(d => d.Fecha, d => d);

            vm.DesperdiciosPorDia = new System.Collections.Generic.List<DesperdicioDiaVM>();
            var fechaCursor = fechaInicio.Date;
            var fechaLimite = fechaFin.Date;

            while (fechaCursor <= fechaLimite)
            {
                if (resumenPorDia.ContainsKey(fechaCursor))
                {
                    vm.DesperdiciosPorDia.Add(resumenPorDia[fechaCursor]);
                }
                else
                {
                    vm.DesperdiciosPorDia.Add(new DesperdicioDiaVM
                    {
                        Fecha = fechaCursor,
                        TotalDesperdiciado = 0,
                        CantidadTransacciones = 0
                    });
                }

                fechaCursor = fechaCursor.AddDays(1);
            }

            // ===== MATERIALES CON ALTO DESPERDICIO (basado en % de desperdicios por material) =====
            var desperdiciosPorMaterialTotal = desperdicios
                .GroupBy(d => d.ID_MATERIAL)
                .ToDictionary(
                    g => g.Key, 
                    g => new 
                    { 
                        Total = g.Sum(x => x.CANTIDAD_DESPERDICIADA),
                        Transacciones = g.Count()
                    });

            vm.MaterialesAltoDesperdicio = db.MATERIALES
                .Include(m => m.ESTADO)
                .Where(m => m.ID_ESTADO == 1)
                .ToList()
                .Select(m => 
                {
                    var desperdicioMaterial = desperdiciosPorMaterialTotal.ContainsKey(m.ID_MATERIAL)
                        ? desperdiciosPorMaterialTotal[m.ID_MATERIAL]
                        : null;

                    var porcentajeDesperdicio = desperdicioMaterial != null && vm.TotalDesperdiciado > 0
                        ? (desperdicioMaterial.Total / vm.TotalDesperdiciado * 100)
                        : 0m;

                    return new MaterialAltoDesperdicioVM
                    {
                        IdMaterial = m.ID_MATERIAL,
                        NombreMaterial = m.NOMBRE,
                        PorcentajeDesperdicio = (decimal)porcentajeDesperdicio,
                        TotalDesperdiciado = desperdicioMaterial?.Total ?? 0,
                        CantidadTransacciones = desperdicioMaterial?.Transacciones ?? 0,
                        Recomendacion = porcentajeDesperdicio > 15
                            ? "⚠ CRÍTICO: Alto desperdicio. Revisar manejo/almacenamiento del material inmediatamente."
                            : porcentajeDesperdicio > 5
                            ? "⚠ Monitorear: Desperdicio moderado. Mejorar control de procesos."
                            : "✓ Aceptable: Porcentaje de desperdicio dentro de parámetros normales."
                    };
                })
                .Where(m => m.TotalDesperdiciado > 0 || m.PorcentajeDesperdicio > 5)
                .OrderByDescending(m => m.PorcentajeDesperdicio)
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
                .Where(d => d.FECHA >= fechaInicio && d.FECHA <= fechaFinAjustada)
                .Include(d => d.MATERIALES)
                .Include(d => d.PRODUCTOS)
                .Include(d => d.PRODUCTOS.CATEGORIAS)
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
                    Unidad = d.PRODUCTOS?.CATEGORIAS?.NOMBRE ?? d.MATERIALES?.TIPO ?? "",
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
                materialesAltoDesperdicio = vm.MaterialesAltoDesperdicio.Select(m => new
                {
                    nombreMaterial = m.NombreMaterial,
                    porcentajeDesperdicio = Math.Round(m.PorcentajeDesperdicio, 2),
                    totalDesperdiciado = m.TotalDesperdiciado,
                    recomendacion = m.Recomendacion
                }),
                porProducto = vm.DesperdiciosPorProducto.Select(p => new
                {
                    NombreProducto = p.NombreProducto,
                    nombreProducto = p.NombreProducto,
                    TotalDesperdiciado = p.TotalDesperdiciado,
                    totalDesperdiciado = p.TotalDesperdiciado,
                    PorcentajeDesperdicio = Math.Round(p.PorcentajeDesperdicio, 2),
                    porcentajeDesperdicio = Math.Round(p.PorcentajeDesperdicio, 2)
                }),
                porMaterial = vm.DesperdiciosPorMaterial.Select(m => new
                {
                    NombreMaterial = m.NombreMaterial,
                    nombreMaterial = m.NombreMaterial,
                    TotalDesperdiciado = m.TotalDesperdiciado,
                    totalDesperdiciado = m.TotalDesperdiciado,
                    Unidad = m.Unidad,
                    unidad = m.Unidad,
                    PorcentajeDesperdicio = Math.Round(m.PorcentajeDesperdicio, 2),
                    porcentajeDesperdicio = Math.Round(m.PorcentajeDesperdicio, 2)
                }),
                porOrigen = vm.DesperdiciosPorOrigen.Select(o => new
                {
                    Origen = o.Origen,
                    origen = o.Origen,
                    TotalDesperdiciado = o.TotalDesperdiciado,
                    totalDesperdiciado = o.TotalDesperdiciado,
                    CantidadTransacciones = o.CantidadTransacciones,
                    cantidadTransacciones = o.CantidadTransacciones
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
