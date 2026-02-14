namespace Policia.Web.Models
{
    // Representa cada usuario que devuelve GET /seguridad/Listar
    // Lo usamos para llenar la tabla en ListarUsuarios.razor
    public class UsuarioListadoDTO
    {
        public int IdUsuario { get; set; }
        public string Cip { get; set; } = "";
        public int? IdPersonal { get; set; }
        public bool? Estado { get; set; }
        public string Rol { get; set; } = "";           // Nombre del rol (viene del Include)
        public string? CreadoPor { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public string? ModificadoPor { get; set; }
        public DateTime? FechaModificacion { get; set; }
    }
}