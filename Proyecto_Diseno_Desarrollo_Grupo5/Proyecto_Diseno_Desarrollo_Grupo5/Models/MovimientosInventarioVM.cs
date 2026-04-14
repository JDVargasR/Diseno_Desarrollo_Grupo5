using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Proyecto_Diseno_Desarrollo_Grupo5.Models
{
    public class MovimientoInventarioFormVM
    {
        public string Tipo { get; set; }

        [Required]
        public int IdMaterial { get; set; }

        [Required]
        [Range(0.01, 999999)]
        public decimal Cantidad { get; set; }

        [Required]
        public DateTime Fecha { get; set; }

        [Required]
        [StringLength(150)]
        public string Motivo { get; set; }

        public List<SelectListItem> Materiales { get; set; } = new List<SelectListItem>();
    }

    public class MovimientoInventarioFilaVM
    {
        public int IdMovimiento { get; set; }
        public DateTime Fecha { get; set; }
        public string Tipo { get; set; }
        public string Material { get; set; }
        public string Proveedor { get; set; }
        public decimal Cantidad { get; set; }
        public string Motivo { get; set; }
        public string Estado { get; set; }
        public bool PuedeAnular { get; set; }
    }

    public class MovimientoInventarioIndexVM
    {
        public string Tipo { get; set; }
        public int? IdMaterial { get; set; }
        public DateTime? Desde { get; set; }
        public DateTime? Hasta { get; set; }
        public string Q { get; set; }

        public List<SelectListItem> Materiales { get; set; } = new List<SelectListItem>();
        public List<MovimientoInventarioFilaVM> Movimientos { get; set; } = new List<MovimientoInventarioFilaVM>();
    }

    public class MovimientoInventarioComprobanteVM
    {
        public int IdMovimiento { get; set; }
        public string Tipo { get; set; }
        public DateTime Fecha { get; set; }
        public string Material { get; set; }
        public string Proveedor { get; set; }
        public decimal Cantidad { get; set; }
        public string Motivo { get; set; }
        public string Estado { get; set; }
    }
}
