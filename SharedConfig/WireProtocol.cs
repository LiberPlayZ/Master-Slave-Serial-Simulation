using System;

namespace SharedConfig
{
    public enum WireRequestParseStatus
    {
        Ok,
        BadRequest,
        UnknownCommand
    }

    public sealed class WireRequest
    {
        public WireRequest(string requestId, SerialCommand command, string? param)
        {
            RequestId = requestId;
            Command = command;
            Param = param;
        }

        public string RequestId { get; }
        public SerialCommand Command { get; }
        public string? Param { get; }
    }

    public readonly record struct WireRequestParseResult(
        WireRequestParseStatus Status,
        string? RequestId,
        WireRequest? Request);

    public static class WireProtocol
    {
        public static WireRequestParseResult ParseRequest(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                return new WireRequestParseResult(WireRequestParseStatus.BadRequest, null, null);
            }

            if (!line.StartsWith("REQ,", StringComparison.Ordinal))
            {
                return new WireRequestParseResult(WireRequestParseStatus.BadRequest, null, null);
            }

            var parts = line.Split(',', 4, StringSplitOptions.None);
            if (parts.Length < 3)
            {
                return new WireRequestParseResult(WireRequestParseStatus.BadRequest, null, null);
            }

            var requestId = parts[1];
            if (string.IsNullOrWhiteSpace(requestId))
            {
                return new WireRequestParseResult(WireRequestParseStatus.BadRequest, null, null);
            }

            var commandText = parts[2];
            if (!SerialCommandExtensions.TryParseWireString(commandText, out var command))
            {
                return new WireRequestParseResult(WireRequestParseStatus.UnknownCommand, requestId, null);
            }

            var param = parts.Length > 3 ? parts[3] : null;
            var request = new WireRequest(requestId, command, param);
            return new WireRequestParseResult(WireRequestParseStatus.Ok, requestId, request);
        }

        public static bool TryParseAck(string line, out string requestId)
        {
            requestId = string.Empty;
            if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("ACK,", StringComparison.Ordinal))
            {
                return false;
            }

            var parts = line.Split(',', 2);
            if (parts.Length != 2 || string.IsNullOrWhiteSpace(parts[1]))
            {
                return false;
            }

            requestId = parts[1];
            return true;
        }
    }
}
