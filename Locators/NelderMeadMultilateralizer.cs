﻿using ESPresense.Extensions;
using ESPresense.Models;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Optimization;
using MathNet.Spatial.Euclidean;
using Serilog;

namespace ESPresense.Locators;

public class NelderMeadMultilateralizer : ILocate
{
    private readonly Device _device;
    private readonly Floor _floor;

    public NelderMeadMultilateralizer(Device device, Floor floor)
    {
        _device = device;
        _floor = floor;
    }

    public bool Locate(Scenario scenario)
    {
        bool OutOfBounds(Vector<double> x, Vector<double> lowerBound, Vector<double> upperBound)
        {
            for (var i = 0; i < x.Count; i++)
                if (x[i] < lowerBound[i] || x[i] > upperBound[i])
                    return true;
            return false;
        }

        double Weight(int index, int total) => Math.Pow((float)total - index, 3) / Math.Pow(total, 3);
        double Error(IList<double> x, DeviceNode dn) => new Point3D(x[0], x[1], x[2]).DistanceTo(dn.Node!.Location) - dn.Distance;

        var confidence = scenario.Confidence;

        var nodes = _device.Nodes.Values.Where(a => a.Current && (a.Node?.Floors?.Contains(_floor) ?? false)).OrderBy(a => a.Distance).ToArray();
        var pos = nodes.Select(a => a.Node!.Location).ToArray();

        scenario.Fixes = pos.Length;

        if (pos.Length <= 1)
        {
            scenario.Room = null;
            scenario.Confidence = 0;
            scenario.Error = null;
            scenario.Floor = null;
            return false;
        }

        scenario.Floor = _floor;

        var guess = confidence < 5
            ? Point3D.MidPoint(pos[0], pos[1])
            : scenario.Location;
        try
        {
            if (pos.Length < 3 || _floor.Bounds == null)
            {
                confidence = 1;
                scenario.Location = guess;
            }
            else
            {
                var lowerBound = Vector<double>.Build.DenseOfArray(new[] { _floor.Bounds[0].X, _floor.Bounds[0].Y, _floor.Bounds[0].Z });
                var upperBound = Vector<double>.Build.DenseOfArray(new[] { _floor.Bounds[1].X, _floor.Bounds[1].Y, _floor.Bounds[1].Z });
                var obj = ObjectiveFunction.Value(
                    x =>
                    {
                        if (OutOfBounds(x, lowerBound, upperBound)) return double.PositiveInfinity;
                        return nodes
                            .Select((dn, i) => new { err = Error(x, dn), weight = Weight(i, nodes.Length) })
                            .Sum(a => a.weight * Math.Pow(a.err, 2));
                    });

                var initialGuess = Vector<double>.Build.DenseOfArray(new[]
                {
                    Math.Max(_floor.Bounds[0].X, Math.Min(_floor.Bounds[1].X, guess.X)),
                    Math.Max(_floor.Bounds[0].Y, Math.Min(_floor.Bounds[1].Y, guess.Y)),
                    Math.Max(_floor.Bounds[0].Z, Math.Min(_floor.Bounds[1].Z, guess.Z))
                });
                var solver = new NelderMeadSimplex(1e-7, 10000);
                var result = solver.FindMinimum(obj, initialGuess);
                scenario.Location = new Point3D(result.MinimizingPoint[0], result.MinimizingPoint[1], result.MinimizingPoint[2]);
                scenario.Fixes = pos.Length;
                scenario.Error = result.FunctionInfoAtMinimum.Value;
                scenario.Iterations = result switch
                {
                    MinimizationWithLineSearchResult mwl =>
                        mwl.IterationsWithNonTrivialLineSearch + mwl.TotalLineSearchIterations,
                    _ => result.Iterations
                };

                scenario.ReasonForExit = result.ReasonForExit;
                confidence = (int)Math.Max(10, Math.Min(100, Math.Min(100, 100 * pos.Length / 4.0) - result.FunctionInfoAtMinimum.Value));
            }
        }
        catch (Exception ex)
        {
            confidence = 1;
            scenario.Location = guess;
            Log.Error("Error finding location for {0}: {1}", _device, ex.Message);
        }

        scenario.Confidence = confidence;

        if (confidence <= 0) return false;
        if (Math.Abs(scenario.Location.DistanceTo(scenario.LastLocation)) < 0.1) return false;
        scenario.Room = _floor.Rooms.Values.FirstOrDefault(a => a.Polygon?.EnclosesPoint(scenario.Location.ToPoint2D()) ?? false);
        return true;
    }
}