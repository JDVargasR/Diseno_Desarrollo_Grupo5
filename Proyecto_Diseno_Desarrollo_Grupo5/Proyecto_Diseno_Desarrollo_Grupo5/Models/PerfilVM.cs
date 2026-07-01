using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Proyecto_Diseno_Desarrollo_Grupo5.Models
{
    public class PerfilVM
    {
        public int IdUsuario { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(80)]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El correo es obligatorio.")]
        [StringLength(120)]
        public string Correo { get; set; }
        public string Rol { get; set; }
        public string Estado { get; set; }

        // Campos para cambio de contraseña
        public string ContrasenaActual { get; set; }
        public string ContrasenaNueva { get; set; }
        public string ConfirmarContrasena { get; set; }
    }
}

