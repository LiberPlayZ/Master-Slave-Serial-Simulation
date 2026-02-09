namespace Visualizer.Models;

public sealed record LogTailHealth(
    string DistanceFilePath,
    bool DistanceFileExists,
    DateTime? DistanceLastWriteUtc,
    DateTime? DistanceLastReadUtc,
    string PositionsFilePath,
    bool PositionsFileExists,
    DateTime? PositionsLastWriteUtc,
    DateTime? PositionsLastReadUtc);
