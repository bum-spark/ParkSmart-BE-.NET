namespace ParkSmart;

public enum RolUsuario
{
    Admin,
    Gerente,
    Empleado
}

public enum EstadoSede
{
    activo,
    mantenimiento,
    inactivo
}

public enum TipoCajon
{
    normal,
    electrico,
    discapacitado
}

public enum EstadoCajon
{
    libre,
    ocupado,
    reservado,
    inactivo
}

public enum EstadoTicket
{
    activo,
    completado,
    cancelado
}

public enum EstadoReserva
{
    pendiente,
    completado,
    cancelado
}