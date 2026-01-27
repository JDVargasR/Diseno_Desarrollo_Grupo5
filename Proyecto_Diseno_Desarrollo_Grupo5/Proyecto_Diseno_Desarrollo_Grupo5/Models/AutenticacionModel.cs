using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Proyecto_Diseno_Desarrollo_Grupo5.Models
{
    public class AutenticacionModel
    {
        public string Nombre { get; set; }

        // Login / Registro
        [Required(ErrorMessage = "El correo es obligatorio")]
        public string Correo { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        public string Contrasena { get; set; }

        public int ? IdRol { get; set; }

        public int ? IdEstado { get; set; }
    }
}
