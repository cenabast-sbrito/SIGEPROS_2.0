let tableroUsuario;
$(document).ready(function () {
    $.fn.getTableroUsuario = function () {

        tableroUsuario = $('#tableroUsuario').DataTable({

            ajax: {
                url: "../Usuario/GetUsuarios",
                dataSrc: function (json) {
                    console.log(json);
                    return Array.isArray(json)
                        ? json
                        : [];
                }
            },
            columns: [
                { "data": "rut", title: "Rut" }, { "data": "organismo", title: "Organismo" },
                { "data": "nombre", title: "Nombre" },
                { "data": "apellido", title: "Apellido" },
                {
                    "data": "estado",
                    "render": function (data, type, row) {
                        const estado = row.estado;     // null o 1 = activo
                        const rut = row.rut;
                        if (estado == null || estado === true) {
                            // Activo → mostrar botón para DESACTIVAR
                            return `
                            <i class="fas fa-toggle-on action-icon icon-toggle-on accionDesactivarUsuario"
                               title="Desactivar Usuario"
                               aria-label="Desactivar Usuario"
                               role="button"
                               data-rut="${rut}">
                            </i>`;
                        } else {
                            // Inactivo → mostrar botón para ACTIVAR
                            return `
                            <i class="fas fa-toggle-off action-icon icon-toggle-off accionActivarUsuario"
                               title="Activar Usuario"
                               aria-label="Activar Usuario"
                               role="button"
                               data-rut="${rut}">
                            </i>`;
                        }
                    }, title: "Activo"
                },
                { "data": "usuarioLDAP", title: "Usuario LDAP" }, 


                {
                    data: "rol",
                    title: "Roles",
                    render: function (data, type, row) {

                        const roles = (data || "")
                            .split(',')
                            .filter(x => x);

                        let html = `
            <div class="rol-container"
                 data-rut="${row.rut}"
                 data-organismo="${row.organismo}"
                 data-roles="${data || ''}">
        `;

                        roles.forEach(r => {

                            let color = "bg-secondary";

                            switch (r) {
                                case "ADM":
                                case "ADP":
                                case "MIN":
                                case "MIP":
                                    color = "bg-success";
                                    break;

                                case "CON":
                                case "COP":
                                    color = "bg-primary";
                                    break;
                            }

                            html += `
                <span class="badge ${color} me-1">
                    ${r}
                </span>
            `;
                        });

                        html += `
                <i class="fas fa-edit btnEditarRoles action-icon icon-edit"
                   style="cursor:pointer"
                   title="Editar roles">
                </i>
            </div>
        `;

                        return html;
                    }
                },               
                {
                    "data": "rut",
                    "render": function (data, type, row) {
                        ////console.log(row);
                        const rut = data;
                        const estado = row.estado;
                        const Nombre = row.nombre || '';
                        const Apellido = row.apellido;
                        const Rol = row.rol;
                        const Organismo = row.organismo;

                        if (estado == null || estado === true) {
                            // Ícono lápiz habilitado
                            return `
                                <i class="fas fa-edit action-icon icon-edit btnModificarUsuario"
                                   title="Modificar Usuario"
                                   aria-label="Modificar Usuario"
                                   role="button"
                                   data-rut="${rut}">
                                </i>`;
                        } else {
                            // Inactivo → deshabilitado
                            return `
                                <i class="fas fa-edit action-icon disabled"
                                   title="Usuario desactivado"
                                   aria-label="Usuario desactivado">
                                </i>`;
                        }
                        // return "<button class='btn btn-primary btnModificarUsuario' data-toggle='modal' data-target='#modalNuevoUsuario' data-rut='" + data + "'>Modificar</button>";
                    }, title: "Editar"
                }

            ],
            language: {
                url: '//cdn.datatables.net/plug-ins/1.13.8/i18n/es-ES.json'
            },
            responsive: true,
            destroy: true
        });

        $('#tableroUsuario tbody')
            .off('click')
            .on('click', 'tr', function () {

                tableroUsuario
                    .rows()
                    .nodes()
                    .to$()
                    .removeClass('fila-seleccionada');

                $(this)
                    .addClass('fila-seleccionada');

                const data =
                    tableroUsuario
                        .row(this)
                        .data();

                eximicionSeleccionado = data;

                ////cargarEximicion(data);

            });

    };

    $.fn.getTableroUsuario();

    $(document).on('click', '#guardarNuevoUsuario', function () {
        postUsuario();
        tableroUsuario.ajax.reload();
    });
    $(document).on('click', '#modificarUsuario', function () {
        putUsuario();
        tableroUsuario.ajax.reload();
    });
    $(document).on('click', '.accionDesactivarPNACPACAM', function () {

        const rut = $(this).data('rut');
        const programa = 0;

        toggleRol(rut, programa);

    });
    // Activar PNAC-PACAM
    $(document).on('click', '.accionActivarPNACPACAM', function () {
        var rut = $(this).parents('tr').find('td:eq(0)').html();
        var programa = 0;
        togglePrograma(rut, 0, 1);
        tableroUsuario.ajax.reload();
    });
    // Desactivar PNI
    $(document).on('click', '.accionDesactivarPNI', function () {
        var rut = $(this).parents('tr').find('td:eq(0)').html();
        togglePrograma(rut, 1, 0);
        tableroUsuario.ajax.reload();
    });
    // Activar PNI
    $(document).on('click', '.accionActivarPNI', function () {
        var rut = $(this).parents('tr').find('td:eq(0)').html();
        togglePrograma(rut, 1, 1);
        tableroUsuario.ajax.reload();
    });

    $(document).on('click', '.btnModificarUsuario', function () {

        $('#guardarNuevoUsuario').hide();
        $('#modificarUsuario').show();

        $('#new_rut').val('');
        $('#new_rut').prop('disabled', true);

        const rut = $(this).data('rut');

        GetUsuario(rut);

    });

    $(document).on('click', '#asignarRolUsuario', function () {

        rolUsuario = $('#listaRoles').val();
        rut = $('#new_rut').val();

        setRolUsuario(rut, rolUsuario)


    });

    $(document).on('click', '#btnNuevoUsuario', function () {

        $('#guardarNuevoUsuario').show();
        $('#modificarUsuario').hide();

        $('#formUsuario')[0].reset();
        $('#new_rut').val(''); 
        $('#new_rut').prop('disabled', false);
        $('#new_usuario_clave').val('');
        $('#new_organismo').prop('disabled', false);
    });

    $(document).on('click', '.nombre-seleccionado', function () {
        var rut = $(this).attr('id');
        var nombre = $(this).attr('data');
        $('#new_nombre').val(nombre);
        $('#new_rut').val(rut);
        $("#new_sugerencia-nombre").html("");
    });

});
function GetName(val) {
    try {
        $.ajax({
            type: "GET",
            url: '../Usuario/GetName',
            data: 'name=' + val,
            success: function (data) {
                var opt = "";
                for (var user of data) {
                    opt = opt + "<div><a class='nombre-seleccionado' id='" + user.rut + "' data='" + user.nombre + "'>" + user.nombre + "</a></div>";
                }
                $("#sugerencia-nombre").html(opt);
            },
            error: function (xhr) {
                //if (xhr.status === 404) { // 404: No se encuentra el recurso
                //    $(".loader_Tablas").fadeOut("slow");
                //    alert('003 Tu sesión ha expirado.');
                //    window.location.href = apiUrl("");
                //} else {
                avisoFinProceso("GetName: Ocurrio algún error");
                ///}
            }
        });
    } catch (ex) {
        showToast(ex.description, "error", 4000);
        //console.log(ex.description);
    }
}
function GetUsuario(rut) {

    $.ajax({
        type: "POST",
        url: "../Usuario/GetUsuario",
        data: {
            rut: rut
        },
        success: function (res) {
            $('#new_nombre').val(res.nombre);
            $('#new_apellido').val(res.apellido);
            $('#new_organismo').val(res.organismo);
            $('#new_rut').val(res.rut);
            $('#new_usuarioLDAP').val(res.usuarioLDAP);
            $('#new_usuario_estado').val(res.estado ? 1 : 0);
            $('#new_usuario_clave').val(res.clave || '');

            $('#modalNuevoUsuario').modal('show');
        }
    });
}
function GetFuncionario(rut) {

    try {
        $.ajax({
            type: "post",
            url: '../Usuario/GetFuncionario',
            contentType: 'application/json; charset=utf-8',
            data: '{ rut : ' + rut + '}',
            success: function (res) {
                $('#CodigoDpto').val(res.IdUnidadGestora, GetResponsableUnidadGestora(res.IdUnidadGestora))
            },
            error: function (xhr) {

                showToast("GetFuncionario: Ocurrió algún error", "error", 4000);
                
            }
        });
    } catch (ex) {
        showToast(ex.description, "error", 4000);
        //alert(ex.description);
    }
}
function postUsuario() {

    const data = {
        organismo: document.getElementById("new_organismo").value,
        rut: document.getElementById("new_rut").value,
        nombre: document.getElementById("new_nombre").value,
        apellido: document.getElementById("new_apellido").value,
        estado: $('#new_usuario_estado').val(), 
        usuarioLDAP: document.getElementById("new_usuarioLDAP").value,
        clave: document.getElementById("new_usuario_clave").value
    };
    $.ajax({
        type: "post",
        url: '../Usuario/PostUsuario',
        data: data,
        success: function (res) {
            $('#modalNuevoUsuario').modal('hide');
            const ok = res && (res.ok === true || res.status === true || res.success === true);
            if (ok) {
                showToast(res.message || "Usuario creado correctamente.", "success", 3000);
                if (tableroUsuario) tableroUsuario.ajax.reload(null, false);
            } else {
                showToast(res?.message || "No fue posible crear al usuario.", "error", 4000);
            }
        },
        error: function (xhr) {
            avisoFinProceso("postUsuario: Ocurrió algún error");
        }
    });
}

