using System;
using System.Collections.Generic;

namespace Policia.RRHH.API.Models;

public partial class Situacion
{
    public int IdSituacion { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual ICollection<Personal> Personals { get; set; } = new List<Personal>();
}
