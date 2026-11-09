let tableroProveedor;
$(document).ready(function () {
    $.fn.getTableroProveedor = function () {

        tableroProveedor = $('#tableroProveedor').DataTable({

            ajax: {
                url: "../Proveedor/GetProveedores",
                dataSrc: function (json) {

                    return Array.isArray(json)
                        ? json
                        : [];
                }
            },
            columns: [
                {
                    data: null,
                    title: "#",
                    render: function (data, type, row, meta) {
                        return meta.row + 1;
                    },

                },

                { "data": "rutProveedor", title: "Rut" }, 
                { "data": "nombre", title: "Nombre" },
                { "data": "direccion", title: "Direccion" },
                {
                    data: "cPrograma",
                    title: "Programa",
                    render: function (data) {

                        return data == 1
                            ? '<span class="badge bg-info">PNI</span>'
                            : '<span class="badge bg-success">PNAC-PACAM</span>';
                    }
                },
                {
                    data: "estado",
                    title: "Activo",
                    render: function (data, type, row) {

                        if (data) {
                            return `
                            <i class="fas fa-toggle-on
                                      action-icon
                                      icon-toggle-on
                                      accionDesactivarProveedor"
                               data-rutproveedor="${row.rutProveedor}">
                            </i>`;
                        } else {
                            return `
                            <i class="fas fa-toggle-off
                                      action-icon
                                      icon-toggle-off
                                      accionActivarProveedor"
                               data-rutproveedor="${row.rutProveedor}">
                            </i>`;

                        }

                    }
                },
                {
                    data: "rutProveedor",
                    title: "Acciones",
                    render: function (data, type, row) {

                        if (!row.estado) {

                            return `
                <i class="fas fa-edit action-icon disabled">
                </i>`;
                        }

                        return `
            <i class="fas fa-edit
                      action-icon
                      icon-edit
                      btnModificarProveedorLocal"
               data-rutproveedor="${data}">
            </i>`;
                    }
                }

            ],
            language: {
                url: '//cdn.datatables.net/plug-ins/1.13.8/i18n/es-ES.json'
            },
            responsive: true,
            destroy: true,
            pageLength: 10,
            processing: true,
            autoWidth: false,
            order: [[1, 'asc']],

        });


        $('#tableroProveedor tbody')
            .off('click')
            .on('click', 'tr', function () {

                tableroProveedor
                    .rows()
                    .nodes()
                    .to$()
                    .removeClass('fila-seleccionada');

                $(this)
                    .addClass('fila-seleccionada');

                const data =
                    tableroProveedor
                        .row(this)
                        .data();


            });

    };



    function postProveedor() {

        const data = {

            RutProveedor:
                $('#new_rutProveedor').val(),

            Nombre:
                $('#new_nombreProveedor').val(),

            Direccion:
                $('#new_direccionProveedor').val(),

            Estado:
                $('#new_estadoProveedor').val(),

            CPrograma:
                $('#new_programaProveedor').val()
        };

        $.ajax({

            type: "POST",

            url: "../Proveedor/SetProveedor",
            data: data,

            success: function (res) {

                $('#modalNuevoProveedor').modal('hide');

                tableroProveedor.ajax.reload(
                    null,
                    false
                );
            },

            error: function (xhr) {

                console.error(xhr);

                alert('Ocurrió un error al procesar la solicitud');
            }

        });
    }

    function putProveedor() {

        const data = {

            RutProveedor:
                $('#new_rutProveedor').val(),

            Nombre:
                $('#new_nombreProveedor').val(),

            Direccion:
                $('#new_direccionProveedor').val(),

            Estado:
                $('#new_estadoProveedor').val(),

            CPrograma:
                $('#new_programaProveedor').val()
        };

        $.ajax({

            type: "POST",

            url: "../Proveedor/PutProveedor",
            data: data,

            success: function (res) {

                $('#modalNuevoProveedor').modal('hide');

                tableroProveedor.ajax.reload(
                    null,
                    false
                );
            },

            error: function (xhr) {

                console.error(xhr);

                alert('Ocurrió un error al procesar la solicitud');
            }

        });
    }

    function toggleProveedor(rutProveedor, estado) {

        $.ajax({

            type: "POST",

            url: "../Proveedor/ToggleProveedor",

            data: {

                rutProveedor: rutProveedor,
                estado: estado

            },

            success: function (res) {

                tableroProveedor.ajax.reload(
                    null,
                    false
                );
            },

            error: function (xhr) {

                console.error(xhr);

                alert('Ocurrió un error al procesar la solicitud');
            }

        });
    }

    $.fn.getTableroProveedor();

    $(document).on('click', '#guardarNuevoProveedor', function () {

        if (!validarFormularioProveedor()) {
            return;
        }

        postProveedor();
    });

    $(document).on('click', '#modificarProveedor', function () {
        putProveedor();
       //// tableroProveedor.ajax.reload();
    });

    $(document).on('click', '.btnModificarProveedorLocal', function () {

        $('#guardarNuevoProveedor').hide();
        $('#modificarProveedor').show();

        $('#new_rutProveedor').prop('disabled', true);

        const rut = $(this).attr('data-rutproveedor');

        GetProveedorLocal(rut);

    });

    $(document).on('click', '#btnNuevoProveedor', function () {

        $('#guardarNuevoProveedor').show();
        $('#modificarProveedor').hide();

        $('#formProveedor')[0].reset();

        $('#new_rutProveedor').prop('disabled', false);

    });

});

