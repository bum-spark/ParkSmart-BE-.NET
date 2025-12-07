namespace ParkSmart;

public static class FechaHelper
{
    // Zona horaria Centro de México (UTC-6, con horario de verano)
    private static readonly TimeZoneInfo ZonaMexico = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time (Mexico)");

    /// Obtiene la fecha y hora actual en zona horaria de México Centro
    public static DateTime AhoraLocal()
    {
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, ZonaMexico);
    }

}
