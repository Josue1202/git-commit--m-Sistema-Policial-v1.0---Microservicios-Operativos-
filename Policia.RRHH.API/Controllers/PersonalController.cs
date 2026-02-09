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
    }
}