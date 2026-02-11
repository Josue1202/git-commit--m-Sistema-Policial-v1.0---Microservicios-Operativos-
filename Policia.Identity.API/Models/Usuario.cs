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

    public virtual Rol IdRolNavigation { get; set; } = null!;
}
