using System;
using System.Collections.Generic;

namespace Policia.Logistica.API.Models;

public partial class Arma
{
    public int IdArma { get; set; }

    public int IdTipo { get; set; }

    public string Marca { get; set; } = null!;

    public string? Modelo { get; set; }

    public string Serie { get; set; } = null!;

    public string? Estado { get; set; }

    public virtual ICollection<AsignacionArma> AsignacionArmas { get; set; } = new List<AsignacionArma>();

    public virtual TipoArma IdTipoNavigation { get; set; } = null!;
}
