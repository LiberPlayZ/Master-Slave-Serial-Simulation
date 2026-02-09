using SharedConfig;
using Xunit;

namespace SerialSim.Tests;

public class SerialCommandTests
{
    [Theory]
    [InlineData(SerialCommand.GET_DISTANCE, "GET_DISTANCE")]
    [InlineData(SerialCommand.GET_PILOT_POSITION, "GET_PILOT_POSITION")]
    [InlineData(SerialCommand.GET_ANCHOR_POSITION, "GET_ANCHOR_POSITION")]
    [InlineData(SerialCommand.GET_STATUS, "GET_STATUS")]
    [InlineData(SerialCommand.SET_NOISE, "SET_NOISE")]
    [InlineData(SerialCommand.SET_JITTER, "SET_JITTER")]
    public void ToWireString_ReturnsExpected(SerialCommand command, string expected)
    {
        var actual = command.ToWireString();

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("GET_DISTANCE", SerialCommand.GET_DISTANCE)]
    [InlineData("GET_PILOT_POSITION", SerialCommand.GET_PILOT_POSITION)]
    [InlineData("GET_ANCHOR_POSITION", SerialCommand.GET_ANCHOR_POSITION)]
    [InlineData("GET_STATUS", SerialCommand.GET_STATUS)]
    [InlineData("SET_NOISE", SerialCommand.SET_NOISE)]
    [InlineData("SET_JITTER", SerialCommand.SET_JITTER)]
    public void TryParseWireString_KnownCommands_ReturnsTrue(string input, SerialCommand expected)
    {
        var ok = SerialCommandExtensions.TryParseWireString(input, out var command);

        Assert.True(ok);
        Assert.Equal(expected, command);
    }

    [Fact]
    public void TryParseWireString_UnknownCommand_ReturnsFalse()
    {
        var ok = SerialCommandExtensions.TryParseWireString("GET_UNKNOWN", out var command);

        Assert.False(ok);
        Assert.Equal(default, command);
    }
}
