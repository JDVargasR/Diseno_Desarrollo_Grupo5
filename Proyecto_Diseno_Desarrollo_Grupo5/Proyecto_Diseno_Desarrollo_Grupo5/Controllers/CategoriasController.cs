using System.Linq;
using System.Web.Mvc;
using Proyecto_Diseno_Desarrollo_Grupo5.EF;
using System.Data.Entity;
using System;

namespace Proyecto_Diseno_Desarrollo_Grupo5.Controllers
{
    public class CategoriasController : Controller
    {
        private DBGRUPO5Entities db = new DBGRUPO5Entities();

        private bool IsAdmin()
        {
            return (Session["Rol"] ?? "").ToString() == "ADMINISTRADOR";
        }

        // Listado de categorías (todas)
        public ActionResult Index()
        {
            try
            {
                var categorias = db.CATEGORIAS.Include("ESTADO").ToList();
                return View(categorias);
            }
            catch (Exception ex)
            {
                // Si la base de datos no está disponible, devolver una lista vacía y mostrar mensaje.
                TempData["Mensaje"] = "No se pudo conectar a la base de datos. " + ex.Message;
                return View(Enumerable.Empty<CATEGORIAS>());
            }
        }

        // GET: Crear
        [HttpGet]
        public ActionResult Create()
        {
            if (!IsAdmin()) return new HttpStatusCodeResult(403);
            return View(new CATEGORIAS());
        }

        // POST: Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CATEGORIAS model)
        {
            if (!IsAdmin()) return new HttpStatusCodeResult(403);

            if (string.IsNullOrWhiteSpace(model.NOMBRE))
            {
                ModelState.AddModelError("NOMBRE", "El nombre es requerido.");
            }

            // Validar nombre único
            var existe = db.CATEGORIAS.Any(c => c.NOMBRE == model.NOMBRE);
            if (existe)
            {
                ModelState.AddModelError("NOMBRE", "Ya existe una categoría con ese nombre.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Por defecto marcar como Activo (si existe el estado 'Activo' se usa su ID, si no se usa 1)
            var estadoActivo = db.ESTADO.FirstOrDefault(e => e.NOMBRE == "Activo");
            model.ID_ESTADO = estadoActivo != null ? estadoActivo.ID_ESTADO : 1;

            db.CATEGORIAS.Add(model);
            db.SaveChanges();

            TempData["OK"] = "Categoría registrada correctamente.";
            return RedirectToAction("Index");
        }

        // GET: Editar
        [HttpGet]
        public ActionResult Edit(int id)
        {
            if (!IsAdmin()) return new HttpStatusCodeResult(403);

            var categoria = db.CATEGORIAS.Find(id);
            if (categoria == null) return HttpNotFound();
            return View(categoria);
        }

        // POST: Editar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(CATEGORIAS model)
        {
            if (!IsAdmin()) return new HttpStatusCodeResult(403);

            if (string.IsNullOrWhiteSpace(model.NOMBRE))
            {
                ModelState.AddModelError("NOMBRE", "El nombre es requerido.");
            }

            var existe = db.CATEGORIAS.Any(c => c.NOMBRE == model.NOMBRE && c.ID_CATEGORIA != model.ID_CATEGORIA);
            if (existe)
            {
                ModelState.AddModelError("NOMBRE", "Ya existe otra categoría con ese nombre.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var categoria = db.CATEGORIAS.Find(model.ID_CATEGORIA);
            if (categoria == null) return HttpNotFound();

            categoria.NOMBRE = model.NOMBRE;
            categoria.DESCRIPCION = model.DESCRIPCION;
            // No tocar ID_ESTADO aquí (editar no cambia estado)

            db.SaveChanges();

            TempData["OK"] = "Categoría actualizada correctamente.";
            return RedirectToAction("Index");
        }

        // POST: Desactivar (no borra físicamente)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Deactivate(int id)
        {
            if (!IsAdmin()) return new HttpStatusCodeResult(403);

            var categoria = db.CATEGORIAS.Find(id);
            if (categoria == null) return HttpNotFound();

            var estadoInactivo = db.ESTADO.FirstOrDefault(e => e.NOMBRE == "Inactivo");
            categoria.ID_ESTADO = estadoInactivo != null ? estadoInactivo.ID_ESTADO : 2;

            db.SaveChanges();

            TempData["OK"] = "Categoría puesta como Inactiva.";
            return RedirectToAction("Index");
        }

        // Endpoint para obtener categorías activas (uso en productos, ventas, etc.)
        public ActionResult GetActiveCategories()
        {
            var activoId = db.ESTADO.FirstOrDefault(e => e.NOMBRE == "Activo")?.ID_ESTADO ?? 1;
            var list = db.CATEGORIAS
                .Where(c => c.ID_ESTADO == activoId)
                .Select(c => new { c.ID_CATEGORIA, c.NOMBRE })
                .ToList();

            return Json(list, JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
