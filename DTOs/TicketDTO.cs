namespace ParkSmart;

public class TicketDTO
{
    public Guid ticketId { get; set; }
    public string placaVehiculo { get; set; } = string.Empty;
    public DateTime horaEntrada { get; set; }
    public DateTime? horaSalida { get; set; }
    public decimal? montoTotal { get; set; }
    public string estado { get; set; } = string.Empty;
    public Guid cajonId { get; set; }
    public string numeroCajon { get; set; } = string.Empty;

    public TicketDTO(Ticket ticket)
    {
        ticketId = ticket.ticketId;
        placaVehiculo = ticket.placaVehiculo;
        horaEntrada = ticket.horaEntrada;
        horaSalida = ticket.horaSalida;
        montoTotal = ticket.montoTotal;
        estado = ticket.estado;
        cajonId = ticket.cajonId;
        numeroCajon = ticket.cajon?.numeroCajon ?? "N/A";
    }
}

public class RegistrarEntradaDTO
{
    public string placaVehiculo { get; set; } = string.Empty;
    public Guid cajonId { get; set; }
}

public class VistaPreviaSalidaDTO
{
    public DateTime horaEntrada { get; set; }
    public DateTime horaSalida { get; set; }
    public int minutosTranscurridos { get; set; }
    public int horasCobradas { get; set; }
    public decimal tarifaPorHora { get; set; }
    public decimal montoTotal { get; set; }
}

public class ConfirmarSalidaDTO
{
    public Guid ticketId { get; set; }
}

public class AsignarCajonEspecificoDTO
{
    public Guid sedeId { get; set; }
    public Guid cajonId { get; set; }
    public string placaVehiculo { get; set; } = string.Empty;
}

public class AsignarCajonAutomaticoDTO
{
    public Guid sedeId { get; set; }
    public int numeroPiso { get; set; }
    public string placaVehiculo { get; set; } = string.Empty;
}

public class AsignacionCajonDTO
{
    public Guid ticketId { get; set; }
    public Guid cajonId { get; set; }
    public string numeroCajon { get; set; } = string.Empty;
    public string tipoCajon { get; set; } = string.Empty;
    public int numeroPiso { get; set; }
    public string placaVehiculo { get; set; } = string.Empty;
    public DateTime horaEntrada { get; set; }
}

public class CalculoPagoTicketDTO
{
    public Guid ticketId { get; set; }
    public string placaVehiculo { get; set; } = string.Empty;
    public DateTime horaEntrada { get; set; }
    public DateTime horaSalida { get; set; }
    public int minutosTranscurridos { get; set; }
    public int horasCobradas { get; set; }
    public decimal tarifaPorHora { get; set; }
    public decimal montoTotal { get; set; }
    public string numeroCajon { get; set; } = string.Empty;
    public int numeroPiso { get; set; }
}
