using System;
using System.Collections.Generic;

namespace Policia.RRHH.API.Models;

public partial class Personal
{
    public int IdPersonal { get; set; }

    public string Cip { get; set; } = null!;

    public string Dni { get; set; } = null!;

    public string Nombres { get; set; } = null!;

    public string Apellidos { get; set; } = null!;

    public DateOnly? FechaNacimiento { get; set; }

    public string? Sexo { get; set; }

    public int IdGrado { get; set; }

    public int IdUnidadActual { get; set; }

    public int IdSituacion { get; set; }

    public DateOnly? FechaIngreso { get; set; }

    public bool? Estado { get; set; }

    public virtual ICollection<HistorialMovimiento> HistorialMovimientos { get; set; } = new List<HistorialMovimiento>();

    public virtual Grado IdGradoNavigation { get; set; } = null!;

    public virtual Situacion IdSituacionNavigation { get; set; } = null!;

    public virtual Unidad IdUnidadActualNavigation { get; set; } = null!;
}
