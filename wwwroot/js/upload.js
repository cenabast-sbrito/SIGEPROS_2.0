$(document).ready(function () {
    $("#file-upload").on("change", function () {

        let nombre = this.files.length > 0
            ? this.files[0].name
            : "Ningún archivo seleccionado";

        $("#nombreFile").text(nombre);

    });
    /*aqui es donde se ejecuta la carga de los datos */
    $('#form-uploadPlanilla').on('submit', function (event) {
        try {

            event.preventDefault();

            let fileInput = $('#file-upload')[0];
            let fechaCarga = $('#btnPeriodoCarga').val();
            let rutProveedor = $('#_rutProveedorCarga').val();
            let programaDefinido = $('#programaDefinido').val();

            // Validaciones
            if (!fileInput.files.length) {

                Swal.fire('Aviso', 'Debe seleccionar un archivo.', 'warning');
                return;
            }

            //if (!fechaCarga) {
            ////    alert('Debe seleccionar el período de carga.');
            //    return;
            //}

            if (!rutProveedor) {
                Swal.fire('Aviso', 'Debe indicar el proveedor', 'warning');
                return;
            }

            //if (!programaDefinido) {
            //    alert('Debe seleccionar un programa.');
            //    return;
            //}

            // Continúa sólo si todas las validaciones fueron exitosas
            btnPressed = 'btnFileUploadPlanilla';
            ////btnOFF(btnPressed, 'Cargando Planilla... ');
            showSpinnerDocumentos('spinnerCarga');

            let progressBar = $('#progress-bar');
            let antiForgeryToken = $('input[name=__RequestVerificationToken]').val();

            progressBar.css('width', '0%');

            let formData = new FormData();
            formData.append('archivoExcel', fileInput.files[0]);
            formData.append('__RequestVerificationToken', antiForgeryToken);
            formData.append('fechaCarga', fechaCarga);
            formData.append('rutProveedorCarga', rutProveedor);
            formData.append('programaDefinido', programaDefinido);
            formData.append('chkFiltroPeriodo', document.getElementById('chkFiltroPeriodo').checked);

            $("#nombreFile").text('Documento : ' + fileInput.files[0].name);
            $(".file-list-item").removeClass("d-none");

            $.ajax({
                url: "../Upload/UploadFile",
                type: 'POST',
                data: formData,
                processData: false,
                contentType: false,
                xhr: function () {
                    let xhr = new XMLHttpRequest();
                    xhr.upload.addEventListener('progress', function (event) {
                        if (event.lengthComputable) {
                            showToast("leyendo planilla ...", "info", 1000);
                            let percentComplete = (event.loaded / event.total) * 100;
                            progressBar.css('width', percentComplete + '%');
                            progressBar.attr('aria-valuenow', percentComplete);
                            progressBar.parent().show();
                        }
                    }, false);
                    return xhr;
                },
                xhrFields: {
                    withCredentials: true
                },

                success: function (response) {

                    ////btnON(btnPressed, 'Cargar Planilla');
                    hideSpinnerDocumentos('spinnerCarga');

                    if (response.status) {

                        $("#resumeCountFacturas").text(response.count);

                        $("#resume_count_filasSinFactura").text(response.countFilasSinFactura);
                        /////$('#resume_count_facturasNOSAP').text(response.countFacturasNOSAP);
                        $("#resume_count_filasOtroProveedor").text(response.countFilasOtroProveedor);
                        $("#resume_count_filasFueraDelPeriodo").text(response.countFilasFueraDelPeriodo);
                        $("#resume_count_filasOtroCanal").text(response.countFilasOtroCanal);

                        $("#resumeCountRechazos").text(response.countRechazos);
                        $("#resumeCountFacturasAceptadas").text(response.countFacturasAceptadas);

                        $("#resumeMessage").text(response.message);


                        getTableZDis022();

                    } else {

                        showToast("La carga no pudo completarse. Revise el archivo e intente nuevamente.", "error", 4000);

                        $("#resumeCountFacturas").text("");
                        $("#resume_count_filasSinFactura").text("");
                        ////$('#resume_count_facturasNOSAP').text("");
                        $("#resume_count_filasOtroProveedor").text("");
                        $("#resume_count_filasFueraDelPeriodo").text("");
                        $("#resume_count_filasOtroCanal").text("");
                        $("#resumeCountRechazos").text("");
                        $("#resumeCountFacturasAceptadas").text("");
                        $("#resumeMessage").text("");

                    }
                },
                error: function (xhr, status, error) {
                    ////btnON(btnPressed, 'Cargar Planilla');
                    hideSpinnerDocumentos('spinnerCarga');

                    showToast("No fue posible procesar la planilla. Verifique el archivo e intente nuevamente.", "error", 4000);
                    console.error(xhr.responseText);
                    progressBar.addClass('bg-gob-error').removeClass('bg-gob-info').removeClass('bg-gob-exito');
                }
            });
        } catch (error) {
            /////btnON(btnPressed, 'Cargar Planilla');
            hideSpinnerDocumentos('spinnerCarga');
            $("#estadoUpload").text("Ha ocurrido algún error");
        }

    });

    $('#form-consolidar').on('submit', function (event) {

        event.preventDefault();

        const $btn = $('#btnConsolidaCarga');

        try {

            $.ajax({
                url: "../Upload/consolidarUploadFile",
                type: 'POST',
                contentType: 'application/json; charset=utf-8',
                success: function (response) {
                    if (response && response.status === true) {
                        showToast("La información ha sido consolidada exitosamente.", "success", 3000);
                    } else {
                        const msg = (response && response.message) ? response.message : "No fue posible consolidar los datos.";
                        showToast(msg, "error", 6000);
                    }
                },
                error: function (xhr) {
                    //if (xhr && xhr.status === 404) {
                    //    $(".loader_Tablas").fadeOut("slow");
                    //    showToast("Tu sesión ha expirado. Redirigiendo…", "error", 5000);
                    //    setTimeout(() => window.location.href = apiUrl(""), 1500);
                    //} else {
                        showToast("Ocurrió un problema al consolidar los datos.", "error", 6000);
                    ////}
                }
            });

        } catch (error) {
            showToast("Se produjo un error inesperado durante la consolidación.", "error", 6000);
        }
    });
});



