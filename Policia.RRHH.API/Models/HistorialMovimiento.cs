using System;
using System.Collections.Generic;

namespace Policia.RRHH.API.Models;

public partial class HistorialMovimiento
{
    public int IdMovimiento { get; set; }

    public int IdPersonal { get; set; }

    public int? IdUnidadOrigen { get; set; }

    public int IdUnidadDestino { get; set; }

    public DateOnly FechaMovimiento { get; set; }

    public string? Motivo { get; set; }

    public string? Documento { get; set; }

    public virtual Personal IdPersonalNavigation { get; set; } = null!;
}