function putUsuario() {

    const data = {
        organismo: document.getElementById("new_organismo").value,
        rut: document.getElementById("new_rut").value,
        nombre: document.getElementById("new_nombre").value,
        apellido: document.getElementById("new_apellido").value,
        estado: $('#new_usuario_estado').val(),
        usuarioLDAP: document.getElementById("new_usuarioLDAP").value,
        clave: document.getElementById("new_usuario_clave").value
    };
    $.ajax({
        type: "post",
        url: '../Usuario/PutUsuario',
        data: data,
        success: function (res) {
            $('#modalNuevoUsuario').modal('hide');
            const ok = res && (res.ok === true || res.status === true || res.success === true);
            if (ok) {
                showToast(res.message || "Usuario actualizado correctamente.", "success", 3000);
                if (tableroUsuario) tableroUsuario.ajax.reload(null, false);
            } else {
                showToast(res?.message || "No fue posible actualizar el usuario.", "error", 4000);
            }
        },
        error: function (xhr) {
            avisoFinProceso("putUsuario: Ocurrió algún error");
        }
    });
}
function actualizarFilaUsuario(rutModificado, nuevoUsuario) {

    var tabla = $('#tableroUsuario').DataTable();

    tabla.rows().every(function () {
        var data = this.data();
        if (data.rut === rutModificado) {
            // Actualiza los campos necesarios
            data.nombre = nuevoUsuario.nombre;
            data.apellido = nuevoUsuario.apellido;
            data.rol = nuevoUsuario.rol;
            data.estado = nuevoUsuario.estado;  

            // Aplica los cambios
            this.data(data).draw(false); // false evita que se reinicie la paginación
        }
    });

}
function validaRut(rut) {

    // Despejar Puntos
    var valor = rut.value.split('.').join("");;
    // Despejar Guión
    valor = valor.replace('-', '');
    // Aislar Cuerpo y Dígito Verificador
    cuerpo = valor.slice(0, -1);
    dv = valor.slice(-1).toUpperCase();
    // Formatear RUN
    rut.value = cuerpo + '-' + dv
    //
    // Si no cumple con el mínimo ej. (n.nnn.nnn)
    // if (cuerpo.length < 7) { rut.setCustomValidity("RUT Incompleto"); return false; }
    // Calcular Dígito Verificador
    suma = 0;
    multiplo = 2;
    // Para cada dígito del Cuerpo
    for (i = 1; i <= cuerpo.length; i++) {

        // Obtener su Producto con el Múltiplo Correspondiente
        index = multiplo * valor.charAt(cuerpo.length - i);

        // Sumar al Contador General
        suma = suma + index;

        // Consolidar Múltiplo dentro del rango [2,7]
        if (multiplo < 7) { multiplo = multiplo + 1; } else { multiplo = 2; }

    }

    // Calcular Dígito Verificador en base al Módulo 11
    dvEsperado = 11 - (suma % 11);

    // Casos Especiales (0 y K)
    dv = (dv == 'K') ? 10 : dv;
    dv = (dv == 0) ? 11 : dv;

    //    console.log('VALIDANDO....');

    // Validar que el Cuerpo coincide con su Dígito Verificador
    if (dvEsperado != dv) {
        return 'ERROR';
    } else {
        return '';
    }


    // Si todo sale bien, eliminar errores (decretar que es válido)
}

