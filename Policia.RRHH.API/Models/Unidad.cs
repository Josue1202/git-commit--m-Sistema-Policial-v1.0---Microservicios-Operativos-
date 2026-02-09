using System;
using System.Collections.Generic;

namespace Policia.RRHH.API.Models;

public partial class Unidad
{
    public int IdUnidad { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Siglas { get; set; }

    public int? IdUnidadPadre { get; set; }

    public virtual ICollection<Personal> Personals { get; set; } = new List<Personal>();
}
