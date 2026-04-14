using System;

namespace Proyecto_Diseno_Desarrollo_Grupo5.Models.Garantias
{
    public class SolicitudGarantiaFilaVM
    {
        public int IdSolicitud { get; set; }
        public int IdVenta { get; set; }
        public string Cliente { get; set; }
        public string Producto { get; set; }
        public DateTime FechaSolicitud { get; set; }
        public string Estado { get; set; }

        public DateTime? FechaRevision { get; set; }
        public DateTime? FechaResolucion { get; set; }

        public int? IdTecnico { get; set; }
        public string Tecnico { get; set; }
    }
}
