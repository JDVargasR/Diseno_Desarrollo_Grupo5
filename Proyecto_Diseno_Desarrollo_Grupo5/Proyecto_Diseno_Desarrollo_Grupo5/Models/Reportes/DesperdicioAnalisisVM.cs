using System;
using System.Collections.Generic;

namespace Proyecto_Diseno_Desarrollo_Grupo5.Models.Reportes
{
    public class DesperdicioAnalisisVM
    {
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }

        // Totales
        public decimal TotalDesperdiciado { get; set; }
        public int TotalTransacciones { get; set; }
        public decimal PromedioTransaccion { get; set; }

        // Por Producto
        public List<DesperdicioProductoVM> DesperdiciosPorProducto { get; set; }

        // Por Material
        public List<DesperdicioMaterialVM> DesperdiciosPorMaterial { get; set; }

        // Por Día
        public List<DesperdicioDiaVM> DesperdiciosPorDia { get; set; }

        // Productos de Alto Desperdicio
        public List<ProductoAltoDesperdicioVM> ProductosAltoDesperdicio { get; set; }

        public DesperdicioAnalisisVM()
        {
            DesperdiciosPorProducto = new List<DesperdicioProductoVM>();
            DesperdiciosPorMaterial = new List<DesperdicioMaterialVM>();
            DesperdiciosPorDia = new List<DesperdicioDiaVM>();
            ProductosAltoDesperdicio = new List<ProductoAltoDesperdicioVM>();
        }
    }

    public class DesperdicioProductoVM
    {
        public int IdProducto { get; set; }
        public string NombreProducto { get; set; }
        public decimal TotalDesperdiciado { get; set; }
        public decimal PorcentajeDesperdicio { get; set; }
        public int CantidadTransacciones { get; set; }
    }

    public class DesperdicioMaterialVM
    {
        public int IdMaterial { get; set; }
        public string NombreMaterial { get; set; }
        public decimal TotalDesperdiciado { get; set; }
        public string Unidad { get; set; }
        public int CantidadTransacciones { get; set; }
    }

    public class DesperdicioDiaVM
    {
        public DateTime Fecha { get; set; }
        public decimal TotalDesperdiciado { get; set; }
        public int CantidadTransacciones { get; set; }
    }

    public class ProductoAltoDesperdicioVM
    {
        public int IdProducto { get; set; }
        public string NombreProducto { get; set; }
        public decimal PorcentajeDesperdicio { get; set; }
        public decimal TotalDesperdiciado { get; set; }
        public string Recomendacion { get; set; }
    }

    public class DesperdicioFilaVM
    {
        public int IdDesperdicio { get; set; }
        public DateTime Fecha { get; set; }
        public string Material { get; set; }
        public decimal CantidadDesperdiciada { get; set; }
        public string Unidad { get; set; }
        public string Producto { get; set; }
        public int? IdVenta { get; set; }
        public string Motivo { get; set; }
        public string Reutilizable { get; set; }
        public string Usuario { get; set; }
    }
}
