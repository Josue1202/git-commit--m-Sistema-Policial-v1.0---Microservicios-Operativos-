using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens; // <--- NECESARIO PARA CRIPTOGRAFÍA
using Policia.Identity.API.DTOs;
using Policia.Identity.API.Models;
using System.IdentityModel.Tokens.Jwt; // <--- NECESARIO PARA CREAR EL TOKEN
using System.Security.Claims; // <--- NECESARIO PARA LOS "CLAIMS" (DATOS DEL USUARIO)
using System.Text;

namespace Policia.Identity.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly BdSeguridadContext _context;
        private readonly IConfiguration _configuration; // <--- NUEVO: Variable para leer el appsettings.json

        // CONSTRUCTOR MODIFICADO:
        // Ahora pedimos "context" (Base de Datos) y "configuration" (Para leer la Clave Secreta)
        public AuthController(BdSeguridadContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }


        // ==========================================
        // ENDPOINT 1: LOGIN (YA EXISTÍA)
        // ==========================================
        // POST: api/Auth/Login
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO request)
        {
            // --- FASE 1: VALIDACIONES NORMALES (IGUAL QUE ANTES) ---
            // 1. Buscamos al usuario en la BD
            var usuario = await _context.Usuarios
                //.Include(u => u.IdRolNavigation) // Descomenta si usas Roles navegables
                .FirstOrDefaultAsync(u => u.Cip == request.Cip);

            // 2. Si no existe, adiós.
            if (usuario == null)
            {
                return Unauthorized("El usuario no existe.");
            }

            // 3. Si está inactivo (Estado false), adiós.
            if (usuario.Estado == false)
            {
                return Unauthorized("Su usuario está DADO DE BAJA. No puede entrar.");
            }

            // 4. Verificar contraseña con BCrypt (migración automática de texto plano)
            bool passwordValida;
            if (usuario.PasswordHash.StartsWith("$2"))
            {
                // La contraseña ya está hasheada con BCrypt → verificar normalmente
                passwordValida = BCrypt.Net.BCrypt.Verify(request.Password, usuario.PasswordHash);
            }
            else
            {
                // Contraseña legacy en texto plano → verificar y migrar a BCrypt
                passwordValida = usuario.PasswordHash == request.Password;
                if (passwordValida)
                {
                    usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
                    await _context.SaveChangesAsync();
                }
            }

            if (!passwordValida)
            {
                return Unauthorized("Contraseña incorrecta.");
            }

            // --- FASE 2: GENERACIÓN DE LA PLACA (NUEVO) ---
            // Si llegamos aquí, el usuario es legítimo. ¡A fabricar su placa!
            var tokenGenerado = GenerarToken(usuario);

            // 5. RESPUESTA FINAL
            return Ok(new
            {
                Mensaje = "Acceso Autorizado 👮‍♂️",
                Token = tokenGenerado, // <--- AQUÍ ENTREGAMOS LA PLACA AL FRONTEND
                Usuario = usuario.Cip,
                IdPersonal = usuario.IdPersonal,
                IdRol = usuario.IdRol,
                expiracion = DateTime.Now.AddHours(2)
            });
        }

        // ==========================================
        // ENDPOINT 2: CREAR USUARIO (NUEVO)
        // ==========================================
        // 
        // ¿QUÉ HACE?
        // Recibe los datos de un policía (CIP, IdPersonal, Password, IdRol)
        // y crea un registro en la tabla Usuario de BD_Seguridad
        // para que ese policía pueda loguearse en el sistema.
        //
        // ¿QUIÉN PUEDE USARLO?
        // Solo un Admin que ya esté logueado (por eso tiene [Authorize])
        //
        // ¿CÓMO SE LLAMA DESDE BLAZOR?
        // POST https://localhost:7059/seguridad/CrearUsuario
        // Body: { "cip": "12345678", "idPersonal": 5, "password": "123", "idRol": 1 }
        //
        [Authorize]
        [HttpPost("CrearUsuario")]
        public async Task<IActionResult> CrearUsuario([FromBody] CrearUsuarioDTO request)
        {
            // ──────────────────────────────────────────────
            // VALIDACIÓN 1: ¿Ya existe un usuario con ese CIP?
            // ──────────────────────────────────────────────
            // Buscamos en la tabla Usuario si ya hay alguien con ese CIP
            // Si ya existe, no podemos crear otro (el CIP es único)
            var existeUsuario = await _context.Usuarios
                .AnyAsync(u => u.Cip == request.Cip);
            //
            // AnyAsync devuelve true/false
            // true = "Sí, ya hay un usuario con ese CIP"
            // false = "No existe, podemos crearlo"

            if (existeUsuario)
            {
                // Retornamos 409 Conflict → "Ya existe, no lo puedo crear de nuevo"
                return Conflict("Este policía ya tiene una cuenta de usuario creada.");
            }

            // ──────────────────────────────────────────────
            // VALIDACIÓN 2: ¿El Rol existe en la tabla Rol?
            // ──────────────────────────────────────────────
            // Verificamos que el IdRol que mandó el admin sea válido
            // Por ejemplo, si solo hay Rol 1 y 2, y el admin manda 99, eso no existe
            var existeRol = await _context.Rols
                .AnyAsync(r => r.IdRol == request.IdRol);

            if (!existeRol)
            {
                // Retornamos 400 Bad Request → "El rol que me mandaste no existe"
                return BadRequest("El rol especificado no existe.");
            }

            // ──────────────────────────────────────────────
            // OBTENER EL CIP DEL ADMIN QUE ESTÁ CREANDO
            // ──────────────────────────────────────────────
            // Cuando el admin se logueó, su token JWT tiene "claims" (datos)
            // Uno de esos claims es "Sub" que contiene su CIP
            // Aquí lo extraemos para saber QUIÉN está creando este usuario
            //
            // User.FindFirst(...) busca dentro del token del admin logueado
            // JwtRegisteredClaimNames.Sub = "sub" = el CIP del admin
            var cipAdmin = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? "SISTEMA";
            //
            // Si por alguna razón no encuentra el CIP, pone "SISTEMA" como respaldo
            // Esto no debería pasar si el admin está logueado correctamente

            // ──────────────────────────────────────────────
            // CREAR EL OBJETO USUARIO
            // ──────────────────────────────────────────────
            // Aquí armamos el nuevo registro que se va a insertar en la tabla Usuario
            var nuevoUsuario = new Usuario
            {
                // Datos que vienen del formulario (lo que el admin llenó)
                Cip = request.Cip,                    // CIP del policía
                IdPersonal = request.IdPersonal,      // FK al policía en BD_RRHH
                IdRol = request.IdRol,                // FK al rol (Admin, Operador, etc.)
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),

                // Datos automáticos (NO los llena el admin, los llenamos nosotros)
                Estado = true,                        // Siempre inicia ACTIVO
                CreadoPor = cipAdmin,                 // CIP del admin que creó esta cuenta
                FechaCreacion = DateTime.Now           // Fecha y hora exacta de creación
                // ModificadoPor = null               → No se ha modificado aún
                // FechaModificacion = null            → No se ha modificado aún
            };

            // ──────────────────────────────────────────────
            // GUARDAR EN LA BASE DE DATOS
            // ──────────────────────────────────────────────
            // _context.Usuarios.Add() → "Agrega este objeto a la lista pendiente"
            // SaveChangesAsync() → "Ejecuta el INSERT en SQL Server de verdad"
            //
            // Es como hacer:
            // INSERT INTO Usuario (Cip, IdPersonal, IdRol, PasswordHash, Estado, CreadoPor, FechaCreacion)
            // VALUES ('12345678', 5, 1, '123', 1, 'CIP_DEL_ADMIN', '2026-02-13 15:30:00')
            _context.Usuarios.Add(nuevoUsuario);
            await _context.SaveChangesAsync();

            // ──────────────────────────────────────────────
            // RESPUESTA EXITOSA
            // ──────────────────────────────────────────────
            // Retornamos 201 Created → "Se creó correctamente"
            // Le devolvemos algunos datos para que el frontend los muestre
            return Created("", new
            {
                Mensaje = "✅ Usuario creado exitosamente",
                IdUsuario = nuevoUsuario.IdUsuario,   // El ID que SQL Server generó
                Cip = nuevoUsuario.Cip,               // El CIP del nuevo usuario
                IdRol = nuevoUsuario.IdRol,            // El rol asignado
                CreadoPor = nuevoUsuario.CreadoPor,    // Quién lo creó (auditoría)
                FechaCreacion = nuevoUsuario.FechaCreacion // Cuándo se creó (auditoría)
            });
        }

        // ==========================================
        // ENDPOINT 3: LISTAR USUARIOS (NUEVO)
        // ==========================================
        // GET: api/Auth/Listar
        //
        // Devuelve TODOS los usuarios con su Rol.
        // El frontend lo usa para mostrar la tabla en ListarUsuarios.razor
        //
        [Authorize]
        [HttpGet("Listar")]
        public async Task<IActionResult> Listar()
        {
            var usuarios = await _context.Usuarios
                .Include(u => u.IdRolNavigation)   // JOIN con tabla Rol para traer el Nombre
                .Select(u => new                    // Proyección: solo enviamos lo necesario
                {
                    u.IdUsuario,
                    u.Cip,
                    u.IdPersonal,
                    u.Estado,
                    u.IdRol,
                    Rol = u.IdRolNavigation.Nombre, // "Administrador", "Operador", etc.
                    u.CreadoPor,
                    u.FechaCreacion,
                    u.ModificadoPor,
                    u.FechaModificacion
                })
                .ToListAsync();

            return Ok(usuarios);
        }

        // ==========================================
        // ENDPOINT 4: DESACTIVAR USUARIO (NUEVO)
        // ==========================================
        // PUT: api/Auth/Desactivar/5
        //
        // NO borra al usuario, solo pone Estado = false
        // El usuario ya no podrá loguearse (el Login valida Estado)
        //
        [Authorize]
        [HttpPut("Desactivar/{id}")]
        public async Task<IActionResult> Desactivar(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null)
            {
                return NotFound("Usuario no encontrado.");
            }

            if (usuario.Estado == false)
            {
                return BadRequest("Este usuario ya está desactivado.");
            }

            // Cambiar estado
            usuario.Estado = false;

            // Auditoría: quién lo desactivó y cuándo
            usuario.ModificadoPor = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? "SISTEMA";
            usuario.FechaModificacion = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Mensaje = $"⛔ Usuario {usuario.Cip} desactivado correctamente.",
                ModificadoPor = usuario.ModificadoPor,
                FechaModificacion = usuario.FechaModificacion
            });
        }

        // ==========================================
        // ENDPOINT 5: REACTIVAR USUARIO (NUEVO)
        // ==========================================
        // PUT: api/Auth/Reactivar/5
        //
        // Vuelve a poner Estado = true
        // El usuario podrá loguearse otra vez
        //
        [Authorize]
        [HttpPut("Reactivar/{id}")]
        public async Task<IActionResult> Reactivar(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null)
            {
                return NotFound("Usuario no encontrado.");
            }

            if (usuario.Estado == true)
            {
                return BadRequest("Este usuario ya está activo.");
            }

            // Cambiar estado
            usuario.Estado = true;

            // Auditoría
            usuario.ModificadoPor = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? "SISTEMA";
            usuario.FechaModificacion = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Mensaje = $"✅ Usuario {usuario.Cip} reactivado correctamente.",
                ModificadoPor = usuario.ModificadoPor,
                FechaModificacion = usuario.FechaModificacion
            });
        }


        // ==========================================
        // ENDPOINT 6: OBTENER USUARIO POR ID
        // ==========================================
        // GET: api/Auth/ObtenerUsuario/5
        //
        // Devuelve los datos de UN solo usuario para llenar el formulario de edición
        //
        [Authorize]
        [HttpGet("ObtenerUsuario/{id}")]
        public async Task<IActionResult> ObtenerUsuario(int id)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.IdRolNavigation)
                .FirstOrDefaultAsync(u => u.IdUsuario == id);

            if (usuario == null)
            {
                return NotFound("Usuario no encontrado.");
            }

            return Ok(new
            {
                usuario.IdUsuario,
                usuario.Cip,
                usuario.IdPersonal,
                usuario.IdRol,
                Rol = usuario.IdRolNavigation.Nombre,
                usuario.Estado,
                usuario.CreadoPor,
                usuario.FechaCreacion,
                usuario.ModificadoPor,
                usuario.FechaModificacion
            });
        }

        // ==========================================
        // ENDPOINT 7: EDITAR USUARIO
        // ==========================================
        // PUT: api/Auth/EditarUsuario/5
        //
        // Permite cambiar el Rol y opcionalmente resetear la contraseña
        // El CIP y IdPersonal NO se modifican (son datos de identidad)
        //
        [Authorize]
        [HttpPut("EditarUsuario/{id}")]
        public async Task<IActionResult> EditarUsuario(int id, [FromBody] EditarUsuarioDTO request)
        {
            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null)
            {
                return NotFound("Usuario no encontrado.");
            }

            // Validar que el rol exista
            var existeRol = await _context.Rols.AnyAsync(r => r.IdRol == request.IdRol);
            if (!existeRol)
            {
                return BadRequest("El rol especificado no existe.");
            }

            // Actualizar el rol
            usuario.IdRol = request.IdRol;

            // Si el admin envió una nueva contraseña, la hasheamos y actualizamos
            if (!string.IsNullOrWhiteSpace(request.NuevaPassword))
            {
                usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NuevaPassword);
            }

            // Auditoría
            usuario.ModificadoPor = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? "SISTEMA";
            usuario.FechaModificacion = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Mensaje = $"✅ Usuario {usuario.Cip} editado correctamente.",
                usuario.IdUsuario,
                usuario.IdRol,
                usuario.ModificadoPor,
                usuario.FechaModificacion
            });
        }

        // ==========================================
        // ENDPOINT 8: LISTAR ROLES
        // ==========================================
        // GET: api/Auth/Roles
        //
        // Devuelve todos los roles para el dropdown del formulario
        //
        [Authorize]
        [HttpGet("Roles")]
        public async Task<IActionResult> ListarRoles()
        {
            var roles = await _context.Rols
                .Select(r => new { r.IdRol, r.Nombre, r.Descripcion })
                .ToListAsync();

            return Ok(roles);
        }

        // ==========================================
        // MÉTODO PRIVADO: GENERAR TOKEN (YA EXISTÍA)
        // ==========================================

        // --- MÉTODO PRIVADO: LA MÁQUINA DE HACER TOKENS ---
        // Este método no es un endpoint, es una herramienta interna solo para este controller.
        private string GenerarToken(Usuario usuario)
        {
            // A. DEFINIMOS LOS "CLAIMS" (DATOS GRABADOS EN LA PLACA)
            // Estos son los datos que Logística y RRHH leerán sin ir a la base de datos.
            var claims = new[]
            {
                // "Sub" es estándar para el "Subject" (Sujeto/Usuario)
                new Claim(JwtRegisteredClaimNames.Sub, usuario.Cip),             

                // Guardamos el ID Personal para que Logística sepa quién pide el arma
                new Claim("idPersonal", usuario.IdPersonal.ToString()),              

                // Guardamos el Rol para saber si es Admin o Comisario
                new Claim(ClaimTypes.Role, usuario.IdRol.ToString())
            };

            // B. PREPARAMOS LA LLAVE MAESTRA (LA FIRMA DEL COMISARIO)
            // Leemos la palabra secreta del appsettings.json
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

            // C. PREPARAMOS EL SELLO (ALGORITMO DE ENCRIPTACIÓN)
            // HmacSha256 es el estándar militar de encriptación hoy en día.
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // D. FABRICAMOS EL TOKEN FÍSICO
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],    // ¿Quién lo emite? (Policia.Identity)
                audience: _configuration["Jwt:Audience"], // ¿Para quién es? (Policia.Sistemas)
                claims: claims,                           // Los datos del usuario
                expires: DateTime.Now.AddHours(2),        // EXPIRACIÓN: Vence en 2 horas exactas
                signingCredentials: creds                 // ¡FIRMAMOS EL DOCUMENTO!
            );

            // E. CONVERTIMOS EL OBJETO A STRING (TEXTO)
          return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

