## 🎯 GUÍA RÁPIDA - MÓDULO DE REPORTES DE DESPERDICIOS

### ¿Qué es?
Sistema automático que registra y analiza desperdicios de material en cada venta.

---

## 📍 Acceso

### Desde el Menú Principal
1. **Haz clic en "Otros"** (menú superior)
2. Verás dos opciones nuevas:
   - **"Análisis de Desperdicios"** → Gráficos y estadísticas
   - **"Reportes"** → Tabla detallada de registros

---

## 📊 Pantalla 1: Análisis de Desperdicios

### ¿Qué ves?

**Parte Superior - Filtros**
- Campo "Fecha Inicio"
- Campo "Fecha Fin"
- Botón "Filtrar"
- Botón "Ver Detalle" (va a la tabla)

**Parte Media - KPIs (4 tarjetas)**
```
┌─────────────────┐  ┌─────────────────┐  ┌──────────────┐  ┌──────────────────┐
│ Total           │  │ Transacciones   │  │ Promedio x   │  │ Productos Alto   │
│ Desperdiciado   │  │                 │  │ Transacción  │  │ Riesgo (> 5%)    │
│                 │  │                 │  │              │  │                  │
│ (valor en U)    │  │ (cantidad)      │  │ (valor en U) │  │ (cantidad)       │
└─────────────────┘  └─────────────────┘  └──────────────┘  └──────────────────┘
```

**Gráficos**
1. **Pie (izq.)** - "Desperdicios por Producto"
   - Pasa el mouse para ver detalles
   - Haz clic para ver información

2. **Bar (der.)** - "Desperdicios por Material"
   - Ordena de mayor a menor
   - Muestra cantidad desperdiciada

3. **Line (abajo)** - "Tendencia Diaria"
   - Evolución por día
   - Identifica picos

**Tabla Roja Inferior**
- Solo si hay productos con > 5% de desperdicio
- Columnas: Producto | % Desperdicio | Total | Recomendación
- Colores: Rojo (> 15%), Naranja (> 5%)

---

## 📋 Pantalla 2: Reportes (Detalle)

### ¿Qué ves?

**Filtros (igual que Análisis)**
- Rango de fechas
- Botón "Filtrar"
- Botón "Ver Gráficos" (va atrás)

**Tabla de Registros**
```
Fecha      | Material    | Cantidad | Producto   | Venta # | Usuario | Motivo | Reutilizable
-----------|-------------|----------|-----------|---------|---------|--------|-------------
14/04/2026 | Harina      | 0.50 kg  | Pan Dulce | #123    | Admin   | Normal | Si
...
```

**Columnas Explicadas:**
- **Fecha:** Cuándo se registró el desperdicio
- **Material:** Qué material se desperdició
- **Cantidad:** Cuánto se desperdició + unidad
- **Producto:** Qué producto lo causó
- **Venta #:** A qué venta estaba vinculado
- **Usuario:** Quién registró la venta
- **Motivo:** Razón del desperdicio
- **Reutilizable:** Si se puede reutilizar (Si/No)

**Paginación**
- Muestra "Mostrando X de Y"
- Botones: Primera | Anterior | [1] [2] [3] | Siguiente | Última
- 15 registros por página

---

## 🔧 ¿Cómo Funciona Automáticamente?

### Cuando Creas una Venta

```
1. Seleccionas un producto
2. Seleccionas cantidad
3. Das clic en "Crear Venta"

Automáticamente el sistema:
├─ Deduce stock del material (como siempre)
├─ Calcula desperdicio:
│  └─ Desperdicio = (Cantidad Material Usado) × (% del Producto / 100)
└─ Crea un registro de DESPERDICIOS_MATERIAL
   ├─ ID_MATERIAL: El material que se desperdició
   ├─ CANTIDAD: Lo que se calculó que se desperdició
   ├─ ID_PRODUCTO: El producto que lo causó
   ├─ ID_VENTA: La venta que lo generó
   └─ ORIGEN: "VENTA" (para saber que fue automático)
```

### Ejemplo Real

**Producto: "Pan Dulce"**
- % Desperdicio: 10%
- Receta: 5 kg de Harina por unidad

