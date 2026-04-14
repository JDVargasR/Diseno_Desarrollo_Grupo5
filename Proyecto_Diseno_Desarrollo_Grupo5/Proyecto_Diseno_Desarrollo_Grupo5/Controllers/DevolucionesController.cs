using Proyecto_Diseno_Desarrollo_Grupo5.EF;
using Proyecto_Diseno_Desarrollo_Grupo5.EF;
using Proyecto_Diseno_Desarrollo_Grupo5.Filters;
using Proyecto_Diseno_Desarrollo_Grupo5.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;

namespace Proyecto_Diseno_Desarrollo_Grupo5.Controllers
{
    [RolAuthorize(1, 2)]
    public class DevolucionesController : Controller
    {
        private readonly DBGRUPO5Entities db = new DBGRUPO5Entities();

        private const string EstadoPendiente = "PENDIENTE";
        private const string EstadoAprobada = "APROBADA";
        private const string EstadoRechazada = "RECHAZADA";
        private const string EstadoReintegrada = "REINTEGRADA";
        private const string EstadoPerdidaControlada = "PERDIDA_CONTROLADA";

        public ActionResult Index(string q = null, string estado = null, DateTime? desde = null, DateTime? hasta = null)
        {
            q = (q ?? "").Trim();
            estado = (estado ?? "").Trim().ToUpperInvariant();

            var devoluciones = db.DEVOLUCIONES
                .Include(x => x.VENTAS)
                .Include(x => x.VENTAS.CLIENTES)
                .OrderByDescending(x => x.FECHA)
                .ToList();

            var items = MapearDevoluciones(devoluciones);

            if (!string.IsNullOrWhiteSpace(q))
            {
                items = items.Where(x =>
                    x.IdDevolucion.ToString().Contains(q)
                    || x.IdVenta.ToString().Contains(q)
                    || (x.Producto ?? "").IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0
                    || (x.Motivo ?? "").IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0
                ).ToList();
            }

            if (!string.IsNullOrWhiteSpace(estado))
                items = items.Where(x => string.Equals(x.Estado, estado, StringComparison.OrdinalIgnoreCase)).ToList();

            if (desde.HasValue)
                items = items.Where(x => x.Fecha >= desde.Value.Date).ToList();

            if (hasta.HasValue)
            {
                var h = hasta.Value.Date.AddDays(1);
                items = items.Where(x => x.Fecha < h).ToList();
            }

            ViewBag.Q = q;
            ViewBag.Estado = estado;
            ViewBag.Desde = desde?.ToString("yyyy-MM-dd");
            ViewBag.Hasta = hasta?.ToString("yyyy-MM-dd");
            ViewBag.Estados = new[]
            {
                EstadoPendiente,
                EstadoAprobada,
                EstadoRechazada,
                EstadoReintegrada,
                EstadoPerdidaControlada
            };

            ViewBag.EsGerente = GetCurrentRoleId() == 1;
            return View(items);
        }

