using Proyecto_Diseno_Desarrollo_Grupo5.EF;
using Proyecto_Diseno_Desarrollo_Grupo5.Helpers;
using Proyecto_Diseno_Desarrollo_Grupo5.Models;
using System;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web.Mvc;
using System.Web.UI;
using System.IO;
using System.Web;
using System.Globalization;
using Proyecto_Diseno_Desarrollo_Grupo5.Filters;

namespace Proyecto_Diseno_Desarrollo_Grupo5.Controllers
{
    [RolAuthorize(1,2)]
    public class VentasController : Controller
    {
        private DBGRUPO5Entities db = new DBGRUPO5Entities();

        public ActionResult Index(string q = null, int page = 1, int pageSize = 10)
        {
            q = (q ?? "").Trim();

            var ventas = db.VENTAS.Include(v => v.CLIENTES).AsQueryable();

            int idFiltro = 0;
            bool esNumero = int.TryParse(q, out idFiltro);

            if (!string.IsNullOrWhiteSpace(q) && esNumero)
            {
                ventas = ventas.Where(v => v.ID_VENTA == idFiltro || v.ID_CLIENTE == idFiltro);
            }


            var pagosPorVenta = db.PAGOS
                .GroupBy(p => p.ID_VENTA)
                .Select(g => new
                {
                    IdVenta = g.Key,
                    Pagado = g.Sum(x => x.MONTO)
                });

            var lista = (from v in ventas
                         join p in pagosPorVenta
                            on v.ID_VENTA equals p.IdVenta into pagosJoin
                         from p in pagosJoin.DefaultIfEmpty()
                         orderby v.ID_VENTA descending
                         select new VentaFilaVM
                         {
                             IdVenta = v.ID_VENTA,
                             Cliente = v.CLIENTES.NOMBRE,
                             Fecha = v.FECHA,
                             Total = v.TOTAL,
                             Pagado = (p == null ? 0 : p.Pagado),
                             Saldo = v.TOTAL - (p == null ? 0 : p.Pagado),
                             IdEstado = v.ID_ESTADO,
                             Estado = v.ESTADO.NOMBRE
                         }).ToList();

            if (!string.IsNullOrWhiteSpace(q) && !esNumero)
            {
                lista = lista.Where(v => TextHelper.Contiene(v.Cliente, q)).ToList();
            }

            // Paginación server-side
            var total = lista.Count;
            var skip = (Math.Max(page, 1) - 1) * pageSize;
            var paged = lista.Skip(skip).Take(pageSize).ToList();

            ViewBag.Q = q;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = total;
            ViewBag.TotalPages = pageSize <= 0 ? 0 : (int)Math.Ceiling((double)total / pageSize);

            return View(paged);
        }

