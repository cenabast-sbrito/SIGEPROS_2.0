$(document).on('change', '#_nombreProveedorCarga', function () {

    document.getElementById('_rutProveedorCarga').value = this.value

    indiceSeleccionado = $("#_nombreProveedorCarga option[value='" + this.value + "']").index();
    document.getElementById('_dspNombreProveedor').value = document.getElementById("_nombreProveedorCarga").options.item(indiceSeleccionado).text

});
$('#_rutProveedorCarga').on('change', function () {

    try {
        destino = '_nombreProveedorCarga';
        indiceSeleccionado = $("#_nombreProveedorCarga option[value='" + this.value + "']").index();

        if (indiceSeleccionado >= 0) {

            document.getElementById("_nombreProveedorCarga").options.item(indiceSeleccionado).selected = 'selected';
            document.getElementById('_dspNombreProveedor').value = document.getElementById("_nombreProveedorCarga").options.item(indiceSeleccionado).text
            $("#" + destino).change();

        } else {
            avisoFinProceso('Proveedor NO Hallado');
            document.getElementById("_nombreProveedorCarga").options.item(0).selected = 'selected';
            document.getElementById('_dspNombreProveedor').value = "";
            $("#" + destino).change();

        }
    } catch (error) {
        console.error(error);
    }

});


$('#programaProveedorEdit').on('change', function () {
    let valorSeleccionado = $(this).val();
    $('#cprogramaProveedorEdit').val(valorSeleccionado);
});

$('#modalAgregarProveedor').on('shown.bs.modal', function () {
    _getListProgramas('cprogramaProveedorEdit', 'programaProveedorEdit');
});

$(document).on('click', '.btnModificarProveedor', function () {

    let cPrograma = $(this).data('cprograma');
    let rutP = $(this).data('rut');
    /////console.log(rutP);
    _getListProgramas(
        'cprogramaProveedorMod',
        'programaProveedorMod',
        cPrograma
    );

    $('#guardarNuevoProveedor').hide();
    $('#modalModificarProveedor').show();
    $('#nombreProveedorMod').val('');
    $('#rutProveedorMod').val('');
   //// var rut = $(this).parents('tr').find('td:eq(0)').html();
    GetProveedor(cPrograma, rutP);

    $('#modalModificarProveedor').modal('show');

});

function _getListProgramas(campo, listaDestino, valorSeleccionado = '') {

    try {
        $.ajax({
            type: "GET",
            url: apiUrl("Proveedor/getProgramas"),
            success: function (res) {

                let option = "<option value=''>Seleccione un Programa</option>";

                for (let element of res) {
                    let selected = '';
                    if (valorSeleccionado !== '' && element.cprograma == valorSeleccionado) {
                        selected = ' selected';
                    }

                    option +=
                        '<option value="' + element.cprograma + '"' + selected + '>' +
                        element.tprograma +
                        '</option>';
                }
                $('#' + listaDestino).html(option);
                if (valorSeleccionado !== '') {
                    $('#' + campo).val(valorSeleccionado);
                } else {
                    $('#' + campo).val('');
                }
            },
            error: function (xhr) {

                showToast("_getListProgramas: Ocurrió algún error", "error", 4000);
                
            }
        });
    } catch (ex) {
        console.error("Ha ocurrido un error : " + ex.name + " " + ex.message);
    }
}

function prov_getListProveedores(campo, listaDestino) {

    const programa = '@User.FindFirst("Programa")?.Value';
    let tipoConsulta = 'A';
    var json = {
        tipoConsulta: tipoConsulta
    }
    try {
        $.ajax({
            type: "GET",
            url: "../Proveedor/getProveedores",
            data: json,
            success: function (res) {
                ////console.log(res);
                let option = "<option value=''>Seleccione un Proveedor</option>";
                for (var element of res) {
                    option = option +
                        '<option value="' + element.rutProveedor + '">' +
                        element.nombre +
                        '</option>';
                }
                $('#' + campo).val('');
                $('#' + listaDestino).html(option);
            },
            error: function (xhr) {

                console.log("prov_getListProveedores: Ocurrió algún error", "error", 4000);
                
            }
        });
    } catch (ex) {
        console.error("Ha ocurrido un error : " + ex.name + " " + ex.message);
    }
}


