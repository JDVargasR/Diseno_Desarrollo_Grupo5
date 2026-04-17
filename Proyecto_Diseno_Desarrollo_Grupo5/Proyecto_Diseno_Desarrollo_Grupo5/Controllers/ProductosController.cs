using Proyecto_Diseno_Desarrollo_Grupo5.EF;
using Proyecto_Diseno_Desarrollo_Grupo5.Filters;
using Proyecto_Diseno_Desarrollo_Grupo5.Models;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Globalization;

namespace Proyecto_Diseno_Desarrollo_Grupo5.Controllers
{
    public class ProductosController : Controller
    {
        private DBGRUPO5Entities db = new DBGRUPO5Entities();

        [RolAuthorize(1, 2)]
        public ActionResult Index(string q = null, int page = 1, int pageSize = 10)
        {
            ViewBag.EsSoloLectura = (Session["IdRol"] ?? "").ToString() == "2";

            q = (q ?? "").Trim();
            var query = db.PRODUCTOS.AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                query = query.Where(p => p.NOMBRE.Contains(q) || p.CATEGORIAS.NOMBRE.Contains(q));
            }

            query = query.OrderBy(p => p.NOMBRE);

            var total = query.Count();
            var skip = (Math.Max(page, 1) - 1) * pageSize;
            var items = query.Skip(skip).Take(pageSize).ToList();

            var vm = new ProductoCrudVM
            {
                Q = q,
                Page = page,
                PageSize = pageSize,
                TotalItems = total,
                Productos = items
                    .Select(p => new ProductoFilaVM
                    {
                        ID_PRODUCTO = p.ID_PRODUCTO,
                        NOMBRE = p.NOMBRE,
                        PRECIO_VENTA = p.PRECIO_VENTA,
                        ID_CATEGORIA = p.ID_CATEGORIA,
                        CATEGORIA = p.CATEGORIAS.NOMBRE,
                        ID_ESTADO = p.ID_ESTADO,
                        ESTADO = p.ESTADO.NOMBRE,
                        PORC_DESPERDICIO = p.PORC_DESPERDICIO
                    })
                    .ToList(),

                Categorias = db.CATEGORIAS
                    .OrderBy(c => c.NOMBRE)
                    .ToList()
                    .Select(c => new SelectListItem
                    {
                        Value = c.ID_CATEGORIA.ToString(),
                        Text = c.NOMBRE
                    })
                    .ToList(),

                Estados = db.ESTADO
                    .OrderBy(e => e.ID_ESTADO)
                    .ToList()
                    .Select(e => new SelectListItem
                    {
                        Value = e.ID_ESTADO.ToString(),
                        Text = e.NOMBRE
                    })
                    .ToList()
            };

            return View(vm);
        }

