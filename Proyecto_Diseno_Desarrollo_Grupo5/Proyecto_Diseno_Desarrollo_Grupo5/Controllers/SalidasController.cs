using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Proyecto_Diseno_Desarrollo_Grupo5.EF;
using Proyecto_Diseno_Desarrollo_Grupo5.Filters;
using Proyecto_Diseno_Desarrollo_Grupo5.Models;

namespace Proyecto_Diseno_Desarrollo_Grupo5.Controllers
{ 
    [RolAuthorize(1)]
    public class SalidasController : Controller 
    { 
        private DBGRUPO5Entities db = new DBGRUPO5Entities();

        public ActionResult Index(string tipo = null, int? idMaterial = null, DateTime? desde = null, DateTime? hasta = null, string q = null)
        {
            q = (q ?? string.Empty).Trim();
            tipo = (tipo ?? string.Empty).Trim().ToUpperInvariant();

            var query = db.MOVIMIENTOS_INVENTARIO
                .Include(x => x.MATERIALES)
                .Include(x => x.MATERIALES.PROVEEDORES)
                .AsQueryable();

            if (tipo == "ENTRADA" || tipo == "SALIDA")
                query = query.Where(x => x.TIPO_MOVIMIENTO == tipo);

            if (idMaterial.HasValue)
                query = query.Where(x => x.ID_MATERIAL == idMaterial.Value);

            if (desde.HasValue)
                query = query.Where(x => x.FECHA >= desde.Value.Date);

            if (hasta.HasValue)
            {
                var h = hasta.Value.Date.AddDays(1);
                query = query.Where(x => x.FECHA < h);
            }

            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(x => x.MATERIALES.NOMBRE.Contains(q) || (x.OBSERVACION ?? "").Contains(q));

            var movimientos = query
                .OrderByDescending(m => m.FECHA)
                .ToList();

            var model = new MovimientoInventarioIndexVM
            {
                Tipo = tipo,
                IdMaterial = idMaterial,
                Desde = desde,
                Hasta = hasta,
                Q = q,
                Materiales = db.MATERIALES
                    .OrderBy(x => x.NOMBRE)
                    .ToList()
                    .Select(x => new SelectListItem
                    {
                        Value = x.ID_MATERIAL.ToString(),
                        Text = x.NOMBRE
                    })
                    .ToList(),
                Movimientos = movimientos.Select(x =>
                {
                    var meta = ParseObservacion(x.OBSERVACION);
                    return new MovimientoInventarioFilaVM
                    {
                        IdMovimiento = x.ID_MOVIMIENTO,
                        Fecha = x.FECHA,
                        Tipo = x.TIPO_MOVIMIENTO,
                        Material = x.MATERIALES?.NOMBRE,
                        Proveedor = x.MATERIALES?.PROVEEDORES?.NOMBRE,
                        Cantidad = x.CANTIDAD,
                        Motivo = meta.Motivo,
                        Estado = meta.EsAnulado ? "ANULADO" : (meta.EsPendiente ? "PENDIENTE" : "CONFIRMADO"),
                        PuedeAnular = PuedeAnular(x, meta)
                    };
                }).ToList()
            };

            return View(model);
        }

        [HttpGet]
        public ActionResult CreateEntrada()
        {
            return View("Create", BuildForm("ENTRADA"));
        }

