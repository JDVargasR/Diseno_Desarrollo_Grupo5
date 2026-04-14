using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Proyecto_Diseno_Desarrollo_Grupo5.Models.Garantias
{
    public class SolicitudGarantiaEvaluarVM
    {
        [Required]
        public int IdSolicitud { get; set; }

        [Required]
        [Display(Name = "Resultado")]
        public string Estado { get; set; }

        [StringLength(800)]
        [Display(Name = "Observaciones")]
        public string ObservacionesTecnicas { get; set; }

        public List<SelectListItem> Estados { get; set; } = new List<SelectListItem>();

        public int IdVenta { get; set; }
        public string Cliente { get; set; }
        public string Producto { get; set; }
        public DateTime FechaSolicitud { get; set; }
    }
}