        [HttpGet]
        public ActionResult Create()
        {
            var vm = new DevolucionCrearVM();
            CargarListas(vm);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(DevolucionCrearVM vm)
        {
            if (vm == null || !ModelState.IsValid)
            {
                TempData["ERR"] = "Revisá los datos de la devolución.";
                return RedirectToAction("Create");
            }

            var venta = db.VENTAS
                .Include(v => v.DETALLES_VENTAS)
                .FirstOrDefault(v => v.ID_VENTA == vm.IdVenta);

            if (venta == null)
            {
                TempData["ERR"] = "La factura indicada no existe.";
                return RedirectToAction("Create");
            }

            var detalle = venta.DETALLES_VENTAS.FirstOrDefault(d => d.ID_PRODUCTO == vm.IdProducto);
            if (detalle == null)
            {
                TempData["ERR"] = "No se puede devolver ese producto porque no está en la factura seleccionada.";
                return RedirectToAction("Create");
            }

            if (vm.Cantidad > detalle.CANTIDAD)
            {
                TempData["ERR"] = "La cantidad devuelta no puede superar lo vendido.";
                return RedirectToAction("Create");
            }

            var ahora = DateTime.Now;
            var limitePolitica = venta.FECHA.AddDays(vm.PoliticaDias);
            var garantia = db.GARANTIAS.FirstOrDefault(g => g.ID_PRODUCTO == vm.IdProducto);
            var dentroPolitica = ahora <= limitePolitica;
            var dentroGarantia = garantia != null && ahora <= venta.FECHA.AddMonths(garantia.DURACION_MESES);

            if (!dentroPolitica && !dentroGarantia)
            {
                TempData["ERR"] = "No es posible aplicar la devolución: el plazo de devolución/garantía ya expiró.";
                return RedirectToAction("Create");
            }

            var idUsuario = GetCurrentUserId();
            var metadata = new DevolucionMetadata
            {
                Estado = EstadoPendiente,
                IdProducto = vm.IdProducto,
                Cantidad = vm.Cantidad,
                CondicionProducto = "PENDIENTE",
                IdUsuarioRegistro = idUsuario,
                MotivoCliente = (vm.Motivo ?? "").Trim()
            };

            var entidad = new DEVOLUCIONES
            {
                ID_VENTA = vm.IdVenta,
                FECHA = DateTime.Now,
                MOTIVO = SerializarMetadata(metadata)
            };

            db.DEVOLUCIONES.Add(entidad);
            db.SaveChanges();

            BitacoraHelper.Registrar(idUsuario, "CREAR", $"Registro de devolución #{entidad.ID_DEVOLUCION}.", "DEVOLUCIONES", entidad.ID_DEVOLUCION.ToString(), "OK");

            TempData["OK"] = $"Devolución #{entidad.ID_DEVOLUCION} registrada en estado pendiente.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RolAuthorize(1)]
        public ActionResult Resolver(int id, string decision, string observacion)
        {
            var devolucion = db.DEVOLUCIONES.FirstOrDefault(x => x.ID_DEVOLUCION == id);
            if (devolucion == null)
            {
                TempData["ERR"] = "La devolución no existe.";
                return RedirectToAction("Index");
            }

            var metadata = ParsearMetadata(devolucion.MOTIVO);
            if (!string.Equals(metadata.Estado, EstadoPendiente, StringComparison.OrdinalIgnoreCase))
            {
                TempData["ERR"] = "Solo devoluciones en estado pendiente pueden aprobarse o rechazarse.";
                return RedirectToAction("Index");
            }

            var isAprobada = string.Equals((decision ?? "").Trim(), "APROBAR", StringComparison.OrdinalIgnoreCase);
            metadata.Estado = isAprobada ? EstadoAprobada : EstadoRechazada;
            metadata.IdUsuarioDecision = GetCurrentUserId();
            metadata.FechaDecision = DateTime.Now;

            if (!string.IsNullOrWhiteSpace(observacion))
                metadata.MotivoCliente = (metadata.MotivoCliente + " | [REVISION] " + observacion.Trim()).Trim();

            devolucion.MOTIVO = SerializarMetadata(metadata);

            if (metadata.IdUsuarioRegistro.HasValue)
            {
                db.NOTIFICACIONES.Add(new NOTIFICACIONES
                {
                    ID_USUARIO = metadata.IdUsuarioRegistro,
                    TITULO = "Resultado de devolución",
                    MENSAJE = $"La devolución #{devolucion.ID_DEVOLUCION} fue {metadata.Estado}.",
                    FECHA = DateTime.Now,
                    LEIDA = false,
                    MODULO = "DEVOLUCIONES"
                });
            }

            db.SaveChanges();

            BitacoraHelper.Registrar(metadata.IdUsuarioDecision, "EDITAR", $"Resolución de devolución #{devolucion.ID_DEVOLUCION}: {metadata.Estado}.", "DEVOLUCIONES", devolucion.ID_DEVOLUCION.ToString(), "OK");

            TempData["OK"] = "Resolución aplicada correctamente.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RolAuthorize(1)]
        public ActionResult ProcesarInventario(int id, string condicion)
        {
            condicion = (condicion ?? "").Trim().ToUpperInvariant();
            if (condicion != "BUENO" && condicion != "DANADO")
            {
                TempData["ERR"] = "Condición de producto inválida.";
                return RedirectToAction("Index");
            }

            using (var tx = db.Database.BeginTransaction())
            {
                try
                {
                    var devolucion = db.DEVOLUCIONES.FirstOrDefault(x => x.ID_DEVOLUCION == id);
                    if (devolucion == null)
                    {
                        TempData["ERR"] = "La devolución no existe.";
                        return RedirectToAction("Index");
                    }

                    var metadata = ParsearMetadata(devolucion.MOTIVO);
                    if (!string.Equals(metadata.Estado, EstadoAprobada, StringComparison.OrdinalIgnoreCase))
                    {
                        TempData["ERR"] = "Solo devoluciones aprobadas pueden actualizar inventario.";
                        return RedirectToAction("Index");
                    }

                    if (!metadata.IdProducto.HasValue || !metadata.Cantidad.HasValue || metadata.Cantidad.Value <= 0)
                    {
                        TempData["ERR"] = "La devolución no tiene producto/cantidad válida para inventario.";
                        return RedirectToAction("Index");
                    }

                    var receta = db.PRODUCTO_MATERIAL.Where(x => x.ID_PRODUCTO == metadata.IdProducto.Value).ToList();
                    var idUsuario = GetCurrentUserId();

                    var idMaterialRelacionado = receta.Select(x => (int?)x.ID_MATERIAL).FirstOrDefault();

                    foreach (var r in receta)
                    {
                        var material = db.MATERIALES.Find(r.ID_MATERIAL);
                        if (material == null) continue;

                        var cantidadMovimiento = metadata.Cantidad.Value * r.CANTIDAD_USADA;
                        if (condicion == "BUENO")
                            material.STOCK += cantidadMovimiento;

                        db.MOVIMIENTOS_INVENTARIO.Add(new MOVIMIENTOS_INVENTARIO
                        {
                            ID_MATERIAL = material.ID_MATERIAL,
                            TIPO_MOVIMIENTO = condicion == "BUENO" ? "DEVOLUCION_REINTEGRO" : "PERDIDA_CONTROLADA_DEVOLUCION",
                            CANTIDAD = cantidadMovimiento,
                            FECHA = DateTime.Now,
                            OBSERVACION = $"Devolución #{devolucion.ID_DEVOLUCION} - Producto #{metadata.IdProducto.Value}"
                        });
                    }

                    metadata.CondicionProducto = condicion;
                    metadata.Estado = condicion == "BUENO" ? EstadoReintegrada : EstadoPerdidaControlada;
                    metadata.IdMaterial = idMaterialRelacionado;
                    devolucion.MOTIVO = SerializarMetadata(metadata);

                    db.SaveChanges();
                    tx.Commit();

                    BitacoraHelper.Registrar(idUsuario, "EDITAR", $"Actualización de inventario de devolución #{devolucion.ID_DEVOLUCION}. Estado: {metadata.Estado}", "DEVOLUCIONES", devolucion.ID_DEVOLUCION.ToString(), "OK");

                    TempData["OK"] = "Inventario actualizado correctamente.";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    tx.Rollback();
                    TempData["ERR"] = ex.Message;
                    return RedirectToAction("Index");
                }
            }
        }

        [HttpGet]
        [RolAuthorize(1)]
        public ActionResult Reporte(int? anio = null, int? mes = null, int? idCategoria = null, int? idProveedor = null, string motivo = null)
        {
            var year = anio ?? DateTime.Today.Year;
            var month = mes ?? DateTime.Today.Month;
            motivo = (motivo ?? "").Trim();

            var devoluciones = db.DEVOLUCIONES
                .Include(x => x.VENTAS)
                .Include(x => x.VENTAS.DETALLES_VENTAS)
                .Where(x => x.FECHA.Year == year && x.FECHA.Month == month)
                .ToList();

            var metas = devoluciones.Select(d => new { Dev = d, Meta = ParsearMetadata(d.MOTIVO) }).ToList();

            var productoIds = metas.Where(x => x.Meta.IdProducto.HasValue).Select(x => x.Meta.IdProducto.Value).Distinct().ToList();

            var productos = db.PRODUCTOS
                .Include(x => x.CATEGORIAS)
                .Where(x => productoIds.Contains(x.ID_PRODUCTO))
                .ToList()
                .ToDictionary(x => x.ID_PRODUCTO, x => x);

            var materialProveedorPorProducto = (from pm in db.PRODUCTO_MATERIAL
                                                join m in db.MATERIALES on pm.ID_MATERIAL equals m.ID_MATERIAL
                                                join p in db.PROVEEDORES on m.ID_PROVEEDOR equals p.ID_PROVEEDOR
                                                where productoIds.Contains(pm.ID_PRODUCTO)
                                                select new
                                                {
                                                    pm.ID_PRODUCTO,
                                                    m.ID_MATERIAL,
                                                    m.ID_PROVEEDOR,
                                                    PROVEEDOR = p.NOMBRE
                                                })
                                                .ToList();

            var proveedorFallbackPorProducto = materialProveedorPorProducto
                .GroupBy(x => x.ID_PRODUCTO)
                .ToDictionary(x => x.Key, x => x.First());

            var proveedorPorMaterial = materialProveedorPorProducto
                .GroupBy(x => x.ID_MATERIAL)
                .ToDictionary(x => x.Key, x => x.First());

            var filas = new List<DevolucionReporteFilaVM>();

            foreach (var item in metas)
            {
                if (!item.Meta.IdProducto.HasValue || !item.Meta.Cantidad.HasValue) continue;

                if (!productos.ContainsKey(item.Meta.IdProducto.Value)) continue;
                var producto = productos[item.Meta.IdProducto.Value];

                if (idCategoria.HasValue && producto.ID_CATEGORIA != idCategoria.Value) continue;

                string proveedor = "N/D";
                int? proveedorId = null;

                if (item.Meta.IdMaterial.HasValue && proveedorPorMaterial.ContainsKey(item.Meta.IdMaterial.Value))
                {
                    var porMaterial = proveedorPorMaterial[item.Meta.IdMaterial.Value];
                    proveedor = porMaterial.PROVEEDOR;
                    proveedorId = porMaterial.ID_PROVEEDOR;
                }
                else if (proveedorFallbackPorProducto.ContainsKey(producto.ID_PRODUCTO))
                {
                    var fallback = proveedorFallbackPorProducto[producto.ID_PRODUCTO];
                    proveedor = fallback.PROVEEDOR;
                    proveedorId = fallback.ID_PROVEEDOR;
                }

                if (idProveedor.HasValue)
                {
                    var contieneProveedor = proveedorId.HasValue && proveedorId.Value == idProveedor.Value;
                    if (!contieneProveedor) continue;
                }

                if (!string.IsNullOrWhiteSpace(motivo)
                    && (item.Meta.MotivoCliente ?? "").IndexOf(motivo, StringComparison.OrdinalIgnoreCase) < 0)
                    continue;

                var precio = item.Dev.VENTAS?.DETALLES_VENTAS
                    .Where(d => d.ID_PRODUCTO == producto.ID_PRODUCTO)
                    .Select(d => (decimal?)d.PRECIO_UNITARIO)
                    .FirstOrDefault() ?? 0;

                filas.Add(new DevolucionReporteFilaVM
                {
                    Producto = producto.NOMBRE,
                    Categoria = producto.CATEGORIAS?.NOMBRE,
                    Proveedor = proveedor,
                    Motivo = item.Meta.MotivoCliente,
                    CantidadDevuelta = item.Meta.Cantidad.Value,
                    Frecuencia = 1,
                    ValorMonetario = item.Meta.Cantidad.Value * precio
                });
            }

            var agrupado = filas
                .GroupBy(x => new { x.Producto, x.Categoria, x.Proveedor, x.Motivo })
                .Select(g => new DevolucionReporteFilaVM
                {
                    Producto = g.Key.Producto,
                    Categoria = g.Key.Categoria,
                    Proveedor = g.Key.Proveedor,
                    Motivo = g.Key.Motivo,
                    CantidadDevuelta = g.Sum(x => x.CantidadDevuelta),
                    Frecuencia = g.Count(),
                    ValorMonetario = g.Sum(x => x.ValorMonetario)
                })
                .OrderByDescending(x => x.Frecuencia)
                .ToList();

            ViewBag.Anio = year;
            ViewBag.Mes = month;
            ViewBag.IdCategoria = idCategoria;
            ViewBag.IdProveedor = idProveedor;
            ViewBag.Motivo = motivo;
            ViewBag.Categorias = db.CATEGORIAS.OrderBy(x => x.NOMBRE).ToList();
            ViewBag.Proveedores = db.PROVEEDORES.OrderBy(x => x.NOMBRE).ToList();
            ViewBag.TotalFrecuencia = agrupado.Sum(x => x.Frecuencia);
            ViewBag.TotalValor = agrupado.Sum(x => x.ValorMonetario);

            return View(agrupado);
        }

        private List<DevolucionFilaVM> MapearDevoluciones(List<DEVOLUCIONES> devoluciones)
        {
            var metas = devoluciones.Select(d => new { Dev = d, Meta = ParsearMetadata(d.MOTIVO) }).ToList();

            var productosIds = metas
                .Where(x => x.Meta.IdProducto.HasValue)
                .Select(x => x.Meta.IdProducto.Value)
                .Distinct()
                .ToList();

            var usuariosIds = metas
                .SelectMany(x => new[] { x.Meta.IdUsuarioRegistro, x.Meta.IdUsuarioDecision })
                .Where(x => x.HasValue)
                .Select(x => x.Value)
                .Distinct()
                .ToList();

            var productos = db.PRODUCTOS
                .Where(x => productosIds.Contains(x.ID_PRODUCTO))
                .ToList()
                .ToDictionary(x => x.ID_PRODUCTO, x => x.NOMBRE);

            var usuarios = db.USUARIOS
                .Where(x => usuariosIds.Contains(x.ID_USUARIO))
                .ToList()
                .ToDictionary(x => x.ID_USUARIO, x => x.NOMBRE);

            return metas.Select(x => new DevolucionFilaVM
            {
                IdDevolucion = x.Dev.ID_DEVOLUCION,
                IdVenta = x.Dev.ID_VENTA,
                Fecha = x.Dev.FECHA,
                Estado = x.Meta.Estado,
                Motivo = x.Meta.MotivoCliente,
                IdProducto = x.Meta.IdProducto,
                Producto = x.Meta.IdProducto.HasValue && productos.ContainsKey(x.Meta.IdProducto.Value) ? productos[x.Meta.IdProducto.Value] : "N/D",
                Cantidad = x.Meta.Cantidad,
                CondicionProducto = x.Meta.CondicionProducto,
                FechaDecision = x.Meta.FechaDecision,
                UsuarioRegistro = x.Meta.IdUsuarioRegistro.HasValue && usuarios.ContainsKey(x.Meta.IdUsuarioRegistro.Value)
                    ? usuarios[x.Meta.IdUsuarioRegistro.Value]
                    : "N/D",
                UsuarioDecision = x.Meta.IdUsuarioDecision.HasValue && usuarios.ContainsKey(x.Meta.IdUsuarioDecision.Value)
                    ? usuarios[x.Meta.IdUsuarioDecision.Value]
                    : "-"
            }).ToList();
        }

        private void CargarListas(DevolucionCrearVM vm)
        {
            vm.Ventas = db.VENTAS
                .OrderByDescending(x => x.ID_VENTA)
                .Take(300)
                .ToList()
                .Select(x => new SelectListItem
                {
                    Value = x.ID_VENTA.ToString(),
                    Text = $"#{x.ID_VENTA} - {x.FECHA:dd/MM/yyyy}"
                })
                .ToList();

            vm.Productos = db.PRODUCTOS
                .Where(x => x.ID_ESTADO == 1)
                .OrderBy(x => x.NOMBRE)
                .ToList()
                .Select(x => new SelectListItem
                {
                    Value = x.ID_PRODUCTO.ToString(),
                    Text = x.NOMBRE
                })
                .ToList();
        }

        private int? GetCurrentUserId()
        {
            if (Session["IdUsuario"] == null) return null;
            int parsed;
            return int.TryParse(Session["IdUsuario"].ToString(), out parsed) ? parsed : (int?)null;
        }

        private int? GetCurrentRoleId()
        {
            if (Session["IdRol"] == null) return null;
            int parsed;
            return int.TryParse(Session["IdRol"].ToString(), out parsed) ? parsed : (int?)null;
        }

        private string SerializarMetadata(DevolucionMetadata meta)
        {
            return string.Join(";", new[]
            {
                "EST=" + Limpia(meta.Estado),
                "PROD=" + (meta.IdProducto.HasValue ? meta.IdProducto.Value.ToString() : ""),
                "MAT=" + (meta.IdMaterial.HasValue ? meta.IdMaterial.Value.ToString() : ""),
                "CANT=" + (meta.Cantidad.HasValue ? meta.Cantidad.Value.ToString(CultureInfo.InvariantCulture) : ""),
                "COND=" + Limpia(meta.CondicionProducto),
                "USRREG=" + (meta.IdUsuarioRegistro.HasValue ? meta.IdUsuarioRegistro.Value.ToString() : ""),
                "USRDEC=" + (meta.IdUsuarioDecision.HasValue ? meta.IdUsuarioDecision.Value.ToString() : ""),
                "FDEC=" + (meta.FechaDecision.HasValue ? meta.FechaDecision.Value.ToString("O") : ""),
                "MOT=" + Limpia(meta.MotivoCliente)
            });
        }

        private DevolucionMetadata ParsearMetadata(string motivoRaw)
        {
            var meta = new DevolucionMetadata
            {
                Estado = EstadoPendiente,
                MotivoCliente = (motivoRaw ?? "").Trim(),
                CondicionProducto = "PENDIENTE"
            };

            var raw = (motivoRaw ?? "").Trim();
            if (!raw.Contains("EST=") || !raw.Contains("MOT=")) return meta;

            var partes = raw.Split(new[] { ';' }, StringSplitOptions.None);
            var mapa = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var p in partes)
            {
                var idx = p.IndexOf('=');
                if (idx <= 0) continue;

                var key = p.Substring(0, idx).Trim();
                var value = p.Substring(idx + 1).Trim();
                mapa[key] = value;
            }

            int i;
            decimal d;
            DateTime f;

            if (mapa.ContainsKey("EST")) meta.Estado = mapa["EST"];
            if (mapa.ContainsKey("PROD") && int.TryParse(mapa["PROD"], out i)) meta.IdProducto = i;
            if (mapa.ContainsKey("MAT") && int.TryParse(mapa["MAT"], out i)) meta.IdMaterial = i;
            if (mapa.ContainsKey("CANT") && decimal.TryParse(mapa["CANT"], NumberStyles.Any, CultureInfo.InvariantCulture, out d)) meta.Cantidad = d;
            if (mapa.ContainsKey("COND")) meta.CondicionProducto = mapa["COND"];
            if (mapa.ContainsKey("USRREG") && int.TryParse(mapa["USRREG"], out i)) meta.IdUsuarioRegistro = i;
            if (mapa.ContainsKey("USRDEC") && int.TryParse(mapa["USRDEC"], out i)) meta.IdUsuarioDecision = i;
            if (mapa.ContainsKey("FDEC") && DateTime.TryParse(mapa["FDEC"], null, DateTimeStyles.RoundtripKind, out f)) meta.FechaDecision = f;
            if (mapa.ContainsKey("MOT")) meta.MotivoCliente = mapa["MOT"];

            return meta;
        }

        private string Limpia(string valor)
        {
            return (valor ?? "").Replace(";", ",").Replace("=", ":").Trim();
        }

        private class DevolucionMetadata
        {
            public string Estado { get; set; }
            public int? IdProducto { get; set; }
            public int? IdMaterial { get; set; }
            public decimal? Cantidad { get; set; }
            public string CondicionProducto { get; set; }
            public int? IdUsuarioRegistro { get; set; }
            public int? IdUsuarioDecision { get; set; }
            public DateTime? FechaDecision { get; set; }
            public string MotivoCliente { get; set; }
        }
    }
}
