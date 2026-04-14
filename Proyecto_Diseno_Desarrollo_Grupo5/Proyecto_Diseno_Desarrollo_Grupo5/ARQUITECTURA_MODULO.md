# 🏗️ ARQUITECTURA DEL MÓDULO DE REPORTES

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         FLUJO DE DATOS Y COMPONENTES                         │
└─────────────────────────────────────────────────────────────────────────────┘

┌─ USER INTERFACE (VISTAS RAZOR) ────────────────────────────────────────────┐
│                                                                             │
│  ┌─ /Reportes/Index.cshtml ─────────────────────────────────────────────┐  │
│  │ Centro de navegación con cards                                      │  │
│  │ [Análisis de Desperdicios] [Reportes] [Próximamente...]           │  │
│  └──────────────────────────────────────────────────────────────────────┘  │
│                                                                             │
│  ┌─ /Reportes/Desperdicios.cshtml ──────────────────────────────────────┐  │
│  │ ANÁLISIS VISUAL                                                     │  │
│  │ ┌─ Filtros ───────────────────────────────────────────────────────┐ │  │
│  │ │ [Fecha Inicio] [Fecha Fin] [Filtrar] [Ver Detalle]            │ │  │
│  │ └──────────────────────────────────────────────────────────────────┘ │  │
│  │                                                                     │  │
│  │ ┌─ KPIs ──────────────────────────────────────────────────────────┐ │  │
│  │ │ [Total] [Transacciones] [Promedio] [Productos Críticos]      │ │  │
│  │ └──────────────────────────────────────────────────────────────────┘ │  │
│  │                                                                     │  │
│  │ ┌─ Gráficos ──────────────────────────────────────────────────────┐ │  │
│  │ │ ┌─ Pie (Productos) ─────────┐  ┌─ Bar (Materiales) ────────┐ │ │  │
│  │ │ │                           │  │                          │ │ │  │
│  │ │ │  [Gráfico Interactivo]   │  │  [Gráfico Interactivo] │ │ │  │
│  │ │ │                           │  │                          │ │ │  │
│  │ │ └─────────────────────────────┘  └──────────────────────────┘ │ │  │
│  │ │                                                                 │ │  │
│  │ │ ┌─ Line (Tendencia Diaria) ─────────────────────────────────┐ │ │  │
│  │ │ │                                                           │ │ │  │
│  │ │ │ [Gráfico Interactivo - Línea]                          │ │ │  │
│  │ │ │                                                           │ │ │  │
│  │ │ └───────────────────────────────────────────────────────────┘ │ │  │
│  │ └──────────────────────────────────────────────────────────────────┘ │  │
│  │                                                                     │  │
│  │ ┌─ Tabla de Alerta (Productos > 5% desperdicio) ──────────────────┐ │  │
│  │ │ Producto | % Desperdicio | Total | Recomendación             │ │  │
│  │ │ ────────────────────────────────────────────────────────────── │ │  │
│  │ │ Pan Dulce | 15.50% | 120.50 | ⚠️ Alto desperdicio...      │ │  │
│  │ │ Queso | 8.20% | 45.30 | Monitorear desperdicio           │ │  │
│  │ └──────────────────────────────────────────────────────────────────┘ │  │
│  └──────────────────────────────────────────────────────────────────────┘  │
│                                                                             │
│  ┌─ /Reportes/DesperdiciosDetalle.cshtml ───────────────────────────────┐  │
│  │ TABLA DETALLADA CON PAGINACIÓN                                      │  │
│  │ ┌─ Filtros ───────────────────────────────────────────────────────┐ │  │
│  │ │ [Fecha Inicio] [Fecha Fin] [Filtrar] [Ver Gráficos]          │ │  │
│  │ └──────────────────────────────────────────────────────────────────┘ │  │
│  │                                                                     │  │
│  │ ┌─ Tabla ──────────────────────────────────────────────────────────┐ │  │
│  │ │ Mostrando 15 de 245                                            │ │  │
│  │ │ ┌─────┬─────┬────┬────┬───┬───┬────┬─────────────────────────┐ │ │  │
│  │ │ │Fecha│Mat..│Cant│Prod│V #│Usu│Mot│ Reutil│                 │ │ │  │
│  │ │ ├─────┼─────┼────┼────┼───┼───┼────┼─────────────────────────┤ │ │  │
│  │ │ │14/04│Har..│0.5 │Pan │123│Ad │Nor│ Si │                 │ │ │  │
│  │ │ │13/04│Azuc│1.2 │Cak│122│Ad │Nor│ Si │                 │ │ │  │
│  │ │ │12/04│Hue.│0.3 │Pan│121│Ad │Nor│ No │                 │ │ │  │
│  │ │ │ ... │... │... │...│...│...│...|...|                 │ │ │  │
│  │ │ └─────┴─────┴────┴────┴───┴───┴────┴─────────────────────────┘ │ │  │
│  │ │                                                                 │ │  │
│  │ │ Paginación:                                                     │ │  │
│  │ │ [Primera] [Anterior] [1] [2] [3] [Siguiente] [Última]       │ │  │
│  │ └──────────────────────────────────────────────────────────────────┘ │  │
│  └──────────────────────────────────────────────────────────────────────┘  │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘

