using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;
using SharedConfig;
using SimulatorProject.enums;

namespace SimulatorProject.services
{
    public class SerialService
    {
        private readonly SerialPort _serialPort;
        private readonly TimerService _timer;

        private readonly SimulationService _simulationService;
        private double _distanceNoiseMin;
        private double _distanceNoiseMax;
        private double _responseJitterMs;


        public SerialService(TimerService timer, SimulationService simulationService)
        {
            _timer = timer;
            this._simulationService = simulationService;
            _serialPort = new SerialPort(SharedConfig.ConfigManager.Get("SLAVE_PORT_NAME").Trim(), SharedConfig.ConfigManager.GetInt("BAUD_RATE"))
            {
                NewLine = "\n",
                ReadTimeout = 5000,
                WriteTimeout = 5000
            };
            _distanceNoiseMin = ConfigManager.GetDouble("DISTANCE_NOISE_MIN");
            _distanceNoiseMax = ConfigManager.GetDouble("DISTANCE_NOISE_MAX");
            _responseJitterMs = ConfigManager.GetDouble("RESPONSE_JITTER_MS");

        }



        public void Start()
        {
            try
            {
                _timer.Start();
                _serialPort.Open();
                _serialPort.DataReceived += async (sender, e) =>
                {
                    var sp = (SerialPort)sender;
                    try
                    {
                        string request = _serialPort.ReadLine().Trim();
                        Console.WriteLine($"Slave received: {request}");
                        var responses = new List<string>();
                        string time = _timer.GetElapsedTime();
                        var parsed = WireProtocol.ParseRequest(request);
                        if (parsed.Status == WireRequestParseStatus.BadRequest)
                        {
                            Console.WriteLine($"Bad request: {request}");
                            sp.WriteLine("BAD_REQUEST");
                            return;
                        }
                        if (parsed.Status == WireRequestParseStatus.UnknownCommand)
                        {
                            Console.WriteLine($"Unknown command: {request}");
                            if (!string.IsNullOrWhiteSpace(parsed.RequestId))
                            {
                                sp.WriteLine($"ACK,{parsed.RequestId}");
                            }
                            sp.WriteLine("UNKNOWN_COMMAND");
                            return;
                        }

                        var wireRequest = parsed.Request!;
                        string requestId = wireRequest.RequestId;
                        string? param = wireRequest.Param;
                        var command = wireRequest.Command;

                        sp.WriteLine($"ACK,{requestId}");
                        switch (command)
                        {
                            case SerialCommand.GET_DISTANCE:
                                if (string.IsNullOrWhiteSpace(param))
                                {
                                    Console.WriteLine($"Missing anchor id for distance request {requestId}");
                                    sp.WriteLine("MISSING_ID");
                                    return;
                                }
                                var anchor = _simulationService.helicopter.GetAnchorById(param);
                                if (anchor != null)
                                {
                                    double noiseMin = _distanceNoiseMin;
                                    double noiseMax = _distanceNoiseMax;
                                    if (noiseMin > noiseMax)
                                    {
                                        var temp = noiseMin;
                                        noiseMin = noiseMax;
                                        noiseMax = temp;
                                    }
                                    double noise = noiseMin + (Random.Shared.NextDouble() * (noiseMax - noiseMin));
                                    double distance = _simulationService.CalaculateDistance(anchor) + noise;
                                    responses.Add($"DISTANCE,{requestId},{time},{distance},{param}");
                                }
                                else
                                {
                                    Console.WriteLine($"Anchor not found: {param}");
                                    responses.Add($"DISTANCE,{requestId},{time},NA,{param}");
                                }
                                break;

                            case SerialCommand.GET_PILOT_POSITION:
                                _simulationService.SetNewPilotCordinate(
                                    _timer.GetTimePassFromLast(),
                                    CordinateType.X,
                                    DirectionType.BACKWARD);

                                var point = _simulationService.GetPilotPoint();
                                responses.Add($"PILOT_POSITION,{requestId},{time},{point.X},{point.Y},{point.Z}");
                                break;

                            case SerialCommand.GET_ANCHOR_POSITION:
                                if (string.IsNullOrWhiteSpace(param))
                                {
                                    Console.WriteLine($"Missing anchor id for anchor position request {requestId}");
                                    sp.WriteLine("MISSING_ID");
                                    return;
                                }
                                var anchorPosition = _simulationService.helicopter.GetAnchorById(param);
                                responses.Add(anchorPosition != null
                                    ? $"ANCHOR_POSITION,{requestId},{time},{anchorPosition.Id},{anchorPosition.point.X},{anchorPosition.point.Y},{anchorPosition.point.Z}"
                                    : $"ANCHOR_POSITION,{requestId},{time},{param},NA,NA,NA");
                                break;
                            case SerialCommand.GET_STATUS:
                                _simulationService.SetNewPilotCordinate(
                                    _timer.GetTimePassFromLast(),
                                    CordinateType.X,
                                    DirectionType.BACKWARD);
                                var pilotPoint = _simulationService.GetPilotPoint();
                                responses.Add($"STATUS_PILOT,{requestId},{time},{pilotPoint.X},{pilotPoint.Y},{pilotPoint.Z}");
                                foreach (var statusAnchor in _simulationService.helicopter.anchors)
                                {
                                    responses.Add($"STATUS_ANCHOR,{requestId},{time},{statusAnchor.Id},{statusAnchor.point.X},{statusAnchor.point.Y},{statusAnchor.point.Z}");
                                }
                                break;
                            case SerialCommand.SET_NOISE:
                                if (string.IsNullOrWhiteSpace(param))
                                {
                                    Console.WriteLine($"Missing noise params for request {requestId}");
                                    sp.WriteLine("MISSING_PARAMS");
                                    return;
                                }
                                var noiseParts = param.Split(',', 2);
                                if (noiseParts.Length < 2 ||
                                    !double.TryParse(noiseParts[0], out var minNoise) ||
                                    !double.TryParse(noiseParts[1], out var maxNoise))
                                {
                                    Console.WriteLine($"Bad noise params for request {requestId}: {param}");
                                    sp.WriteLine("BAD_PARAMS");
                                    return;
                                }
                                _distanceNoiseMin = minNoise;
                                _distanceNoiseMax = maxNoise;
                                responses.Add($"CONFIG_NOISE,{requestId},{_distanceNoiseMin},{_distanceNoiseMax}");
                                break;
                            case SerialCommand.SET_JITTER:
                                if (string.IsNullOrWhiteSpace(param) ||
                                    !double.TryParse(param, out var jitterMs))
                                {
                                    Console.WriteLine($"Bad jitter param for request {requestId}: {param}");
                                    sp.WriteLine("BAD_PARAMS");
                                    return;
                                }
                                _responseJitterMs = jitterMs;
                                responses.Add($"CONFIG_JITTER,{requestId},{_responseJitterMs}");
                                break;

                            default:
                                sp.WriteLine("UNKNOWN_COMMAND");
                                break;
                        }

                        double baseDelay = ConfigManager.GetDouble("RESPONSE_DELAY");
                        double jitterMax = _responseJitterMs;
                        if (jitterMax < 0)
                        {
                            jitterMax = 0;
                        }
                        double jitter = Random.Shared.NextDouble() * jitterMax;
                        await Task.Delay(TimeSpan.FromMilliseconds(baseDelay + jitter));
                        foreach (var response in responses)
                        {
                            sp.WriteLine(response);
                        }
                        _timer.SetLastTimer();



                    }
                    catch (TimeoutException) { }
                };
                Console.WriteLine("Slave is listening...");
                Console.ReadLine();

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Serial error: {ex.Message}");
            }
        }


    }
}
