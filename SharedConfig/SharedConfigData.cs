using DotNetEnv;
using System.Globalization;
namespace SharedConfig;

public static class ConfigData
{
    public static void Load()
    {
        Env.Load("/home/daniel-liberman/projects/takshaon/.env.shared"); // Shared config first
        Env.Load(); // Then project-specific .env
    }

    public static string Get(string key) => Env.GetString(key);

    public static int? GetInt(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        try
        {   
            var result = int.Parse(value.Trim());
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine("error : " + ex);
        }

        return null;
    }

    public static double? GetDouble(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        if (double.TryParse(value.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
        {
            return result;
        }

        return null;
    }
}
