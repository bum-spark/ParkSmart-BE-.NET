namespace ParkSmart;

public static class FechaHelper
{
    // Zona horaria Centro de México (UTC-6, con horario de verano)
    private static readonly TimeZoneInfo ZonaMexico = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time (Mexico)");

    public static DateTime AhoraLocal()
    {
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, ZonaMexico);
    }

    public static DateTime ConvertirALocal(DateTime fechaUtc)
    {
        return TimeZoneInfo.ConvertTimeFromUtc(fechaUtc, ZonaMexico);
    }

    public static DateTime ConvertirAUtc(DateTime fechaLocal)
    {
        return TimeZoneInfo.ConvertTimeToUtc(fechaLocal, ZonaMexico);
    }
}