**Venta: 3 unidades de Pan Dulce**
- Material usado: 3 × 5 kg = 15 kg de Harina
- Desperdicio calculado: 15 kg × (10 / 100) = 1.5 kg
- Se registra: DESPERDICIOS_MATERIAL { Material: Harina, Cantidad: 1.5 kg, ... }

**Resultado en Reportes**
- Ves 1.5 kg menos en "Total Desperdiciado"
- En gráfico Pie: aparece Pan Dulce con su proporción
- En gráfico Bar: Harina suma +1.5 kg
- En tabla: nuevo registro el día de la venta

---

## ⚙️ Configurar % de Desperdicio

**PRÓXIMO PASO:**
Se añadirá un campo editable en cada producto (Productos → Editar)

**Por ahora:**
- Contacta al desarrollador para cambiar % manualmente
- O edita directamente en BD: `PRODUCTOS.PORC_DESPERDICIO`
- Rango: 0 a 100 (%)

---

## 💡 Casos de Uso

### Caso 1: "Nuestro Pan Dulce tiene mucho desperdicio"
1. Menú → Análisis de Desperdicios
2. Mira la tabla roja "Productos con Alto Desperdicio"
3. Si "Pan Dulce" está con 15%+ → problema crítico
4. Recomendación: Revisar receta, procesos, calidad

### Caso 2: "Quiero revisar qué pasó el 10 de abril"
1. Menú → Reportes
2. Fecha Inicio: 10/04/2026
3. Fecha Fin: 10/04/2026
4. Clic "Filtrar"
5. Ve todos los registros de ese día
6. Haz clic en cada fila si necesitas más info

### Caso 3: "¿Cuál es el material que más se desperdicia?"
1. Menú → Análisis de Desperdicios
2. Mira el gráfico bar "Desperdicios por Material"
3. Material más grande en la barra = mayor desperdicio
4. Considera negociar mejor calidad o cambiar proveedor

### Caso 4: "Necesito datos de los últimos 3 meses"
1. Menú → Análisis de Desperdicios
2. Fecha Inicio: 01/01/2026
3. Fecha Fin: 31/03/2026
4. Clic "Filtrar"
5. Ve gráficos, KPIs y tabla de período completo

---

## 🎨 Leyenda de Colores

### En Análisis de Desperdicios

| Color | Significado |
|-------|-------------|
| 🔴 Rojo | Producto > 15% desperdicio (crítico) |
| 🟠 Naranja | Producto 5-15% desperdicio (atención) |
| 🟢 Verde | Ok, desperdicio normal |
| 🔵 Azul | Tendencia, material |

### En Reportes (Tabla)

| Color | Significado |
|-------|-------------|
| 🟣 Morado | Encabezado (información importante) |
| ⚫ Gris | Filas alternas (para leer mejor) |
| 🟢 Verde | Badge "Si" en Reutilizable |
| ⚫ Gris | Badge "No" en Reutilizable |
| 🔵 Azul | ID Venta |

---

## ❓ Preguntas Frecuentes

**P: ¿Se registran desperdicios automáticamente?**  
R: Sí, cada vez que creas una venta con un producto que tenga % > 0.

**P: ¿Qué pasa si el % de desperdicio es 0?**  
R: No se registra nada en DESPERDICIOS_MATERIAL.

**P: ¿Puedo editar un registro de desperdicio?**  
R: No, son registros auditados. Si necesitas correcciones, contacta admin.

**P: ¿Los gráficos se actualizan automáticamente?**  
R: No. Debes hacer clic "Filtrar" para recargar.

**P: ¿Puedo exportar los datos?**  
R: No en esta versión. Próximamente.

**P: ¿Por qué aparecen 0 registros?**  
R: Posibles razones:
   - No hay ventas en el período seleccionado
   - Todos los productos tienen PORC_DESPERDICIO = 0
   - Intenta ampliar el rango de fechas

---

## 📞 Soporte

Para problemas:
1. Verifica que el % de desperdicio del producto sea > 0
2. Comprueba que hayas creado ventas después de configurar el %
3. Revisa los filtros de fecha
4. Si persiste, contacta al administrador del sistema

---

**Última actualización:** Abril 2026
