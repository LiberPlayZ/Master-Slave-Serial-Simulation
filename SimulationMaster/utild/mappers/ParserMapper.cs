using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimulationMaster.utild.mappers
{
    public static class ParserMapper
    {
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