$(document).ready(function () {

    let table = null;

    $.fn.getFacturasHistoricas = function () {

        /// definidas en el layout
        rol = puedeAdministrarPnac ? "ADM" : puedeAdministrarPni ? "ADP" : "";

        var json = {
            rol: rol,
            rutProveedor: $('#rutProveedor').val(),
            nFactura: $('#filtro_Nfactura').val(),
            ano: '',
            cEstado: $('#filtro_TEstados').val(),
        };


        try {
            ////mostrarLoader("CONSULTANDO ... por favor espere");
            //////$(".loader_Tablas").fadeIn("slow");
            table = new DataTable('#tableroHistorial', {
                ajax: {
                    url: '../App/getFacturasHistoricas',
                    data: json,

                    dataSrc: function (json) {
                        console.log(json);
                        return json.resultado?.length > 0
                            ? json.resultado
                            : [];
                    }

                },
                columns: [
                    {

                        render: function (data, type, full) {

                            return crearBloque(
                                "fa-user",
                                "Proveedor",
                                full.rutProveedor,
                                "Nombre",
                                full.nombreProveedor
                            );

                        }, title: "Proveedor"

                    },
                    {

                        render: function (data, type, full) {

                            return crearBloque(
                                "fa-pills",
                                "Material",
                                full.cMaterial,
                                "Denominación",
                                full.denominacion
                            );

                        }, title: "Material"

                    },
                    {
                        data: "nFactura",
                        render: function (data, type, full) {

                            return `<div>
                                        <div style="font-weight: 800;">
                                            ${full.nFactura} 
                                        </div>
                                    </div>
                                    `;
                        }, title: "Factura"

                    },
                    {
                        data: "tEstadoTiempoPresente", title: "Estado"
                    },
                    {
                        render: function (data, type, full) {

                            return crearEnlaceExpediente(full.existeExpediente, full.rutProveedor, full.nFactura, full.linkExpedientes);

                        }, title: "Ver Expediente"
                    },
                    {
                        render: function (data, type, full, meta) {

                            return crearEnlaceGuias(full.rutProveedor, full.nFactura);

                        }, title: "ver Guías"
                    },
                    {
                        render: function (data, type, full, meta) {

                            return crearEnlaceComision(full.existeComision, full.rutProveedor, full.nFactura, full.linkComision);

                        }, title: "Factura Comisión"
                    },
                    {
                        render: function (data, type, full, meta) {
                            ////console.log(full);
                            let texto = full.nombreProveedor;
                            let url = full.url;
                            texto = texto.replace(/["']/g, "");

                            if (!full.existeDocumentoAdjunto) {
                                return '';
                            }
                            return `
                                <a class="btn-documento"
                                   data-rut="${parseInt(full.rutProveedor)}"
                                   data-factura="${full.nFactura}"
                                   data-nombre="${full.nombreProveedor}">
                                   <i class="fas fa-paperclip" style="font-size:28px"></i>
                                </a>`;

                        }, title: "Documentos Adjuntos"
                    }


                ],
                layout: {
                    topStart: {
                        buttons: {
                            name: 'primary',
                            buttons: ['copy', 'csv', 'excel']
                        }
                    }
                },
                language: { url: '//cdn.datatables.net/plug-ins/1.13.8/i18n/es-ES.json' },
                destroy: true,
                scrollX: true,
                ///"autoWidth": true,
                //order: [[0, "asc"]],
                pageLength: 10,
                lengthMenu: [[5, 10, 15, 100, -1], ["5", "10", "15", "100", "Todos"]],

                "initComplete": function (settings, json) {
                   
                },
                error: function (jqXHR) {
                    ocultarLoader();

                    mensajeSistema(
                        "Ha ocurrido un error: " +
                        jqXHR.responseText
                    );

                    btnON(
                        'btnFiltroConsultaHistorica',
                        'APLICAR FILTRO'
                    );
                }

            });

        } catch (ex) {
            mensajeSistema("Error: " + ex.message);
            console.log("Error: " + ex.message);
        }
    }

    $.fn.getFacturasHistoricas();

    ////$.fn.getListProveedores('', 'filtroHistorico_nombreProveedor');
    prov_getListProveedores('filtroHistorico_rutProveedor', 'filtroHistorico_nombreProveedor');

    $(document).on('change', '#filtroHistorico_nombreProveedor', function () {
        document.getElementById('filtroHistorico_rutProveedor').value = this.value
    });
    $('#filtroHistorico_rutProveedor').on('change', function () {

        let rutProveedor = +document.getElementById('filtroHistorico_rutProveedor').value;
        if (!isNaN(rutProveedor)) {
            try {

                destino = 'filtroHistorico_nombreProveedor';
                indiceSeleccionado = $("#filtroHistorico_nombreProveedor option[value='" + this.value + "']").index();

                if (indiceSeleccionado > 0) {

                    document.getElementById("filtroHistorico_nombreProveedor").options.item(indiceSeleccionado).selected = 'selected';
                    $("#" + destino).change();

                } else {

                    avisoFinProceso('Proveedor NO Hallado');
                    document.getElementById("filtroHistorico_nombreProveedor").options.item(0).selected = 'selected';
                    $("#" + destino).change();

                }

            } catch (error) {
                console.error(error);
            }
        } else {
            $('#modalMensajeSistema').modal('show');
            $('#mensajeSistema').text('Rut de Proveedor no es válido');
        }

    });
    $(document).on('click', '#btnFiltroConsultaHistorica', function () {

        ////btnOFF('btnFiltroConsulta', '...consulting');
        $.fn.getFacturasHistoricas();

    });

});




