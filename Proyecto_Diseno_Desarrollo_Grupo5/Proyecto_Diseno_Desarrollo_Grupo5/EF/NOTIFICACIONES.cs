namespace Proyecto_Diseno_Desarrollo_Grupo5.EF
{
    using System;
    using System.Collections.Generic;
    
    public partial class NOTIFICACIONES
    {
        public int ID_NOTIFICACION { get; set; }
        public Nullable<int> ID_USUARIO { get; set; }
        public Nullable<int> ID_CLIENTE { get; set; }
        public Nullable<int> ID_SOLICITUD { get; set; }
        public string TITULO { get; set; }
        public string MENSAJE { get; set; }
        public System.DateTime FECHA { get; set; }
        public bool LEIDA { get; set; }
        public string MODULO { get; set; }
    
        public virtual CLIENTES CLIENTES { get; set; }
        public virtual SOLICITUD_GARANTIA SOLICITUD_GARANTIA { get; set; }
        public virtual USUARIOS USUARIOS { get; set; }
    }
}
