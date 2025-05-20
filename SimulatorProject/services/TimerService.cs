using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace SimulatorProject.services
{
    public class TimerService
    {
        private readonly Stopwatch _stopwatch = new();

        public void Start()
        {
            _stopwatch.Start();
        }

        public string GetElapsedTime()
        {
            var ts = _stopwatch.Elapsed;
            return $"{ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}.{ts.Milliseconds:D3}";
        }
    }
}