using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;


namespace Proyecto_Diseno_Desarrollo_Grupo5.Models
{
    public class RolesModel
    {
        public int IdRol { get; set; }

        [Required(ErrorMessage = "El nombre del rol es obligatorio")]
        public string Nombre { get; set; }

        [StringLength(200, ErrorMessage = "La descripción no puede superar 200 caracteres")]
        public string Descripcion { get; set; }

        public int? IdEstado { get; set; } // 1 Activo, 2 Inactivo
    }
}