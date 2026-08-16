using System;
using System.Linq;

public static class LaunchFlags
{
    public static bool IsBot =>
        Environment.GetCommandLineArgs().Any(a => a.Equals("-bot", StringComparison.OrdinalIgnoreCase));

    public static string BotReservationId => GetArgumentValue("-botReservation");

    private static string GetArgumentValue(string argumentName)
    {
        string prefix = argumentName + "=";
        string argument = Environment.GetCommandLineArgs()
            .FirstOrDefault(a => a.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));

        return argument?.Substring(prefix.Length);
    }
}
