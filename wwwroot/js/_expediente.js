async function consultarEstadoExpediente(idJob, rut, factura) {

    const timer = setInterval(async () => {

        try {

            const estado = await $.ajax({
                type: "GET",
                url: "../App/ObtenerEstadoJob",
                data: {
                    idJob: idJob
                }
            });

            $('#estadoExpediente').html(`
                <div class="alert alert-info mb-0">
                    <i class="fas fa-cogs fa-spin"></i>
                    ${estado.mensaje}
                    <br>
                    <strong>${estado.porcentaje}%</strong>
                </div>
            `);

            if (
                estado.estado === "FINALIZADO" ||
                estado.estado === "FINALIZADO_CON_OBSERVACIONES"
            ) {

                clearInterval(timer);

                expedienteFinalizado(
                    rut,
                    factura,
                    estado
                );
            }

            if (estado.estado === "ERROR") {

                clearInterval(timer);

                $('#estadoExpediente').html(`
                    <div class="alert alert-danger mb-0">
                        <i class="fas fa-exclamation-triangle"></i>
                        El proceso terminó con error.
                    </div>
                `);
            }

        }
        catch (e) {

            console.error(e);

        }

    }, 3000);
}
function expedienteFinalizado(
    rut,
    factura,
    estado) {
    const botonExpediente =
        $(`#getExpediente_${rut}${factura}`);

    const linkExpediente =
        $(`#linkExpediente_${rut}${factura}`);

    linkExpediente.attr(
        'title',
        'Ver expediente');

    linkExpediente.find('i')
        .css('color', '#1976d2');

    botonExpediente.html(
        `<i class="fas fa-check-circle text-success"></i>`);

    botonExpediente.removeAttr('onclick');

    botonExpediente.addClass('is-disabled');

    botonExpediente.attr(
        'title',
        'Expediente generado');

    $('#estadoExpediente').html(`
        <div class="alert alert-success mb-0">
            <i class="fas fa-check-circle"></i>
            Expediente construido correctamente.
        </div>
    `);

    avisoFinProceso(
        'Expediente construido correctamente');

    setTimeout(() => {

        $('#_Factura_Confirmar_Expediente')
            .modal('hide');

    }, 1000);
}

