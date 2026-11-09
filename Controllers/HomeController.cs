using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SIGEPROS.Models;
using SIGEPROS.Services;
using SIGEPROS.Utils;
using System.Diagnostics;
using System.DirectoryServices;
using System.Security.Claims;

namespace SIGEPROS.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment _environment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly DBSeguridadService _seguridadservice;
        private readonly DBProveedorService _proveedorservice;
        private readonly DBUsuarioService _usuarioservice;

        public HomeController(ILogger<HomeController> logger, IWebHostEnvironment hostingEnvironment, IHttpContextAccessor httpContextAccessor, DBProveedorService proveedorservice, DBUsuarioService usuarioservice, DBSeguridadService seguridadservice)
        {
            _logger = logger;
            _environment = hostingEnvironment;
            _httpContextAccessor = httpContextAccessor;
            _proveedorservice = proveedorservice;
            _usuarioservice = usuarioservice;
            _seguridadservice = seguridadservice;
        }
        [AllowAnonymous]
        public async Task<IActionResult> Index(string returnUrl)
        {
            try
            {
                string token = HttpContext.Request.Query["token"];

                // Si NO viene token (es la pantalla de login estándar)
                if (string.IsNullOrWhiteSpace(token))
                {
                    // Cerrar sesión activa y destruir cookies para que "Atrás" no pueda reusarlas
                    if (User.Identity != null && User.Identity.IsAuthenticated)
                    {
                        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    }

                    foreach (var cookie in Request.Cookies.Keys)
                    {
                        Response.Cookies.Delete(cookie);
                    }

                    ViewBag.ReturnUrl = returnUrl;
                    ViewData["ActiveView"] = "index";
                    return View();
                }

                // Si viene token, proceder con autenticación por token...
                var datosToken = _seguridadservice.GetUsuarioByToken(token);

                if (datosToken == null)
                {
                    TempData["MensajeError"] = "Token inválido";
                    return RedirectToAction("Index");
                }
                if (datosToken.Rut.IsNullOrEmpty()) { 
                TempData["MensajeError"] = "Usuario no encontrado";
                return RedirectToAction("Index");
            }

                // Obtener usuario
                UsuarioModel usuario = _usuarioservice.GetUsuario(datosToken.Rut);

                if (usuario == null)
                {
                    TempData["MensajeError"] = "Usuario no encontrado";
                    return RedirectToAction("Index");
                }

                if (!usuario.Estado)
                {
                    TempData["MensajeError"] = "Esta cuenta se encuentra desactivada";
                    return RedirectToAction("Index");
                }

                string programa = usuario.programa.ToString();


                string programaToken = datosToken.programa.ToString() switch
                {
                    "20" => "0",
                    "21" => "1",
                    _ => datosToken.programa.ToString()
                };

                // Validar autorización al programa
                if (!_seguridadservice.AuthorizationRUT(usuario.Rut, programaToken))
                {
                    string programaSel = programaToken == "0" ? "PNAC-PACAM" : (programaToken == "1" ? "PNI" : programaToken);
                    TempData["MensajeError"] = $"El usuario no cuenta con permisos suficientes para acceder al programa {programaSel}";
                    return RedirectToAction("Index");
                }

                // Crear claims idénticos a Autenticar
                string nombreUsuario = !string.IsNullOrWhiteSpace(usuario.Nombre) ? usuario.Nombre : usuario.Rut;
                var claims = CrearClaims(usuario, nombreUsuario, programaToken);

                var claimsIdentity = new ClaimsIdentity(
                    claims,
                    CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity));

                TempData["App_RutUsuario"] = usuario.Rut;

                if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Dashboard");
            }
            catch (Exception)
            {
                TempData["MensajeError"] = "Error al procesar la autenticación";
                return RedirectToAction("Index");
            }
        }
        [AllowAnonymous]     
        public async Task<IActionResult> AutenticarRUT(FuncionarioModel funcionario)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View("Index");
                }

                // El campo "Rut" en la base de datos se guarda sin puntos, sin guión y sin dígito verificador
                /// ya no es necesario porque se lo estoy pidiendo sin dv al usuario 
                /// string rutLimpio = DBUsuarioService.LimpiarRut(funcionario.nombre_usuario);
                string rutLimpio = funcionario.nombre_usuario;

                UsuarioModel usuario = _usuarioservice.GetUsuario(rutLimpio);

                if (usuario == null || string.IsNullOrWhiteSpace(usuario.Rut))
                {
                    TempData["MensajeError"] = "Ingrese usuario y contraseña válidos";
                    return RedirectToAction("Index");
                }

                if (!_seguridadservice.AuthorizationRUT(
                        rutLimpio,
                        funcionario.nombre_programa.ToString()))
                {
                    var programaSel = funcionario.nombre_programa.ToString();

                    if (programaSel.Equals("0"))
                        programaSel = "PNAC-PACAM";

                    if (programaSel.Equals("1"))
                        programaSel = "PNI";

                    TempData["MensajeError"] =
                        "El usuario no cuenta con permisos suficientes para acceder al programa " +
                        programaSel;

                    return RedirectToAction("Index");
                }

                if (!_seguridadservice.AuthenticateRUT(
                        rutLimpio,
                        funcionario.password))
                {
                    TempData["MensajeError"] =
                        "Usuario o Contraseña incorrectos";

                    return RedirectToAction("Index");
                }

                if (!usuario.Estado)
                {
                    TempData["MensajeError"] =
                        "Esta cuenta se encuentra desactivada";

                    return RedirectToAction("Index");
                }

                var claims = CrearClaims(
                    usuario,
                    usuario.Nombre ?? usuario.Rut,
                    funcionario.nombre_programa?.ToString());

                var claimsIdentity = new ClaimsIdentity(
                    claims,
                    CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity));

                TempData["App_RutUsuario"] = usuario.Rut;

                ViewBag.ReturnUrl = getReturnURL(
                    Request.Form["ReturnUrl"].ToString());

                return RedirectToAction("Dashboard");
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] =
                    "El usuario y/o contraseña no son válidos";

                return RedirectToAction("Index");
            }
        }



        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AutenticarLDAP(FuncionarioModel funcionario)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    //UsuarioModel usuario = _usuarioservice.GetUsuario(
                    //    funcionario.nombre_usuario);


                    UsuarioModel usuario = new UsuarioModel();
                                        var de = new DirectoryEntry(
                                            "LDAP://10.8.0.250/DC=cenabast,DC=local",
                                            funcionario.nombre_usuario,
                                            funcionario.password
                                        );

                                        DirectorySearcher dsearch = new DirectorySearcher(de);
                                        dsearch.Filter = "sAMAccountName=" + funcionario.nombre_usuario;

                                        SearchResult results = dsearch.FindOne();

                    usuario = _seguridadservice
                        .GetUsuarioLDAP(funcionario.nombre_usuario, funcionario.id_programa.ToString());

                    if (!_seguridadservice.AuthorizationLDAP(funcionario.nombre_usuario, funcionario.nombre_programa.ToString()))    // SELECT rut FROM usuarios where rut = @Rut and estado=1;
                    {
                        var programaSel = funcionario.nombre_programa.ToString();
                        if (programaSel.Equals("0")) programaSel = "PNAC-PACAM";
                        if (programaSel.Equals("1")) programaSel = "PNI";
                        TempData["MensajeError"] =
                            "El usuario no cuenta con permisos suficientes para acceder al programa " + programaSel;

                        return RedirectToAction("Index");
                    } 

                    //if (!_seguridadservice.AuthenticateLDAP(funcionario.nombre_usuario, funcionario.password))
                    //{
                    //    TempData["MensajeError"] =
                    //        "Usuario o Contraseña incorrectos";

                    //    return RedirectToAction("Index");
                    //} 

                    ////usuario = _seguridadservice.GetUsuario(user[0], prog);

                    if (!usuario.Estado)
                    {
                        TempData["MensajeError"] =
                            "Esta cuenta se encuentra desactivada";

                        return RedirectToAction("Index");
                    } //return Json(new { state = false, message = "Cuenta desactivada" });

                    if (usuario == null)
                    {
                        TempData["MensajeError"] =
                            "Ingrese usuario y contraseña";

                        return RedirectToAction("Index");
                    }

                    ///                    Funcionario func = new Funcionario();
                    /*
                                        func.email = results.GetDirectoryEntry()
                                                            .Properties["mail"]
                                                            ?.Value?.ToString()
                                                            ?? "";
                    */
                    var claims = new List<Claim>
                            {
                                new Claim(ClaimTypes.Name, funcionario.nombre_usuario),
                                new Claim("Rut", usuario.Rut ?? ""),
                                new Claim("Programa", funcionario.nombre_programa ?? "")
                            };

                    var roles = usuario.Rol?
                        .Split(',')
                        .Select(r => r.Trim())
                        .ToList() ?? new List<string>();

                    string programa = funcionario.nombre_programa?.ToUpper() ?? "";
                    string rolSeleccionado = null;

                    // PNAC/PACAM
                    if (programa.Contains("0")) ///PNAC-PACAM
                    {
                        if (roles.Contains("ADM"))
                            rolSeleccionado = "ADM";
                        else if (roles.Contains("CON"))
                            rolSeleccionado = "CON";
                        else if (roles.Contains("MIN"))
                            rolSeleccionado = "MIN";
                    }

                    // PNI
                    else if (programa.Contains("1")) ///PNI
                    {
                        if (roles.Contains("ADP"))
                            rolSeleccionado = "ADP";
                        else if (roles.Contains("COP"))
                            rolSeleccionado = "COP";
                        else if (roles.Contains("MIP"))
                            rolSeleccionado = "MIP";
                    }

                    if (!string.IsNullOrEmpty(rolSeleccionado))
                    {
                        claims.Add(new Claim(ClaimTypes.Role, rolSeleccionado));
                        claims.Add(new Claim("rolSeleccionado", rolSeleccionado)); /// ROL seleccionado según el programa al cual ha ingresado el usuario
                        switch (rolSeleccionado)
                        {
                            case "ADM":
                                claims.Add(new Claim("Programa_0", "Administra"));
                                break;

                            case "CON":
                            case "MIN":
                                claims.Add(new Claim("Programa_0", "Consulta"));
                                break;

                            case "ADP":
                                claims.Add(new Claim("Programa_1", "Administra"));
                                break;

                            case "COP":
                            case "MIP":
                                claims.Add(new Claim("Programa_1", "Consulta"));
                                break;
                        }
                    }

                    var claimsIdentity = new ClaimsIdentity(
                        claims,
                        CookieAuthenticationDefaults.AuthenticationScheme);

                    await HttpContext.SignInAsync(
                        new ClaimsPrincipal(claimsIdentity));

                    TempData["App_RutUsuario"] = usuario.Rut;
                    ViewBag.ReturnUrl = getReturnURL(Request.Form["ReturnUrl"].ToString());

                    return RedirectToAction("Dashboard");

                    ///                    return RedirectToAction("Dashboard");
                }
            }
            catch (Exception)
            {
                TempData["MensajeError"] =
                    "El usuario y/o contraseña no son válidos";

                return RedirectToAction("Index");
            }

            return View("Index");
        }

        private List<Claim> CrearClaims( UsuarioModel usuario, string nombreUsuario, string programa)
        {
            var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, nombreUsuario),
                            new Claim("Rut", usuario.Rut ?? ""),
                            new Claim("Programa", programa ?? "")
                        };

            var roles = usuario.Rol?
                .Split(',')
                .Select(r => r.Trim())
                .ToList() ?? new List<string>();

            string rolSeleccionado = null;

            // PNAC-PACAM
            if (programa == "0")
            {
                if (roles.Contains("ADM"))
                    rolSeleccionado = "ADM";
                else if (roles.Contains("CON"))
                    rolSeleccionado = "CON";
                else if (roles.Contains("MIN"))
                    rolSeleccionado = "MIN";
            }

            // PNI
            else if (programa == "1")
            {
                if (roles.Contains("ADP"))
                    rolSeleccionado = "ADP";
                else if (roles.Contains("COP"))
                    rolSeleccionado = "COP";
                else if (roles.Contains("MIP"))
                    rolSeleccionado = "MIP";
            }

            if (!string.IsNullOrEmpty(rolSeleccionado))
            {
                claims.Add(new Claim(ClaimTypes.Role, rolSeleccionado));
                claims.Add(new Claim("rolSeleccionado", rolSeleccionado));

                switch (rolSeleccionado)
                {
                    case "ADM":
                        claims.Add(new Claim("Programa_0", "Administra"));
                        break;

                    case "CON":
                    case "MIN":
                        claims.Add(new Claim("Programa_0", "Consulta"));
                        break;

                    case "ADP":
                        claims.Add(new Claim("Programa_1", "Administra"));
                        break;

                    case "COP":
                    case "MIP":
                        claims.Add(new Claim("Programa_1", "Consulta"));
                        break;
                }
            }

            return claims;
        }


        [AllowAnonymous]
        public IActionResult _logout()
        {
            ViewData["ActiveView"] = "_logout";
            return View();
        }
        public IActionResult AccessDenied()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult _error()
        {
            ViewBag.ErrorMessage =
                TempData["AuthError"]?.ToString()
                ?? "No tiene credenciales válidas para acceder al sistema.";

            return View();
        }
        [HttpGet]
        public IActionResult Dashboard()
        {
            return View();
        }
        [HttpGet]
        public IActionResult viewFacturacion()
        {
            return View("../PerfilAdministrador/FiltroTabla");
        }


        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new AuthenticationProperties
                {
                    RedirectUri = Url.Action("_logout", "Home")
                });

            // Eliminar la cookie de autenticación de forma explícita
            Response.Cookies.Delete(".SIGEPROS.Auth", new CookieOptions
            {
                Path = "/"
            });

            // Limpieza preventiva de cualquier otra cookie
            foreach (var cookie in Request.Cookies.Keys)
            {
                Response.Cookies.Delete(cookie, new CookieOptions { Path = "/" });
            }
            //return RedirectToAction("index", "Home");
            return RedirectToAction("_logout", "Home");
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        string getReturnURL(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return PathHelper.GetPath(url, _httpContextAccessor.HttpContext);
            }


            return string.Empty;
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> AutenticarToken(string token, string returnUrl = null)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                TempData["AuthError"] = "Token vacío o nulo.";
                return RedirectToAction("_error", "Home");
            }

            try
            {
                var usuario = _proveedorservice.GetUsuarioByToken(token);

                if (usuario != null && usuario.autenticado)
                {
                    string nombre = string.IsNullOrWhiteSpace(usuario.Nombre) ? "Usuario" : usuario.Nombre;
                    string rutStr = usuario.Rut?.ToString() ?? string.Empty;

                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, nombre),
                new Claim(ClaimTypes.NameIdentifier, rutStr),
              //  new Claim(ClaimTypes.Role, RolesApp.Usuario)
            };

                    TempData["canjeApp_RutProveedor"] = rutStr;
                    TempData["canjeApp_nombreproveedor"] = nombre;

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        principal,
                        new AuthenticationProperties
                        {
                            IsPersistent = false,
                            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(1)
                        }
                    );

                    if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                        return Redirect(returnUrl);

                    return RedirectToAction("Dashboard", "Home");
                }

                // ❌ Token inválido / usado / vencido
                TempData["AuthError"] = "Token inválido, vencido o ya utilizado.";
                return RedirectToAction("_error", "Home");
            }
            catch (Exception)
            {
                TempData["AuthError"] = "Error técnico al validar el token.";
                return RedirectToAction("_error", "Home");
            }
        }

    }
}
