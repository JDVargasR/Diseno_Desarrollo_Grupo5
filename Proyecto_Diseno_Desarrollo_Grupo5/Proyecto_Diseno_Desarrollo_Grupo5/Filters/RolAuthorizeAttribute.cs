using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Proyecto_Diseno_Desarrollo_Grupo5;

namespace Proyecto_Diseno_Desarrollo_Grupo5.Filters
{
    public class RolAuthorizeAttribute : AuthorizeAttribute
    {
        private readonly int[] _rolesPermitidos;

        public RolAuthorizeAttribute(params int[] rolesPermitidos)
        {
            _rolesPermitidos = rolesPermitidos ?? Array.Empty<int>();
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var session = httpContext?.Session;
            if (session == null) return false;

            if (session["IdUsuario"] == null) return false;

            if (_rolesPermitidos.Length == 0) return true;

            // Leemos el IdRol de la sesión
            if (session["IdRol"] == null) return false;

            int idRol;
            if (!int.TryParse(session["IdRol"].ToString(), out idRol)) return false;

            return _rolesPermitidos.Contains(idRol);
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.Session["IdUsuario"] == null)
            {
                filterContext.Result = new RedirectToRouteResult(
                    new System.Web.Routing.RouteValueDictionary(new { controller = "Autenticacion", action = "Login" })
                );
                return;
            }

            filterContext.Result = new RedirectToRouteResult(
                new System.Web.Routing.RouteValueDictionary(new { controller = "Autenticacion", action = "AccesoDenegado" })
            );
        }
    }

    public class BitacoraAuditAttribute : ActionFilterAttribute
    {
        private const string BitacoraIdKey = "Bitacora.IdAfectado";
        private const string BitacoraAccionKey = "Bitacora.AccionForzada";

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var idAfectado = ExtraerIdAfectado(filterContext?.ActionParameters);
            var accionForzada = ExtraerAccionBitacora(filterContext?.ActionParameters);
            if (filterContext?.HttpContext != null)
            {
                filterContext.HttpContext.Items[BitacoraIdKey] = idAfectado;
                filterContext.HttpContext.Items[BitacoraAccionKey] = accionForzada;
            }

            base.OnActionExecuting(filterContext);
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (filterContext == null || filterContext.Exception != null)
                return;

            var request = filterContext.HttpContext?.Request;
            if (request == null)
                return;

            var method = (request.HttpMethod ?? string.Empty).ToUpperInvariant();
            if (method == "GET" || method == "HEAD")
                return;

            var controller = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
            var action = filterContext.ActionDescriptor.ActionName;

            var idUsuario = ObtenerIdUsuario(filterContext.HttpContext?.Session);
            if (!idUsuario.HasValue)
                return;

            if (controller.Equals("Home", StringComparison.OrdinalIgnoreCase) &&
                action.Equals("Bitacora", StringComparison.OrdinalIgnoreCase))
                return;

            var accion = MapearAccion(action, method);
            var idAfectado = filterContext.HttpContext.Items[BitacoraIdKey] as string;
            var accionForzada = filterContext.HttpContext.Items[BitacoraAccionKey] as string;

            if (!string.IsNullOrWhiteSpace(accionForzada))
                accion = accionForzada.Trim().ToUpperInvariant();

            var descripcion = "Accion ejecutada: " + NormalizarDescripcionAccion(action, accionForzada);

            BitacoraHelper.Registrar(idUsuario, accion, descripcion, controller, idAfectado, "OK");
        }

        private int? ObtenerIdUsuario(HttpSessionStateBase session)
        {
            if (session == null || session["IdUsuario"] == null)
                return null;

            var raw = session["IdUsuario"];
            if (raw is int)
                return (int)raw;

            int parsed;
            return int.TryParse(raw.ToString(), out parsed) ? parsed : (int?)null;
        }

        private string MapearAccion(string action, string method)
        {
            var a = (action ?? string.Empty).ToLowerInvariant();

            if (a.Contains("deactivate") || a.Contains("inactivar") || a.Contains("toggle") || a.Contains("estado")) return "DESACTIVAR";
            if (a.Contains("activate") || a.Contains("activar")) return "ACTIVAR";
            if (a.Contains("create") || a.Contains("insert") || a.Contains("registr")) return "CREAR";
            if (a.Contains("edit") || a.Contains("update")) return "EDITAR";
            if (a.Contains("delete") || a.Contains("remove")) return "ELIMINAR";

            return method == "POST" ? "ACCION" : method;
        }

        private string NormalizarDescripcionAccion(string action, string accionForzada)
        {
            if (!string.IsNullOrWhiteSpace(accionForzada))
            {
                var forced = accionForzada.Trim().ToUpperInvariant();
                if (forced == "ACTIVAR") return "Activar";
                if (forced == "DESACTIVAR") return "Desactivar";
            }

            var a = (action ?? string.Empty).Trim().ToLowerInvariant();

            if (a.Contains("deactivate") || a.Contains("inactivar") || a.Contains("toggle") || a.Contains("estado")) return "Desactivar";
            if (a.Contains("activate") || a.Contains("activar")) return "Activar";
            if (a.Contains("create") || a.Contains("insert") || a.Contains("registr")) return "Crear";
            if (a.Contains("edit") || a.Contains("update")) return "Editar";
            if (a.Contains("delete") || a.Contains("remove")) return "Eliminar";
            if (a.Contains("login")) return "Login";
            if (a.Contains("logout")) return "Logout";

            return action;
        }

        private string ExtraerAccionBitacora(System.Collections.Generic.IDictionary<string, object> parametros)
        {
            if (parametros == null || parametros.Count == 0)
                return null;

            object raw;
            if (parametros.TryGetValue("accionBitacora", out raw) && raw != null)
            {
                var v = raw.ToString().Trim().ToUpperInvariant();
                if (v == "ACTIVAR" || v == "DESACTIVAR") return v;
            }

            return null;
        }

        private string ExtraerIdAfectado(System.Collections.Generic.IDictionary<string, object> parametros)
        {
            if (parametros == null || parametros.Count == 0)
                return null;

            if (parametros.ContainsKey("id") && parametros["id"] != null)
                return parametros["id"].ToString();

            foreach (var kv in parametros)
            {
                if (kv.Value == null) continue;

                var propId = kv.Value
                    .GetType()
                    .GetProperties()
                    .FirstOrDefault(p => p.Name.StartsWith("ID_", StringComparison.OrdinalIgnoreCase) || p.Name.StartsWith("Id", StringComparison.OrdinalIgnoreCase));

                if (propId == null) continue;

                var val = propId.GetValue(kv.Value, null);
                if (val != null && !string.Equals(val.ToString(), "0"))
                    return val.ToString();
            }

            return null;
        }
    }
}