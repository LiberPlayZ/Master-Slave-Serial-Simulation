using System;
using System.Collections.Generic;
using System.Linq;
using System.IO.Ports;
using System.Threading.Tasks;
using SharedConfig;
namespace SimulationMaster.services
{
    public class SerialService
    {
        private readonly SerialPort _serialPort;

        private readonly CsvService _csvService;
        private readonly int _maxRetries;
        private readonly int _timeoutMs;
        private readonly object _ackLock = new();
        private TaskCompletionSource<string>? _ackTcs;
        private string? _pendingAckId;


        public SerialService(CsvService csvService)
        {
            this._serialPort = new SerialPort(SharedConfig.ConfigManager.Get("MASTER_PORT_NAME").Trim(), SharedConfig.ConfigManager.GetInt("BAUD_RATE"))
            {
                NewLine = "\n",
                ReadTimeout = 5000,
                WriteTimeout = 5000
            };
            this._csvService = csvService;
            this._maxRetries = ConfigManager.GetInt("ACK_MAX_RETRIES");
            this._timeoutMs = ConfigManager.GetInt("ACK_TIMEOUT_MS");


        }



        private string CreateGetDistanceRequest(string anchorId)
        {
            return $"{SharedConfig.SerialCommand.GET_DISTANCE.ToWireString()}:{anchorId}";
        }

        private string CreateGetPilotPositionRequest()
        {
            return SharedConfig.SerialCommand.GET_PILOT_POSITION.ToWireString();

        }

        private string CreateGetAnchorPositionRequest(string anchorId)
        {
            return $"{SerialCommand.GET_ANCHOR_POSITION.ToWireString()}:{anchorId}";

        }



        private string? BuildPayloadFromInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;

            var parts = input.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var command = parts[0].ToLowerInvariant();

            switch (command)
            {
                case "distance":
                    if (parts.Length < 2) return null;
                    return $"GET_DISTANCE,{parts[1]}";

                case "pilot":
                    return "GET_PILOT_POSITION";

                case "anchor":
                    if (parts.Length < 2) return null;
                    return $"GET_ANCHOR_POSITION,{parts[1]}";

                case "help":
                    return "HELP";

                default:
                    return null;
            }
        }

        private async Task SendWithRetry(string payload, int maxRetries, int timeoutMs)
        {
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                string requestId = Guid.NewGuid().ToString("N");
                var tcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
                lock (_ackLock)
                {
                    _pendingAckId = requestId;
                    _ackTcs = tcs;
                }
                _serialPort.WriteLine($"REQ,{requestId},{payload}");
                var completed = await Task.WhenAny(tcs.Task, Task.Delay(timeoutMs));
                if (completed == tcs.Task)
                {
                    return;
                }
                Console.WriteLine($"No ACK for request {requestId}, retrying...");
            }
            Console.WriteLine("Failed to receive ACK after retries.");
        }


        private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var port = (SerialPort)sender;
            try
            {
                string response = port.ReadLine();
                if (response.StartsWith("ACK,"))
                {
                    var parts = response.Split(',', 2);
                    if (parts.Length == 2)
                    {
                        string ackId = parts[1];
                        TaskCompletionSource<string>? tcs = null;
                        lock (_ackLock)
                        {
                            if (_pendingAckId == ackId)
                            {
                                tcs = _ackTcs;
                                _ackTcs = null;
                            }
                        }
                        tcs?.TrySetResult(ackId);
                    }
                    return;
                }
                if (response.StartsWith("PILOT_POSITION,") || response.StartsWith("ANCHOR_POSITION,"))
                {
                    _csvService.LogPosition(response);
                }
                else if (response.StartsWith("DISTANCE,"))
                {
                    this._csvService.LogDistance(response);
                }

                Console.WriteLine($"[Master] Received: {response}");

            }
            catch (TimeoutException) { }
        }

        public async Task Start()
        {
            try
            {
                this._serialPort.Open();
                this._serialPort.DataReceived += OnDataReceived;
                Console.WriteLine("Enter commands: distance <id>, pilot, anchor <id>, help, exit");
                while (true)
                {
                    var input = Console.ReadLine();
                    if (input == null) continue;

                    if (input.Trim().Equals("exit", StringComparison.OrdinalIgnoreCase))
                        break;

                    var payload = BuildPayloadFromInput(input);

                    if (payload == "HELP")
                    {
                        Console.WriteLine("Commands: distance <id>, pilot, anchor <id>, exit");
                        continue;
                    }

                    if (payload == null)
                    {
                        Console.WriteLine("Unknown command. Try: distance <id>, pilot, anchor <id>");
                        continue;
                    }


                    await SendWithRetry(payload, this._maxRetries, this._timeoutMs);
                }

                await Task.Delay(TimeSpan.FromSeconds(5));



            }
            catch (Exception ex)
            {
                Console.WriteLine($"Serial error: {ex.Message}");
            }
        }
    }
}
