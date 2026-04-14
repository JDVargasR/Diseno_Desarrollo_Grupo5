# 🖼️ VISTA PREVIA DE INTERFACES

## 1️⃣ MENÚ PRINCIPAL (Actualizado)

```
┌─────────────────────────────────────────────────────────────────────────┐
│  AlphaStock                      Inicio    Servicios    Otros ▼    Usuario ▼ │
└─────────────────────────────────────────────────────────────────────────┘

┌─ MENÚ "OTROS" ──────────────────────┐
│                                     │
│  ✓ Movimientos                      │  ← Existente
│  ► Análisis de Desperdicios   ◄─── NUEVO
│  ► Reportes                  ◄─── NUEVO
│  ✓ Control Usuarios                 │
│                                     │
└─────────────────────────────────────┘
```

---

## 2️⃣ CENTRO DE REPORTES (Página de Bienvenida)

```
╔═════════════════════════════════════════════════════════════════════════╗
║                    Centro de Análisis y Reportes                       ║
║  Accede a los análisis detallados del sistema de inventarios           ║
╚═════════════════════════════════════════════════════════════════════════╝

┌─────────────┐  ┌─────────────┐  ┌─────────────┐
│ 📊 Análisis │  │ 📋 Reportes │  │ 📈 Más      │
│             │  │             │  │ Reportes    │
│ Desperdicios│  │ Desperdicios│  │             │
│             │  │ Detallado   │  │ (Próx.)     │
│ [Ver]       │  │             │  │ [Próx.]     │
│             │  │ [Ver Detalle│  │             │
└─────────────┘  └─────────────┘  └─────────────┘
```

---

## 3️⃣ PANEL DE ANÁLISIS (Dashboard Principal)

```
╔═════════════════════════════════════════════════════════════════════════╗
║ 📊 Análisis de Desperdicios                                            ║
║ Análisis detallado de desperdicios de material en el período seleccionado
╚═════════════════════════════════════════════════════════════════════════╝

┌─ FILTROS ───────────────────────────────────────────────────────────────┐
│                                                                         │
│ Fecha Inicio: [2026-03-15]  Fecha Fin: [2026-04-14]  [Filtrar] [Detalle]
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘

┌─ KPIs ──────────────────────────────────────────────────────────────────┐
│                                                                         │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  ┌────────────┐ │
│  │ Total        │  │ Transacciones│  │ Promedio x   │  │ Productos  │ │
│  │ Desperdiciado│  │              │  │ Transacción  │  │ Alto Riesgo│ │
│  │              │  │              │  │              │  │            │ │
│  │ 450.75       │  │ 23           │  │ 19.60        │  │ 4          │ │
│  │ Unidades     │  │ Registros    │  │ Unidades     │  │ > 5%       │ │
│  └──────────────┘  └──────────────┘  └──────────────┘  └────────────┘ │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘

┌─ GRÁFICOS ──────────────────────────────────────────────────────────────┐
│                                                                         │
│  ┌─────────────────────────────┐  ┌────────────────────────────────┐  │
│  │ Desperdicios por Producto   │  │ Desperdicios por Material     │  │
│  │                             │  │                              │  │
│  │        Pan (45%)            │  │  Harina        ████████ 180  │  │
│  │     Queso (25%)             │  │  Azúcar        ████ 95       │  │
│  │   Galletas (20%)            │  │  Mantequilla   ██ 50         │  │
│  │    Otros (10%)              │  │  Huevos        ██ 35         │  │
│  │                             │  │  Levadura      █ 10          │  │
│  └─────────────────────────────┘  └────────────────────────────────┘  │
│                                                                         │
│  ┌────────────────────────────────────────────────────────────────┐   │
│  │ Tendencia Diaria de Desperdicios                             │   │
│  │                                                               │   │
│  │  Unidades ▲                                                   │   │
│  │      50 │     ╱╲                                             │   │
│  │      40 │    ╱  ╲      ╱╲                                    │   │
│  │      30 │   ╱    ╲    ╱  ╲    ╱╲                             │   │
│  │      20 │  ╱      ╲  ╱    ╲  ╱  ╲                            │   │
│  │      10 │─╱────────╲╱──────╲╱────╲─ Tendencia               │   │
│  │       0 └──────────────────────────────────► Fechas          │   │
│  │                                                               │   │
│  └────────────────────────────────────────────────────────────────┘   │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘

┌─ TABLA: Productos con Alto Desperdicio ─────────────────────────────────┐
│                                                                         │
│  Producto  │ % Desperdicio │ Total Desperdiciado │ Recomendación      │
│ ─────────────────────────────────────────────────────────────────────── │
│ Pan Dulce  │ ⚠️ 15.50%    │ 120.50              │ ⚠️ Revisar proceso │
│ Queso      │ 8.20%        │ 45.30               │ Monitorear         │
│ Galletas   │ 6.75%        │ 38.25               │ Monitorear         │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 4️⃣ REPORTE DETALLADO (Tabla con Paginación)

```
╔═════════════════════════════════════════════════════════════════════════╗
║ 📋 Detalle de Desperdicios                                             ║
║ Listado completo de registros de desperdicios con filtros              ║
╚═════════════════════════════════════════════════════════════════════════╝

