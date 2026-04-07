using Proyecto_Diseno_Desarrollo_Grupo5.EF;
using Proyecto_Diseno_Desarrollo_Grupo5.Models;
using Proyecto_Diseno_Desarrollo_Grupo5;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Proyecto_Diseno_Desarrollo_Grupo5.Controllers
{
    public class AutenticacionController : Controller
    {
        #region login

        public ActionResult Logout()
        {
            int? idUsuario = Session["IdUsuario"] as int?;
            if (!idUsuario.HasValue && Session["IdUsuario"] != null)
            {
                int parsed;
                if (int.TryParse(Session["IdUsuario"].ToString(), out parsed))
                    idUsuario = parsed;
            }

            BitacoraHelper.Registrar(idUsuario, "LOGOUT", "Cierre de sesión.", "AUTENTICACION", null, "OK");
            Session.Clear();
            return RedirectToAction("Login", "Autenticacion");
        }

        [HttpGet]
        public ActionResult Login()
        {
            ViewBag.Mensaje = TempData["Mensaje"];
            ViewBag.OK = TempData["OK"];

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(AutenticacionModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Mensaje"] = "Revisá los campos. Hay información inválida.";
                return View(model);
            }

            using (var context = new DBGRUPO5Entities())
            {
                var res = context.USUARIO_LOGIN_SP(model.Correo, model.Contrasena).FirstOrDefault();

                if (res != null)
                {
                    Session["IdUsuario"] = res.ID_USUARIO;
                    Session["NombreUsuario"] = res.NOMBRE;
                    Session["IdRol"] = res.ID_ROL;
                    Session["Rol"] = res.ROL;

                    // Redirección según rol
                    if (res.ROL == "ADMINISTRADOR") return RedirectToAction("Index", "Home");
                    if (res.ROL == "VENDEDOR") return RedirectToAction("Index", "Vendedor");
                    return RedirectToAction("Index", "Home");
                }

                // ✅ Para que se muestre SweetAlert (error)
                ViewBag.Mensaje = "Correo o contraseña incorrectos, o tu usuario está inactivo.";
                return View(model);
            }
        }

        #endregion

        #region registro

        [HttpGet]
        public ActionResult Registro()
        {
            ViewBag.Mensaje = TempData["Mensaje"];
            ViewBag.OK = TempData["OK"];

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registro(AutenticacionModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Mensaje = "Revisá los campos, hay información que no es valida.";
                return View(model);
            }

            var rol = model.IdRol ?? 3;
            var estado = model.IdEstado ?? 1;

            using (var context = new DBGRUPO5Entities())
            {
                var resultado = new System.Data.Entity.Core.Objects.ObjectParameter("Resultado", typeof(int));

                context.USUARIO_REGISTRAR_SP(
                    model.Nombre,
                    model.Correo,
                    model.Contrasena,
                    rol,
                    estado,
                    resultado
                );

                int res = (int)resultado.Value;

                if (res == 1)
                {
                    TempData["OK"] = "Cuenta creada correctamente, ahora podés ve a iniciar sesión.";
                    return RedirectToAction("Login", "Autenticacion");
                }

                if (res == -1)
                {
                    ViewBag.Mensaje = "Ese correo ya está registrado en el sistema, usá otro o iniciá sesión.";
                    return View(model);
                }

                ViewBag.Mensaje = "No se pudo registrar.";
                return View(model);
            }
        }

        [HttpGet]
        public ActionResult Perfil()
        {
            if (Session["IdUsuario"] == null)
                return RedirectToAction("Login", "Autenticacion");

            int idUsuario = (int)Session["IdUsuario"];

            using (var context = new DBGRUPO5Entities())
            {
                var u = context.SP_USUARIO_PERFIL_OBTENER(idUsuario).FirstOrDefault();
                if (u == null)
                    return RedirectToAction("Login", "Autenticacion");

                var vm = new PerfilVM
                {
                    IdUsuario = u.IdUsuario,
                    Nombre = u.Nombre,
                    Correo = u.Correo,
                    Rol = u.ROL_NOMBRE,
                    Estado = u.ESTADO_NOMBRE
                };

                return View(vm);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Perfil(PerfilVM model)
        {
            if (Session["IdUsuario"] == null)
                return RedirectToAction("Login", "Autenticacion");

            int idUsuario = (int)Session["IdUsuario"];
            model.IdUsuario = idUsuario;

            // Validar que si se quiere cambiar contraseña, ambas coincidan
            if (!string.IsNullOrWhiteSpace(model.ContrasenaNueva))
            {
                if (string.IsNullOrWhiteSpace(model.ContrasenaActual))
                {
                    ViewBag.Mensaje = "Debés ingresar tu contraseña actual para cambiarla.";
                    return RecargarPerfil(model);
                }

                if (model.ContrasenaNueva != model.ConfirmarContrasena)
                {
                    ViewBag.Mensaje = "La nueva contraseña y la confirmación no coinciden.";
                    return RecargarPerfil(model);
                }
            }

            using (var context = new DBGRUPO5Entities())
            {
                var idParam = new System.Data.SqlClient.SqlParameter("@ID_USUARIO", idUsuario);
                var nombreParam = new System.Data.SqlClient.SqlParameter("@NOMBRE", (object)model.Nombre ?? System.DBNull.Value);
                var correoParam = new System.Data.SqlClient.SqlParameter("@CORREO", (object)model.Correo ?? System.DBNull.Value);
                var passActualParam = new System.Data.SqlClient.SqlParameter("@CONTRASENA_ACTUAL", (object)model.ContrasenaActual ?? (object)System.DBNull.Value);
                var passNuevaParam = new System.Data.SqlClient.SqlParameter("@CONTRASENA_NUEVA", (object)model.ContrasenaNueva ?? (object)System.DBNull.Value);

                var okParam = new System.Data.SqlClient.SqlParameter("@OK", System.Data.SqlDbType.Bit)
                {
                    Direction = System.Data.ParameterDirection.Output
                };

                var msgParam = new System.Data.SqlClient.SqlParameter("@MSG", System.Data.SqlDbType.NVarChar, 200)
                {
                    Direction = System.Data.ParameterDirection.Output
                };

                context.Database.ExecuteSqlCommand(
                    "EXEC dbo.SP_USUARIO_PERFIL_ACTUALIZAR @ID_USUARIO, @NOMBRE, @CORREO, @CONTRASENA_ACTUAL, @CONTRASENA_NUEVA, @OK OUTPUT, @MSG OUTPUT",
                    idParam,
                    nombreParam,
                    correoParam,
                    passActualParam,
                    passNuevaParam,
                    okParam,
                    msgParam
                );

                bool ok = okParam.Value != System.DBNull.Value && okParam.Value != null && (bool)okParam.Value;
                string msg = (msgParam.Value ?? "").ToString();

                if (ok)
                {
                    // Actualizar el nombre en sesión si cambió
                    Session["NombreUsuario"] = model.Nombre;
                    ViewBag.OK = msg;
                }
                else
                {
                    ViewBag.Mensaje = msg;
                }

                return RecargarPerfil(model);
            }
        }

        private ActionResult RecargarPerfil(PerfilVM model)
        {
            using (var context = new DBGRUPO5Entities())
            {
                var u = context.SP_USUARIO_PERFIL_OBTENER(model.IdUsuario).FirstOrDefault();
                if (u != null)
                {
                    model.Nombre = u.Nombre;
                    model.Correo = u.Correo;
                    model.Rol = u.ROL_NOMBRE;
                    model.Estado = u.ESTADO_NOMBRE;
                }
            }

            model.ContrasenaActual = null;
            model.ContrasenaNueva = null;
            model.ConfirmarContrasena = null;

            return View("Perfil", model);
        }

        #endregion

        #region AccesoDenegado
        [HttpGet]
        public ActionResult AccesoDenegado()
        {
            return View();
        }
        #endregion

    }
}
