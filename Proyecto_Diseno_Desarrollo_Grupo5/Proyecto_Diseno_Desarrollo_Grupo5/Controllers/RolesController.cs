using Proyecto_Diseno_Desarrollo_Grupo5.EF;
using Proyecto_Diseno_Desarrollo_Grupo5.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Proyecto_Diseno_Desarrollo_Grupo5.Controllers
{
    public class RolesController : Controller
    {
        #region Index (Listar)
        [HttpGet]
        public ActionResult Index(string q = "", int estado = 0)
        {
            // Mensajes para SweetAlert
            ViewBag.Mensaje = TempData["Mensaje"];
            ViewBag.OK = TempData["OK"];

            q = (q ?? "").Trim();

            using (var context = new DBGRUPO5Entities())
            {
                var roles = context.ROLES.AsQueryable();

                if (!string.IsNullOrEmpty(q))
                {
                    roles = roles.Where(r => r.NOMBRE.Contains(q) || r.DESCRIPCION.Contains(q));
                }

                if (estado == 1 || estado == 2)
                {
                    roles = roles.Where(r => r.ID_ESTADO == estado);
                }

                var lista = roles.OrderBy(r => r.ID_ROL).ToList();

                // Para mantener filtros en la vista
                ViewBag.Q = q;
                ViewBag.Estado = estado;

                return View(lista);
            }
        }
        #endregion


        #region Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(RolesModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Mensaje"] = "Revisá los campos. Hay información inválida.";
                return RedirectToAction("Index");
            }

            var estado = model.IdEstado ?? 1;

            using (var context = new DBGRUPO5Entities())
            {
                // Validación no duplicar nombre
                var existe = context.ROLES.Any(r => r.NOMBRE == model.Nombre);
                if (existe)
                {
                    TempData["Mensaje"] = "Ya existe un rol con ese nombre.";
                    return RedirectToAction("Index");
                }

                var rol = new ROLES
                {
                    NOMBRE = model.Nombre.Trim(),
                    DESCRIPCION = (model.Descripcion ?? "").Trim(),
                    ID_ESTADO = (estado == 2 ? 2 : 1)
                };

                context.ROLES.Add(rol);
                context.SaveChanges();

                TempData["OK"] = "Rol creado correctamente.";
                return RedirectToAction("Index");
            }
        }
        #endregion


        #region Update
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(RolesModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Mensaje"] = "Revisá los campos. Hay información inválida.";
                return RedirectToAction("Index");
            }

            using (var context = new DBGRUPO5Entities())
            {
                var rol = context.ROLES.FirstOrDefault(r => r.ID_ROL == model.IdRol);
                if (rol == null)
                {
                    TempData["Mensaje"] = "No se encontró el rol seleccionado.";
                    return RedirectToAction("Index");
                }

                // Validación evitar duplicado de nombre en otro rol
                var existe = context.ROLES.Any(r => r.NOMBRE == model.Nombre && r.ID_ROL != model.IdRol);
                if (existe)
                {
                    TempData["Mensaje"] = "Ya existe otro rol con ese nombre.";
                    return RedirectToAction("Index");
                }

                rol.NOMBRE = model.Nombre.Trim();
                rol.DESCRIPCION = (model.Descripcion ?? "").Trim();
                rol.ID_ESTADO = (model.IdEstado == 2 ? 2 : 1);

                context.SaveChanges();

                TempData["OK"] = "Rol actualizado correctamente.";
                return RedirectToAction("Index");
            }
        }
        #endregion


        #region Deactivate (Borrado lógico)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Deactivate(int id)
        {
            using (var context = new DBGRUPO5Entities())
            {
                var rol = context.ROLES.FirstOrDefault(r => r.ID_ROL == id);
                if (rol == null)
                {
                    TempData["Mensaje"] = "No se encontró el rol para inactivar.";
                    return RedirectToAction("Index");
                }

                rol.ID_ESTADO = 2;
                context.SaveChanges();

                TempData["OK"] = "Rol inactivado correctamente.";
                return RedirectToAction("Index");
            }
        }
        #endregion
    }
}