function getTableroProveedores() {

    const programa = $('#programaDefinido').val();

    let tipoConsulta = 'C';
    const json = { tipoConsulta: tipoConsulta, programa:programa };

    try {
        const table = $('#tableroProveedores').DataTable({
            ajax: {
                url: '../Proveedor/getProveedores',
                data: json,
                dataSrc: ''
            },
            columns: [
                { data: "RutProveedor" },
                { data: "Nombre", className: "dt-left" },
                { data: "TPrograma" },

                // Columna Modificar (icono lápiz)
                {
                    data: null,
                    orderable: false,
                    className: "text-center",
                    render: function (data, type, row) {
                        const estado = row.Estado;              // null o 1 = activo
                        const cPrograma = row.CPrograma;
                        const rut = row.RutProveedor;
                        const nombre = row.Nombre || '';

                        if (estado == null || estado === 1) {
                            // Ícono lápiz habilitado
                            return `
                                <i class="fas fa-edit action-icon icon-edit btnModificarProveedor"
                                   title="Modificar proveedor"
                                   aria-label="Modificar proveedor"
                                   role="button"
                                   data-cprograma="${cPrograma}"
                                   data-rut="${rut}">
                                </i>`;
                        } else {
                            // Inactivo → deshabilitado
                            return `
                                <i class="fas fa-edit action-icon disabled"
                                   title="Proveedor desactivado"
                                   aria-label="Proveedor desactivado">
                                </i>`;
                        }
                    }
                },

                // Columna Activar/Desactivar (toggle)
//    if(estado == null || estado === 1) {
//        html = "<button class='btn btn-warning' id='btnDesactivarProveedor'>Desactivar</button>";
//    } else {
//        html = "<button class='btn btn-success' id='btnActivarProveedor'>Activar</button>";
//    }
                {
                    data: null,
                    orderable: false,
                    className: "text-center",
                    render: function (data, type, row) {
                        const estado = row.Estado;     // null o 1 = activo
                        const rut = row.RutProveedor;

                        if (estado == null || estado === 1) {
                            // Activo → mostrar botón para DESACTIVAR
                            return `
                                <i class="fas fa-toggle-on action-icon icon-toggle-on accionDesactivarProveedor"
                                   title="Desactivar proveedor"
                                   aria-label="Desactivar proveedor"
                                   role="button"
                                   data-rut="${rut}">
                                </i>`;
                        } else {
                            // Inactivo → mostrar botón para ACTIVAR
                            return `
                                <i class="fas fa-toggle-off action-icon icon-toggle-off accionActivarProveedor"
                                   title="Activar proveedor"
                                   aria-label="Activar proveedor"
                                   role="button"
                                   data-rut="${rut}">
                                </i>`;
                        }
                    }
                }
            ],

            createdRow: function (row, data) {
                // Si está inactivo (Estado = 0) → fila en gris
                if (data && data.Estado === 0) {
                    $(row).css({ backgroundColor: '#f8fafc', color: '#6b7280' }); // gris suave
                }
            },

            language: {
                lengthMenu: "Mostrar _MENU_ registros por página",
                zeroRecords: "No hay registros disponibles",
                info: "Mostrando registro _START_ de _END_",
                infoEmpty: "No hay registros habilitados",
                infoFiltered: "(filtrado de _MAX_ registros totales)",
                paginate: { next: "Siguiente", previous: "Anterior" },
                search: "Buscar",
                searchPlaceholder: "Buscar ",
                select: true
            },
            responsive: true,
            destroy: true,
            scrollX: true,
            dom:
                "<'row'<'toolbar'><'col-sm-5'l><'col-sm-4'f>>" +
                "<'row'<'col-sm-12'tr>>" +
                "<'row'<'col-sm-6'i><'col-sm-6'p>>"
        });

        // Toolbar
        $('div.toolbar').html(
            "<div class='col-sm-3'>" +
            "<button class='btn btn-primary' style='margin-right:3px;' data-toggle='modal' data-target='#modalAgregarProveedor' id='btnNuevoProveedor'>Nuevo Proveedor</button>" +
            "</div>"
        );

        // Inicializa tooltips si usas Bootstrap
        if (typeof $().tooltip === "function") {
            $('#tableroProveedores').on('draw.dt', function () {
                $('[title]').tooltip({ container: 'body', trigger: 'hover' });
            });
        }

    } catch (ex) {
        alert(ex.description || ex.message || ex);
    }
}


$(document).on('click', '#modalAgregarProveedorBtnGuardar', function () {
    setProveedor();
    getTableroProveedores();
});



$(document).on('click', '#modificaProveedor', function () {
    putProveedor();
    getTableroProveedores();

});

function GetProveedor(cprograma, rut) {

    try {
        $.ajax({
            type: "post",
            url: '../Proveedor/GetProveedor',
            contentType: 'application/json; charset=utf-8',
            data: '{ rut : ' + rut + ', programa : ' + cprograma+ '}',
            success: function (res) { 
                $('#nombreProveedorMod').val(res.Nombre);
                $('#rutProveedorMod').val(res.RutProveedor);
            },
            error: function (xhr) {

                showToast("GetProveedor: Ocurrió algún error", "error", 4000);
                
            }
        });
    } catch (ex) {
        showToast(ex.description, "error", 4000);
        //alert(ex.description);
    }
}

//function setProveedor() {
//    var data = {
//        RutProveedor: $('#rutProveedorEdit').val(),
//        Nombre: $('#nombreProveedorEdit').val(),
//        CPrograma: $('#programaProveedorEdit').val()
//    };
//    try {
//        $.ajax({
//            type: "post",
//            url: '../Proveedor/SetProveedor',
//            contentType: 'application/json; charset=utf-8',
//            data: JSON.stringify(data),
//            success: function (res) {
//                $('#tableroProveedor').DataTable().ajax.reload(null, false);
//                $('#modalAgregarProveedor').modal('hide');
//                $('#modalMensajeSistema').modal('show');
//                $('#mensajeSistema').text(res.message);
//            },
//            error: function (xhr) {

//                    showToast("Ocurrió algún error", "error", 4000);
                
//            }
//        });
//    } catch (ex) {
//        showToast(ex.description, "error", 4000);
//       // alert(ex.description);
//    }
//}
//function putProveedor() {
//    $('#modalModificarProveedor').modal('hide');
//    var data = {
//        "RutProveedor": $('#rutProveedorMod').val(),
//        "Nombre": $('#nombreProveedorMod').val()
//    };
//    try {
//        $.ajax({
//            type: "post",
//            url: '../Proveedor/PutProveedor',
//            contentType: 'application/json; charset=utf-8',
//            data: JSON.stringify(data),
//            success: function (res) {
//                $('#tableroProveedor').DataTable().ajax.reload(null, false);
//                $('#modalModificarProveedor').modal('hide');
//                $('#modalMensajeSistema').modal('show');
//                $('#mensajeSistema').text(res.message);
//            },
//            error: function (xhr) {//return json.state ? json.resultado : [];


//                    showToast("Ocurrió algún error", "error", 4000);
                
//            }
//        });
//    } catch (ex) {
//        showToast(ex.description, "error", 4000);
//        //alert(ex.description);
//    }
//}
