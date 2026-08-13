/**
 * ALPHASTOCK - COMPONENTES Y ALERTAS ESTANDARIZADAS
 * Sistema centralizado de notificaciones y diálogos de confirmación
 */

const AlphaStockUI = {
    /**
     * Mostrar alerta de éxito
     */
    success: function(mensaje, titulo = '¡Éxito!', duracion = 5000) {
        this._mostrarAlerta(mensaje, titulo, 'success', '?', duracion);
    },

    /**
     * Mostrar alerta de error
     */
    error: function(mensaje, titulo = '¡Error!', duracion = 5000) {
        this._mostrarAlerta(mensaje, titulo, 'danger', '?', duracion);
    },

    /**
     * Mostrar alerta de advertencia
     */
    warning: function(mensaje, titulo = 'Advertencia', duracion = 5000) {
        this._mostrarAlerta(mensaje, titulo, 'warning', '?', duracion);
    },

    /**
     * Mostrar alerta de información
     */
    info: function(mensaje, titulo = 'Información', duracion = 5000) {
        this._mostrarAlerta(mensaje, titulo, 'info', '?', duracion);
    },

    /**
     * Diálogo de confirmación
     */
    confirm: function(mensaje, titulo = '¿Confirmar acción?', callback = null) {
        const dialog = document.createElement('div');
        dialog.className = 'confirm-overlay';
        dialog.innerHTML = `
            <div class="confirm-dialog">
                <div class="confirm-dialog__title">${titulo}</div>
                <div class="confirm-dialog__message">${mensaje}</div>
                <div class="confirm-dialog__actions">
                    <button class="btn btn-outline-primary" onclick="this.closest('.confirm-overlay').remove()">
                        Cancelar
                    </button>
                    <button class="btn btn-danger" onclick="AlphaStockUI._confirmar(this)">
                        Confirmar
                    </button>
                </div>
            </div>
        `;
        
        document.body.appendChild(dialog);
        
        // Guardar callback
        dialog._callback = callback;
        
        // Cerrar al hacer click fuera
        dialog.addEventListener('click', (e) => {
            if (e.target === dialog) dialog.remove();
        });
        
        // Cerrar con ESC
        const handler = (e) => {
            if (e.key === 'Escape') {
                dialog.remove();
                document.removeEventListener('keydown', handler);
            }
        };
        document.addEventListener('keydown', handler);
    },

    /**
     * Confirmación con dos opciones personalizadas
     */
    confirmCustom: function(mensaje, titulo, textBtnIzq, textBtnDer, callbackIzq, callbackDer) {
        const dialog = document.createElement('div');
        dialog.className = 'confirm-overlay';
        dialog.innerHTML = `
            <div class="confirm-dialog">
                <div class="confirm-dialog__title">${titulo}</div>
                <div class="confirm-dialog__message">${mensaje}</div>
                <div class="confirm-dialog__actions">
                    <button class="btn btn-outline-primary" onclick="AlphaStockUI._confirmarCustom(this, 'izq')">
                        ${textBtnIzq}
                    </button>
                    <button class="btn btn-danger" onclick="AlphaStockUI._confirmarCustom(this, 'der')">
                        ${textBtnDer}
                    </button>
                </div>
            </div>
        `;
        
        document.body.appendChild(dialog);
        dialog._callbackIzq = callbackIzq;
        dialog._callbackDer = callbackDer;
        
        dialog.addEventListener('click', (e) => {
            if (e.target === dialog) dialog.remove();
        });
    },

    /**
     * Modal de carga
     */
    loading: function(show = true, mensaje = 'Cargando...') {
        let loader = document.getElementById('alphastock-loader');
        if (!loader) {
            loader = document.createElement('div');
            loader.id = 'alphastock-loader';
            loader.className = 'alphastock-loader';
            loader.innerHTML = `
                <div class="loader-content">
                    <div class="spinner"></div>
                    <p>${mensaje}</p>
                </div>
            `;
            document.body.appendChild(loader);
        }
        
        if (show) {
            loader.style.display = 'flex';
            loader.querySelector('p').textContent = mensaje;
        } else {
            loader.style.display = 'none';
        }
    },

    /**
     * Tabla dinámica con filtro
     */
    crearTabla: function(contenedorId, columnas, datos) {
        const container = document.getElementById(contenedorId);
        if (!container) return;

        let html = `
            <div class="tabla-wrapper">
                <input type="text" class="tabla-filtro" placeholder="Buscar...">
                <table class="tabla-dinamica">
                    <thead>
                        <tr>
                            ${columnas.map(col => `<th>${col}</th>`).join('')}
                        </tr>
                    </thead>
                    <tbody>
                        ${datos.map(fila => `
                            <tr>
                                ${fila.map(celda => `<td>${celda}</td>`).join('')}
                            </tr>
                        `).join('')}
                    </tbody>
                </table>
            </div>
        `;

        container.innerHTML = html;

        // Agregar filtro
        const input = container.querySelector('.tabla-filtro');
        const tabla = container.querySelector('.tabla-dinamica');
        
        input.addEventListener('keyup', (e) => {
            const filtro = e.target.value.toLowerCase();
            const filas = tabla.querySelectorAll('tbody tr');
            filas.forEach(fila => {
                fila.style.display = fila.textContent.toLowerCase().includes(filtro) ? '' : 'none';
            });
        });
    },

    /**
     * Validar formulario
     */
    validarFormulario: function(formularioId) {
        const form = document.getElementById(formularioId);
        if (!form) return false;

        const campos = form.querySelectorAll('[required]');
        let valido = true;

        campos.forEach(campo => {
            if (!campo.value || campo.value.trim() === '') {
                campo.classList.add('is-invalid');
                valido = false;
            } else {
                campo.classList.remove('is-invalid');
            }
        });

        return valido;
    },

    /**
     * Método privado: mostrar alerta
     */
    _mostrarAlerta: function(mensaje, titulo, tipo, icono, duracion) {
        const alerta = document.createElement('div');
        alerta.className = `alert alert-${tipo}`;
        alerta.innerHTML = `
            <div class="alert-icon">${icono}</div>
            <div>
                <strong>${titulo}</strong><br>
                ${mensaje}
            </div>
        `;

        const contenedor = document.querySelector('.page-content') || document.body;
        contenedor.insertBefore(alerta, contenedor.firstChild);

        if (duracion > 0) {
            setTimeout(() => {
                alerta.style.animation = 'fadeOut 0.3s ease-out';
                setTimeout(() => alerta.remove(), 300);
            }, duracion);
        }
    },

    /**
     * Método privado: confirmar
     */
    _confirmar: function(btn) {
        const dialog = btn.closest('.confirm-overlay');
        const callback = dialog._callback;
        dialog.remove();
        if (typeof callback === 'function') callback();
    },

    /**
     * Método privado: confirmar custom
     */
    _confirmarCustom: function(btn, lado) {
        const dialog = btn.closest('.confirm-overlay');
        const callback = lado === 'izq' ? dialog._callbackIzq : dialog._callbackDer;
        dialog.remove();
        if (typeof callback === 'function') callback();
    }
};

