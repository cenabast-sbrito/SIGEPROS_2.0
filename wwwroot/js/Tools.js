function btnOFF(idBtn, textBtn) {

    const myButton = document.getElementById(idBtn);
    myButton.disabled = true;
    myButton.style.opacity = 0.7;
    myButton.textContent = textBtn;

}
function btnON(idBtn, textBtn) {

    const myButton = document.getElementById(idBtn);
    myButton.disabled = false;
    myButton.style.opacity = 1;
    myButton.textContent = textBtn;

}
function anchorOFF(idBtn) {

    const myButton = document.getElementById(idBtn);
    myButton.disabled = true;
    myButton.style.opacity = 0.1;

}
function anchorON(idBtn) {

    const myButton = document.getElementById(idBtn);
    myButton.disabled = false;
    myButton.style.opacity = 1;

}

function mensajeSistema(aviso) {

    Swal.fire('Aviso', aviso, 'info');
////    $('#mensajeSistema').text(aviso);
////    $('#modalMensajeSistema').modal('show');

}
function mensajeAccionSistema(aviso) {

    $('#mensajeAccionSistema').text(aviso);
    $('#modalAccionesSistema').modal('show');

}

function mensajeDecision(aviso) {

    $('#modalDecisionTexto').text(aviso);
    $('#modalDecision').modal('show');

}

function avisoFinProceso(mensaje, tipo = 'warning') {

    //const clases = {
    //    success: "bg-success text-white",
    //    warning: "bg-warning text-dark",
    //    error: "bg-danger text-white",
    //    info: "bg-primary text-white"
    //};
    const clases = {
        success: "success",
        warning: "warning",
        error: "error",
        info: "info"
    };

    Swal.fire('Aviso', mensaje, tipo);
/*
    showToast(
        mensaje,
        tipo,
        3000
    );*/

    /*
    const toastId = "toastMensaje";

    if ($("#" + toastId).length === 0) {

        $("body").append(`
            <div class="toast-container position-fixed top-0 end-0 p-3">
                <div id="${toastId}" class="toast" role="alert">
                    <div class="toast-body"></div>
                </div>
            </div>
        `);
    }

    const toast = $("#" + toastId);

    toast.removeClass("bg-success bg-warning bg-danger bg-primary text-white text-dark");
    toast.addClass(clases[tipo]);

    toast.find(".toast-body").text(mensaje);

    new bootstrap.Toast(toast[0], {
        delay: 5000
    }).show();
    
    */
}













function mostrarLoader(mensaje = "Consultando información...") {
    $(".loader-text").text(mensaje);
    $(".loader_Tablas").fadeIn(200);
}

function ocultarLoader() {
    $(".loader_Tablas").fadeOut(200);
}