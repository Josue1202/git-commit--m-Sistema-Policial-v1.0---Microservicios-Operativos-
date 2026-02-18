// Este archivo define las clases que usa el FRONTEND para:
// 1. Enviar los datos al API cuando crea un usuario
// 2. Recibir la respuesta del API después de crear
// 3. Mostrar los datos del policía que encontró en RRHH

    namespace Policia.Web.Models
{
    // ──────────────────────────────────────────────
    // CLASE 1: Lo que ENVIAMOS al API para crear el usuario
    // ──────────────────────────────────────────────
    // Es la "maleta de ida" → Sale del formulario de Blazor
    // y llega al endpoint POST /seguridad/CrearUsuario
    public class CrearUsuarioRequest
    {
        public string Cip { get; set; } = "";
        public int IdPersonal { get; set; }
        public string Password { get; set; } = "";
        public int IdRol { get; set; }
    }

    // ──────────────────────────────────────────────
    // CLASE 2: Lo que RECIBIMOS del API de RRHH al buscar un policía
    // ──────────────────────────────────────────────
    // Cuando hacemos GET /rrhh/Personal/buscar-cip/12345678
    // el API nos devuelve estos datos para mostrarlos en pantalla
    public class PersonalBuscadoDTO
    {
        public int IdPersonal { get; set; }
        public string Cip { get; set; } = "";
        public string Dni { get; set; } = "";
        public string Nombres { get; set; } = "";
        public string Apellidos { get; set; } = "";

        // Estos objetos vienen del Include() que hicimos en PersonalController
        // Include(p => p.IdGradoNavigation) → Trae esto:
        public GradoDTO? IdGradoNavigation { get; set; }

        // Include(p => p.IdUnidadActualNavigation) → Trae esto:
        public UnidadDTO? IdUnidadActualNavigation { get; set; }

        // Include(p => p.IdSituacionNavigation) → Trae esto:
        public SituacionDTO? IdSituacionNavigation { get; set; }
    }

    // ──────────────────────────────────────────────
    // CLASES AUXILIARES: Representan las tablas relacionadas
    // ──────────────────────────────────────────────
    // Son "mini-clases" que solo tienen lo que necesitamos mostrar
    public class GradoDTO
    {
        public int IdGrado { get; set; }
        public string Nombre { get; set; } = "";  // "SUBOFICIAL 2DA", "MAYOR", etc.
    }

    public class UnidadDTO
    {
        public int IdUnidad { get; set; }
        public string Nombre { get; set; } = "";  // "DIVINCRI LIMA", "DIRANDRO", etc.
    }

    public class SituacionDTO
    {
        public int IdSituacion { get; set; }
        public string Nombre { get; set; } = "";  // "ACTIVIDAD", "RETIRO", etc.
    }
}
