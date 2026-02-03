// StaticFiles/js/Index.js

document.addEventListener("DOMContentLoaded", () => {
    // Siempre arrancar arriba
    if ("scrollRestoration" in history) history.scrollRestoration = "manual";
    window.scrollTo(0, 0);

    // Footer visible solo arriba y abajo
    const footer = document.getElementById("lpFooter");
    if (!footer) return;

    const TH = 28;

    const toggleFooter = () => {
        const y = window.scrollY || 0;
        const h = window.innerHeight || 0;
        const doc = Math.max(document.body.scrollHeight, document.documentElement.scrollHeight);

        footer.classList.toggle("show", y <= TH || (y + h) >= (doc - TH));
    };

    toggleFooter();
    window.addEventListener("scroll", toggleFooter, { passive: true });
    window.addEventListener("resize", toggleFooter);
});

window.addEventListener("load", () => window.scrollTo(0, 0));
