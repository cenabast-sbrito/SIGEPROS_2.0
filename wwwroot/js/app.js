$(document).ready(function () {
    let table = null;
    $('#_Factura_Guias').on('hidden.bs.modal', function () {

        $('#tablaDocumentos tbody').empty();

        $('#_guiaPreview').attr('src', '');

        if ($.fn.DataTable.isDataTable('#tablaGuias')) {
            $('#tablaGuias').DataTable().clear().draw();
        }

    });
    $('#_Factura_Guias').on('shown.bs.modal', function () {

        if (window.tablaguia) {
            window.tablaguia.columns.adjust().draw();
        }

    });
    $(document).on('change', '#nombreProveedor', function () {
        document.getElementById('rutProveedor').value = this.value
    });
    $(document).on('click', '.btn-documento', function () {
        $.fn.getDocumentosAdjuntos(
            $(this).data('rut'),
            $(this).data('factura'),
            $(this).data('nombre')
        );
    });
    $(document).on('click', '#btnFiltroConsulta', function () {

        btnOFF('btnFiltroConsulta', '...consulting');
        $.fn.getFacturas();

    });
    $(document).on('click', '#_generar-Expediente-Factura', async function () {

        const btn = $(this);

        // Guardar texto original
        const textoOriginal = btn.html();

        // Bloquear botón y mostrar spinner
        btn.prop('disabled', true);
        btn.html(`<i class="fas fa-spinner fa-spin"></i>Construyendo expediente...`);

        // Mensaje inicial al usuario
        $('#estadoExpediente').html(`<div class="alert alert-info mb-0"><i class="fas fa-spinner fa-spin"></i>Generando expediente, por favor espere...</div>`);

        try {

            const rut = $('#_Expediente_rutProveedor').val();
            const factura = $('#_Expediente_nFactura').val();

            const respuesta = await crearExpediente(rut, factura);

            $('#estadoExpediente').html(`
            <div class="alert alert-success mb-0">
                <i class="fas fa-check-circle"></i>
                Expediente generado correctamente.
            </div>

        `);

        }
        catch (error) {

            console.error('Error al crear expediente:', error);

            $('#estadoExpediente').html(`
            <div class="alert alert-danger mb-0">
                <i class="fas fa-exclamation-triangle"></i>
                Ocurrió un error al generar el expediente.
            </div>
        `);

        }
        finally {

            // Restaurar botón
            btn.prop('disabled', false);
            btn.html(textoOriginal);

        }

    });
    $(document).on('click', '#_acepta-cambio-estado', function () {

        let rut = $('#modal_rut').val();
        let factura = $('#modal_factura').val();
        let estadoActual = $('#modal_estadoActual').val();
        let estadoNuevo = $('#modal_estadoNuevo').val();
        let fila = $('#modal_fila').val();
        let obj = $('#modal_obj').val();
        let obs = $('#_CampoObservacion').val().trim();

        const campoObservacion = $('#_CampoObservacion');
        const observacionObligatoria = campoObservacion.prop('required');

        if (observacionObligatoria && !obs) {

            $('#_mensaje_Observaciones')
                .text('Debe ingresar una observación para continuar.');

            campoObservacion.focus();

            return;
        }

        $('#_mensaje_Observaciones').text('');

        putEstadoConfirmado(
            obj,
            rut,
            factura,
            estadoActual,
            estadoNuevo,
            fila,
            obs
        );

        document.activeElement?.blur();
        $('#_Acepta_Observacion').modal('hide');
    });

    //$(document).on('click', '#_acepta-cambio-estado', function () {

    //    let rut = $('#modal_rut').val();
    //    let factura = $('#modal_factura').val();
    //    let estadoActual = $('#modal_estadoActual').val();
    //    let estadoNuevo = $('#modal_estadoNuevo').val();
    //    let fila = $('#modal_fila').val();
    //    let obj = $('#modal_obj').val();
    //    let obs = $('#_CampoObservacion').val();
    //    putEstadoConfirmado(obj, rut, factura, estadoActual, estadoNuevo, fila, obs);
    //    document.activeElement?.blur();
    //    $('#_Acepta_Observacion').modal('hide');

    //});
    $('#rutProveedor').on('change', function () {

        let rutProveedor = +document.getElementById('rutProveedor').value;
        if (!isNaN(rutProveedor)) {
            try {

                destino = 'nombreProveedor';
                indiceSeleccionado = $("#nombreProveedor option[value='" + this.value + "']").index();

                if (indiceSeleccionado > 0) {

                    document.getElementById("nombreProveedor").options.item(indiceSeleccionado).selected = 'selected';
                    $("#" + destino).change();

                } else {

                    avisoFinProceso('Proveedor NO Hallado');
                    document.getElementById("nombreProveedor").options.item(0).selected = 'selected';
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
    $(".busqueda-por-texto-seleccionFiltro2").select2({
        placeholder: "Seleccione Funcionario",
        allowClear: true
    });
    $.fn.getFacturas = function () {

        /// definidas en el layout
        rol = puedeAdministrarPnac ? "ADM" : puedeAdministrarPni ? "ADP" : "";

        var json = {
            rol: rol,
            rutProveedor: $('#rutProveedor').val(),
            nFactura: $('#filtro_Nfactura').val(),
            ano: '',
            cEstado: $('#filtro_TEstados').val(),
        };

        /////console.log(json);

        try {
            mostrarLoader("CONSULTANDO ... por favor espere");
            //////$(".loader_Tablas").fadeIn("slow");
            table = new DataTable('#_tablaFacturas', {
                ajax: {
                    url: '../App/getFacturas',
                    data: json,

                    dataSrc: function (json) {
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
                        "render": function (data, type, full, meta) {
                            ////console.log(full);
                            return _administraEstados(full, meta.row);
                        }, title: "Estado Actual"
                    },
                    {
                        render: function (data, type, full, meta) {

                            var textoCabeceraExpediente = `<div style="font-weight: 600;">
                                                        ${full.expedienteTexto}
                                                    </div>`

                            // ¿Tiene observación?
                            const tieneObs = full.tObservacion && full.tObservacion.trim() !== "";

                            // Ícono de observación (solo si tiene)
                            const iconObs = tieneObs
                                ? `<i class="fas fa-comment-dots action-icon icon-observacion"
                                       title="Ver observación"
                                       onclick="mostrarObservacion('${full.rutProveedor}', '${full.nFactura}', \`${full.tObservacion.replace(/`/g, "\\`")}\`)">
                                   </i>`
                                : "";


                            var habilitado = `
                                        <a class="button btn-expediente"
                                           data-rut="${parseInt(full.rutProveedor)}"
                                           data-factura="${full.nFactura}"
                                           onclick="getExpediente(${parseInt(full.rutProveedor)}, ${full.nFactura})"
                                           id="getExpediente_${parseInt(full.rutProveedor)}${full.nFactura}"
                                           title="Construir expediente PDF">
                                            <i class="fas fa-file-pdf action-icon icon-create"></i>
                                        </a>
                                        ${iconObs}
                                    `;

                            // Botón deshabilitado
                            var deshabilitado = `
                                                <a class="button is-disabled"
                                                   aria-disabled="true"
                                                   tabindex="-1"
                                                   title="No disponible">
                                                    <i class="fas fa-file-pdf action-icon icon-disabled"></i>
                                                </a>  
                                        ${iconObs}
                                            `;
                            if (parseInt(full.cEstado) === 1) {
                                return `
                                            <div id="expediente_${full.rutProveedor}_${full.nFactura}">
                                                ${textoCabeceraExpediente}
                                                <div class="text-secondary" style="font-size:18px;">
                                                    ${habilitado}
                                                </div>
                                            </div>
                                        `;
                                ////return `<div>${textoCabeceraExpediente} <div class="text-secondary" style="font-size:18px;"> ${habilitado} </div> </div>`;

                            }
                            else {
                                return `
                                    <div id="expediente_${full.rutProveedor}_${full.nFactura}">
                                        ${textoCabeceraExpediente}
                                        <div class="text-secondary" style="font-size:18px;">
                                            ${deshabilitado}
                                        </div>
                                    </div>
                                `;
                                /////return `<div>${textoCabeceraExpediente} <div class="text-secondary" style="font-size:18px;"> ${deshabilitado} </div> </div>`;
                            }
                        }, title:"Crear Expediente", visible: ["ADM", "ADP"].includes(String(rol).toUpperCase())
                        //,
                        //visible: ["ADM", "ADP"].includes(String(rol).toUpperCase())
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
                language: {url: '//cdn.datatables.net/plug-ins/1.13.8/i18n/es-ES.json'},
                destroy: true,
                scrollX: true,
                ///"autoWidth": true,
                //order: [[0, "asc"]],
                pageLength: 10,
                lengthMenu: [[5, 10, 15, 100, -1], ["5", "10", "15", "100", "Todos"]],

                "initComplete": function (settings, json) {
                    ocultarLoader();//// (".loader_Tablas").fadeOut("slow"); 
                    btnON('btnFiltroConsulta', 'APLICAR FILTRO');
                },
                error: function (jqXHR) {
                    ocultarLoader();

                    mensajeSistema(
                        "Ha ocurrido un error: " +
                        jqXHR.responseText
                    );

                    btnON(
                        'btnFiltroConsulta',
                        'APLICAR FILTRO'
                    );
                }

            });

        } catch (ex) {

            mensajeSistema("Error: " + ex.message);
            ///console.log("Error: " + ex.message);
        }
    }
    $.fn.getListProveedores = function (campo, listaDestino) {

        let json = {
            parametro1: campo,
            programa: 0
        };

        $.ajax({
            type: "GET",
            url: "../App/GetListaProveedores",
            data: json,

            success: function (res) {

                let options =
                    "<option value=''>Seleccione un Proveedor</option>";

                if (!res || !res.status) {

                    avisoFinProceso(
                        res?.message || "No fue posible obtener la información."
                    );

                    $("#" + listaDestino).html(options);
                    return;
                }

                if (res.resultado && res.resultado.length > 0) {

                    $.each(res.resultado, function (i, item) {

                        let rut =
                            item.rutProveedor ??
                            item.RutProveedor ??
                            "";

                        let nombre =
                            item.nombre ??
                            item.Nombre ??
                            "";

                        options +=
                            "<option value='" + rut + "'>" +
                            nombre +
                            "</option>";
                    });
                }

                $("#" + listaDestino).html(options);
            },

            error: function (xhr) {


                ocultarLoader();//// (".loader_Tablas").fadeOut("slow");


                avisoFinProceso(
                    "Ocurrió un error al obtener los proveedores."
                );
            }
        });
    };
    $.fn.getListEstados = function (cEstado, listaDestino) {

        let json = {
            cEstado: cEstado
        };

        $.ajax({
            type: "GET",
            url: "../App/GetListaEstados",
            data: json,

            success: function (res) {

                let options =
                    "<option value=''>Seleccione un Estado</option>";

                if (!res || !res.status) {

                    avisoFinProceso(
                        res?.message || "No fue posible obtener la información."
                    );

                    $("#" + listaDestino).html(options);
                    return;

                }

                if (res.resultado && res.resultado.length > 0) {

                    $.each(res.resultado, function (i, item) {

                        let cestado =
                            item.cEstado ??
                            item.cEstado ??
                            "";

                        let testado =
                            item.tEstado ??
                            item.tEstado ??
                            "";

                        options += "<option value='" + cestado + "'>" + testado + "</option>";

                    });
                }

                $("#" + listaDestino).html(options);
            },

            error: function (xhr) {

                ocultarLoader();////  (".loader_Tablas").fadeOut("slow");


                avisoFinProceso(
                    "Ocurrió un error al obtener los proveedores."
                );
            }
        });
    };
    $.fn.getDocumentosAdjuntos = function (rut, factura, nombreProveedor) {

        const iframe = '_documentoAdjuntoPreview';

        limpiarIFrame(iframe);
        inicializarOTablaVacia('_tablaDocumentosAdjuntos');
        try {
            var modal = '_modal_DocumentosAdjuntos'
            var json = {
                rut: rut,
                factura: factura,
            };

            $.ajax({
                type: "GET",
                url: '../Documentos/getDocumentosAdjuntos',
                data: json,
                success: function (res) {
                    ///console.log(res);
                    if (modal !== "" && res?.state) {

                        document.getElementById('rutModal').value = rut;
                        document.getElementById('facturaModal').value = factura;
                        document.getElementById('nombreProveedorModal').value = nombreProveedor;
                        $('#' + modal).modal('show');

                    }
                    // Cargar datos
                    res.archivos.nombres.forEach(function (item, i) {

                        //let url = td === 'DA'
                        //    ? `${res.archivos.urlDA}${item.nombre}`
                        //    : `${res.archivos.url}${item.nombre}`;


                        let fila = ///[i + 1, item.nombre, ''];
                            [
                                i + 1,
                                item.nombre,
                                `<a target='_documentoAdjuntoPreview' href='${res.archivos.urlDA}${item.nombre}'>
                                    <i class='fas fa-eye' style='font-size:28px'></i>
                                 </a>`
                            ]
                            ;
                        tableDocumentos.row.add(fila);

                    });
                    tableDocumentos.draw();
                },

                complete: function () {
                    //                    hideSpinnerDocumentos('spinnerDocumentos');

                    //if (target === "_guiaPreview") {
                    //    hideLoading('.box-loading');
                    //}
                },

                error: function (xhr) {

                    /////console.log(xhr);
                    //                    hideSpinnerDocumentos('spinnerDocumentos');

                    avisoFinProceso("fn.getDocumentosAdjuntos: Ocurrió algún error", xhr);
                }
            });

        } catch (ex) {
            /////console.log(ex);
            //          hideSpinnerDocumentos('spinnerDocumentos');
            avisoFinProceso("fn.getDocumentosAdjuntos: Ocurrió algún error");
            console.error("Error:", ex);
        }
    };
    $.fn.verDocumento = function (tipoDocumento, rut, ndoc, url) {

        try {
            $.ajax({
                type: "GET",
                data: {
                    tipoDocumento: tipoDocumento,
                    rut: rut,
                    factura: ndoc
                },
                url: "../Documentos/existeDocumento",
                success: function (res) {

                    if (res) {
                        /////console.log(url);
                        $('#_frameDocumento').attr('src', url);
                        $('#_modalDocumento').modal('show');

                    }
                    else
                        Swal.fire('Aviso', 'El expediente solicitado aún no ha sido creado.', 'warning');

                },
                error: function (xhr) {//return json.state ? json.resultado : [];
                    Swal.fire('Aviso', 'Ocurrió algun error.', 'warning');
                    

                }
            });
        }
        catch (ex) {
            avisoFinProceso("$.fn.verDocumento: Ocurrio algún error");
            console.error("Ha ocurrido un error : " + ex.name + " " + ex.message);
        }
    };

    let fontSizeActual = 16;

    $("#masFuente").click(function () {
        fontSizeActual += 1;
        $("body").css("font-size", fontSizeActual + "px");
    });

    $("#menosFuente").click(function () {
        if (fontSizeActual > 12) {
            fontSizeActual -= 1;
            $("body").css("font-size", fontSizeActual + "px");
        }
    });

    $("#resetFuente").click(function () {
        fontSizeActual = 16;
        $("body").css("font-size", "16px");
    });

});
function _administraEstados(obj, fila) {

    const rol = rolReal; 

    const codigo = `${parseInt(obj.rutProveedor)}-${obj.nFactura}`;

    const estadosConfig = {

        0: { icono: "fa-eye-slash", color: "#6c757d" },      // No visto
        1: { icono: "fa-user-check", color: "#0d6efd" },     // Rev. Cenabast
        2: { icono: "fa-undo-alt", color: "#fd7e14" },       // Devuelto
        3: { icono: "fa-file-signature", color: "#6f42c1" }, // Contratos
        4: { icono: "fa-paper-plane", color: "#0dcaf0" },    // Enviado Minsal
        5: { icono: "fa-user-tie", color: "#0d6efd" },       // Rev. Minsal
        6: { icono: "fa-times-circle", color: "#dc3545" },   // Rechazar
        7: { icono: "fa-check-circle", color: "#198754" },   // Aceptar
        8: { icono: "fa-comment-dots", color: "#ffc107" },   // Observaciones
        9: { icono: "fa-money-bill-wave", color: "#198754" },// Pago
        10: { icono: "fa-lock", color: "#495057" },          // Cerrado
        11: { icono: "fa-stamp", color: "#20c997" }          // OK Contratos
    };

    function obtenerEstadoInfo(cEstado) {

        return estadosConfig[cEstado] || {
            icono: "fa-arrow-right",
            color: "#6c757d"
        };
    }

    function crearAccion(texto, estadoDestino, ejecutable, ok) {

        if (!texto || estadoDestino === null || estadoDestino === 0)
            return "";

        const infoEstado = obtenerEstadoInfo(Number(estadoDestino));

        const disabled =
            Number(ejecutable) !== 1
                ? "disabled"
                : "";

        const clase =
            ok === 1
                ? "accion accion-ok"
                : "accion accion-nook";

        return `
            <button
                type="button"
                class="${clase}"
                ${disabled}
                onclick="putEstado(
                    this,
                    '${obj.tEstadoOK}',
                    '${obj.tEstadoNOOK || ''}',
                    ${parseInt(obj.rutProveedor)},
                    ${obj.nFactura},
                    ${obj.cEstado},
                    ${estadoDestino},
                    ${fila},
                    ${ok}
                )">
                <i class="fa ${infoEstado.icono}"
                   style="color:${infoEstado.color}">
                </i>
                ${estadoDestino}: ${texto}
            </button>
        `;
    }

    const estadoActual =
        obtenerEstadoInfo(Number(obj.cEstado));

    let htmlEstado = `
        <div class="estado-actual">
            <i class="fa ${estadoActual.icono}"
               style="color:${estadoActual.color}">
            </i>
         ${obj.cEstado}:  ${obj.tEstadoTiempoPresente || 'Sin estado'}
        </div>
    `;
    //////console.log(obj);
    let botonOK = crearAccion(
        obj.tEstadoOK,
        obj.estadoOK,
        obj.puedeEjecutarOK,
        1
    );

    let botonNOOK = crearAccion(
        obj.tEstadoNOOK,
        obj.estadoNOOK,
        obj.puedeEjecutarNOOK,
        0
    );

    let botonObservaciones = "";

    if (
        obj.tEstadoOKObservaciones &&
        obj.cEstadoOKObservaciones &&
        Number(obj.puedeEjecutarOK) === 1
    ) {

        const infoObservacion =
            obtenerEstadoInfo(
                Number(obj.cEstadoOKObservaciones)
            );

        botonObservaciones = `
            <button
                type="button"
                class="accion accion-observacion"
                onclick="putEstado(
                    this,
                    '${obj.tEstadoOKObservaciones}',
                    '${obj.tEstadoNOOK || ''}',
                    ${parseInt(obj.rutProveedor)},
                    ${obj.nFactura},
                    ${obj.cEstadoOKObservaciones},
                    ${obj.estadoOK},
                    ${fila},
                    1
                )">

                <i class="fa ${infoObservacion.icono}"
                   style="color:${infoObservacion.color}">
                </i>

                ${obj.cEstadoOKObservaciones}: ${obj.tEstadoOKObservaciones}

            </button>
        `;
    }

    let acciones = "";

    if (
        obj.cEstado == 5 &&
        (rol == "MIN" || rol == "MIP")
    ) {

        acciones =
            botonOK +
            botonObservaciones +
            botonNOOK;
    }
    else {

        acciones =
            botonOK +
            botonNOOK;
    }

    if (obj.cEstado == 10) {
        acciones = "";
    }

    return `
        <div id="${codigo}" class="estado-container">   
            ${htmlEstado}
            <div class="acciones-container">
                ${acciones}
            </div>
        </div>
    `;
}

function putEstadoConfirmado(obj, rut, factura, estadoActual, estadoNuevo, fila, obs) {

    let rutUsuario = rutLogueado;
    var json = {
        rutUsuario: rutUsuario,
        rutProveedor: rut,
        NFactura: factura,
        estadoActual: estadoActual,
        estadoNuevo: estadoNuevo,
        obs: obs
    };

    try {
        $.ajax({
            type: "GET",
            url: "../Documentos/putEstado",
            data: json,
            success: function (data) {

                if (data.resultado === null) {
                    _getFacturas();
                } else {
                    if (data.status == true) {
                        
                        botonera = _administraEstados(data.resultado, fila);

                        actualizarFilaFactura(
                            rut,
                            factura, 
                            botonera
                        );
                        activarExpediente(
                            rut,
                            factura
                        );


                    }
                    else {
                        Swal.fire('Aviso', data.message, 'warning');
                    }
                }
            },
            error: function (xhr, status, error) {

                let mensaje = "No se pudo actualizar el estado de la factura.";

                if (xhr.responseJSON && xhr.responseJSON.mensaje) {
                    mensaje = xhr.responseJSON.mensaje;
                }

                else if (xhr.responseText) {
                    mensaje = xhr.responseText;
                }

                if (xhr.status === 0) {
                    console.log("No hay conexión con el servidor.");
                } else if (xhr.status === 404) {
                    console.log("Recurso no encontrado (404).");
                    window.location.href = apiUrl("");
                } else if (xhr.status === 500) {
                    console.log("Error interno del servidor (500).");
                } else {
                    console.log("Error inesperado:", error);
                }
                avisoFinProceso(mensaje);
            }
        });

    } catch (ex) {
        console.log(ex);
    }
    obj.disabled = true;
}
function actualizarFilaFactura(RutProveedor, Factura, botonera) {

    $(`#${RutProveedor}-${Factura}`).html(botonera);

}
function activarExpediente(rutProveedor, nFactura) {
      var habilitado = `
                                        <a class="button btn-expediente"
                                           data-rut="${parseInt(rutProveedor)}"
                                           data-factura="${nFactura}"
                                           onclick="getExpediente(${parseInt(rutProveedor)}, ${nFactura})"
                                           id="getExpediente_${parseInt(rutProveedor)}${nFactura}"
                                           title="Construir expediente PDF">
                                            <i class="fas fa-file-pdf action-icon icon-create"></i>
                                        </a>
                                        
                                    `;


   
        var contenido =  `
                                            <div id="expediente_${rutProveedor}_${nFactura}">
                                                <div class="text-secondary" style="font-size:18px;">
                                                    ${habilitado}
                                                </div>
                                            </div>
                                        `;


    $(`#expediente_${rutProveedor}_${nFactura}`).html(contenido);
   


}

function activarVerExpediente(rutProveedor, nFactura, linkExpediente) {

    var objlinkExpediente = ` 
                            <a href="javascript:void(0)"
                            onclick="$.fn.verDocumento('Expediente',${parseInt(rutProveedor)}, ${nFactura}, '${linkExpediente}')"
                               >
                                <i class="fas fa-folder-open action-icon"
                                   style="
                                        
                                        transition: color .2s ease;
                                    ">
                                </i>
                            </a>
                        `;

    return `
                                            <div id="linkExpediente_${rutProveedor}${nFactura}">
                                                
                                                <div class="text-secondary" style="font-size:18px;">
                                                    ${objlinkExpediente}
                                                </div>
                                            </div>
                                        `;





    let html = `PRUEBA
        <a class="button btn-expediente"
           data-rut="${rutProveedor}"
           data-factura="${factura}"
           onclick="verExpediente(${rutProveedor}, ${factura})"
           title="Construir expediente PDF">
            <i class="fas fa-file-pdf action-icon icon-create"></i>
        </a>`;

    $(`#linkExpediente_${rut}${factura}`).html(html);
}
function getExpediente(rut, factura) {
    $('#_Expediente_rutProveedor').val(rut);
    $('#_Expediente_nFactura').val(factura);
    $('#_Factura_Confirmar_Expediente').modal('show');
}

async function crearExpediente(rut, factura) {

    try {

        $('#estadoExpediente').html(`<div class="alert alert-info mb-0"><i class="fas fa-spinner fa-spin"></i>Enviando solicitud de construcción del expediente...</div>`);
        $('#estadoExpediente').html(`<div class="alert alert-warning mb-0"> <i class="fas fa-cogs fa-spin"></i> Descargando y validando documentos... </div>`);

        const res = await $.ajax({
            type: "POST",
            url: "../App/crearExpediente",
            data: {
                rutProveedor: rut,
                factura: factura
            }
        });
        consultarEstadoJob(res.idJob);
        if (!res.state) {

            throw new Error(res.message);

        }
        const botonExpediente = $(`#getExpediente_${rut}${factura}`);
        const linkExpediente = $(`#linkExpediente_${rut}${factura}`);
        const urlExp = res.linkExpediente;

        linkExpediente.attr('title', 'Ver expediente');
        linkExpediente.attr('onclick', `$.fn.verExpediente(${rut}, ${factura}, '${res.linkExpediente}')`);
        linkExpediente.find('i').css('color', '#1976d2');

        botonExpediente.html(`<i class="fas fa-check-circle text-success"></i>`);

        botonExpediente.removeAttr('onclick');

        botonExpediente.addClass('is-disabled');

        botonExpediente.attr('title', 'Expediente generado');

        $('#estadoExpediente').html(`<div class="alert alert-success mb-0"><i class="fas fa-check-circle"></i>Expediente construido correctamente.</div>`);

        avisoFinProceso('Expediente construido correctamente');

        activarExpediente(
            rut,
            factura
        );

        nuevoContenidoVerExpediente = activarVerExpediente(
            rut,
            factura, urlExp
        );
        $(`#linkExpediente_${rut}${factura}`).html(nuevoContenidoVerExpediente);


        // Esperar un instante para que el usuario vea el éxito
        setTimeout(() => {
            $('#_Factura_Confirmar_Expediente').modal('hide');
        }, 1000);


        return res;

    } catch (error) {

        let mensajeError = 'Ocurrió un error inesperado.';

        if (error.status === 404) {

            mensajeError = 'Error 404: Servicio no encontrado.';

            ocultarLoader();//// (".loader_Tablas").fadeOut("slow");

            avisoFinProceso(
                "crearExpediente: Ocurrió algún error",
                "error.status 404"
            );

        } else {

            if (error.responseJSON?.message) {
                mensajeError = error.responseJSON.message;
            }
            else if (error.message) {
                mensajeError = error.message;
            }

            avisoFinProceso(
                "crearExpediente 2: Ocurrió algún error",
                "error",
                4000
            );
        }

        $('#estadoExpediente').html(`
            <div class="alert alert-danger mb-0">
                <i class="fas fa-exclamation-triangle"></i>
                ${mensajeError}
            </div>
        `);

        console.error("Error en crearExpediente:", error);

        throw error;
    }
}
function crearBloque(icono, titulo, valorPrincipal, etiqueta, valorSecundario) {

    return `
        <div> 
            <div style="font-weight:600;">
              ${valorPrincipal}
            </div>
            <div  style="font-size:12px; color: rgba(var(--bs-secondary-rgb), var(--bs-text-opacity)) !important;">
                <i class="fas ${icono} me-1"></i>
                ${etiqueta}: <b>${valorSecundario}</b>
            </div>
        </div>
    `;
}
function crearBloque2(icono, titulo, valorPrincipal, etiqueta, valorSecundario, etiqueta2, valorSecundario2) {
    return `
        <div>
            <div style="font-weight:600;">
                <i class="fas ${icono} me-1"></i>
                ${valorPrincipal}
            </div>

            <div style="font-size:12px; color: rgba(var(--bs-secondary-rgb), var(--bs-text-opacity)) !important;">
                ${etiqueta ? `${etiqueta} : <b>${valorSecundario}</b>` : `<b>${valorSecundario}</b>`}
            </div>

            <div style="font-size:12px; color: rgba(var(--bs-secondary-rgb), var(--bs-text-opacity)) !important;">
                ${etiqueta2 ? `${etiqueta2} : <b>${valorSecundario2}</b>` : `<b>${valorSecundario2}</b>`}
            </div>
        </div>
    `;
}

function crearBloque3(icono, titulo, valorPrincipal, etiqueta, valorSecundario, etiqueta2, valorSecundario2, etiqueta3, valorSecundario3) {

    return `
        <div>
            <div style="font-weight:600;">
            <i class="fas ${icono} me-1"></i>
                ${valorPrincipal}
            </div>
            <div style="font-size:12px; color: rgba(var(--bs-secondary-rgb), var(--bs-text-opacity)) !important;">
                
                ${etiqueta}: <b>${valorSecundario}</b>
            </div>
            <div style="font-size:12px; color: rgba(var(--bs-secondary-rgb), var(--bs-text-opacity)) !important;"
                
                ${etiqueta2}: <b>${valorSecundario2}</b>
            </div>
            <div style="font-size:12px; color: rgba(var(--bs-secondary-rgb), var(--bs-text-opacity)) !important;">

                ${etiqueta3}: <b>${valorSecundario3}</b>
            </div>
        </div>
    `;
}
function crearEnlaceExpediente(
    ExisteExpediente,
    rutProveedor,
    nFactura,
    linkExpedientes) {

    // Validación robusta
    const linkValido =
        linkExpedientes &&
        linkExpedientes !== 'null' &&
        linkExpedientes.trim() !== '';

    const colorIcono = ExisteExpediente
        ? '#1976d2'
        : '#90caf9';

    const titulo = ExisteExpediente
        ? 'Ver expediente'
        : 'Expediente aún no disponible';

    var deshabilitado = `
                            <a href="javascript:void(0)"
                            onclick="$.fn.verDocumento('Expediente',${parseInt(rutProveedor)}, ${nFactura}, '${linkExpedientes}')"
                               title="${titulo}">
                                <i class="fas fa-folder-open action-icon"
                                   style="
                                        color:${colorIcono};
                                        transition: color .2s ease;
                                    ">
                                </i>
                            </a>
                        `;

    return `
                                            <div id="linkExpediente_${rutProveedor}${nFactura}">
                                                
                                                <div class="text-secondary" style="font-size:18px;">
                                                    ${deshabilitado}
                                                </div>
                                            </div>
                                        `;

}
function crearEnlaceGuias(rutProveedor, nFactura) {

    return `<a href='#' class='button' onclick='verGuias(${parseInt(rutProveedor)},${nFactura});'
                        id='getGuias_${parseInt(rutProveedor)}${nFactura}'>
                        <i class="fas fa-truck action-icon icon-guides" title="Ver guías"></i>
                        </a>`;

}
function crearEnlaceComision(existeComision, rutProveedor, nFactura, linkComision) {

    if (!existeComision) {
        return '';
    }
    return `
        <a href="javascript:void(0)"
        onclick="$.fn.verDocumento('Comision',${parseInt(rutProveedor)}, ${nFactura}, '${linkComision}')"
           title="">
            <i class="fas fa-folder-open action-icon"
               style="
                    
                    transition: color .2s ease;
                ">
            </i>
        </a>
    `;

}

/**
 * Documentos Adjuntos
 * @param {any} tablaId
 */
function inicializarOTablaVacia(tablaId) {


    if ($.fn.DataTable.isDataTable('#' + tablaId)) {
        tableDocumentos.clear().draw();
    } else {
        tableDocumentos = $('#' + tablaId).DataTable({
            paging: true,
            searching: false,
            info: false,
            autoWidth: false, lengthChange: false,
            language: { url: '//cdn.datatables.net/plug-ins/1.13.8/i18n/es-ES.json' },
        });
    }
}

/**
 * Factura comision
 * @param {any} spinner
 */

/* Funciones Spinner */
function hideSpinnerDocumentos(spinner) {
    $('#' + spinner).hide();
}
function hideSpinnerDocumentos(spinner) {
    $('#' + spinner).hide();
}
function showLoading(selector) {
    $(selector).find('.loading-overlay').removeClass('d-none');
}
function hideLoading(selector) {
    $(selector).find('.loading-overlay').addClass('d-none');
}
/* Fin Funciones Spinner */

async function verGuias(rut, factura) {
    anchorOFF('getGuias_' + rut + factura);
    await compruebaTablaSAP_ASYNC(rut, factura, 'verGuias');
}

async function compruebaTablaSAP_ASYNC(rut, factura, accion) {

    try {

        getGuiasByFactura(rut, factura);
        ///            anchorON('getGuias_' + rut + factura);
        $('#_Factura_Guias').modal('show');


        ///        return res; // importante

    } catch (error) {

        // Error de sesión expirada

        console.error("Error:", error);
        throw error; // importante para el finally del botón
    }
}

function getGuiasByFactura(rut, factura) {

    let incluirDatosAdicionales = false; //// document.getElementById('incluirDatosAdicionales');

    mostrarLoader("obteniendo Guías");

    var json = {
        rutProveedor: rut,
        factura: factura,
        incluye: incluirDatosAdicionales.checked
    }

    try {

        if ($.fn.DataTable.isDataTable('#tablaGuias')) {

            $('#tablaGuias')
                .DataTable()
                .clear()
                .destroy();

            $('#tablaGuias').empty();
        }
        const tablaGuia = $('#tablaGuias').DataTable({
            "ajax": {
                url: "../App/getDespachosByFactura",
                data: json,
                dataType: "json",
                "dataSrc": function (json) {

                    if (json.state === false) {
                        return [];
                    }

                    if (json.resultado && json.resultado.length > 0) {
                        return json.resultado;
                    }

                    return [];

                },
                error: function (xhr) {
                    if (xhr.status === 401 || xhr.status === 440 || xhr.status === 419 || xhr.status === 404) {
                        avisoFinProceso("El recurso solicitado no está disponible.");
                    } else {
                        avisoFinProceso("getGuiasByFactura: Ocurrio algún error");
                    }
                }

            },
            columns: [

                {

                    render: function (data, type, full) {

                        return crearBloque3(
                            "fa-boxes-stacked",
                            "Pedido de Compra",
                            "idMP: " + full.zzidmeR_E + "  / PC: " + full.pedidoCompra,
                            "Factura",
                            full.nFactura,
                            "Guía Despacho",
                            full.guiaDespacho,
                            "Documento de Venta",
                            full.documentoVenta
                        );
                    }, title: "Documentos"

                },

                {

                    render: function (data, type, full) {

                        return crearBloque(
                            "fa-network-wired",
                            "Canal",
                            full.canal + " " + full.descripcionCanal,
                            "Grupo Artículo",
                            full.grupoArticulos
                        );
                    }, title: "Canal"

                },
                {

                    render: function (data, type, full) {

                        return crearBloque2(
                            "fa-boxes-stacked",
                            "Material",
                            full.codigoMaterial,
                            "", ////Denominación
                            full.denominacion,
                            "Lote",
                            full.lote
                        );
                    }, title: "Material"

                },
                {

                    render: function (data, type, full) {

                        return crearBloque(
                            "fa-scale-balanced",
                            "Cantidad Documento",
                            "Documento: " + full.cantidadDocumento,
                            "Informado",
                            full.cantidadInformada
                        );
                    }, title: "Cantidades"

                },
                { data: "valorNeto", title: "Valor Neto" },
                {

                    render: function (data, type, full) {

                        return crearBloque2(
                            "fa-hospital",
                            "Rut Establemiento",
                            full.rutEstablecimiento,
                            "",/// Destinatario
                            full.destinatario,
                            "Servicio de Salud",
                            full.servicioSalud
                        );
                    }, title: "Destinatario"

                }
            ],
            columnDefs: [
                {
                    targets: "_all",
                    className: "text-nowrap"
                }
            ],
            language: { url: '//cdn.datatables.net/plug-ins/1.13.8/i18n/es-ES.json' },
            layout: {
                topStart: {
                    buttons: {
                        name: 'primary',
                        buttons: ['copy', 'csv', 'excel']
                    }
                }
            },
            destroy: true,
            pageLength: 5,
            paging: true, lengthChange: false,
            "initComplete": function () {
                ocultarLoader(); //// (".loader_Tablas").fadeOut("slow");

                $('#tablaGuias tbody').on('click', 'tr', function () {

                    const table = $('#tablaGuias').DataTable();
                    const data = table.row(this).data();

                    if (!data) return;

                    // Estilo de selección
                    $('#tablaGuias tbody tr').removeClass('row_selected');
                    $(this).addClass('row_selected');

                    fn_getDatos_Documentos(
                        '',
                        '',
                        '',
                        '_guiaPreview',
                        'tablaDocumentos',
                        data.documentoVenta,
                        data.lifnr,
                        '',
                        true
                    );
                });

            },
            error: function (jqXHR, textStatus, errorThrown) {
                console.error("Ha ocurrido un error : " + jqXHR.responseText);
            }
        });

    } catch (ex) {
        console.log("Error: " + ex.message);
    }
}
function fn_getDatos_Documentos(td, full, modal, target, tabla, doc, rut, nombre, permiteVer) {
    try {

        const iframe = document.getElementById(target);
        if (iframe) iframe.src = 'about:blank';

        inicializarOTablaVacia(tabla);
        showSpinnerDocumentos('spinnerDocumentos');


        let ruta, json;

        ruta = "../Documentos/ObtenerDocumentos";
        json = { rut: rut, doc: doc };

        $.ajax({
            type: "GET",
            data: json,
            url: ruta,

            success: function (res) {
                hideSpinnerDocumentos('spinnerDocumentos');
                if (modal !== "" && res?.state) {
                    document.getElementById('rutModal').value = rut;
                    document.getElementById('nombreModal').value = nombre;
                    document.getElementById('facturaModal').value = doc;
                    $('#' + modal).modal('show');
                }

                if (res?.state && res.archivos && Array.isArray(res.archivos.nombres)) {
                    res.archivos.nombres.forEach(function (item, i) {
                        let url = `${res.archivos.url}${item.nombre}`;
                        let fila = permiteVer
                            ? [
                                i + 1,
                                item.nombre,
                                `<a target='${target}' href='${url}'>
                                    <i class='fas fa-eye' style='font-size:22px'></i>
                                 </a>`
                            ]
                            : [
                                i + 1,
                                item.nombre, ''];

                        tableDocumentos.row.add(fila);
                    });
                }
                tableDocumentos.draw();
            },
            error: function (xhr) {
                hideSpinnerDocumentos('spinnerDocumentos');
                avisoFinProceso("fn_getDatos_Documentos: Ocurrió algún error", xhr);
            }
        });

    } catch (ex) {
        /////console.log(ex);
        hideSpinnerDocumentos('spinnerDocumentos');
        avisoFinProceso("fn_getDatos_Documentos: Ocurrió algún error");
        console.error("Error:", ex);
    }
}
function obtenerMensajeAccion(estadoNuevo, nombreEstado) {

    switch (parseInt(estadoNuevo)) {

        case 1:
            return 'Usted está a punto de iniciar la revisión del expediente por un Ejecutivo Cenabast.';

        case 3:
            return 'Usted está a punto de enviar el expediente a Contratos.';

        case 4:
            return 'Usted está a punto de enviar el expediente a MINSAL.';

        case 5:
            return 'Usted está a punto de iniciar la revisión del expediente por un Ejecutivo MINSAL.';

        case 6:
            return 'Usted está a punto de rechazar el expediente.';

        case 7:
            return 'Usted está a punto de aprobar el expediente.';

        case 8:
            return 'Usted está a punto de aprobar el expediente con observaciones.';

        case 9:
            return 'Usted está a punto de gestionar el pago del expediente.';

        case 10:
            return 'Usted está a punto de cerrar el expediente.';

        case 11:
            return 'Usted está a punto de marcar el expediente como OK Contratos.';

        default:
            return 'Usted está a punto de ejecutar la siguiente acción: ' + nombreEstado;
    }
}

function putEstado(
    obj,
    TEstadoOK,
    tEstadoNOOK,
    rut,
    factura,
    estadoActual,
    estadoNuevo,
    fila,
    bEstadoSeleccionado) {
    console.log(estadoActual, estadoNuevo);
    // Limpia observación
    $('#_CampoObservacion').val('');

    // Solo se muestra para "Aceptar con Observaciones"


    if (estadoActual == '8') {
        $('#_CampoObservacion')
            .show()
            .prop('required', true)
            .focus();
    } else {
        $('#_CampoObservacion')
            .show()
            .prop('required', false)
            .val('');
    }

    if (estadoActual == '8') {
        $('#_CampoObservacion').show();
    } else {
        $('#_CampoObservacion').hide();
    }

    // Determina el estado seleccionado
    let nombreEstado = '';

    if (bEstadoSeleccionado) {
        nombreEstado = TEstadoOK;
    } else {
        nombreEstado = tEstadoNOOK;
    }

    // Construye mensaje amigable
    let mensajeAccion = obtenerMensajeAccion(
        estadoNuevo,
        nombreEstado
    );

    // Mensaje principal del modal
    $('#lblAccion').text(mensajeAccion);

    // Opcional: mostrar el nombre exacto del estado debajo
    $('#modal_TEstadoOK')
        .val(nombreEstado)
        .show();

    // Datos ocultos
    $('#modal_rut').val(rut);
    $('#modal_factura').val(factura);
    $('#modal_estadoActual').val(estadoActual);
    $('#modal_estadoNuevo').val(estadoNuevo);
    $('#modal_fila').val(fila);
    $('#modal_obj').val(obj);

    // Abrir modal
    $('#_Acepta_Observacion').modal('show');
}

function showSpinnerDocumentos(spinner) {
    $('#' + spinner).show();
}

function hideSpinnerDocumentos(spinner) {
    $('#' + spinner).hide();
} 

function showToast(message, type = "info", duration = 4000) {

    const container = document.getElementById("toast-container-app");

    const toast = document.createElement("div");
    toast.className = `toast toast-${type}`;
    toast.innerHTML = message;

    container.appendChild(toast);

    // Activar animación
    setTimeout(() => toast.classList.add("show"), 50);

    // Auto eliminar
    setTimeout(() => {
        toast.classList.remove("show");
        setTimeout(() => toast.remove(), 300);
    }, duration);
}

function consultarEstadoJob(idJob) {

    const intervalo = setInterval(async () => {

        const estado = await $.get(
            "../App/GetEstadoJob",
            { idJob }
        );

        $('#estadoExpediente').html(`
            <div class="alert alert-info">
                ${estado.mensaje}
                (${estado.porcentaje}%)
            </div>
        `);

        if (
            estado.estado === "FINALIZADO" ||
            estado.estado === "ERROR"
        ) {
            clearInterval(intervalo);
        }

    }, 2000);
}

function mostrarObservacion(lifnr, factura, texto) {


    document.getElementById("textoObservacion").textContent = texto;
    $('#modalObservacion').modal('show');

}


function limpiarIFrame(idIframe) {
    const iframe = document.getElementById(idIframe);

    if (iframe) {
        iframe.src = 'about:blank';
    }
}