        private bool TryParseDecimalFromRequest(string key, out decimal value)
        {
            value = 0m;
            var raw = (Request[key] ?? "").Trim();
            if (string.IsNullOrEmpty(raw)) return false;

            if (decimal.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
                return true;
            if (decimal.TryParse(raw, NumberStyles.Any, CultureInfo.CurrentCulture, out value))
                return true;

            var alt = raw.Replace(',', '.');
            if (decimal.TryParse(alt, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
                return true;

            return false;
        }

        private string ValidarCamposProducto(string nombre, decimal precio, decimal porc)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return "El nombre del producto es obligatorio.";

            if (nombre.Trim().Length > 140)
                return "El nombre no puede superar los 140 caracteres.";

            if (!Regex.IsMatch(nombre.Trim(), @"^[A-Za-zÁÉÍÓÚáéíóúÑñ0-9 \-_.,()]+$"))
                return "El nombre contiene caracteres no permitidos. Use letras, números, espacios y signos básicos (-, _, ., ,).";

            if (precio < 0.01m || precio > 10_000_000m)
                return "El precio de venta debe estar entre ₡0.01 y ₡10,000,000.";

            if (porc < 0m || porc > 100m)
                return "El porcentaje de desperdicio debe estar entre 0 y 100.";

            return null;
        }

        [RolAuthorize(1)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ProductoCrudVM vm)
        {
            TryParseDecimalFromRequest("PRECIO_VENTA", out decimal precio);
            TryParseDecimalFromRequest("PORC_DESPERDICIO", out decimal porc);

            var error = ValidarCamposProducto(vm.NOMBRE, precio, porc);
            if (error != null)
            {
                TempData["ERR"] = error;
                return RedirectToAction("Index", new { page = vm.Page, q = vm.Q });
            }

            if (vm.ID_CATEGORIA <= 0)
            {
                TempData["ERR"] = "Debe seleccionar una categoría.";
                return RedirectToAction("Index", new { page = vm.Page, q = vm.Q });
            }

            var estadoActivo = db.ESTADO.FirstOrDefault(e => e.NOMBRE == "Activo");
            var activoId = estadoActivo != null ? estadoActivo.ID_ESTADO : 1;

            var p = new PRODUCTOS
            {
                NOMBRE = vm.NOMBRE.Trim(),
                PRECIO_VENTA = precio,
                ID_CATEGORIA = vm.ID_CATEGORIA,
                ID_ESTADO = (vm.ID_ESTADO > 0 ? vm.ID_ESTADO : activoId),
                PORC_DESPERDICIO = porc
            };

            db.PRODUCTOS.Add(p);
            db.SaveChanges();
            TempData["OK"] = "Producto creado correctamente.";
            return RedirectToAction("Index", new { page = vm.Page, q = vm.Q });
        }

        [RolAuthorize(1)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ProductoCrudVM vm)
        {
            bool precioParsed = TryParseDecimalFromRequest("PRECIO_VENTA", out decimal precio);
            bool porcParsed = TryParseDecimalFromRequest("PORC_DESPERDICIO", out decimal porc);

            if (!precioParsed) precio = vm.PRECIO_VENTA;
            if (!porcParsed) porc = vm.PORC_DESPERDICIO;

            var error = ValidarCamposProducto(vm.NOMBRE, precio, porc);
            if (error != null)
            {
                TempData["ERR"] = error;
                return RedirectToAction("Index", new { page = vm.Page, q = vm.Q });
            }

            if (vm.ID_CATEGORIA <= 0)
            {
                TempData["ERR"] = "Debe seleccionar una categoría.";
                return RedirectToAction("Index", new { page = vm.Page, q = vm.Q });
            }

            var p = db.PRODUCTOS.Find(vm.ID_PRODUCTO);
            if (p == null) return RedirectToAction("Index");

            p.NOMBRE = vm.NOMBRE.Trim();
            p.PRECIO_VENTA = precio;
            p.ID_CATEGORIA = vm.ID_CATEGORIA;
            p.PORC_DESPERDICIO = porc;

            db.SaveChanges();
            TempData["OK"] = "Producto actualizado correctamente.";
            return RedirectToAction("Index", new { page = vm.Page, q = vm.Q });
        }

        [RolAuthorize(1)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ToggleActive(int id, int page = 1, string q = null)
        {
            var p = db.PRODUCTOS.Find(id);
            if (p == null) return RedirectToAction("Index", new { page, q });

            var estadoActivo = db.ESTADO.FirstOrDefault(e => e.NOMBRE == "Activo");
            var activoId = estadoActivo != null ? estadoActivo.ID_ESTADO : 1;

            var estadoInactivo = db.ESTADO.FirstOrDefault(e => e.NOMBRE == "Inactivo");
            var inactivoId = estadoInactivo != null ? estadoInactivo.ID_ESTADO : 2;

            bool activar = p.ID_ESTADO == inactivoId;
            p.ID_ESTADO = activar ? activoId : inactivoId;

            db.SaveChanges();
            TempData["OK"] = activar ? "Producto activado correctamente." : "Producto inactivado correctamente.";
            return RedirectToAction("Index", new { page, q });
        }

        [RolAuthorize(1)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, int page = 1, string q = null)
        {
            var p = db.PRODUCTOS.Find(id);
            if (p == null) return RedirectToAction("Index", new { page, q });

            var estadoInactivo = db.ESTADO.FirstOrDefault(e => e.NOMBRE == "Inactivo");
            var inactivoId = estadoInactivo != null ? estadoInactivo.ID_ESTADO : 2;

            if (p.ID_ESTADO != inactivoId)
            {
                p.ID_ESTADO = inactivoId;
                db.SaveChanges();
                TempData["OK"] = "Producto inactivado correctamente.";
            }
            else
            {
                TempData["Mensaje"] = "El producto ya está inactivo.";
            }

            return RedirectToAction("Index", new { page, q });
        }
    }
}
