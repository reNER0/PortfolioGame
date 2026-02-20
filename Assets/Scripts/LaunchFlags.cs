using System;
using System.Linq;

public static class LaunchFlags
{
    public static bool IsBot =>
        Environment.GetCommandLineArgs().Any(a => a.Equals("-bot", StringComparison.OrdinalIgnoreCase));
}