        [HttpGet]
        public ActionResult Create()
        {
            var productosActivos = db.PRODUCTOS
                .Where(p => p.ID_ESTADO == 1)
                .OrderBy(p => p.NOMBRE)
                .ToList();

            var materialesActivos = db.MATERIALES
                .Where(m => m.ID_ESTADO == 1)
                .OrderBy(m => m.NOMBRE)
                .ToList();

            var vm = new VentaCrearVM
            {
                Productos = productosActivos
                    .Select(p => new SelectListItem
                    {
                        Value = p.ID_PRODUCTO.ToString(),
                        Text = p.NOMBRE
                    })
                    .ToList(),

                Materiales = materialesActivos
                    .Select(m => new SelectListItem
                    {
                        Value = m.ID_MATERIAL.ToString(),
                        Text = m.NOMBRE
                    })
                    .ToList(),

                ProductosConPrecio = productosActivos
                    .Select(p => new ProductoVentaVM
                    {
                        IdProducto = p.ID_PRODUCTO,
                        Nombre = p.NOMBRE,
                        PrecioVenta = p.PRECIO_VENTA
                    })
                    .ToList(),

                MaterialesConPrecio = materialesActivos
                    .Select(m => new MaterialVentaVM
                    {
                        IdMaterial = m.ID_MATERIAL,
                        Nombre = m.NOMBRE,
                        PrecioVenta = m.COSTO_UNITARIO
                    })
                    .ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(int? IdCliente, string[] TipoItem, int?[] IdProducto, int?[] IdMaterial, decimal[] Cantidad, decimal[] PrecioUnitario)
        {
            if (!IdCliente.HasValue || IdCliente.Value <= 0 || TipoItem == null || TipoItem.Length == 0)
            {
                TempData["ERR"] = "Debés seleccionar cliente y agregar al menos un producto o material.";
                return RedirectToAction("Create");
            }

            if (Cantidad == null || PrecioUnitario == null || IdProducto == null || IdMaterial == null
                || Cantidad.Length != TipoItem.Length || PrecioUnitario.Length != TipoItem.Length
                || IdProducto.Length != TipoItem.Length || IdMaterial.Length != TipoItem.Length)
            {
                TempData["ERR"] = "Detalle de ítems inválido. Volvé a agregar los productos o materiales.";
                return RedirectToAction("Create");
            }

            using (var tx = db.Database.BeginTransaction())
            {
                try
                {
                    var venta = new VENTAS
                    {
                        ID_CLIENTE = IdCliente.Value,
                        FECHA = DateTime.Now,
                        TOTAL = 0,
                        ID_ESTADO = 1
                    };

                    db.VENTAS.Add(venta);
                    db.SaveChanges();

                    decimal total = 0;
                    int? idUsuario = null;
                    if (Session["IdUsuario"] != null)
                        idUsuario = Convert.ToInt32(Session["IdUsuario"]);

                    for (int i = 0; i < TipoItem.Length; i++)
                    {
                        var tipo = (TipoItem[i] ?? "").Trim().ToUpperInvariant();
                        var cant = Cantidad[i];
                        var precio = PrecioUnitario[i];

                        if (cant <= 0) continue;
                        if (precio < 0) precio = 0;

                        var subtotal = cant * precio;

                        if (tipo == "MATERIAL")
                        {
                            var idMat = IdMaterial[i];
                            if (!idMat.HasValue || idMat.Value <= 0)
                                throw new Exception("Ítem de tipo material inválido.");

                            var material = db.MATERIALES.Find(idMat.Value);
                            if (material == null)
                                throw new Exception("Material no encontrado.");

                            if (material.STOCK < cant)
                                throw new Exception($"Stock insuficiente en material: {material.NOMBRE}. Disponible: {material.STOCK}, requerido: {cant}");

                            material.STOCK -= cant;

                            db.DETALLES_VENTAS.Add(new DETALLES_VENTAS
                            {
                                ID_VENTA = venta.ID_VENTA,
                                ID_MATERIAL = idMat.Value,
                                CANTIDAD = cant,
                                PRECIO_UNITARIO = precio,
                                SUBTOTAL = subtotal,
                                TIPO_ITEM = "MATERIAL"
                            });

                            // Registrar desperdicio usando el porcentaje configurado en el material.
                            if (material.PORC_DESPERDICIO > 0)
                            {
                                var cantidadDesperdiciada = cant * (material.PORC_DESPERDICIO / 100m);

                                if (cantidadDesperdiciada > 0)
                                {
                                    db.DESPERDICIOS_MATERIAL.Add(new DESPERDICIOS_MATERIAL
                                    {
                                        ID_MATERIAL = material.ID_MATERIAL,
                                        CANTIDAD_DESPERDICIADA = cantidadDesperdiciada,
                                        REUTILIZABLE = "N",
                                        CANTIDAD_REUTILIZADA = 0,
                                        MOTIVO = "Desperdicio generado por venta directa de material.",
                                        FECHA = DateTime.Now,
                                        ID_USUARIO = idUsuario,
                                        ID_VENTA = venta.ID_VENTA,
                                        ORIGEN = "VENTA"
                                    });
                                }
                            }
                        }
                        else
                        {
                            var idProd = IdProducto[i];
                            if (!idProd.HasValue || idProd.Value <= 0)
                                throw new Exception("Ítem de tipo producto inválido.");

                            var producto = db.PRODUCTOS.Find(idProd.Value);
                            if (producto == null)
                                throw new Exception("Producto no encontrado.");

                            if (producto.STOCK < cant)
                                throw new Exception($"Stock insuficiente en producto: {producto.NOMBRE}. Disponible: {producto.STOCK}, requerido: {cant}");

                            producto.STOCK -= cant;

                            db.DETALLES_VENTAS.Add(new DETALLES_VENTAS
                            {
                                ID_VENTA = venta.ID_VENTA,
                                ID_PRODUCTO = idProd.Value,
                                CANTIDAD = cant,
                                PRECIO_UNITARIO = precio,
                                SUBTOTAL = subtotal,
                                TIPO_ITEM = "PRODUCTO"
                            });
                        }

                        total += subtotal;
                    }

                    venta.TOTAL = total;
                    db.SaveChanges();

                    tx.Commit();
                    TempData["OK"] = $"Venta #{venta.ID_VENTA} registrada correctamente.";
                    return RedirectToAction("Index");
                }
                catch (DbEntityValidationException ex)
                {
                    tx.Rollback();

                    var errores = ex.EntityValidationErrors
                        .SelectMany(e => e.ValidationErrors)
                        .Select(e => $"{e.PropertyName}: {e.ErrorMessage}")
                        .ToList();

                    TempData["ERR"] = "Error de validación: " + string.Join(" | ", errores);
                    return RedirectToAction("Create");
                }
                catch (Exception ex)
                {
                    tx.Rollback();
                    TempData["ERR"] = ex.Message;
                    return RedirectToAction("Create");
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Pagar(PagoCrearVM vm, HttpPostedFileBase comprobanteSinpe)
        {
            // Leer el monto crudo del form para evitar problemas de cultura
            var montoRaw = (Request["Monto"] ?? "").Trim();

            decimal montoParseado;

            // Intentar con punto decimal
            bool okMonto =
                decimal.TryParse(montoRaw, NumberStyles.Any, CultureInfo.InvariantCulture, out montoParseado)
                || decimal.TryParse(montoRaw, NumberStyles.Any, CultureInfo.CurrentCulture, out montoParseado);

            if (vm == null)
            {
                TempData["ERR"] = "No se recibieron datos del pago.";
                return RedirectToAction("Index");
            }

            if (vm.IdVenta <= 0)
            {
                TempData["ERR"] = "La venta no es válida.";
                return RedirectToAction("Index");
            }

            if (!okMonto || montoParseado <= 0)
            {
                TempData["ERR"] = "El monto ingresado no es válido.";
                return RedirectToAction("Index");
            }

            if (string.IsNullOrWhiteSpace(vm.Metodo))
            {
                TempData["ERR"] = "Debe seleccionar un método de pago.";
                return RedirectToAction("Index");
            }

            var venta = db.VENTAS.Find(vm.IdVenta);
            if (venta == null)
            {
                TempData["ERR"] = "La venta no existe.";
                return RedirectToAction("Index");
            }

            if (venta.ID_ESTADO != 1)
            {
                TempData["ERR"] = "No se puede registrar pago a una venta cancelada.";
                return RedirectToAction("Index");
            }

            decimal totalPagado = db.PAGOS
                .Where(p => p.ID_VENTA == vm.IdVenta)
                .Select(p => (decimal?)p.MONTO)
                .Sum() ?? 0;

            decimal saldoActual = venta.TOTAL - totalPagado;

            if (saldoActual <= 0)
            {
                TempData["ERR"] = "Esta venta ya está completamente pagada.";
                return RedirectToAction("Index");
            }

            if (montoParseado > saldoActual)
            {
                TempData["ERR"] = $"El monto ingresado supera el saldo pendiente. Saldo actual: {saldoActual}";
                return RedirectToAction("Index");
            }

            string rutaImagen = null;
            var metodo = (vm.Metodo ?? "").Trim();

            if (metodo.Equals("Sinpe", StringComparison.OrdinalIgnoreCase))
            {
                if (comprobanteSinpe == null || comprobanteSinpe.ContentLength <= 0)
                {
                    TempData["ERR"] = "Debe adjuntar el comprobante de SINPE.";
                    return RedirectToAction("Index");
                }

                var extension = Path.GetExtension(comprobanteSinpe.FileName)?.ToLower();
                var extensionesValidas = new[] { ".jpg", ".jpeg", ".png", ".webp" };

                if (!extensionesValidas.Contains(extension))
                {
                    TempData["ERR"] = "El comprobante debe ser una imagen válida (.jpg, .jpeg, .png, .webp).";
                    return RedirectToAction("Index");
                }

                var nombreArchivo = $"sinpe_{vm.IdVenta}_{DateTime.Now:yyyyMMddHHmmss}{extension}";
                var carpetaFisica = Server.MapPath("~/Uploads/Sinpe");

                if (!Directory.Exists(carpetaFisica))
                    Directory.CreateDirectory(carpetaFisica);

                var rutaFisica = Path.Combine(carpetaFisica, nombreArchivo);
                comprobanteSinpe.SaveAs(rutaFisica);

                rutaImagen = "/Uploads/Sinpe/" + nombreArchivo;
            }

            int? idUsuario = null;
            if (Session["IdUsuario"] != null)
                idUsuario = Convert.ToInt32(Session["IdUsuario"]);

            db.PAGOS.Add(new PAGOS
            {
                ID_VENTA = vm.IdVenta,
                MONTO = montoParseado,
                METODO = metodo,
                REFERENCIA = string.IsNullOrWhiteSpace(rutaImagen)
                    ? (vm.Referencia ?? "").Trim()
                    : (((vm.Referencia ?? "").Trim()) + " | IMG:" + rutaImagen).Trim(' ', '|'),
                FECHA = DateTime.Now,
                ID_USUARIO = idUsuario
            });

            db.SaveChanges();

            TempData["OK"] = "Pago registrado correctamente.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public JsonResult ObtenerDesperdicios(int idVenta)
        {
            var desperdicios = db.DESPERDICIOS_MATERIAL
                .Where(d => d.ID_VENTA == idVenta)
                .Include(d => d.MATERIALES)
                .Include(d => d.PRODUCTOS)
                .Select(d => new
                {
                    idMaterial = d.ID_MATERIAL,
                    nombreMaterial = d.MATERIALES.NOMBRE,
                    cantidad = d.CANTIDAD_DESPERDICIADA,
                    unidad = d.MATERIALES.TIPO,
                    producto = d.PRODUCTOS == null ? null : d.PRODUCTOS.NOMBRE,
                    fecha = d.FECHA,
                    motivo = d.MOTIVO
                })
                .ToList();

            return Json(desperdicios, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult DiagnosticoDesperdicios()
        {
            var total = db.DESPERDICIOS_MATERIAL.Count();
            var ventaCount = db.DESPERDICIOS_MATERIAL.Where(d => d.ORIGEN == "VENTA").Count();
            var recientes = db.DESPERDICIOS_MATERIAL
                .OrderByDescending(d => d.FECHA)
                .Take(5)
                .Select(d => new
                {
                    id = d.ID_DESPERDICIO,
                    venta = d.ID_VENTA,
                    cantidad = d.CANTIDAD_DESPERDICIADA,
                    origen = d.ORIGEN,
                    fecha = d.FECHA
                })
                .ToList();

            return Json(new
            {
                totalDesperdicios = total,
                desperdiciodeVentas = ventaCount,
                recientes = recientes
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Desperdicios(int id)
        {
            var venta = db.VENTAS
                .Include(v => v.DETALLES_VENTAS)
                .FirstOrDefault(v => v.ID_VENTA == id);

            if (venta == null)
                return HttpNotFound();

            var desperdicios = db.DESPERDICIOS_MATERIAL
                .Where(d => d.ID_VENTA == id)
                .Include(d => d.MATERIALES)
                .Include(d => d.PRODUCTOS)
                .ToList();

            ViewBag.Venta = venta;
            ViewBag.TotalDesperdiciado = desperdicios.Sum(d => d.CANTIDAD_DESPERDICIADA);

            return View(desperdicios);
        }

        [HttpGet]
        public JsonResult BuscarClientes(string term)
        {
            term = (term ?? "").Trim().ToLower();

            var clientes = db.CLIENTES
                .Where(c =>
                    c.NOMBRE.ToLower().Contains(term) ||
                    (c.TELEFONO ?? "").ToLower().Contains(term) ||
                    (c.CORREO ?? "").ToLower().Contains(term)
                )
                .OrderBy(c => c.NOMBRE)
                .Take(10)
                .Select(c => new
                {
                    id = c.ID_CLIENTE,
                    nombre = c.NOMBRE,
                    telefono = c.TELEFONO,
                    correo = c.CORREO
                })
                .ToList();

            return Json(clientes, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Cancelar(int id)
        {
            using (var tx = db.Database.BeginTransaction())
            {
                try
                {
                    var venta = db.VENTAS
                        .Include(v => v.DETALLES_VENTAS)
                        .FirstOrDefault(v => v.ID_VENTA == id);

                    if (venta == null)
                    {
                        TempData["ERR"] = "Venta no encontrada.";
                        return RedirectToAction("Index");
                    }

                    if (venta.ID_ESTADO == 2)
                    {
                        TempData["ERR"] = "La venta ya está cancelada.";
                        return RedirectToAction("Index");
                    }

                    foreach (var det in venta.DETALLES_VENTAS.ToList())
                    {
                        var tipo = (det.TIPO_ITEM ?? "PRODUCTO").Trim().ToUpperInvariant();

                        if (tipo == "MATERIAL")
                        {
                            if (det.ID_MATERIAL.HasValue)
                            {
                                var materialVendido = db.MATERIALES.Find(det.ID_MATERIAL.Value);
                                if (materialVendido != null)
                                    materialVendido.STOCK += det.CANTIDAD;
                            }
                            continue;
                        }

                        // Compatibilidad con ventas antiguas: reintegrar stock del producto terminado
                        if (det.ID_PRODUCTO.HasValue)
                        {
                            var productoVendido = db.PRODUCTOS.Find(det.ID_PRODUCTO.Value);
                            if (productoVendido != null)
                                productoVendido.STOCK += det.CANTIDAD;
                        }
                    }

                    venta.ID_ESTADO = 2;
                    db.SaveChanges();

                    tx.Commit();
                    TempData["OK"] = $"Venta #{id} cancelada (inventario revertido).";
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
    }
}