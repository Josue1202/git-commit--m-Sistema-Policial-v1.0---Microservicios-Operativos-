using Microsoft.AspNetCore.Authorization; // <--- 1. HERRAMIENTA NUEVA: Necesaria para usar la etiqueta [Authorize]
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Policia.Logistica.API.Models;

namespace Policia.Logistica.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // <--- 2. EL CANDADO NUEVO: Al poner esto aquí arriba, BLOQUEAMOS todo el controlador. Nadie entra sin Token.
    public class ArmamentoController : ControllerBase
    {
        private readonly BdLogisticaContext _context;

        // Inyección de dependencia: Aquí recibimos la conexión a la BD
        public ArmamentoController(BdLogisticaContext context)
        {
            _context = context;
        }

        // 1. GET: api/Armamento
        // Misión: Listar TODAS las armas del almacén (Inventario General)
        [HttpGet]
        [Authorize(Roles = "1")] // 1 = Admin
        public async Task<ActionResult<IEnumerable<Arma>>> GetInventario()
        {
            // Como pusimos [Authorize] arriba, el código no llegará aquí si el usuario no tiene Token.
            return await _context.Armas
                .Include(a => a.IdTipoNavigation) // Trae si es Pistola o Fusil
                .ToListAsync();
        }

        // 2. GET: api/Armamento/MisArmas/1
        // Misión: Ver qué arma tiene asignada un policía específico (Por su ID)
        [HttpGet("MisArmas/{idPersonal}")]
        public async Task<ActionResult<IEnumerable<AsignacionArma>>> GetArmasDePolicia(int idPersonal)
        {
            var asignaciones = await _context.AsignacionArmas
                .Include(asig => asig.IdArmaNavigation) // Datos del Arma (Serie, Marca)
                .ThenInclude(arma => arma.IdTipoNavigation) // Tipo de Arma
                .Where(x => x.IdPersonal == idPersonal && x.FechaDevolucion == null) // Solo las que tiene en su poder
                .ToListAsync();

            if (asignaciones == null || !asignaciones.Any())
            {
                return NotFound($"El efectivo con ID {idPersonal} no tiene armamento asignado.");
            }

            return asignaciones;
        }

        // ══════════════════════════════════════
        // 3. GET: api/Armamento/{id}
        // Obtener un arma por ID
        // ══════════════════════════════════════
        [HttpGet("{id:int}")]
        [Authorize(Roles = "1")]
        public async Task<ActionResult<Arma>> GetArmaById(int id)
        {
            var arma = await _context.Armas
                .Include(a => a.IdTipoNavigation)
                .FirstOrDefaultAsync(a => a.IdArma == id);

            if (arma == null)
                return NotFound("Arma no encontrada.");

            return Ok(arma);
        }

        // ══════════════════════════════════════
        // 4. GET: api/Armamento/tipos
        // Listar tipos de arma (para dropdown)
        // ══════════════════════════════════════
        [HttpGet("tipos")]
        public async Task<ActionResult<IEnumerable<TipoArma>>> GetTipos()
        {
            return await _context.TipoArmas.OrderBy(t => t.Nombre).ToListAsync();
        }

        // ══════════════════════════════════════
        // 5. GET: api/Armamento/disponibles
        // Armas operativas sin asignación activa
        // ══════════════════════════════════════
        [HttpGet("disponibles")]
        [Authorize(Roles = "1")]
        public async Task<ActionResult<IEnumerable<Arma>>> GetArmasDisponibles()
        {
            var armasAsignadasIds = await _context.AsignacionArmas
                .Where(a => a.FechaDevolucion == null)
                .Select(a => a.IdArma)
                .ToListAsync();

            var disponibles = await _context.Armas
                .Include(a => a.IdTipoNavigation)
                .Where(a => a.Estado == "OPERATIVO" && !armasAsignadasIds.Contains(a.IdArma))
                .ToListAsync();

            return disponibles;
        }

        // ══════════════════════════════════════
        // 6. POST: api/Armamento
        // Registrar nueva arma en el inventario
        // ══════════════════════════════════════
        [HttpPost]
        [Authorize(Roles = "1")]
        public async Task<ActionResult<Arma>> CrearArma(Arma arma)
        {
            var existeSerie = await _context.Armas.AnyAsync(a => a.Serie == arma.Serie);
            if (existeSerie)
                return BadRequest("Ya existe un arma con ese número de serie.");

            arma.Estado = "OPERATIVO";
            _context.Armas.Add(arma);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetArmaById), new { id = arma.IdArma }, arma);
        }

        // ══════════════════════════════════════
        // 7. PUT: api/Armamento/{id}
        // Editar datos de un arma
        // ══════════════════════════════════════
        [HttpPut("{id:int}")]
        [Authorize(Roles = "1")]
        public async Task<IActionResult> EditarArma(int id, Arma arma)
        {
            if (id != arma.IdArma)
                return BadRequest("El ID no coincide.");

            var existente = await _context.Armas.FindAsync(id);
            if (existente == null)
                return NotFound("Arma no encontrada.");

            existente.IdTipo = arma.IdTipo;
            existente.Marca = arma.Marca;
            existente.Modelo = arma.Modelo;
            existente.Serie = arma.Serie;
            existente.Estado = arma.Estado;

            await _context.SaveChangesAsync();
            return Ok(existente);
        }

        // ══════════════════════════════════════
        // 8. POST: api/Armamento/Asignar
        // Asignar arma a un efectivo policial
        // ══════════════════════════════════════
        [HttpPost("Asignar")]
        [Authorize(Roles = "1")]
        public async Task<IActionResult> AsignarArma(AsignacionArma asignacion)
        {
            var arma = await _context.Armas.FindAsync(asignacion.IdArma);
            if (arma == null)
                return NotFound("El arma especificada no existe.");

            var yaAsignada = await _context.AsignacionArmas
                .AnyAsync(a => a.IdArma == asignacion.IdArma && a.FechaDevolucion == null);
            if (yaAsignada)
                return BadRequest("Esta arma ya está asignada a otro efectivo.");

            asignacion.FechaEntrega = DateOnly.FromDateTime(DateTime.Now);
            _context.AsignacionArmas.Add(asignacion);

            arma.Estado = "ASIGNADO";
            await _context.SaveChangesAsync();

            return Created("", new
            {
                Mensaje = "Arma asignada correctamente.",
                asignacion.IdAsignacion
            });
        }

        // ══════════════════════════════════════
        // 9. PUT: api/Armamento/Devolver/{idAsignacion}
        // Registrar devolución de arma
        // ══════════════════════════════════════
        [HttpPut("Devolver/{idAsignacion}")]
        [Authorize(Roles = "1")]
        public async Task<IActionResult> DevolverArma(int idAsignacion)
        {
            var asignacion = await _context.AsignacionArmas
                .Include(a => a.IdArmaNavigation)
                .FirstOrDefaultAsync(a => a.IdAsignacion == idAsignacion);

            if (asignacion == null)
                return NotFound("Asignación no encontrada.");

            if (asignacion.FechaDevolucion != null)
                return BadRequest("Esta arma ya fue devuelta.");

            asignacion.FechaDevolucion = DateOnly.FromDateTime(DateTime.Now);

            if (asignacion.IdArmaNavigation != null)
                asignacion.IdArmaNavigation.Estado = "OPERATIVO";

            await _context.SaveChangesAsync();

            return Ok(new { Mensaje = "Arma devuelta correctamente." });
        }
    }
}