function getTableZDis022() { 
    try {
        var json = {
            topx: "TOP 500"
        }
        $('#tablaZDis022').DataTable({
            ajax: {
                url: "../Upload/getZDis022",
                data: json,
                dataType: "json",
                "dataSrc": function (json) {
                    ////console.log('ZDIS022 ->', json);
                    if (json.length > 0) {
                        return json;
                    } else {
                        return [];
                    }
                }
            },
            columns: [
                  { data: "documento_de_venta", title: "Documento Venta" }
                , { data: "pedido_de_Compra", title: "Pedido Compra" }
////                ,{ data: "Grupo_de_articulos" }
///                ,{ data: "código_Material", title: "Material" }
////                ,{ data: "nFactura" },
                ////                ,{ data: "guía_de_despacho" }
                , {
                    render: function (data, type, full) {

                        return crearBloque(
                            "fa-box",
                            "Material",
                            full.código_Material,
                            "lote",
                            full.lote
                        );

                    }, title: "Material"
                }
                , {
                    render: function (data, type, full) {

                        return crearBloque(
                            "fa-truck",
                            "Factura",
                            full.nFactura,
                            "Guía",
                            full.guía_de_despacho
                        );

                    }, title: "Documentos" 
                }
                , { data: "valor_neto", title: "Valor Neto" }
                , { data: "canal", title: "Canal" }
                , {

                    render: function (data, type, full) {

                        return crearBloque3(
                            "fa-truck",
                            "",
                            "Entrega al OL: " + (full.fecha_entrega_al_OL??""),
                            "Recepción Conforme",
                            full.fecha_recepción_conf ?? "",
                            "Carga Información",
                            full.fecha_de_carga_de_in ?? "",
                            "Vencimiento",
                            full.fecha_vencimiento ?? ""
                        );

                    }, title: "Fechas"

                }
                //{ data: "Fecha_de_entrega_al" },
                //{ data: "Cantidad_documento_d" },
                //{ data: "Cantidad_informada" },
                //{ data: "Lote" },
                //{ data: "Fecha_entrega_al_ol_" },
                //{ data: "OS" },
                //{ data: "Observaciones" },
                //{ data: "Suspensión_de_entreg" },
                //{ data: "Autoriza_suspensión" },
                //{ data: "Salida_mercancía" },
                //{ data: "Documento_de_entrega" },
                //{ data: "Status_carga_cedible" },
                //{ data: "Fecha_Sal_mercancía" },
                //{ data: "Fact_comisiones_pro" },
                //{ data: "Fecha_fact_comision" },
                //{ data: "Fecha_entrega_cedibl" },
                //{ data: "Centro" },
                //{ data: "Bultos" },
                //{ data: "Volumen" },
                //{ data: "Peso" },
                //{ data: "Fecha_vencimiento" },
                //{ data: "GTIN" },
                //{ data: "Agrupador" },
                //{ data: "Motivo_Rechazo" },
                //{ data: "OC" },
                //{ data: "DocVenta" }
            ],
            language: {
                "lengthMenu": "Mostrar _MENU_ registros por página",
                "zeroRecords": "No hay registros disponibles",
                "info": "Mostrando registro _START_ de _END_",
                "infoEmpty": "No hay registros habilitados",
                "infoFiltered": "(filtrado de _MAX_ registros totales)",
                "paginate": {
                    "next": "Siguiente",
                    "previous": "Anterior"
                },
                "search": "Buscar",
                "searchPlaceholder": "Buscar ",
            },
            destroy: true,
            scrollX: true,
            paging: true,
        });


    } catch (ex) {
        console.log("Error: " + ex.message);
    }
}