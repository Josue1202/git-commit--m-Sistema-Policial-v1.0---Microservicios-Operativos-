using Microsoft.AspNetCore.Authorization; // <--- 1. IMPORTANTE: Necesario para usar [Authorize]
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Policia.RRHH.API.Models;

namespace Policia.RRHH.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // <--- 2. EL CANDADO: "Nadie entra a ver Personal sin una placa válida"
    public class PersonalController : ControllerBase
    {
        private readonly BdRrhhContext _context;

        // Aquí inyectamos la conexión a la base de datos
        public PersonalController(BdRrhhContext context)
        {
            _context = context;
        }


        // ==========================================
        // ENDPOINT 1: LISTAR TODO (YA EXISTÍA)
        // ==========================================
        // GET: api/Personal
        // Misión: Ver la lista de efectivos (AHORA PROTEGIDA)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Personal>>> GetPersonal()
        {
            // Busca en la tabla Personal e incluye el nombre del Grado y la Unidad
            return await _context.Personals
                .Include(p => p.IdGradoNavigation)  // Trae el nombre del grado
                .Include(p => p.IdUnidadActualNavigation) // Trae el nombre de la unidad
                .ToListAsync();
        }
        // ==========================================
        // ENDPOINT 2: BUSCAR POR CIP (NUEVO)
        // ==========================================
        //
        // ¿QUÉ HACE?
        // Recibe un CIP y busca al policía en la tabla Personal de BD_RRHH
        // Devuelve sus datos: nombre, apellidos, grado, unidad, etc.
        //
        // ¿PARA QUÉ SIRVE?
        // Cuando el admin quiere crear un usuario, primero necesita buscar
        // al policía por CIP para confirmar que existe y ver sus datos
        //
        // ¿CÓMO SE LLAMA DESDE BLAZOR?
        // GET https://localhost:7059/rrhh/Personal/buscar-cip/12345678
        //
        // El parámetro {cip} viaja en la URL (no en el body)
        // Ejemplo: /buscar-cip/12345678 → cip = "12345678"
        //
        [HttpGet("buscar-cip/{cip}")]
        public async Task<ActionResult<Personal>> BuscarPorCip(string cip)
        {
            // Buscamos al policía en la tabla Personal donde CIP coincida
            // Include → Trae también los datos de las tablas relacionadas:
            //   - IdGradoNavigation → Nombre del grado (SUBOFICIAL, MAYOR, etc.)
            //   - IdUnidadActualNavigation → Nombre de la unidad (DIVINCRI, etc.)
            //   - IdSituacionNavigation → Situación (ACTIVIDAD, RETIRO, etc.)
            var personal = await _context.Personals
                .Include(p => p.IdGradoNavigation)
                .Include(p => p.IdUnidadActualNavigation)
                .Include(p => p.IdSituacionNavigation)
                .FirstOrDefaultAsync(p => p.Cip == cip);
            //
            // FirstOrDefaultAsync → Busca el PRIMERO que coincida
            // Si no encuentra ninguno → devuelve null

            // Si no existe, retornamos 404 Not Found
            if (personal == null)
            {
                return NotFound("No se encontró ningún policía con ese CIP.");
            }

            // Si existe, retornamos 200 OK con los datos del policía
            return Ok(personal);
        }
    }
}