using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Proyecto_Diseno_Desarrollo_Grupo5.Helpers
{
    /// <summary>
    /// Helper para gestionar menús y permisos según el rol del usuario
    /// Centraliza la lógica de qué módulos/acciones ve cada rol
    /// </summary>
    public static class PermisosHelper
    {
        /// <summary>
        /// Define qué menús ve cada rol
        /// Estructura: Rol -> Lista de items de menú con sus acciones
        /// </summary>
        public static Dictionary<int, List<MenuItem>> ObtenerMenusPorRol()
        {
            return new Dictionary<int, List<MenuItem>>
            {
                // ROL 1: ADMINISTRADOR - Acceso total
                {
                    1, new List<MenuItem>
                    {
                        // MENÚ INICIO
                        new MenuItem
                        {
                            Grupo = "Inicio",
                            Items = new List<SubMenuItem>
                            {
                                new SubMenuItem { Texto = "Productos", Accion = "Index", Controlador = "Productos", Icono = "bi-box-seam" },
                                new SubMenuItem { Texto = "Clientes", Accion = "Index", Controlador = "Clientes", Icono = "bi-people" },
                                new SubMenuItem { Texto = "Proveedores", Accion = "Index", Controlador = "Proveedores", Icono = "bi-truck" },
                                new SubMenuItem { Texto = "Ventas", Accion = "Index", Controlador = "Ventas", Icono = "bi-cash-coin" },
                                new SubMenuItem { Texto = "Materiales", Accion = "Index", Controlador = "Materiales", Icono = "bi-box" },
                                new SubMenuItem { Texto = "Control Usuarios", Accion = "Index", Controlador = "Usuarios", Icono = "bi-shield-lock" },
                                new SubMenuItem { Texto = "Roles", Accion = "Index", Controlador = "Roles", Icono = "bi-key" },
                                new SubMenuItem { Texto = "Categorías", Accion = "Index", Controlador = "Categorias", Icono = "bi-tags" }
                            }
                        },
                        // MENÚ SERVICIOS
                        new MenuItem
                        {
                            Grupo = "Servicios",
                            Items = new List<SubMenuItem>
                            {
                                new SubMenuItem { Texto = "Bitácora / Auditoría", Accion = "Bitacora", Controlador = "Home", Icono = "bi-file-text" },
                                new SubMenuItem { Texto = "Garantías", Accion = "Index", Controlador = "Garantias", Icono = "bi-shield-check" },
                                new SubMenuItem { Texto = "Devoluciones", Accion = "Index", Controlador = "Devoluciones", Icono = "bi-arrow-return-left" }
                            }
                        },
                        // MENÚ OTROS
                        new MenuItem
                        {
                            Grupo = "Otros",
                            Items = new List<SubMenuItem>
                            {
                                new SubMenuItem { Texto = "Movimientos", Accion = "Index", Controlador = "Salidas", Icono = "bi-arrows-move" },
                                new SubMenuItem { Texto = "Análisis de Desperdicios", Accion = "Desperdicios", Controlador = "Reportes", Icono = "bi-graph-up" },
                                new SubMenuItem { Texto = "Reportes", Accion = "DesperdiciosDetalle", Controlador = "Reportes", Icono = "bi-file-earmark-bar-graph" }
                            }
                        }
                    }
                },

                // ROL 2: VENDEDOR - Acceso limitado a operaciones de venta
                {
                    2, new List<MenuItem>
                    {
                        // MENÚ INICIO (Solo lo operacional)
                        new MenuItem
                        {
                            Grupo = "Inicio",
                            Items = new List<SubMenuItem>
                            {
                                new SubMenuItem { Texto = "Productos", Accion = "Index", Controlador = "Productos", Icono = "bi-box-seam" },
                                new SubMenuItem { Texto = "Clientes", Accion = "Index", Controlador = "Clientes", Icono = "bi-people" },
                                new SubMenuItem { Texto = "Ventas", Accion = "Index", Controlador = "Ventas", Icono = "bi-cash-coin" },
                                new SubMenuItem { Texto = "Materiales (Lectura)", Accion = "Index", Controlador = "Materiales", Icono = "bi-box" }
                            }
                        },
                        // MENÚ SERVICIOS (Solo lo relacionado a ventas)
                        new MenuItem
                        {
                            Grupo = "Servicios",
                            Items = new List<SubMenuItem>
                            {
                                new SubMenuItem { Texto = "Garantías", Accion = "Index", Controlador = "Garantias", Icono = "bi-shield-check" },
                                new SubMenuItem { Texto = "Devoluciones", Accion = "Index", Controlador = "Devoluciones", Icono = "bi-arrow-return-left" }
                            }
                        }
                    }
                },

                // ROL 3: ENCARGADO DE BODEGA - Acceso a inventario y movimientos
                {
                    3, new List<MenuItem>
                    {
                        // MENÚ BODEGA
                        new MenuItem
                        {
                            Grupo = "Bodega",
                            Items = new List<SubMenuItem>
                            {
                                new SubMenuItem { Texto = "Movimientos", Accion = "Index", Controlador = "Salidas", Icono = "bi-arrows-move" },
                                new SubMenuItem { Texto = "Materiales", Accion = "Index", Controlador = "Materiales", Icono = "bi-box" }
                            }
                        }
                    }
                },

                // ROL 4: GERENTE - Acceso total a reportes y análisis (si existe)
                {
                    4, new List<MenuItem>
                    {
                        new MenuItem
                        {
                            Grupo = "Reportes",
                            Items = new List<SubMenuItem>
                            {
                                new SubMenuItem { Texto = "Análisis de Desperdicios", Accion = "Desperdicios", Controlador = "Reportes", Icono = "bi-graph-up" },
                                new SubMenuItem { Texto = "Reportes Detallados", Accion = "DesperdiciosDetalle", Controlador = "Reportes", Icono = "bi-file-earmark-bar-graph" },
                                new SubMenuItem { Texto = "Devoluciones", Accion = "Reporte", Controlador = "Devoluciones", Icono = "bi-bar-chart" }
                            }
                        }
                    }
                }
            };
        }

        /// <summary>
        /// Obtiene los menús para un rol específico
        /// </summary>
        public static List<MenuItem> ObtenerMenusPorRol(int idRol)
        {
            var todosMenues = ObtenerMenusPorRol();
            return todosMenues.ContainsKey(idRol) ? todosMenues[idRol] : new List<MenuItem>();
        }

        /// <summary>
        /// Verifica si un usuario tiene permiso para acceder a una acción
        /// </summary>
        public static bool TienePermisoAccion(int idRol, string controlador, string accion)
        {
            var menus = ObtenerMenusPorRol(idRol);
            
            foreach (var menu in menus)
            {
                var subitem = menu.Items.FirstOrDefault(
                    x => x.Controlador.Equals(controlador, StringComparison.OrdinalIgnoreCase) &&
                         x.Accion.Equals(accion, StringComparison.OrdinalIgnoreCase)
                );
                
                if (subitem != null)
                    return true;
            }
            
            return false;
        }

        /// <summary>
        /// Obtiene el nombre descriptivo de un rol
        /// </summary>
        public static string ObtenerNombreRol(int idRol)
        {
            var nombres = new Dictionary<int, string>
            {
                { 1, "Administrador" },
                { 2, "Vendedor" },
                { 3, "Encargado de Bodega" },
                { 4, "Gerente" }
            };

            return nombres.ContainsKey(idRol) ? nombres[idRol] : "Usuario";
        }
    }

    /// <summary>
    /// Representa un grupo de menú con sus subítems
    /// </summary>
    public class MenuItem
    {
        public string Grupo { get; set; }
        public List<SubMenuItem> Items { get; set; } = new List<SubMenuItem>();
    }

    /// <summary>
    /// Representa un item de menú individual
    /// </summary>
    public class SubMenuItem
    {
        public string Texto { get; set; }
        public string Accion { get; set; }
        public string Controlador { get; set; }
        public string Icono { get; set; } = "bi-link";
    }
}
