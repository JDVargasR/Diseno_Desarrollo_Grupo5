using System;
using System.ComponentModel.DataAnnotations;

namespace Proyecto_Diseno_Desarrollo_Grupo5.Models.Garantias
{
    public class SolicitudGarantiaCerrarVM
    {
        [Required]
        public int IdSolicitud { get; set; }

        [Required]
        [Display(Name = "Fecha de entrega")]
        public DateTime FechaEntrega { get; set; }

        [StringLength(500)]
        [Display(Name = "Observación final (opcional)")]
        public string ObservacionFinal { get; set; }

        // Cabecera
        public int IdVenta { get; set; }
        public string Cliente { get; set; }
        public string Producto { get; set; }
        public string Estado { get; set; }
    }
}