┌─ FILTROS ───────────────────────────────────────────────────────────────┐
│                                                                         │
│ Fecha Inicio: [2026-03-15]  Fecha Fin: [2026-04-14]  [Filtrar] [Gráficos]
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘

┌─ TABLA DE REGISTROS (Mostrando 15 de 245) ──────────────────────────────┐
│                                                                         │
│ Fecha      │ Material  │ Cant. │ Producto  │ V.# │ Usuario │ Motivo │ R.
│ ───────────┼───────────┼───────┼───────────┼─────┼─────────┼────────┼─
│14/04 14:30 │ Harina    │ 0.50kg│ Pan Dulce │ 456 │ Admin   │ Normal │ Si
│14/04 13:15 │ Azúcar    │ 0.30kg│ Galletas  │ 455 │ Vendor1 │ Normal │ Si
│14/04 12:00 │ Mantequill│ 0.20kg│ Queso     │ 454 │ Admin   │ Normal │ No
│13/04 16:45 │ Harina    │ 1.20kg│ Pan Dulce │ 453 │ Vendor2 │ Normal │ Si
│13/04 14:20 │ Levadura  │ 0.05kg│ Pan       │ 452 │ Admin   │ Normal │ Si
│13/04 10:30 │ Huevos    │ 0.15kg│ Galletas  │ 451 │ Vendor1 │ Normal │ Si
│12/04 16:00 │ Harina    │ 0.80kg│ Pan Dulce │ 450 │ Admin   │ Normal │ Si
│12/04 13:45 │ Azúcar    │ 0.40kg│ Queso     │ 449 │ Vendor2 │ Normal │ No
│12/04 11:30 │ Mantequill│ 0.25kg│ Pan       │ 448 │ Admin   │ Normal │ Si
│11/04 15:00 │ Harina    │ 0.60kg│ Galletas  │ 447 │ Vendor1 │ Normal │ Si
│11/04 12:15 │ Levadura  │ 0.10kg│ Pan Dulce │ 446 │ Admin   │ Normal │ Si
│11/04 09:00 │ Huevos    │ 0.20kg│ Queso     │ 445 │ Vendor2 │ Normal │ Si
│10/04 17:30 │ Harina    │ 0.45kg│ Pan       │ 444 │ Admin   │ Normal │ Si
│10/04 14:00 │ Azúcar    │ 0.35kg│ Galletas  │ 443 │ Vendor1 │ Normal │ No
│09/04 16:20 │ Mantequill│ 0.18kg│ Pan Dulce │ 442 │ Admin   │ Normal │ Si
│                                                                         │
│ Paginación: [Primera] [Anterior] [1] [2] [3] [Siguiente] [Última]    │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 5️⃣ ELEMENTOS INTERACTIVOS

### Gráficos Chart.js
```
┌─ Pie Chart ─────────────────────┐
│  Al pasar el mouse:             │
│  "Pan Dulce: 200.50 unidades"   │
│                                 │
│  Al hacer clic:                 │
│  Muestra detalles               │
└─────────────────────────────────┘

┌─ Bar Chart ─────────────────────┐
│  Horizontal, ordena de mayor    │
│  a menor                        │
│  Tooltip: cantidad exacta       │
└─────────────────────────────────┘

┌─ Line Chart ────────────────────┐
│  Línea azul con relleno         │
│  Tooltip: fecha y cantidad      │
│  Legend interactivo             │
└─────────────────────────────────┘
```

### Botones
```
┌─ Primarios (Azul) ──┐
│ [Filtrar]           │
│ [Ver Detalle]       │
│ [Ver Gráficos]      │
└─────────────────────┘

┌─ Secundarios ─────────┐
│ [Anterior] [Siguiente]│
│ [Primera] [Última]    │
└───────────────────────┘
```

