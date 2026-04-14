## 📊 MÓDULO DE REPORTES Y ANÁLISIS DE DESPERDICIOS

### Descripción
Se implementó un módulo completo de análisis y reportes de desperdicios de material conectado automáticamente al proceso de ventas.

### ✨ Características Implementadas

#### 1. **Grabación Automática de Desperdicios en Ventas**
- **Ubicación:** `VentasController.cs` - Método `Create()`
- **Funcionalidad:** Al crear una venta, el sistema calcula automáticamente:
  - Cantidad de material usado = `cantidad_vendida × cantidad_usada_receta`
  - Cantidad desperdiciada = `cantidad_usada × (PORC_DESPERDICIO / 100)`
- **Datos Grabados:**
  - ID_MATERIAL (material desperdiciado)
  - CANTIDAD_DESPERDICIADA (calculada)
  - ID_VENTA (referencia a la venta)
  - ID_PRODUCTO (producto que causó el desperdicio)
  - ORIGEN = "VENTA" (para auditoría)
  - USUARIO (quien registró la venta)
  - FECHA (timestamp actual)
  - REUTILIZABLE = "Si" (por defecto)
  - MOTIVO = "Desperdicio normal de producción"

#### 2. **Controlador de Reportes**
- **Ubicación:** `Controllers/ReportesController.cs`
- **Acciones Principales:**
  - `Index()` → Redirecciona a Desperdicios
  - `Desperdicios()` → Análisis visual con gráficos
  - `DesperdiciosDetalle()` → Tabla detallada con paginación
  - `DesperdiciosJSON()` → API para gráficos dinámicos

#### 3. **Análisis Visual (Dashboard)**
- **Ubicación:** `Views/Reportes/Desperdicios.cshtml`
- **KPIs Mostrados:**
  - Total Desperdiciado (unidades)
  - Cantidad de Transacciones
  - Promedio por Transacción
  - Productos con Alto Riesgo

- **Gráficos Implementados:**
  1. **Pie Chart** → Desperdicios por Producto (% distribución)
  2. **Bar Chart** → Desperdicios por Material (ranking)
  3. **Line Chart** → Tendencia Diaria de Desperdicios

- **Tabla de Alerta:**
  - Productos con % desperdicio > 5%
  - Resaltados en rojo si > 15%
  - Incluye recomendaciones

- **Filtros:**
  - Fecha Inicio / Fecha Fin
  - Rango por defecto: últimos 30 días
  - Botones rápidos: Gráficos ↔ Detalle

#### 4. **Reporte Detallado**
- **Ubicación:** `Views/Reportes/DesperdiciosDetalle.cshtml`
- **Información por Registro:**
  - Fecha/Hora
  - Material desperdiciado
  - Cantidad + Unidad
  - Producto origen
  - ID Venta
  - Usuario responsable
  - Motivo
  - Reutilizable (Si/No)

- **Características:**
  - Paginación (15 registros por página)
  - Tabla responsive
  - Formato de fecha/hora legible
  - Badges de estado
  - Botón para volver a gráficos

#### 5. **ViewModels**
- **Ubicación:** `Models/Reportes/DesperdicioAnalisisVM.cs`
- **Clases:**
  - `DesperdicioAnalisisVM` (contenedor principal)
  - `DesperdicioProductoVM` (agregación por producto)
  - `DesperdicioMaterialVM` (agregación por material)
  - `DesperdicioDiaVM` (tendencia diaria)
  - `ProductoAltoDesperdicioVM` (productos críticos)
  - `DesperdicioFilaVM` (fila de tabla)

### 🔗 Integración en el Menú

**Menú:** "Otros" (en `_LayoutAdministrador.cshtml`)
- ✅ Movimientos (existente)
- ✅ **Análisis de Desperdicios** → `/Reportes/Desperdicios`
- ✅ **Reportes** → `/Reportes/DesperdiciosDetalle`
- Control Usuarios (existente)

### 📈 Flujo de Datos

```
VENTAS (Crear) 
    ↓
VentasController.Create()
    ↓
Para cada PRODUCTO_MATERIAL en la receta:
    - Calcular desperdicio = (cant_vendida × cant_receta) × (PORC_DESPERDICIO / 100)
    - Crear registro DESPERDICIOS_MATERIAL
    - Enlazar a VENTA, PRODUCTO, MATERIAL
    ↓
DESPERDICIOS_MATERIAL (tabla)
    ↓
ReportesController.Desperdicios() → Gráficos + KPIs
ReportesController.DesperdiciosDetalle() → Tabla detallada
```

### 🛠️ Configuración de PORC_DESPERDICIO

El % de desperdicio se configura por producto en:
- **Ubicación:** `PRODUCTOS` tabla
- **Campo:** `PORC_DESPERDICIO` (DECIMAL, 0-100)
- **Edición:** Será en la pantalla de Productos/Edit (próximo paso si lo deseas)
- **Valor por Defecto:** 0 (sin desperdicio)

### 📊 Ejemplo de Cálculo

Si una venta incluye:
- Producto "Pan Dulce" con PORC_DESPERDICIO = 10%
- Material "Harina" usado: 5 kg × cantidad_vendida

Resultado:
- Desperdicio calculado = 5 kg × (10 / 100) = 0.5 kg
- Se crea registro: DESPERDICIOS_MATERIAL { ID_MATERIAL: Harina, CANTIDAD_DESPERDICIADA: 0.5, ... }

### ✅ Validaciones

- Solo registra desperdicios con ORIGEN = "VENTA"
- Solo incluye productos con PORC_DESPERDICIO > 0
- Filtra por fecha automáticamente
- Rango por defecto: últimos 30 días (configurable)

### 🔐 Seguridad

- Acceso restringido a `[RolAuthorize(1)]` (solo administrador)
- Datos auditados en BITACORA
- Transacciones protegidas en Create()

### 📱 Diseño

- Responsive Bootstrap 5
- Gráficos con Chart.js 3.9
- Cards con sombra elegante
- Colores diferenciados por severidad

### 🚀 Próximos Pasos (Opcional)

1. **Editor de PORC_DESPERDICIO** en Productos/Edit
2. **Exportar a Excel** desde reportes
3. **Alertas automáticas** si desperdicio excede umbral
4. **Comparativas mensuales** o anuales
5. **Integración con "Movimientos"** (unificar en un dashboard)