$(document).on('click', '.accionDesactivarUsuario', function () {

    const rut = $(this).data('rut');

    toggleUsuario(rut, false);

});

$(document).on('click', '.accionActivarUsuario', function () {

    const rut = $(this).data('rut');

    toggleUsuario(rut, true);

});

$(document).on('click', '.accionDesactivarRol', function () {

    //data-rut="${rut}" data-estado="true" data-rol="administrador" data-programa="0"
    const rut = $(this).data('rut');
    const estado = false;
    const esAdmin = $(this).data('rol');
    const programa = $(this).data('programa');


    toggleRol(rut, estado, esAdmin, programa);

});

$(document).on('click', '.accionActivarRol', function () {

    const rut = $(this).data('rut');
    const estado = true;
    const esAdmin = $(this).data('rol');
    const programa = $(this).data('programa');


    toggleRol(rut, estado, esAdmin, programa);

});






function toggleUsuario(rut, estado) {

    const payload = {
        Rut: String(rut || "").trim(),
        Estado: estado
    };

    if (!payload.Rut) {
        showToast("El RUT es obligatorio.", "error", 4000);
        return;
    }

    $.ajax({
        type: "POST",
        url: "../Usuario/toggleUsuario",
        dataType: "json",
        data: payload,

        success: function (res) {

            const ok =
                res &&
                (
                    res.ok === true ||
                    res.status === true ||
                    res.success === true
                );

            if (ok) {

                showToast(
                    res.message || "Estado actualizado correctamente.",
                    "success",
                    3000
                );

                if (tableroUsuario) {
                    tableroUsuario.ajax.reload(null, false);
                }

            } else {

                showToast(
                    res?.message || "No fue posible actualizar el estado del usuario.",
                    "error",
                    4000
                );
            }
        },

        error: function (xhr) {

            let mensaje = "Ocurrió un error al actualizar el usuario.";

            if (xhr.responseJSON?.message) {
                mensaje = xhr.responseJSON.message;
            }

            if (xhr.status === 404) {

                showToast(
                    "Tu sesión ha expirado.",
                    "error",
                    5000
                );

                return;
            }

            showToast(
                mensaje,
                "error",
                6000
            );
        }
    });
}