---

## 6️⃣ COLORES Y ESTILOS

### Paleta
```
PRIMARIOS:
  - Azul (#0D6EFD): Botones, líneas, información
  - Rojo (#DC3545): Crítico, alerta
  - Naranja (#FFC107): Atención
  - Verde (#198754): Ok, confirmación

NEUTRALES:
  - Gris (#6C757D): Texto secundario
  - Blanco (#FFFFFF): Fondo
  - Negro (#000000): Texto principal

GRÁFICOS (10 colores):
  - #FF6384 (Rosa-rojo)
  - #36A2EB (Azul claro)
  - #FFCE56 (Amarillo)
  - #4BC0C0 (Turquesa)
  - #9966FF (Púrpura)
  - #FF9F40 (Naranja)
  - #FF6384 (Rosa)
  - #C9CBCF (Gris)
  - #4BC0C0 (Verde azul)
  - #FF9F40 (Naranja)
```

### Badges
```
┌─ Severidad ─────────────────┐
│ 🔴 CRÍTICO (> 15%)          │
│ 🟠 ATENCIÓN (5-15%)         │
│ 🟢 OK (< 5%)                │
│ 🔵 INFORMACIÓN              │
└─────────────────────────────┘

┌─ Estado ────────────────────┐
│ 🟢 Si (reutilizable)        │
│ ⚫ No (no reutilizable)      │
│ 🔵 ID Venta                 │
└─────────────────────────────┘
```

---

## 7️⃣ RESPONSIVIDAD

### Desktop (1200px+)
```
KPIs: 4 columnas
Gráficos: Pie (50%) | Bar (50%)
Línea: 100% ancho
Tabla: Scroll normal
```

### Tablet (768-1199px)
```
KPIs: 2 columnas (2 arriba, 2 abajo)
Gráficos: Pie (100%) + Bar (100%)
Línea: 100% ancho
Tabla: Scroll horizontal
```

### Mobile (< 768px)
```
KPIs: 1 columna (apilado)
Gráficos: Apilados (100%)
Línea: Apilada (100%)
Tabla: Scroll horizontal
Botones: Full width
```

---

## 8️⃣ ICONOGRAFÍA

```
Dashboard:        📊 📈
Tablas:           📋 📑
Reportes:         📄 🗂️
Desperdicios:     ♻️ ⚠️
Análisis:         📊 📉
Filtros:          🔍 🎚️
Usuarios:         👤 👥
Ventas:           💰 🛒
Productos:        📦 🏷️
Materiales:       🧪 ⚗️
```

---

## 9️⃣ FLUJO DE USUARIO (Happy Path)

```
USUARIO ENTRA A ALPHASTOCK
         ↓
    Hace clic en "Otros"
         ↓
    Ve "Análisis de Desperdicios"
         ↓
    Hace clic
         ↓
    ┌─ Selecciona rango de fechas
    │     ↓
    │  Clic "Filtrar"
    ├─────────────────────────────┐
    │                             │
    ├─> Ve 4 KPIs                 ├─> Analiza gráficos
    ├─> Ve 3 gráficos             ├─> Identifica problemas
    ├─> Ve tabla de críticos      ├─> Toma decisiones
    ├─> Puede ir a "Detalle"      ├─> Reporta al gerente
    │   para revisar registros    │
    │                             │
    └─ Haz clic en "Ver Detalle"  └─ Haz clic "Ver Gráficos"
                 ↓                           ↓
            Tabla con 15         Vuelve al Dashboard
            registros por página
                 ↓
            Pagina, filtra
            revisa detalles
                 ↓
             FIN EXITOSO
```

---

## 🔟 TOOLTIPS Y AYUDA

```
Encima de KPI:
  "Información: Total de unidades desperdiciadas en el período"

Encima de Gráfico:
  "Haz clic o pasa el mouse para detalles"

Encima de Badge Crítico:
  "Producto con alto porcentaje de desperdicio. Revisar procesos."

Encima de Paginación:
  "Página X de Y. Haz clic para navegar."

Encima de Filtro:
  "Selecciona el rango de fechas. Defecto: últimos 30 días."
```

---

**Nota:** Estas son representaciones ASCII de las interfaces. Las vistas Razor generan HTML real con Bootstrap 5 y Chart.js para máxima calidad visual.
