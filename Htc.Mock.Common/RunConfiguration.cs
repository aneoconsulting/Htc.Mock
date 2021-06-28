// This file is part of the Htc.Mock solution.
// 
// Copyright (c) ANEO. All rights reserved.
// * Wilfried KIRSCHENMANN (ANEO)
// 
// 

using System;
using System.Diagnostics;

using JetBrains.Annotations;

using MathNet.Numerics.Distributions;

namespace Htc.Mock.Common
{

  static class Beta
  {
    public static bool IsValidParameterSet(double a, double b) => a >= 0.0 && b >= 0.0;

    public static double Sample(Random ran, double a, double b)
    {
      var numA = GammaSample(ran, a, 1.0);
      var numB = GammaSample(ran, b, 1.0);
      return numA / (numA + numB);
    }

    private static double NormalSample(Random ran)
    {
      var u1 = ran.NextDouble();
      while (u1==0.0)
      {
        u1 = ran.NextDouble();
      }
      var u2 = ran.NextDouble();

      var r = Math.Sqrt(-2.0 * Math.Log(u1));

      var t = Math.Cos(2 * Math.PI * u2);

      var z0 = r * t;

      return z0;
    }

    private static double GammaSample(Random ran, double shape, double rate)
    {
      if (double.IsPositiveInfinity(rate))
        return shape;
      var num1 = shape;
      var num2 = 1.0;
      if (shape < 1.0)
      {
        num1 = shape + 1.0;
        num2 = Math.Pow(ran.NextDouble(), 1.0 / shape);
      }
      var num3 = num1 - 1.0 / 3.0;
      var num4 = 1.0 / Math.Sqrt(9.0 * num3);
      double d1;
      double d2;
      double num5;
      do
      {
        var    num6 = NormalSample(ran);
        double num7;
        for (num7 = 1.0 + num4 * num6 ; num7 <= 0.0 ; num7 = 1.0 + num4 * num6)
          num6 = NormalSample(ran);
        d1   = num7 * num7 * num7;
        d2   = ran.NextDouble();
        num5 = num6 * num6;
      }
      while (d2 >= 1.0 - 0.0331 * num5 * num5 && Math.Log(d2) >= 0.5 * num5 + num3 * (1.0 - d1 + Math.Log(d1)));
      return num2 * num3 * d1 / rate;
    }
  }

  public static class RunConfigurationExt
  {
    public static Request DefaultHeadRequest(this RunConfiguration configuration)
    => new((int)configuration.AvgDurationMs, configuration.Memory, configuration.Data, configuration.SubTasksLevels);
  }

  [Serializable]
  public class RunConfiguration
  {
    public RunConfiguration(TimeSpan totalCalculationTime,
                                  int      totalNbSubTasks,
                                  int      data,
                                  int      memory,
                                  int      subTasksLevels,
                                  int      minDurationMs   = -1,
                                  int      maxDurationMs   = -1,
                                  double   subTaskFraction = 0.01,
                                  int      minSubTasks     = 50,
                                  int      maxSubTasks     = 20000)
    {
      Debug.Assert(totalCalculationTime.TotalMilliseconds > 0);
      Debug.Assert(subTasksLevels == 0 || totalNbSubTasks > 0);
      Debug.Assert(data > 0);
      Debug.Assert(memory > 0);
      Debug.Assert(subTasksLevels >= 0);
      Debug.Assert(subTaskFraction is > 0.0 and <= 1.0);
      Debug.Assert(subTasksLevels == 0 || minSubTasks > 0.0);
      Debug.Assert(subTasksLevels == 0 || maxSubTasks > 0.0);
      TotalCalculationTime = totalCalculationTime;
      TotalNbSubTasks      = totalNbSubTasks;
      Data                 = data;
      Memory               = memory;
      SubTasksLevels       = subTasksLevels;
      SubTaskFraction      = subTaskFraction;
      MinSubTasks          = minSubTasks;
      MaxSubTasks          = maxSubTasks;
      AvgSubtasks = subTasksLevels > 1
                      ? Math.Pow(TotalNbSubTasks, 1.0 / SubTasksLevels) 
                      / SubTaskFraction * (SubTasksLevels-1)/ SubTasksLevels
                      : TotalNbSubTasks;
      AvgDurationMs = TotalCalculationTime.TotalMilliseconds / (TotalNbSubTasks + 1);
      MinDurationMs = minDurationMs == -1 ? (int) (AvgDurationMs / 3) : minDurationMs;
      MaxDurationMs = maxDurationMs == -1 ? (int) (AvgDurationMs * 30) : maxDurationMs;

      Console.WriteLine($"TotalCalculationTime={TotalCalculationTime}");
      Console.WriteLine($"TotalNbSubTasks={TotalNbSubTasks}");
      Console.WriteLine($"Data={Data}");
      Console.WriteLine($"Memory={Memory}");
      Console.WriteLine($"SubTasksLevels={SubTasksLevels}");
      Console.WriteLine($"SubTaskFraction={SubTaskFraction}");
      Console.WriteLine($"MinSubTasks={MinSubTasks}");
      Console.WriteLine($"MaxSubTasks={MaxSubTasks}");
      Console.WriteLine($"AvgSubtasks={AvgSubtasks}");
      Console.WriteLine($"AvgDurationMs={AvgDurationMs}");
      Console.WriteLine($"MinDurationMs={MinDurationMs}");
      Console.WriteLine($"MaxDurationMs={MaxDurationMs}");

      if (subTasksLevels > 0 && (AvgSubtasks < MinSubTasks || AvgSubtasks > MaxSubTasks))
        throw new ArgumentException("The provided arguments are incompatible: cannot satisfy the number of subtasks constraints");
    }

