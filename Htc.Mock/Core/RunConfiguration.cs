// RunConfiguration.cs is part of the Htc.Mock solution.
// 
// Copyright (c) 2021-2021 ANEO. All rights reserved.
// * Wilfried KIRSCHENMANN (https://github.com/wkirschenmann)
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Diagnostics;
using System.Linq;

using Google.Protobuf;

using Htc.Mock.Utils;

using JetBrains.Annotations;

namespace Htc.Mock.Core
{
  [PublicAPI]
  public class RunConfiguration
  {
    internal ByteString Serialized { get; set; } = ByteString.Empty;

    private const int NbSamples = 10000;

    public RunConfiguration()
    {
    }


    public RunConfiguration(TimeSpan totalCalculationTime,
                            int      totalNbSubTasks,
                            int      data,
                            int      memory,
                            int      subTasksLevels,
                            int      minDurationMs = -1,
                            int      maxDurationMs = -1)
    {
      Debug.Assert(totalCalculationTime.TotalMilliseconds > 0);
      Debug.Assert(subTasksLevels == 0 || totalNbSubTasks > 0);
      Debug.Assert(data > 0);
      Debug.Assert(memory > 0);
      Debug.Assert(subTasksLevels >= 0);
      TotalCalculationTime = totalCalculationTime;
      TotalNbSubTasks      = totalNbSubTasks;
      Data                 = data;
      Memory               = memory;
      SubTasksLevels       = subTasksLevels;
      AvgDurationMs        = TotalCalculationTime.TotalMilliseconds / (TotalNbSubTasks + 1);
      MinDurationMs        = minDurationMs == -1 ? (int)(AvgDurationMs / 3) : minDurationMs;
      MaxDurationMs        = maxDurationMs == -1 ? (int)(AvgDurationMs * 30) : maxDurationMs;

      var seed = (int)TotalCalculationTime.Ticks +
                 (2 * TotalNbSubTasks) +
                 (3 * Data) +
                 (4 * Memory) +
                 (5 * SubTasksLevels) +
                 (6 * MinDurationMs) +
                 (7 * MaxDurationMs) +
                 (8 * NbSamples);

      var ran = new Random(seed);

      DurationSamples = Enumerable.Range(0, NbSamples)
                                  .Select(_ => (int)Beta.Sample(ran, MinDurationMs, AvgDurationMs, MaxDurationMs))
                                  .Select(i => i < 0 ? 1 : i)
                                  .ToArray();

      Seed = ran.Next();

      Console.WriteLine($"[Htc.Mock] {nameof(TotalCalculationTime)}={TotalCalculationTime}");
      Console.WriteLine($"[Htc.Mock] {nameof(TotalNbSubTasks)}={TotalNbSubTasks}");
      Console.WriteLine($"[Htc.Mock] {nameof(Data)}={Data}");
      Console.WriteLine($"[Htc.Mock] {nameof(Memory)}={Memory}");
      Console.WriteLine($"[Htc.Mock] {nameof(SubTasksLevels)}={SubTasksLevels}");
      Console.WriteLine($"[Htc.Mock] {nameof(AvgDurationMs)}={AvgDurationMs}");
      Console.WriteLine($"[Htc.Mock] {nameof(MinDurationMs)}={MinDurationMs}");
      Console.WriteLine($"[Htc.Mock] {nameof(MaxDurationMs)}={MaxDurationMs}");
      Console.WriteLine($"[Htc.Mock] {nameof(Seed)}={Seed}");
    }

    public int[] DurationSamples { get; set; }

    public TimeSpan TotalCalculationTime { get; set; }

    public int TotalNbSubTasks { get; set; }

    public int Data { get; set; }

    public int Memory { get; set; }

    public int SubTasksLevels { get; set; }

    public int MinDurationMs { get; set; }

    public int MaxDurationMs { get; set; }

    public double AvgDurationMs { get; set; }

    public int Seed { get; set; }

    [PublicAPI]
    public static RunConfiguration Minimal => new RunConfiguration(new TimeSpan(0, 0, 0, 0, 100),
                                                                   0,
                                                                   1,
                                                                   1,
                                                                   0);

    [PublicAPI]
    public static RunConfiguration XSmall => new RunConfiguration(new TimeSpan(0, 2, 0),
                                                                  10,
                                                                  1,
                                                                  1,
                                                                  1);

    [PublicAPI]
    public static RunConfiguration Small => new RunConfiguration(new TimeSpan(0, 10, 0),
                                                                 100,
                                                                 1,
                                                                 1,
                                                                 2);

    [PublicAPI]
    public static RunConfiguration Medium => new RunConfiguration(new TimeSpan(1, 0, 0),
                                                                  10000,
                                                                  1,
                                                                  1,
                                                                  3);

    [PublicAPI]
    public static RunConfiguration Large => new RunConfiguration(new TimeSpan(36, 0, 0),
                                                                 4000000,
                                                                 1,
                                                                 2,
                                                                 5);

    [PublicAPI]
    public static RunConfiguration XLarge => new RunConfiguration(new TimeSpan(24000, 0, 0),
                                                                  6000000,
                                                                  1,
                                                                  3,
                                                                  7);


    public int GetTaskDurationMs(string taskId)
    {
      Debug.Assert(DurationSamples != null, nameof(DurationSamples) + " != null");
      var hash   = taskId.GetCryptoHashCode();
      var result = DurationSamples[hash % NbSamples];
      return result;
    }

    /// <inheritdoc />
    public override string ToString() => $"{{{nameof(RunConfiguration)}: {{{nameof(TotalCalculationTime)}: {TotalCalculationTime}," +
                                         $"{nameof(TotalNbSubTasks)}: {TotalNbSubTasks}," +
                                         $"{nameof(Data)}: {Data}," +
                                         $"{nameof(Memory)}: {Memory}," +
                                         $"{nameof(SubTasksLevels)}: {SubTasksLevels}," +
                                         $"{nameof(AvgDurationMs)}: {AvgDurationMs}," +
                                         $"{nameof(MinDurationMs)}: {MinDurationMs}," +
                                         $"{nameof(MaxDurationMs)}: {MaxDurationMs}}}}}";
  }
}
