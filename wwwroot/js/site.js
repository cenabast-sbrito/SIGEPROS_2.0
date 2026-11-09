$(function () {
    // Inicializa ToolTip
    const tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="tooltip"]');
    const tooltipList = [...tooltipTriggerList].map(tooltipTriggerEl => new bootstrap.Tooltip(tooltipTriggerEl));


    // Menú Sidebar
    $('#hamburgerButton').on('click', function (e) {
        e.preventDefault();
        $("#sidebarMenu").toggleClass("show");
        $("#mainContent").toggleClass("shifted");
    });

    let hideTimeout;

    $("#sidebarMenu").on("mouseenter", function () {
        clearTimeout(hideTimeout);
    });

    $("#sidebarMenu").on("mouseleave", function () {
        hideTimeout = setTimeout(function () {
            $("#sidebarMenu").removeClass("show");
            $("#mainContent").removeClass("shifted");
        }, 1000);
    });

});

// 1. Deshabilitar el botón "Atrás" atrapando el evento popstate
(function () {
    window.history.pushState(null, "", window.location.href);
    window.onpopstate = function () {
        window.history.pushState(null, "", window.location.href);
    };
})();

// 2. Si el navegador recupera la página del BFCache (Back-Forward Cache), forzar recarga
window.addEventListener("pageshow", function (event) {
    if (event.persisted || (window.performance && window.performance.navigation.type === 2)) {
        window.location.reload();
    }
});

// Detectar si la página proviene del BFCache o navegación "Atrás" y forzar verificación en servidor
window.addEventListener("pageshow", function (event) {
    var historyTraversal = event.persisted || 
        (typeof window.performance != "undefined" && 
         window.performance.getEntriesByType("navigation")[0]?.type === "back_forward");

    if (historyTraversal) {
        // Redirige al inicio/logout forzando consulta real al servidor
        window.location.replace(window.location.origin + window.location.pathname);
    }
});