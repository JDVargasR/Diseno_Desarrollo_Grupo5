// AlphaStock UI Helper Library
window.AlphaConfirm = {
    preguntar: async function (opciones) {
        var opts = {
            titulo: (opciones && opciones.titulo) || "Confirmación",
            mensaje: (opciones && opciones.mensaje) || "¿Está seguro?",
            btnAceptar: (opciones && opciones.btnAceptar) || "Aceptar",
            btnCancelar: (opciones && opciones.btnCancelar) || "Cancelar"
        };

        if (typeof Swal !== "undefined") {
            var result = await Swal.fire({
                title: opts.titulo,
                text: opts.mensaje,
                icon: "warning",
                showCancelButton: true,
                confirmButtonColor: "#3085d6",
                cancelButtonColor: "#d33",
                confirmButtonText: opts.btnAceptar,
                cancelButtonText: opts.btnCancelar
            });
            return result.isConfirmed;
        } else {
            return confirm(opts.titulo + "\n\n" + opts.mensaje);
        }
    },

    antes: function (event, opciones) {
        var self = this;
        event.preventDefault();
        this.preguntar(opciones).then(function(confirmado) {
            if (confirmado) {
                var form = event.target.closest("form");
                if (form) form.submit();
            }
        });
    }
};

// Alias para compatibilidad
window.AlphaStockUI = window.AlphaConfirm;

