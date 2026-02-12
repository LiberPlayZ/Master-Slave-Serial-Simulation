namespace Visualizer.Models;

public sealed record VisualizerConfig(
    double DistanceNoiseMin,
    double DistanceNoiseMax,
    double ResponseJitterMs);
