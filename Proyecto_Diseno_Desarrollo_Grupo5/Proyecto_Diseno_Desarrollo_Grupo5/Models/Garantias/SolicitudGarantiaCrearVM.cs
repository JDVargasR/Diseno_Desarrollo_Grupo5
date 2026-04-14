using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Proyecto_Diseno_Desarrollo_Grupo5.Models.Garantias
{
    public class SolicitudGarantiaCrearVM
    {
        [Required]
        [Display(Name = "Venta")]
        public int IdVenta { get; set; }

        [Required]
        [Display(Name = "Producto")]
        public int IdProducto { get; set; }

        [Required]
        [Display(Name = "Cliente")]
        public int IdCliente { get; set; }

        [Required]
        [StringLength(500)]
        [Display(Name = "Descripción de la falla")]
        public string DescripcionFalla { get; set; }

        public List<SelectListItem> Ventas { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Productos { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Clientes { get; set; } = new List<SelectListItem>();
    }
}
