using System;
using System.Collections.Generic;

namespace Policia.RRHH.API.Models;

public partial class Grado
{
    public int IdGrado { get; set; }

    public string Nombre { get; set; } = null!;

    public int Jerarquia { get; set; }

    public virtual ICollection<Personal> Personals { get; set; } = new List<Personal>();
}