function toggleRol(rut, estado, esAdmin, programa) {

    const payload = {
        Rut: rut,
        activo: estado,
        esAdmin: (esAdmin=='Admnistrador')? 1: 0,
        programa: programa
    };

    if (!payload.Rut) {
        showToast("El RUT es obligatorio.", "error", 4000);
        return;
    }

    $.ajax({
        type: "POST",
        url: "../Rol/toggleRol",
        dataType: "json",
        data: payload,

        success: function (res) {

            const ok =
                res &&
                (
                    res.ok === true ||
                    res.status === true ||
                    res.success === true
                );

            if (ok) {

                showToast(
                    res.message || "Estado actualizado correctamente.",
                    "success",
                    3000
                );

                if (tableroUsuario) {
                    tableroUsuario.ajax.reload(null, false);
                }

            } else {

                showToast(
                    res?.message || "No fue posible actualizar el estado del usuario.",
                    "error",
                    4000
                );
            }
        },

        error: function (xhr) {

            let mensaje = "Ocurrió un error al actualizar el usuario.";

            if (xhr.responseJSON?.message) {
                mensaje = xhr.responseJSON.message;
            }

            if (xhr.status === 404) {

                showToast(
                    "Tu sesión ha expirado.",
                    "error",
                    5000
                );

                return;
            }

            showToast(
                mensaje,
                "error",
                6000
            );
        }
    });
}











