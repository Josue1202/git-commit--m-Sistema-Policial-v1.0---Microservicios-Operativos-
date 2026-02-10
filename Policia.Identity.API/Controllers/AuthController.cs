using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens; // <--- NECESARIO PARA CRIPTOGRAFÍA
using Policia.Identity.API.Models;
using System.IdentityModel.Tokens.Jwt; // <--- NECESARIO PARA CREAR EL TOKEN
using System.Security.Claims; // <--- NECESARIO PARA LOS "CLAIMS" (DATOS DEL USUARIO)
using System.Text; // <--- NECESARIO PARA LEER TEXTO

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

        // POST: api/Auth/Login
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // --- FASE 1: VALIDACIONES NORMALES (IGUAL QUE ANTES) ---

            // 1. Buscamos al usuario en la BD
            var usuario = await _context.Usuarios
                //.Include(u => u.IdRolNavigation) // Descomenta si usas Roles navegables
                .FirstOrDefaultAsync(u => u.Username == request.Usuario);

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

            // 4. Si la contraseña no coincide, adiós.
            if (usuario.PasswordHash != request.Password)
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
                Usuario = usuario.Username,
                IdPersonal = usuario.IdPersonal,
                IdRol = usuario.IdRol,
                expiracion = DateTime.Now.AddMinutes(60) //para que funcione el timepo de inactividad ps gilerto jjaj
            });
        }

        // --- MÉTODO PRIVADO: LA MÁQUINA DE HACER TOKENS ---
        // Este método no es un endpoint, es una herramienta interna solo para este controller.
        private string GenerarToken(Usuario usuario)
        {
            // A. DEFINIMOS LOS "CLAIMS" (DATOS GRABADOS EN LA PLACA)
            // Estos son los datos que Logística y RRHH leerán sin ir a la base de datos.
            var claims = new[]
            {
                // "Sub" es estándar para el "Subject" (Sujeto/Usuario)
                new Claim(JwtRegisteredClaimNames.Sub, usuario.Username),
                
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

    // CLASE DTO (IGUAL QUE ANTES)
    public class LoginRequest
    {
        public string Usuario { get; set; }
        public string Password { get; set; }
    }
}