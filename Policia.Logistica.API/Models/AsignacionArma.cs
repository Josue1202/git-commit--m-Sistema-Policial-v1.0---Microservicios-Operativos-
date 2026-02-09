using System;
using System.Collections.Generic;

namespace Policia.Logistica.API.Models;

public partial class AsignacionArma
{
    public int IdAsignacion { get; set; }

    public int IdArma { get; set; }

    public int IdPersonal { get; set; }

    public DateOnly FechaEntrega { get; set; }

    public DateOnly? FechaDevolucion { get; set; }

    public string? Observacion { get; set; }

    public virtual Arma IdArmaNavigation { get; set; } = null!;
}
