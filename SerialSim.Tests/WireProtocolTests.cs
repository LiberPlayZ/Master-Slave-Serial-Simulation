using SharedConfig;
using Xunit;

namespace SerialSim.Tests;

public class WireProtocolTests
{
    [Fact]
    public void ParseRequest_ValidDistance_ReturnsOk()
    {
        var result = WireProtocol.ParseRequest("REQ,abc123,GET_DISTANCE,2");

        Assert.Equal(WireRequestParseStatus.Ok, result.Status);
        Assert.NotNull(result.Request);
        Assert.Equal("abc123", result.Request!.RequestId);
        Assert.Equal(SerialCommand.GET_DISTANCE, result.Request.Command);
        Assert.Equal("2", result.Request.Param);
    }

    [Fact]
    public void ParseRequest_ValidPilot_NoParam()
    {
        var result = WireProtocol.ParseRequest("REQ,req1,GET_PILOT_POSITION");

        Assert.Equal(WireRequestParseStatus.Ok, result.Status);
        Assert.NotNull(result.Request);
        Assert.Equal(SerialCommand.GET_PILOT_POSITION, result.Request!.Command);
        Assert.Null(result.Request.Param);
    }

    [Fact]
    public void ParseRequest_BadPrefix_ReturnsBadRequest()
    {
        var result = WireProtocol.ParseRequest("BAD,req1,GET_STATUS");

        Assert.Equal(WireRequestParseStatus.BadRequest, result.Status);
        Assert.Null(result.RequestId);
        Assert.Null(result.Request);
    }

    [Fact]
    public void ParseRequest_EmptyId_ReturnsBadRequest()
    {
        var result = WireProtocol.ParseRequest("REQ,,GET_STATUS");

        Assert.Equal(WireRequestParseStatus.BadRequest, result.Status);
        Assert.Null(result.RequestId);
        Assert.Null(result.Request);
    }

    [Fact]
    public void ParseRequest_UnknownCommand_ReturnsUnknownCommandWithId()
    {
        var result = WireProtocol.ParseRequest("REQ,req9,GET_UNKNOWN");

        Assert.Equal(WireRequestParseStatus.UnknownCommand, result.Status);
        Assert.Equal("req9", result.RequestId);
        Assert.Null(result.Request);
    }

    [Fact]
    public void TryParseAck_Valid_ReturnsTrue()
    {
        var ok = WireProtocol.TryParseAck("ACK,req1", out var requestId);

        Assert.True(ok);
        Assert.Equal("req1", requestId);
    }

    [Fact]
    public void TryParseAck_Invalid_ReturnsFalse()
    {
        var ok = WireProtocol.TryParseAck("ACK,", out var requestId);

        Assert.False(ok);
        Assert.Equal(string.Empty, requestId);
    }
}
