// Este archivo define QUÉ DATOS necesita el Admin enviar para crear un usuario
// Es como un "formulario" que dice: "Para crear un usuario necesito estos campos"
//
// ¿Por qué no usamos directamente la clase Usuario?
// Porque Usuario tiene campos que el Admin NO debe enviar:
//   - IdUsuario → Lo genera SQL Server automáticamente (es autoincremental)
//   - Estado → Siempre empieza en true (activo)
//   - CreadoPor → Lo llenamos nosotros en el backend con el CIP del admin logueado
//   - FechaCreacion → Se llena con DateTime.Now automáticamente
// 
// Entonces el DTO solo pide lo ESTRICTAMENTE necesario

namespace Policia.Identity.API.DTOs
{
    public class CrearUsuarioDTO
    {
        // El CIP del policía al que le vamos a crear la cuenta
        // Este CIP ya debe existir en la tabla Personal de BD_RRHH
        // Ejemplo: "12345678"
        public string Cip { get; set; } = null!;

        // El ID del policía en la tabla Personal de BD_RRHH
        // Este número viene de la búsqueda que hicimos antes en el frontend
        // Ejemplo: 5
        public int IdPersonal { get; set; }

        // La contraseña que tendrá el nuevo usuario para loguearse
        // Ejemplo: "MiContraseña123"
        public string Password { get; set; } = null!;

        // El rol que tendrá: 1 = Admin, 2 = Operador, etc.
        // Viene de la tabla Rol en BD_Seguridad
        // Ejemplo: 1
        public int IdRol { get; set; }
    }
}
