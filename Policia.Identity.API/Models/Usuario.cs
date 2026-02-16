using System;
using System.Collections.Generic;

namespace Policia.Identity.API.Models;

public partial class Usuario
{
    public int IdUsuario { get; set; }

    public int? IdPersonal { get; set; }

    public int IdRol { get; set; }

    public string Cip { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public bool? Estado { get; set; }
    // ¿Quién creó este usuario? → Guarda el CIP del admin
    public string? CreadoPor { get; set; }

    // ¿Cuándo se creó? → Fecha y hora exacta
    public DateTime? FechaCreacion { get; set; }

    // ¿Quién lo modificó por última vez? → CIP del admin
    public string? ModificadoPor { get; set; }

    // ¿Cuándo se modificó? → Fecha y hora exacta
    public DateTime? FechaModificacion { get; set; }

    public virtual Rol IdRolNavigation { get; set; } = null!;
}
