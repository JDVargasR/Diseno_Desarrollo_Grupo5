namespace Proyecto_Diseno_Desarrollo_Grupo5.EF
{
    using System;
    using System.Collections.Generic;
    
    public partial class SOLICITUD_GARANTIA
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SOLICITUD_GARANTIA()
        {
            this.NOTIFICACIONES = new HashSet<NOTIFICACIONES>();
        }
    
        public int ID_SOLICITUD { get; set; }
        public int ID_VENTA { get; set; }
        public int ID_PRODUCTO { get; set; }
        public int ID_CLIENTE { get; set; }
        public System.DateTime FECHA_SOLICITUD { get; set; }
        public string DESCRIPCION_FALLA { get; set; }
        public string ESTADO { get; set; }
        public Nullable<int> ID_TECNICO { get; set; }
        public Nullable<System.DateTime> FECHA_REVISION { get; set; }
        public Nullable<System.DateTime> FECHA_RESOLUCION { get; set; }
        public string OBSERVACIONES_TECNICAS { get; set; }
        public Nullable<int> ID_ADMIN_CIERRE { get; set; }
        public Nullable<System.DateTime> FECHA_ENTREGA { get; set; }
    
        public virtual CLIENTES CLIENTES { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<NOTIFICACIONES> NOTIFICACIONES { get; set; }
        public virtual PRODUCTOS PRODUCTOS { get; set; }
        public virtual USUARIOS USUARIOS { get; set; }
        public virtual USUARIOS USUARIOS1 { get; set; }
        public virtual VENTAS VENTAS { get; set; }
        public object USUARIOS_TECNICO { get; internal set; }
    }
}