/**
 * Estilos para componentes (se inyectan en la página)
 */
const estilosComponentes = `
<style>
    /* Overlay de confirmación */
    .confirm-overlay {
        position: fixed;
        top: 0;
        left: 0;
        right: 0;
        bottom: 0;
        background-color: rgba(0, 0, 0, 0.5);
        display: flex;
        align-items: center;
        justify-content: center;
        z-index: 9999;
        animation: fadeIn 0.3s ease-in;
    }

    @keyframes fadeIn {
        from { opacity: 0; }
        to { opacity: 1; }
    }

    @keyframes fadeOut {
        from { opacity: 1; }
        to { opacity: 0; }
    }

    /* Loader */
    .alphastock-loader {
        position: fixed;
        top: 0;
        left: 0;
        right: 0;
        bottom: 0;
        background-color: rgba(0, 0, 0, 0.7);
        display: none;
        align-items: center;
        justify-content: center;
        z-index: 10000;
    }

    .loader-content {
        background: white;
        border-radius: 12px;
        padding: 2rem;
        text-align: center;
        box-shadow: 0 10px 40px rgba(0, 0, 0, 0.2);
    }

    .loader-content .spinner {
        margin-bottom: 1rem;
    }

    /* Tabla dinámica */
    .tabla-wrapper {
        margin-bottom: 1.5rem;
    }

    .tabla-filtro {
        width: 100%;
        max-width: 300px;
        margin-bottom: 1rem;
        padding: 0.5rem 0.75rem;
        border: 1px solid #dee2e6;
        border-radius: 8px;
    }

    .tabla-dinamica {
        width: 100%;
        border-collapse: collapse;
    }

    .tabla-dinamica thead th {
        background-color: #f8f9fa;
        font-weight: 600;
        padding: 1rem;
        border-bottom: 2px solid #dee2e6;
    }

    .tabla-dinamica tbody td {
        padding: 1rem;
        border-bottom: 1px solid #dee2e6;
    }

    .tabla-dinamica tbody tr:hover {
        background-color: #f8f9fa;
    }

    /* Formulario inválido */
    .is-invalid {
        border-color: #dc3545 !important;
        box-shadow: 0 0 0 0.2rem rgba(220, 53, 69, 0.25) !important;
    }
</style>
`;

// Inyectar estilos al cargar
document.addEventListener('DOMContentLoaded', () => {
    if (!document.querySelector('#alphastock-estilos')) {
        const style = document.createElement('div');
        style.id = 'alphastock-estilos';
        style.innerHTML = estilosComponentes;
        document.head.appendChild(style);
    }
});