    public TimeSpan TotalCalculationTime { get; }

    public int TotalNbSubTasks { get; }

    public int Data { get; }

    public int Memory { get; }

    public int SubTasksLevels { get; }

    // 1 => all tasks have subtasks up to SubTasksLevels levels.
    // 0.5 => only half the tasks will have subtasks
    public double SubTaskFraction { get; }

    public int MinSubTasks { get; }

    public int MaxSubTasks { get; }

    public double AvgSubtasks { get; }

    public int MinDurationMs { get; }

    public int MaxDurationMs { get; }

    public double AvgDurationMs { get; }

    public int GetNbSubtasks(Random ran) => GetDistributedValue(MinSubTasks, MaxSubTasks, AvgSubtasks, ran);

    public int GetTaskDurationMs(Random ran) => GetDistributedValue(MinDurationMs, MaxDurationMs, AvgDurationMs, ran);

    public static int GetDistributedValue(int min, int max, double avg, Random ran)
    {
      Debug.Assert(min < avg && avg < max);
      Debug.Assert(ran is not null);

      var width         = max - min;
      var normalizedAvg = (avg - min) / width;

      /* We use the beta distribution.
       * It takes 2 parameters a and b and provide results in [0;1]
       * a and b must be positive
       * The average value is equal to a/(a+b)
       *
       * normalizedAvg = a/(a+b)
       *
       * a*normalizedAvg + b*normalizedAvg = a
       *
       * b*normalizedAvg = a(1-normalizedAvg)
       *
       * a = b*normalizedAvg/(1-normalizedAvg)  // normalizedAvg != 1 due to Assets above
       *
       * b can be chosen arbitrarily
       */

      const double b = 5.0;

      var a = b * normalizedAvg / (1 - normalizedAvg);

      Debug.Assert(Beta.IsValidParameterSet(a, b), $"Parameters a={a} and b={b} are invalid.");

      var x = Beta.Sample(ran, a, b);

      var result = (int) (x * width + min);
      Debug.Assert(result >= min, $"Result ({result}) for x={x} is smaller than min ({min})");
      Debug.Assert(result <= max, $"Result ({result}) for x={x} is bigger than min ({max})");
      return result;
    }

    [PublicAPI]
    public static RunConfiguration Minimal => new(new TimeSpan(0,0,0,0,100),
                                                      0,
                                                      1,
                                                      1,
                                                      0);
    [PublicAPI]
    public static RunConfiguration XSmall => new(new TimeSpan(0, 2, 0),
                                                      10,
                                                      1,
                                                      1,
                                                      1,
                                                      minSubTasks: 2,
                                                      maxSubTasks: 20);
    [PublicAPI]
    public static RunConfiguration Small => new(new TimeSpan(0, 10, 0),
                                                      100,
                                                      1,
                                                      1,
                                                      2,
                                                      subTaskFraction: 0.2,
                                                      minSubTasks: 5,
                                                      maxSubTasks: 100);
    [PublicAPI]
    public static RunConfiguration Medium => new(new TimeSpan(1, 0, 0),
                                                      10000,
                                                      1,
                                                      1,
                                                      4,
                                                      subTaskFraction: 0.2,
                                                      minSubTasks: 10,
                                                      maxSubTasks: 100);
    [PublicAPI]
    public static RunConfiguration Large => new(new TimeSpan(36, 0, 0),
                                                      4000000,
                                                      1,
                                                      1,
                                                      5,
                                                      subTaskFraction: 0.05,
                                                      minSubTasks: 50,
                                                      maxSubTasks: 25000);
    [PublicAPI]
    public static RunConfiguration XLarge => new(new TimeSpan(24000, 0, 0),
                                                      6000000,
                                                      1,
                                                      1,
                                                      7,
                                                      subTaskFraction: 0.05,
                                                      minSubTasks: 100,
                                                      maxSubTasks: 50000);
  }
}
