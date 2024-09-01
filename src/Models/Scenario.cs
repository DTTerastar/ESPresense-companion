﻿using ESPresense.Locators;
using MathNet.Numerics.Optimization;
using MathNet.Spatial.Euclidean;
using Newtonsoft.Json;

namespace ESPresense.Models;

public class Scenario(Config? config, ILocate locator, string? name)
{
    private Config? Config { get; } = config;
    private ILocate Locator { get; } = locator;

    public bool Current => DateTime.UtcNow - LastHit < TimeSpan.FromSeconds(Config?.Timeout ?? 30);
    public int? Confidence { get; set; }
    public double? Minimum { get; set; }
    [JsonIgnore] public Point3D LastLocation { get; set; }
    public Point3D Location { get; set; }
    public double? Scale { get; set; }
    public int? Fixes { get; set; }
    public string? Name { get; } = name;
    public Room? Room { get; set; }
    public double? Error { get; set; }
    public int? Iterations { get; set; }
    public ExitCondition ReasonForExit { get; set; }
    public Floor? Floor { get; set; }
    public DateTime? LastHit { get; set; }
    public double Probability { get; set; } = 1.0;

    public bool Locate()
    {
        return Locator.Locate(this);
    }
}