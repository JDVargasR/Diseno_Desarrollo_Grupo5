(() => {
    "use strict";

    const $ = (id) => document.getElementById(id);

    // ===== CRUD (Modal) =====
    const btnNuevoRol = $("btnNuevoRol");
    const formRol = $("formRol"); // Form para el modal
    const rolId = $("rolId");
    const rolNombre = $("rolNombre");
    const rolDescripcion = $("rolDescripcion");
    const rolEstado = $("rolEstado");
    const modalTitulo = $("modalTitulo");

    const modalEl = $("modalRol");
    let modalInstance = null;

    // ===== Filtros (Tiempo real) =====
    const frmFiltroRoles = $("frmFiltroRoles");
    const txtBuscar = $("txtBuscar");
    const selEstado = $("selEstado");

    document.addEventListener("DOMContentLoaded", () => {
        // 1) Inicializar Bootstrap modal
        if (window.bootstrap && modalEl) {
            modalInstance = new bootstrap.Modal(modalEl, { backdrop: "static", keyboard: false });
        }

        // 2) Mostrar SweetAlert de TempData (si existe)
        showServerSweetAlert();

        // 3) Nuevo rol
        btnNuevoRol?.addEventListener("click", () => openCreateModal());

        // 4) Click en Editar (botones en tabla)
        document.querySelectorAll("[data-action='edit']").forEach((btn) => {
            btn.addEventListener("click", () => openEditModal(btn));
        });

        // 5) Click en Inactivar (borrado lógico)
        document.querySelectorAll("[data-action='deactivate']").forEach((btn) => {
            btn.addEventListener("click", () => confirmDeactivate(btn));
        });

        // 6) Guardar (submit del modal)
        formRol?.addEventListener("submit", (e) => {
            e.preventDefault();
            confirmSave();
        });

        // 7) Filtros en tiempo real (sin botón Filtrar)
        initLiveFilters();
    });

    // ===== SweetAlert desde servidor (TempData -> data-attrs) =====
    function showServerSweetAlert() {
        // En la vista debe existir:
        // <div id="swalMsg" data-ok="..." data-error="..." style="display:none"></div>
        const el = $("swalMsg");
        if (!el || !window.Swal) return;

        const ok = (el.dataset.ok || "").trim();
        const error = (el.dataset.error || "").trim();

        if (ok.length > 0) {
            Swal.fire({ icon: "success", title: "Listo", text: ok });
            return;
        }

        if (error.length > 0) {
            Swal.fire({ icon: "error", title: "Ups", text: error });
        }
    }

    // ===== Filtros en vivo (GET submit) =====
    function initLiveFilters() {
        if (!frmFiltroRoles) return;

        // Debounce para no spammear submits mientras escribe
        let t = null;

        // Escribir en buscar -> submit con pausa
        txtBuscar?.addEventListener("input", () => {
            clearTimeout(t);
            t = setTimeout(() => {
                frmFiltroRoles.submit();
            }, 450);
        });

        // Enter en buscar -> submit inmediato
        txtBuscar?.addEventListener("keydown", (e) => {
            if (e.key === "Enter") {
                e.preventDefault();
                clearTimeout(t);
                frmFiltroRoles.submit();
            }
        });

        // Cambiar estado -> submit inmediato
        selEstado?.addEventListener("change", () => {
            clearTimeout(t);
            frmFiltroRoles.submit();
        });
    }

    // ===== Modal =====
    function openCreateModal() {
        modalTitulo.textContent = "Nuevo Rol";
        rolId.value = "0";
        rolNombre.value = "";
        rolDescripcion.value = "";
        rolEstado.value = "1";

        // En el form debe venir: data-create-url y data-update-url
        if (formRol?.dataset?.createUrl) {
            formRol.action = formRol.dataset.createUrl;
        }

        modalInstance?.show();
        setTimeout(() => rolNombre.focus(), 150);
    }

    function openEditModal(btn) {
        const id = btn.dataset.id || "0";
        const nombre = btn.dataset.nombre || "";
        const descripcion = btn.dataset.descripcion || "";
        const estado = btn.dataset.estado || "1";

        modalTitulo.textContent = `Editar Rol #${id}`;
        rolId.value = id;
        rolNombre.value = nombre;
        rolDescripcion.value = descripcion;
        rolEstado.value = estado;

        if (formRol?.dataset?.updateUrl) {
            formRol.action = formRol.dataset.updateUrl;
        }

        modalInstance?.show();
        setTimeout(() => rolNombre.focus(), 150);
    }

    // ===== Guardar =====
    async function confirmSave() {
        const nombre = (rolNombre?.value || "").trim();

        if (nombre.length < 2) {
            await Swal.fire({
                icon: "warning",
                title: "Validación",
                text: "El nombre del rol debe tener al menos 2 caracteres.",
            });
            rolNombre?.focus();
            return;
        }

        const isCreate = (rolId?.value || "0") === "0";
        const title = isCreate ? "¿Crear rol?" : "¿Guardar cambios?";

        const res = await Swal.fire({
            icon: "question",
            title,
            html: `Rol: <b>${escapeHtml(nombre)}</b>`,
            showCancelButton: true,
            confirmButtonText: "Sí, guardar",
            cancelButtonText: "Cancelar",
        });

        if (!res.isConfirmed) return;

        formRol?.submit();
    }

    // ===== Inactivar =====
    async function confirmDeactivate(btn) {
        const formId = btn.dataset.form; // Id del form oculto
        const nombre = btn.dataset.nombre || "";

        const res = await Swal.fire({
            icon: "warning",
            title: "¿Inactivar rol?",
            html: `Se marcará como <b>INACTIVO</b>:<br><b>${escapeHtml(nombre)}</b>`,
            showCancelButton: true,
            confirmButtonText: "Sí, inactivar",
            cancelButtonText: "Cancelar",
            confirmButtonColor: "#d33",
        });

        if (!res.isConfirmed) return;

        const f = document.getElementById(formId);
        if (f) f.submit();
    }

    // ===== Utils =====
    function escapeHtml(str) {
        return String(str)
            .replaceAll("&", "&amp;")
            .replaceAll("<", "&lt;")
            .replaceAll(">", "&gt;")
            .replaceAll('"', "&quot;")
            .replaceAll("'", "&#039;");
    }
})();
