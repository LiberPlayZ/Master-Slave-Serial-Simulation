using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace SimulatorProject.utils.mappers
{
    public static class ParserMapper
    {
        public static double? DoubleParse(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
            {
                return result;
            }

            return null;
        }

        public static int? IntParse(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            try
            {
                var result = int.Parse(value);
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine("error : " + ex);
            }

            return null;
        }
    }
}