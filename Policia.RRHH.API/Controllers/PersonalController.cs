using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Policia.RRHH.API.Models;

namespace Policia.RRHH.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PersonalController : ControllerBase
    {
        private readonly BdRrhhContext _context;

        public PersonalController(BdRrhhContext context)
        {
            _context = context;
        }

        // ══════════════════════════════════════
        // GET: api/Personal
        // Listar todos los efectivos
        // ══════════════════════════════════════
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Personal>>> GetPersonal()
        {
            return await _context.Personals
                .Include(p => p.IdGradoNavigation)
                .Include(p => p.IdUnidadActualNavigation)
                .Include(p => p.IdSituacionNavigation)
                .ToListAsync();
        }

        // ══════════════════════════════════════
        // GET: api/Personal/5
        // Obtener un personal por ID
        // ══════════════════════════════════════
        [HttpGet("{id}")]
        public async Task<ActionResult<Personal>> GetPersonalById(int id)
        {
            var personal = await _context.Personals
                .Include(p => p.IdGradoNavigation)
                .Include(p => p.IdUnidadActualNavigation)
                .Include(p => p.IdSituacionNavigation)
                .FirstOrDefaultAsync(p => p.IdPersonal == id);

            if (personal == null)
                return NotFound("No se encontró al efectivo.");

            return Ok(personal);
        }

        // ══════════════════════════════════════
        // GET: api/Personal/buscar-cip/{cip}
        // Buscar por CIP
        // ══════════════════════════════════════
        [HttpGet("buscar-cip/{cip}")]
        public async Task<ActionResult<Personal>> BuscarPorCip(string cip)
        {
            var personal = await _context.Personals
                .Include(p => p.IdGradoNavigation)
                .Include(p => p.IdUnidadActualNavigation)
                .Include(p => p.IdSituacionNavigation)
                .FirstOrDefaultAsync(p => p.Cip == cip);

            if (personal == null)
                return NotFound("No se encontró ningún policía con ese CIP.");

            return Ok(personal);
        }

        // ══════════════════════════════════════
        // POST: api/Personal
        // Crear nuevo personal
        // ══════════════════════════════════════
        [HttpPost]
        public async Task<ActionResult<Personal>> CrearPersonal(Personal personal)
        {
            // Verificar que CIP no exista
            var existeCip = await _context.Personals.AnyAsync(p => p.Cip == personal.Cip);
            if (existeCip)
                return BadRequest("Ya existe un efectivo con ese CIP.");

            // Verificar que DNI no exista
            var existeDni = await _context.Personals.AnyAsync(p => p.Dni == personal.Dni);
            if (existeDni)
                return BadRequest("Ya existe un efectivo con ese DNI.");

            personal.Estado = true;
            _context.Personals.Add(personal);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPersonalById), new { id = personal.IdPersonal }, personal);
        }

        // ══════════════════════════════════════
        // PUT: api/Personal/5
        // Editar personal existente
        // ══════════════════════════════════════
        [HttpPut("{id}")]
        public async Task<IActionResult> EditarPersonal(int id, Personal personal)
        {
            if (id != personal.IdPersonal)
                return BadRequest("El ID no coincide.");

            var existente = await _context.Personals.FindAsync(id);
            if (existente == null)
                return NotFound("No se encontró al efectivo.");

            // Actualizar campos
            existente.Nombres = personal.Nombres;
            existente.Apellidos = personal.Apellidos;
            existente.Dni = personal.Dni;
            existente.Cip = personal.Cip;
            existente.Sexo = personal.Sexo;
            existente.FechaNacimiento = personal.FechaNacimiento;
            existente.FechaIngreso = personal.FechaIngreso;
            existente.IdGrado = personal.IdGrado;
            existente.IdUnidadActual = personal.IdUnidadActual;
            existente.IdSituacion = personal.IdSituacion;
            existente.Estado = personal.Estado;

            await _context.SaveChangesAsync();
            return Ok(existente);
        }

        // ══════════════════════════════════════
        // GET: api/Personal/grados
        // Listar todos los grados (para dropdown)
        // ══════════════════════════════════════
        [HttpGet("grados")]
        public async Task<ActionResult<IEnumerable<Grado>>> GetGrados()
        {
            return await _context.Grados.OrderBy(g => g.Jerarquia).ToListAsync();
        }

        // ══════════════════════════════════════
        // GET: api/Personal/unidades
        // Listar todas las unidades (para dropdown)
        // ══════════════════════════════════════
        [HttpGet("unidades")]
        public async Task<ActionResult<IEnumerable<Unidad>>> GetUnidades()
        {
            return await _context.Unidads.OrderBy(u => u.Nombre).ToListAsync();
        }

        // ══════════════════════════════════════
        // GET: api/Personal/situaciones
        // Listar todas las situaciones (para dropdown)
        // ══════════════════════════════════════
        [HttpGet("situaciones")]
        public async Task<ActionResult<IEnumerable<Situacion>>> GetSituaciones()
        {
            return await _context.Situacions.OrderBy(s => s.Nombre).ToListAsync();
        }

        // ══════════════════════════════════════
        // GET: api/Personal/5/historial
        // Historial de movimientos de un personal
        // ══════════════════════════════════════
        [HttpGet("{id}/historial")]
        public async Task<ActionResult<IEnumerable<HistorialMovimiento>>> GetHistorial(int id)
        {
            var existe = await _context.Personals.AnyAsync(p => p.IdPersonal == id);
            if (!existe)
                return NotFound("No se encontró al efectivo.");

            var historial = await _context.HistorialMovimientos
                .Where(h => h.IdPersonal == id)
                .OrderByDescending(h => h.FechaMovimiento)
                .ToListAsync();

            return Ok(historial);
        }
    }
}