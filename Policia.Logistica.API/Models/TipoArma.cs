using System;
using System.Collections.Generic;

namespace Policia.Logistica.API.Models;

public partial class TipoArma
{
    public int IdTipo { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual ICollection<Arma> Armas { get; set; } = new List<Arma>();
}
