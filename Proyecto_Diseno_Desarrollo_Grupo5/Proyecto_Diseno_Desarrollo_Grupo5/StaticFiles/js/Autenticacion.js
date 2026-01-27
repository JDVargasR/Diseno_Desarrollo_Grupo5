document.addEventListener("DOMContentLoaded", () => {

    // Mostrar SweetAlert si la vista trae mensajes en data-*
    const box = document.querySelector("[data-swal]");
    if (box) {
        const type = box.getAttribute("data-type") || "info";
        const title = box.getAttribute("data-title") || "Mensaje";
        const text = box.getAttribute("data-text") || "";

        Swal.fire({
            icon: type,
            title: title,
            text: text,
            confirmButtonColor: "#DB763B"
        });
    }

});