function GetProveedorLocal(rut) {
    $.ajax({
        type: "POST",
        url: "../Proveedor/GetProveedor",
        data: {
            rut: rut
        },
        success: function (res) {
            console.log('PROvEEDOR: ',res);
            $('#new_rutProveedor').val('');
            $('#new_nombreProveedor').val('');
            $('#new_direccionProveedor').val('');
            $('#new_estadoProveedor').val('');
            $('#new_programaProveedor').val('');

            $('#new_rutProveedor').val(res.rutProveedor);
            $('#new_nombreProveedor').val(res.nombre);
            $('#new_direccionProveedor').val(res.direccion);
            $('#new_estadoProveedor').val(res.estado ? 1 : 0);
            $('#new_programaProveedor').val(res.cPrograma);
            

            $('#modalNuevoProveedor').modal('show');
        }
    });
}

function actualizarFilaProveedor(rutModificado, nuevoProveedor) {

    var tabla = $('#tableroProveedor').DataTable();

    tabla.rows().every(function () {
        var data = this.data();
        if (data.rutProveedor === rutModificado) {
            data.nombre = nuevoProveedor.nombre;
            data.direccion = nuevoProveedor.direccion;
            data.cPrograma = nuevoProveedor.cPrograma;
            data.estado = nuevoProveedor.estado;
            this.data(data).draw(false); // false evita que se reinicie la paginación
        }
    });

}

//$(document).on('click', '.accionDesactivarProveedor', function () {

//    const rut = $(this).data('rutProveedor');

//    ////toggleProveedor(rut, false);

//});

//$(document).on('click', '.accionActivarProveedor', function () {

//    const rut = $(this).data('rutProveedor');

//    ////toggleProveedor(rut, true);

//});



function validarFormularioProveedor() {

    if ($('#new_programaProveedor').val() === '') {
        alert('Debe seleccionar un programa');
        return false;
    }

    if ($('#new_rutProveedor').val().trim() === '') {
        alert('Debe ingresar un RUT');
        return false;
    }

    if ($('#new_nombreProveedor').val().trim() === '') {
        alert('Debe ingresar un nombre');
        return false;
    }

    if ($('#new_direccionProveedor').val().trim() === '') {
        alert('Debe ingresar una dirección');
        return false;
    }

    return true;
}




// Desactivar proveedor
$(document).on('click', '.accionDesactivarProveedor', function () {

    var rut = $(this).parents('tr').find('td:eq(1)').html();
    updateProveedor(rut, 0);

});

// Activar proveedor
$(document).on('click', '.accionActivarProveedor', function () {

    var rut = $(this).parents('tr').find('td:eq(1)').html();
    updateProveedor(rut, 1);

});


function updateProveedor(rut, estado) {
    json = {
        rutProveedor: rut, estado: estado
    }
    try {
        $.ajax({
            type: "post",
            url: '../Proveedor/updateProveedor',
            data: json,
            success: function (res) {

                tableroProveedor.ajax.reload(
                    null,
                    false
                );
                showToast("La información ha sido consolidada exitosamente.", "success", 3000);

            },
            error: function (xhr) {
                showToast("updateProveedor: Ocurrió algún error", "error", 4000);
            }
        });
    } catch (ex) {
        showToast(ex.description, "error", 4000);
    }
}