┌─ CONTROLADOR (C#) ─────────────────────────────────────────────────────────┐
│                                                                             │
│  ReportesController : Controller                                           │
│  ├─ Index()                                                                │
│  │  └─> Redirecciona a Desperdicios()                                    │
│  │                                                                         │
│  ├─ Desperdicios(fechaInicio, fechaFin)                                  │
│  │  ├─> ObtenerAnalisisDesPerdicio()                                     │
│  │  │   ├─ Query DESPERDICIOS_MATERIAL (filtro ORIGEN="VENTA")          │
│  │  │   ├─ Agregar por Producto                                         │
│  │  │   ├─ Agregar por Material                                         │
│  │  │   ├─ Agregar por Día                                              │
│  │  │   ├─ Calcular KPIs (Total, Promedio)                             │
│  │  │   ├─ Identificar Productos Críticos (> 5%)                       │
│  │  │   └─> DesperdicioAnalisisVM (objeto completo)                    │
│  │  └─> View("Desperdicios", vm)                                         │
│  │                                                                         │
│  ├─ DesperdiciosDetalle(fechaInicio, fechaFin, page, pageSize)          │
│  │  ├─> Query DESPERDICIOS_MATERIAL                                     │
│  │  ├─> Include(MATERIALES, PRODUCTOS, USUARIOS)                       │
│  │  ├─> Mapear a DesperdicioFilaVM                                      │
│  │  ├─> Aplicar paginación (15 items)                                   │
│  │  ├─> ViewBag (totales, página actual)                                │
│  │  └─> View("DesperdiciosDetalle", lista)                              │
│  │                                                                         │
│  ├─ DesperdiciosJSON(fechaInicio, fechaFin)                              │
│  │  ├─> ObtenerAnalisisDesPerdicio()                                     │
│  │  └─> Json(result, JsonRequestBehavior.AllowGet)                      │
│  │                                                                         │
│  └─ ObtenerAnalisisDesPerdicio(fechaInicio, fechaFin) - MÉTODO PRIVADO   │
│     ├─ Obtener desperdicios en rango                                     │
│     ├─ Calcular totales                                                  │
│     ├─ GROUP BY ID_PRODUCTO                                              │
│     ├─ GROUP BY ID_MATERIAL                                              │
│     ├─ GROUP BY FECHA                                                    │
│     ├─ Consultar PRODUCTOS (PORC_DESPERDICIO > 5%)                     │
│     └─> Retornar DesperdicioAnalisisVM                                   │
│                                                                             │
│  [RolAuthorize(1)] - Solo administrador                                   │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘

┌─ MODELOS (ViewModels) ─────────────────────────────────────────────────────┐
│                                                                             │
│  DesperdicioAnalisisVM                                                     │
│  ├─ FechaInicio : DateTime                                                │
│  ├─ FechaFin : DateTime                                                   │
│  ├─ TotalDesperdiciado : decimal                                          │
│  ├─ TotalTransacciones : int                                              │
│  ├─ PromedioTransaccion : decimal                                         │
│  ├─ DesperdiciosPorProducto : List<DesperdicioProductoVM>               │
│  ├─ DesperdiciosPorMaterial : List<DesperdicioMaterialVM>               │
│  ├─ DesperdiciosPorDia : List<DesperdicioDiaVM>                          │
│  └─ ProductosAltoDesperdicio : List<ProductoAltoDesperdicioVM>          │
│                                                                             │
│  DesperdicioProductoVM                                                     │
│  ├─ IdProducto : int                                                      │
│  ├─ NombreProducto : string                                               │
│  ├─ TotalDesperdiciado : decimal                                          │
│  ├─ PorcentajeDesperdicio : decimal                                       │
│  └─ CantidadTransacciones : int                                           │
│                                                                             │
│  DesperdicioMaterialVM                                                     │
│  ├─ IdMaterial : int                                                      │
│  ├─ NombreMaterial : string                                               │
│  ├─ TotalDesperdiciado : decimal                                          │
│  ├─ Unidad : string                                                       │
│  └─ CantidadTransacciones : int                                           │
│                                                                             │
│  DesperdicioDiaVM                                                          │
│  ├─ Fecha : DateTime                                                      │
│  ├─ TotalDesperdiciado : decimal                                          │
│  └─ CantidadTransacciones : int                                           │
│                                                                             │
│  ProductoAltoDesperdicioVM                                                 │
│  ├─ IdProducto : int                                                      │
│  ├─ NombreProducto : string                                               │
│  ├─ PorcentajeDesperdicio : decimal                                       │
│  ├─ TotalDesperdiciado : decimal                                          │
│  └─ Recomendacion : string                                                │
│                                                                             │
│  DesperdicioFilaVM                                                         │
│  ├─ IdDesperdicio : int                                                   │
│  ├─ Fecha : DateTime                                                      │
│  ├─ Material : string                                                     │
│  ├─ CantidadDesperdiciada : decimal                                       │
│  ├─ Unidad : string                                                       │
│  ├─ Producto : string                                                     │
│  ├─ IdVenta : int?                                                        │
│  ├─ Motivo : string                                                       │
│  ├─ Reutilizable : string                                                 │
│  └─ Usuario : string                                                      │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘

┌─ BASE DE DATOS ────────────────────────────────────────────────────────────┐
│                                                                             │
│  DESPERDICIOS_MATERIAL                                                     │
│  ├─ ID_DESPERDICIO (PK)                                                  │
│  ├─ ID_MATERIAL (FK) ──────────────────────────────> MATERIALES         │
│  ├─ CANTIDAD_DESPERDICIADA : decimal (NOT NULL)                         │
│  ├─ REUTILIZABLE : string                                                │
│  ├─ CANTIDAD_REUTILIZADA : decimal                                       │
│  ├─ MOTIVO : string                                                      │
│  ├─ FECHA : DateTime                                                     │
│  ├─ ID_USUARIO (FK) ────────────────────────────────> USUARIOS          │
│  ├─ ID_VENTA (FK) ──────────────────────────────────> VENTAS           │
│  ├─ ID_PRODUCTO (FK) ───────────────────────────────> PRODUCTOS        │
│  └─ ORIGEN : string (= "VENTA")                                          │
│                                                                             │
│  PRODUCTOS                                                                 │
│  ├─ ID_PRODUCTO (PK)                                                    │
│  ├─ NOMBRE : string                                                      │
│  ├─ PORC_DESPERDICIO : decimal (0-100) ◄─── NUEVO                       │
│  ├─ ... (otros campos)                                                   │
│  └─ Relación 1:N con DESPERDICIOS_MATERIAL                             │
│                                                                             │
│  VENTAS                                                                    │
│  ├─ ID_VENTA (PK)                                                       │
│  ├─ ID_CLIENTE                                                            │
│  ├─ FECHA                                                                 │
│  ├─ TOTAL                                                                 │
│  └─ Relación 1:N con DESPERDICIOS_MATERIAL                             │
│                                                                             │
│  MATERIALES                                                                │
│  ├─ ID_MATERIAL (PK)                                                    │
│  ├─ NOMBRE : string                                                      │
│  ├─ TIPO : string (se usa como unidad)                                  │
│  └─ Relación 1:N con DESPERDICIOS_MATERIAL                             │
│                                                                             │
│  USUARIOS                                                                  │
│  ├─ ID_USUARIO (PK)                                                    │
│  ├─ NOMBRE : string                                                      │
│  └─ Relación 1:N con DESPERDICIOS_MATERIAL                             │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘

┌─ FLUJO DE CREACIÓN DE VENTA ────────────────────────────────────────────────┐
│                                                                             │
│  POST /Ventas/Create                                                       │
│  ├─> Crear VENTAS (insert)                                                │
│  ├─> Para cada PRODUCTO en venta:                                         │
│  │   ├─> Crear DETALLES_VENTAS                                           │
│  │   ├─> Para cada MATERIAL en receta:                                   │
│  │   │   ├─> Deducir STOCK en MATERIALES (existente)                    │
│  │   │   │                                                               │
│  │   │   ├─> ◆ NUEVO: Si PRODUCTO.PORC_DESPERDICIO > 0:                │
│  │   │   │   ├─> Calcular: desperdicio = (cant_mat × porc) / 100       │
│  │   │   │   ├─> Crear DESPERDICIOS_MATERIAL con:                       │
│  │   │   │   │   ├─ ID_MATERIAL = r.ID_MATERIAL                        │
│  │   │   │   │   ├─ CANTIDAD_DESPERDICIADA = desperdicio               │
│  │   │   │   │   ├─ ID_VENTA = venta.ID_VENTA                          │
│  │   │   │   │   ├─ ID_PRODUCTO = idProd                               │
│  │   │   │   │   ├─ ORIGEN = "VENTA"                                   │
│  │   │   │   │   ├─ USUARIO = Session["IdUsuario"]                     │
│  │   │   │   │   ├─ FECHA = DateTime.Now                               │
│  │   │   │   │   ├─ REUTILIZABLE = "Si"                                │
│  │   │   │   │   └─ MOTIVO = "Desperdicio normal de producción"       │
│  │   │   │   └─> db.SaveChanges()                                       │
│  │   │   └─────────────────────────────────────────────────────────────│
│  │   └─> Actualizar venta.TOTAL                                         │
│  │                                                                       │
│  ├─> db.SaveChanges()                                                    │
│  ├─> tx.Commit()                                                         │
│  └─> RedirectToAction("Index")                                           │
│                                                                             │
│  Si error: tx.Rollback() y RedirectToAction("Create")                    │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘

┌─ INTEGRACIÓN EN MENÚ ───────────────────────────────────────────────────────┐
│                                                                             │
│  _LayoutAdministrador.cshtml                                              │
│  └─ Menú "Otros"                                                          │
│     ├─ Movimientos → /Salidas/Index                                       │
│     ├─ ► Análisis de Desperdicios → /Reportes/Desperdicios  ◄ NUEVO     │
│     ├─ ► Reportes → /Reportes/DesperdiciosDetalle           ◄ NUEVO     │
│     └─ Control Usuarios                                                   │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘

┌─ LIBRERÍAS Y FRAMEWORKS ──────────────────────────────────────────────────┐
│                                                                             │
│  Backend:                                                                  │
│  ├─ ASP.NET MVC 5.2                                                      │
│  ├─ Entity Framework 6                                                   │
│  ├─ .NET Framework 4.8                                                   │
│                                                                             │
│  Frontend (Desperdicios.cshtml):                                          │
│  ├─ Bootstrap 5.3.3 (responsive)                                         │
│  ├─ Chart.js 3.9.1 (gráficos interactivos)                              │
│  ├─ Bootstrap Icons 1.11.3 (iconos)                                      │
│  └─ HTML5 + CSS3                                                         │
│                                                                             │
│  Frontend (DesperdiciosDetalle.cshtml):                                   │
│  ├─ Bootstrap 5.3.3 (tabla responsive)                                   │
│  ├─ Bootstrap Icons 1.11.3 (iconos)                                      │
│  └─ HTML5 + CSS3                                                         │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 📊 Queries Principales

### Query 1: Obtener desperdicios totales por período
```sql
SELECT 
  SUM(CANTIDAD_DESPERDICIADA) as Total,
  COUNT(*) as Transacciones
FROM DESPERDICIOS_MATERIAL
WHERE FECHA >= @fechaInicio 
  AND FECHA <= @fechaFin 
  AND ORIGEN = 'VENTA'
```

### Query 2: Desperdicios agrupados por producto
```sql
SELECT 
  p.ID_PRODUCTO,
  p.NOMBRE,
  SUM(dm.CANTIDAD_DESPERDICIADA) as Total,
  COUNT(*) as Cantidad
FROM DESPERDICIOS_MATERIAL dm
LEFT JOIN PRODUCTOS p ON dm.ID_PRODUCTO = p.ID_PRODUCTO
WHERE dm.FECHA >= @fechaInicio 
  AND dm.FECHA <= @fechaFin 
  AND dm.ORIGEN = 'VENTA'
GROUP BY p.ID_PRODUCTO, p.NOMBRE
ORDER BY Total DESC
```

### Query 3: Productos con alto desperdicio
```sql
SELECT 
  ID_PRODUCTO,
  NOMBRE,
  PORC_DESPERDICIO
FROM PRODUCTOS
WHERE PORC_DESPERDICIO > 5 
  AND ID_ESTADO = 1
ORDER BY PORC_DESPERDICIO DESC
```

---

**Diagrama actualizado:** Abril 2026
