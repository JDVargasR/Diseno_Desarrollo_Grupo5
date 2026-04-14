using Proyecto_Diseno_Desarrollo_Grupo5.EF;
using Proyecto_Diseno_Desarrollo_Grupo5.Filters;
using Proyecto_Diseno_Desarrollo_Grupo5.Models.Garantias;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace Proyecto_Diseno_Desarrollo_Grupo5.Controllers
{
    [RolAuthorize(1, 2)]
    public class GarantiasController : Controller
    {
        private readonly DBGRUPO5Entities db = new DBGRUPO5Entities();

        public ActionResult Index(string q = null, string estado = null, DateTime? desde = null, DateTime? hasta = null, int page = 1, int pageSize = 10)
        {
            q = (q ?? "").Trim();
            estado = (estado ?? "").Trim();

            var query = db.SOLICITUD_GARANTIA.AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                if (int.TryParse(q, out var n))
                    query = query.Where(x => x.ID_SOLICITUD == n || x.ID_VENTA == n || x.ID_CLIENTE == n);
                else
                    query = query.Where(x => (x.CLIENTES.NOMBRE ?? "").Contains(q) || (x.PRODUCTOS.NOMBRE ?? "").Contains(q));
            }

            if (!string.IsNullOrWhiteSpace(estado))
                query = query.Where(x => x.ESTADO == estado);

            if (desde.HasValue)
                query = query.Where(x => x.FECHA_SOLICITUD >= desde.Value);

            if (hasta.HasValue)
            {
                var h = hasta.Value.Date.AddDays(1);
                query = query.Where(x => x.FECHA_SOLICITUD < h);
            }

            var lista = (from s in query
                         join u in db.USUARIOS on s.ID_TECNICO equals (int?)u.ID_USUARIO into uj
                         from u in uj.DefaultIfEmpty()
                         orderby s.FECHA_SOLICITUD descending
                         select new SolicitudGarantiaFilaVM
                         {
                             IdSolicitud = s.ID_SOLICITUD,
                             IdVenta = s.ID_VENTA,
                             Cliente = s.CLIENTES.NOMBRE,
                             Producto = s.PRODUCTOS.NOMBRE,
                             FechaSolicitud = s.FECHA_SOLICITUD,
                             Estado = s.ESTADO,
                             FechaRevision = s.FECHA_REVISION,
                             FechaResolucion = s.FECHA_RESOLUCION,
                             IdTecnico = s.ID_TECNICO,
                             Tecnico = (u == null ? null : u.NOMBRE)
                         }).ToList();

            var total = lista.Count;
            var skip = (Math.Max(page, 1) - 1) * pageSize;
            var paged = lista.Skip(skip).Take(pageSize).ToList();

            ViewBag.Q = q;
            ViewBag.Estado = estado;
            ViewBag.Desde = desde?.ToString("yyyy-MM-dd");
            ViewBag.Hasta = hasta?.ToString("yyyy-MM-dd");
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = total;
            ViewBag.TotalPages = pageSize <= 0 ? 0 : (int)Math.Ceiling((double)total / pageSize);

            ViewBag.Estados = new[]
            {
                "PENDIENTE_REVISION",
                "APROBADA_REPARACION",
                "APROBADA_REEMPLAZO",
                "RECHAZADA",
                "FINALIZADA"
            };

            return View(paged);
        }

        [HttpGet]
        public ActionResult Create()
        {
            var vm = new SolicitudGarantiaCrearVM();

            vm.Ventas = db.VENTAS
                .OrderByDescending(v => v.ID_VENTA)
                .Take(200)
                .ToList()
                .Select(v => new SelectListItem { Value = v.ID_VENTA.ToString(), Text = "#" + v.ID_VENTA + " - " + v.FECHA.ToString("dd/MM/yyyy") })
                .ToList();

            vm.Clientes = db.CLIENTES
                .OrderBy(c => c.NOMBRE)
                .ToList()
                .Select(c => new SelectListItem { Value = c.ID_CLIENTE.ToString(), Text = c.NOMBRE })
                .ToList();

            vm.Productos = db.PRODUCTOS
                .OrderBy(p => p.NOMBRE)
                .ToList()
                .Select(p => new SelectListItem { Value = p.ID_PRODUCTO.ToString(), Text = p.NOMBRE })
                .ToList();

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(SolicitudGarantiaCrearVM vm)
        {
            if (vm == null)
            {
                TempData["ERR"] = "No se recibieron datos.";
                return RedirectToAction("Create");
            }

            if (!ModelState.IsValid)
            {
                TempData["ERR"] = "Revisá los campos, hay información inválida.";
                return RedirectToAction("Create");
            }

            var venta = db.VENTAS
                .Include(v => v.DETALLES_VENTAS)
                .FirstOrDefault(v => v.ID_VENTA == vm.IdVenta);

            if (venta == null)
            {
                TempData["ERR"] = "La venta no existe.";
                return RedirectToAction("Create");
            }

            if (venta.ID_CLIENTE != vm.IdCliente)
            {
                TempData["ERR"] = "El cliente no coincide con la venta.";
                return RedirectToAction("Create");
            }

            var prodEnVenta = venta.DETALLES_VENTAS.Any(d => d.ID_PRODUCTO == vm.IdProducto);
            if (!prodEnVenta)
            {
                TempData["ERR"] = "El producto seleccionado no pertenece a la venta.";
                return RedirectToAction("Create");
            }

            var garantiaProd = db.GARANTIAS.FirstOrDefault(g => g.ID_PRODUCTO == vm.IdProducto);
            if (garantiaProd != null)
            {
                var limite = venta.FECHA.AddMonths(garantiaProd.DURACION_MESES);
                if (DateTime.Now > limite)
                {
                    TempData["ERR"] = "La venta está fuera del periodo de garantía para este producto.";
                    return RedirectToAction("Create");
                }
            }

            int? idUsuario = null;
            if (Session["IdUsuario"] != null) idUsuario = Convert.ToInt32(Session["IdUsuario"]);

            var entidad = new SOLICITUD_GARANTIA
            {
                ID_VENTA = vm.IdVenta,
                ID_PRODUCTO = vm.IdProducto,
                ID_CLIENTE = vm.IdCliente,
                FECHA_SOLICITUD = DateTime.Now,
                DESCRIPCION_FALLA = (vm.DescripcionFalla ?? "").Trim(),
                ESTADO = "PENDIENTE_REVISION"
            };

            db.SOLICITUD_GARANTIA.Add(entidad);
            db.SaveChanges();

            db.NOTIFICACIONES.Add(new NOTIFICACIONES
            {
                ID_SOLICITUD = entidad.ID_SOLICITUD,
                TITULO = "Nueva solicitud de garantía",
                MENSAJE = $"Solicitud #{entidad.ID_SOLICITUD} registrada. Estado: PENDIENTE_REVISION.",
                FECHA = DateTime.Now,
                LEIDA = false,
                MODULO = "GARANTIAS"
            });
            db.SaveChanges();

            BitacoraHelper.Registrar(idUsuario, "CREAR", $"Registro de solicitud de garantía #{entidad.ID_SOLICITUD}", "GARANTIAS", entidad.ID_SOLICITUD.ToString(), "OK");

            TempData["OK"] = $"Solicitud de garantía #{entidad.ID_SOLICITUD} registrada.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        [RolAuthorize(1, 2)]
        public ActionResult Evaluar(int id)
        {
            var s = db.SOLICITUD_GARANTIA
                .Include(x => x.CLIENTES)
                .Include(x => x.PRODUCTOS)
                .FirstOrDefault(x => x.ID_SOLICITUD == id);

            if (s == null)
            {
                TempData["ERR"] = "Solicitud no encontrada.";
                return RedirectToAction("Index");
            }

            var vm = new SolicitudGarantiaEvaluarVM
            {
                IdSolicitud = s.ID_SOLICITUD,
                IdVenta = s.ID_VENTA,
                Cliente = s.CLIENTES?.NOMBRE,
                Producto = s.PRODUCTOS?.NOMBRE,
                FechaSolicitud = s.FECHA_SOLICITUD,
                Estado = s.ESTADO,
                ObservacionesTecnicas = s.OBSERVACIONES_TECNICAS
            };

            vm.Estados.Add(new SelectListItem { Value = "APROBADA_REPARACION", Text = "Aprobada (Reparación)" });
            vm.Estados.Add(new SelectListItem { Value = "APROBADA_REEMPLAZO", Text = "Aprobada (Reemplazo)" });
            vm.Estados.Add(new SelectListItem { Value = "RECHAZADA", Text = "Rechazada" });

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RolAuthorize(1, 2)]
        public ActionResult Evaluar(SolicitudGarantiaEvaluarVM vm)
        {
            if (vm == null || !ModelState.IsValid)
            {
                TempData["ERR"] = "Revisá los campos.";
                return RedirectToAction("Index");
            }

            var s = db.SOLICITUD_GARANTIA.FirstOrDefault(x => x.ID_SOLICITUD == vm.IdSolicitud);
            if (s == null)
            {
                TempData["ERR"] = "Solicitud no encontrada.";
                return RedirectToAction("Index");
            }

            if (s.ESTADO == "FINALIZADA")
            {
                TempData["ERR"] = "La solicitud ya está finalizada.";
                return RedirectToAction("Index");
            }

            var nuevoEstado = (vm.Estado ?? "").Trim().ToUpperInvariant();
            var permitidos = new[] { "APROBADA_REPARACION", "APROBADA_REEMPLAZO", "RECHAZADA" };
            if (!permitidos.Contains(nuevoEstado))
            {
                TempData["ERR"] = "Estado no permitido.";
                return RedirectToAction("Index");
            }

            int? idUsuario = null;
            if (Session["IdUsuario"] != null) idUsuario = Convert.ToInt32(Session["IdUsuario"]);

            s.ESTADO = nuevoEstado;
            s.OBSERVACIONES_TECNICAS = (vm.ObservacionesTecnicas ?? "").Trim();
            s.FECHA_REVISION = s.FECHA_REVISION ?? DateTime.Now;
            s.FECHA_RESOLUCION = DateTime.Now;
            s.ID_TECNICO = idUsuario;

            db.NOTIFICACIONES.Add(new NOTIFICACIONES
            {
                ID_SOLICITUD = s.ID_SOLICITUD,
                ID_CLIENTE = s.ID_CLIENTE,
                TITULO = "Resultado de garantía",
                MENSAJE = $"Tu solicitud #{s.ID_SOLICITUD} fue evaluada. Resultado: {s.ESTADO}.",
                FECHA = DateTime.Now,
                LEIDA = false,
                MODULO = "GARANTIAS"
            });

            db.SaveChanges();

            BitacoraHelper.Registrar(idUsuario, "EDITAR", $"Evaluación de solicitud de garantía #{s.ID_SOLICITUD}. Estado: {s.ESTADO}", "GARANTIAS", s.ID_SOLICITUD.ToString(), "OK");

            TempData["OK"] = "Evaluación guardada.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        [RolAuthorize(1)]
        public ActionResult Cerrar(int id)
        {
            var s = db.SOLICITUD_GARANTIA
                .Include(x => x.CLIENTES)
                .Include(x => x.PRODUCTOS)
                .FirstOrDefault(x => x.ID_SOLICITUD == id);

            if (s == null)
            {
                TempData["ERR"] = "Solicitud no encontrada.";
                return RedirectToAction("Index");
            }

            var vm = new SolicitudGarantiaCerrarVM
            {
                IdSolicitud = s.ID_SOLICITUD,
                FechaEntrega = DateTime.Now,
                IdVenta = s.ID_VENTA,
                Cliente = s.CLIENTES?.NOMBRE,
                Producto = s.PRODUCTOS?.NOMBRE,
                Estado = s.ESTADO
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RolAuthorize(1)]
        public ActionResult Cerrar(SolicitudGarantiaCerrarVM vm)
        {
            if (vm == null || !ModelState.IsValid)
            {
                TempData["ERR"] = "Revisá los campos.";
                return RedirectToAction("Index");
            }

            var s = db.SOLICITUD_GARANTIA.FirstOrDefault(x => x.ID_SOLICITUD == vm.IdSolicitud);
            if (s == null)
            {
                TempData["ERR"] = "Solicitud no encontrada.";
                return RedirectToAction("Index");
            }

            if (s.ESTADO == "FINALIZADA")
            {
                TempData["ERR"] = "La solicitud ya está finalizada.";
                return RedirectToAction("Index");
            }

            int? idUsuario = null;
            if (Session["IdUsuario"] != null) idUsuario = Convert.ToInt32(Session["IdUsuario"]);

            s.ESTADO = "FINALIZADA";
            s.FECHA_ENTREGA = vm.FechaEntrega;
            s.ID_ADMIN_CIERRE = idUsuario;

            if (!string.IsNullOrWhiteSpace(vm.ObservacionFinal))
            {
                s.OBSERVACIONES_TECNICAS = (s.OBSERVACIONES_TECNICAS ?? "").Trim();
                s.OBSERVACIONES_TECNICAS = (s.OBSERVACIONES_TECNICAS + "\n[CIERRE] " + vm.ObservacionFinal.Trim()).Trim();
            }

            db.NOTIFICACIONES.Add(new NOTIFICACIONES
            {
                ID_SOLICITUD = s.ID_SOLICITUD,
                ID_CLIENTE = s.ID_CLIENTE,
                TITULO = "Garantía finalizada",
                MENSAJE = $"Tu solicitud #{s.ID_SOLICITUD} fue marcada como FINALIZADA. Gracias.",
                FECHA = DateTime.Now,
                LEIDA = false,
                MODULO = "GARANTIAS"
            });

            db.SaveChanges();

            BitacoraHelper.Registrar(idUsuario, "EDITAR", $"Cierre de solicitud de garantía #{s.ID_SOLICITUD}.", "GARANTIAS", s.ID_SOLICITUD.ToString(), "OK");

            TempData["OK"] = "Solicitud finalizada.";
            return RedirectToAction("Index");
        }
    }
}
