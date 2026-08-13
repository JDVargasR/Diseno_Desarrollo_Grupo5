using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Proyecto_Diseno_Desarrollo_Grupo5.Models
{
    public class DevolucionFilaVM
    {
        public int IdDevolucion { get; set; }
        public int IdVenta { get; set; }
        public DateTime Fecha { get; set; }
        public string Estado { get; set; }
        public string Motivo { get; set; }
        public int? IdProducto { get; set; }
        public string Producto { get; set; }
        public decimal? Cantidad { get; set; }
        public string CondicionProducto { get; set; }
        public string UsuarioRegistro { get; set; }
        public string UsuarioDecision { get; set; }
        public DateTime? FechaDecision { get; set; }
        public string NombrePersonaDevuelve { get; set; }
    }

    public class DevolucionCrearVM
    {
        [Required]
        [Display(Name = "Factura")]
        public int IdVenta { get; set; }

        [Required]
        [Display(Name = "Producto")]
        public int IdProducto { get; set; }

        [Required]
        [Range(0.01, 999999)]
        public decimal Cantidad { get; set; }

        [Required]
        [StringLength(300)]
        public string Motivo { get; set; }

        [Required]
        [StringLength(140)]
        [Display(Name = "Nombre de la persona que devuelve")]
        public string NombrePersonaDevuelve { get; set; }

        [Required]
        [Display(Name = "Fecha de devolución")]
        public DateTime FechaDevolucion { get; set; } = DateTime.Today;

        [Range(1, 365)]
        public int PoliticaDias { get; set; } = 30;

        public List<SelectListItem> Ventas { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Productos { get; set; } = new List<SelectListItem>();
    }

    public class DevolucionReporteFilaVM
    {
        public string Producto { get; set; }
        public string Categoria { get; set; }
        public string Proveedor { get; set; }
        public string Motivo { get; set; }
        public decimal CantidadDevuelta { get; set; }
        public int Frecuencia { get; set; }
        public decimal ValorMonetario { get; set; }
    }
}
