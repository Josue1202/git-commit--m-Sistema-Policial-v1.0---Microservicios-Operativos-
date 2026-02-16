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
    }
}