namespace Policia.Identity.API.DTOs
{
    public class EditarUsuarioDTO
    {
        // El rol nuevo que tendrá: 1 = Admin, 2 = Operador, etc.
        public int IdRol { get; set; }

        // Opcional: solo si el admin quiere resetear la contraseña
        // Si viene vacío o null, NO se cambia la contraseña
        public string? NuevaPassword { get; set; }
    }
}