        [HttpGet]
        public ActionResult CreateSalida()
        {
            return View("Create", BuildForm("SALIDA"));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateEntrada(MovimientoInventarioFormVM vm)
        {
            return RegistrarMovimiento("ENTRADA", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateSalida(MovimientoInventarioFormVM vm)
        {
            return RegistrarMovimiento("SALIDA", vm);
        }

        [HttpGet]
        public ActionResult Comprobante(int id)
        {
            var movimiento = db.MOVIMIENTOS_INVENTARIO
                .Include(x => x.MATERIALES)
                .Include(x => x.MATERIALES.PROVEEDORES)
                .FirstOrDefault(x => x.ID_MOVIMIENTO == id);

            if (movimiento == null)
            {
                TempData["Mensaje"] = "Movimiento no encontrado.";
                return RedirectToAction("Index");
            }

            var meta = ParseObservacion(movimiento.OBSERVACION);
            var vm = new MovimientoInventarioComprobanteVM
            {
                IdMovimiento = movimiento.ID_MOVIMIENTO,
                Tipo = movimiento.TIPO_MOVIMIENTO,
                Fecha = movimiento.FECHA,
                Material = movimiento.MATERIALES?.NOMBRE,
                Proveedor = movimiento.MATERIALES?.PROVEEDORES?.NOMBRE,
                Cantidad = movimiento.CANTIDAD,
                Motivo = meta.Motivo,
                Estado = meta.EsAnulado ? "ANULADO" : "CONFIRMADO"
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Anular(int id)
        {
            using (var tx = db.Database.BeginTransaction())
            {
                try
                {
                    var movimiento = db.MOVIMIENTOS_INVENTARIO
                        .Include(x => x.MATERIALES)
                        .FirstOrDefault(x => x.ID_MOVIMIENTO == id);

                    if (movimiento == null)
                    {
                        TempData["Mensaje"] = "Movimiento no encontrado.";
                        return RedirectToAction("Index");
                    }

                    var meta = ParseObservacion(movimiento.OBSERVACION);
                    if (!PuedeAnular(movimiento, meta))
                    {
                        TempData["Mensaje"] = "No se puede anular este movimiento porque ya está contabilizado o vinculado a una operación del sistema.";
                        return RedirectToAction("Index");
                    }

                    if (movimiento.TIPO_MOVIMIENTO == "ENTRADA")
                    {
                        if (movimiento.MATERIALES.STOCK < movimiento.CANTIDAD)
                        {
                            TempData["Mensaje"] = "No se puede anular la entrada porque el inventario actual es insuficiente para revertirla.";
                            return RedirectToAction("Index");
                        }

                        movimiento.MATERIALES.STOCK -= movimiento.CANTIDAD;
                    }
                    else if (movimiento.TIPO_MOVIMIENTO == "SALIDA")
                    {
                        movimiento.MATERIALES.STOCK += movimiento.CANTIDAD;
                    }

                    movimiento.OBSERVACION = (movimiento.OBSERVACION ?? string.Empty)
                        + ";ANULADO=SI;FECHA_ANULACION=" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    db.SaveChanges();
                    tx.Commit();

                    TempData["OK"] = "Movimiento anulado correctamente.";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    tx.Rollback();
                    TempData["Mensaje"] = "No se pudo anular el movimiento. " + ex.Message;
                    return RedirectToAction("Index");
                }
            }
        }

        private ActionResult RegistrarMovimiento(string tipo, MovimientoInventarioFormVM vm)
        {
            tipo = (tipo ?? string.Empty).Trim().ToUpperInvariant();

            if (vm == null || !ModelState.IsValid)
            {
                TempData["Mensaje"] = "Completa todos los datos del movimiento.";
                return View("Create", BuildForm(tipo, vm));
            }

            var material = db.MATERIALES
                .Include(x => x.PROVEEDORES)
                .FirstOrDefault(x => x.ID_MATERIAL == vm.IdMaterial);

            if (material == null)
            {
                TempData["Mensaje"] = "El producto/material no existe en el catálogo.";
                return View("Create", BuildForm(tipo, vm));
            }

            if (vm.Cantidad <= 0)
            {
                TempData["Mensaje"] = "La cantidad debe ser mayor a cero.";
                return View("Create", BuildForm(tipo, vm));
            }

            if (tipo == "SALIDA" && vm.Cantidad > material.STOCK)
            {
                TempData["Mensaje"] = "No hay stock suficiente para registrar la salida.";
                return View("Create", BuildForm(tipo, vm));
            }

            var observacion = "ORIGEN=MANUAL;ESTADO=PENDIENTE;MOTIVO=" + Sanitizar(vm.Motivo);
            if (observacion.Length > 250) observacion = observacion.Substring(0, 250);

            var movimiento = new MOVIMIENTOS_INVENTARIO
            {
                ID_MATERIAL = material.ID_MATERIAL,
                TIPO_MOVIMIENTO = tipo,
                CANTIDAD = vm.Cantidad,
                FECHA = vm.Fecha,
                OBSERVACION = observacion
            };

            db.MOVIMIENTOS_INVENTARIO.Add(movimiento);

            if (tipo == "ENTRADA")
                material.STOCK += vm.Cantidad;
            else
                material.STOCK -= vm.Cantidad;

            db.SaveChanges();
            return RedirectToAction("Comprobante", new { id = movimiento.ID_MOVIMIENTO });
        }

        private MovimientoInventarioFormVM BuildForm(string tipo, MovimientoInventarioFormVM vm = null)
        {
            var model = vm ?? new MovimientoInventarioFormVM
            {
                Tipo = tipo,
                Fecha = DateTime.Now
            };

            model.Tipo = tipo;
            model.Materiales = db.MATERIALES
                .OrderBy(x => x.NOMBRE)
                .ToList()
                .Select(x => new SelectListItem
                {
                    Value = x.ID_MATERIAL.ToString(),
                    Text = x.NOMBRE
                })
                .ToList();

            return model;
        }

        private bool PuedeAnular(MOVIMIENTOS_INVENTARIO m, MovimientoObservacionMeta meta)
        {
            if (m == null || meta == null) return false;
            if (meta.EsAnulado) return false;
            if (!meta.EsPendiente) return false;
            if (!meta.EsOrigenManual) return false;

            var observacion = (m.OBSERVACION ?? string.Empty).ToUpperInvariant();
            if (observacion.Contains("VENTA") || observacion.Contains("FACTURA") || observacion.Contains("DEVOLUCION"))
                return false;

            return m.TIPO_MOVIMIENTO == "ENTRADA" || m.TIPO_MOVIMIENTO == "SALIDA";
        }

        private MovimientoObservacionMeta ParseObservacion(string obs)
        {
            var meta = new MovimientoObservacionMeta
            {
                Motivo = string.Empty,
                EsPendiente = false,
                EsAnulado = false,
                EsOrigenManual = false
            };

            var raw = (obs ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(raw)) return meta;

            meta.EsPendiente = raw.IndexOf("ESTADO=PENDIENTE", StringComparison.OrdinalIgnoreCase) >= 0;
            meta.EsAnulado = raw.IndexOf("ANULADO=SI", StringComparison.OrdinalIgnoreCase) >= 0;
            meta.EsOrigenManual = raw.IndexOf("ORIGEN=MANUAL", StringComparison.OrdinalIgnoreCase) >= 0;

            var i = raw.IndexOf("MOTIVO=", StringComparison.OrdinalIgnoreCase);
            if (i >= 0)
            {
                var val = raw.Substring(i + 7);
                var cut = val.IndexOf(';');
                meta.Motivo = (cut >= 0 ? val.Substring(0, cut) : val).Trim();
            }
            else
            {
                meta.Motivo = raw;
            }

            return meta;
        }

        private string Sanitizar(string s)
        {
            return (s ?? string.Empty).Replace(";", ",").Replace("=", ":").Trim();
        }

        private class MovimientoObservacionMeta
        {
            public string Motivo { get; set; }
            public bool EsPendiente { get; set; }
            public bool EsAnulado { get; set; }
            public bool EsOrigenManual { get; set; }
        }
    }
}