function obtenerRolesOrganismo(organismo) {

    if (organismo === "CENABAST") {

        return [
            {
                codigo: "ADM",
                descripcion: "Administrador PNAC-PACAM"
            },
            {
                codigo: "ADP",
                descripcion: "Administrador PNI"
            },
            {
                codigo: "CON",
                descripcion: "Consulta PNAC-PACAM"
            },
            {
                codigo: "COP",
                descripcion: "Consulta PNI"
            }
        ];
    }

    return [
        {
            codigo: "MIN",
            descripcion: "Administrador PNAC-PACAM"
        },
        {
            codigo: "MIP",
            descripcion: "Administrador PNI"
        }
        //,
        //{
        //    codigo: "CON",
        //    descripcion: "Consulta PNAC-PACAM"
        //},
        //{
        //    codigo: "COP",
        //    descripcion: "Consulta PNI"
        //}
    ];
}

$(document).on('click', '.btnEditarRoles', function () {

    const contenedor = $(this).closest('.rol-container');

    const organismo = contenedor.data('organismo');

    const rolesActuales =
        String(contenedor.data('roles') || '')
            .split(',');

    const opciones =
        obtenerRolesOrganismo(organismo);

    let html = '';

    opciones.forEach(opcion => {

        const checked =
            rolesActuales.includes(opcion.codigo)
                ? 'checked'
                : '';

        html += `
            <div class="form-check">

                <input
                    class="form-check-input chkRol"
                    type="checkbox"
                    value="${opcion.codigo}"
                    ${checked}>

                <label class="form-check-label">
                    ${opcion.descripcion}
                    (${opcion.codigo})
                </label>

            </div>
        `;
    });

    html += `

        <div class="mt-2">

            <button
                class="btn btn-success btn-sm btnGuardarRoles">
                <i class="fas fa-save"></i>
            </button>

            <button
                class="btn btn-secondary btn-sm btnCancelarRoles">
                <i class="fas fa-times"></i>
            </button>

        </div>
    `;

    contenedor.html(html);

});

$(document).on('click', '.btnCancelarRoles', function () {

    tableroUsuario.ajax.reload(
        null,
        false
    );

});



$(document).on('click', '.btnGuardarRoles', function () {

    const contenedor = $(this).closest('.rol-container');
        const rut   = contenedor.data('rut');
        const roles = contenedor.find('.chkRol:checked')
            .map(function () {
                return this.value;
            })
            .get()
            .join(',');

    $.ajax({

        type: "POST",

        url: "../Rol/ActualizarRoles",

        data: {
                rut: rut,
                roles: roles
                },

        success: function (res) {

            const ok =
                res &&
                (
                    res.ok ||
                    res.status ||
                    res.success
                );

            if (ok) {

                showToast(
                    "Roles actualizados correctamente",
                    "success",
                    3000
                );

                tableroUsuario.ajax.reload(
                    null,
                    false
                );
            }
            else {

                showToast(
                    res.message ||
                    "No fue posible actualizar los roles.",
                    "error",
                    4000
                );
            }
        },

        error: function () {

            showToast(
                "Error al guardar los roles.",
                "error",
                4000
            );
        }
    });

});





$(document).on('change', '#new_organismo', function () {

    if ($(this).val() === 'CENABAST') {
        $('#new_usuarioLDAP').prop('disabled', false);
    } else {
        $('#new_usuarioLDAP').prop('disabled', true);
    }

});


function toggleUsuarioLDAP() {
    const organismo = $('#new_organismo').val();

    $('#new_usuarioLDAP').prop(
        'disabled',
        organismo !== 'CENABAST'
    );
}

$(document).ready(function () {

    toggleUsuarioLDAP();

    $(document).on('change', '#new_organismo', function () {
        toggleUsuarioLDAP();
    });

});