/* RunConfiguration.cs is part of the Htc.Mock.Common solution.
    
   Copyright (c) 2021-2021 ANEO. 
     W. Kirschenmann (https://github.com/wkirschenmann)
  
   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at
   
       http://www.apache.org/licenses/LICENSE-2.0
   
   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.

*/ 


using System;
using System.Diagnostics;
using System.Linq;

using JetBrains.Annotations;


namespace Htc.Mock.Common
{
  public static class RunConfigurationExt
  {
    public static Request DefaultHeadRequest(this RunConfiguration configuration)
    => new((int)configuration.AvgDurationMs, configuration.Memory, configuration.Data, configuration.SubTasksLevels);
  }

  [Serializable]
  public class RunConfiguration
  {
    private const    int      NbSamples           = 50000;
    private const    int      SubTaskFractionPrec = 5000;

    private readonly int[] subTaskSamples_;
    private readonly int[] durationSamples_;

    private readonly int      seed_;

    public RunConfiguration(TimeSpan totalCalculationTime,
                            int      totalNbSubTasks,
                            int      data,
                            int      memory,
                            int      subTasksLevels,
                            int      minDurationMs   = -1,
                            int      maxDurationMs   = -1,
                            double   subTaskFraction = 0.01,
                            int      minSubTasks     = 1,
                            int      maxSubTasks     = 10000)
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
                      ? subTasksLevels > 3
                          ? Math.Pow(TotalNbSubTasks, 1.0 / SubTasksLevels)
                          / SubTaskFraction * (SubTasksLevels - 1) / (SubTasksLevels)
                          : Math.Pow(TotalNbSubTasks, 1.0 / SubTasksLevels)
                          / SubTaskFraction * (SubTasksLevels - 1) / SubTasksLevels
                      : TotalNbSubTasks;
      AvgDurationMs = TotalCalculationTime.TotalMilliseconds / (TotalNbSubTasks + 1);
      MinDurationMs = minDurationMs == -1 ? (int) (AvgDurationMs / 3) : minDurationMs;
      MaxDurationMs = maxDurationMs == -1 ? (int) (AvgDurationMs * 30) : maxDurationMs;

      seed_ = (int) TotalCalculationTime.Ticks +
              2 * TotalNbSubTasks +
              3 * Data +
              4 * Memory +
              5 * SubTasksLevels +
              6 * MinSubTasks +
              7 * MaxSubTasks +
              8 * MinDurationMs +
              9 * MaxDurationMs +
              (int) (10 * 1 / subTaskFraction);


      var ran = new Random(seed_);

      if (SubTasksLevels > 0)
      {
        subTaskSamples_ = Enumerable.Range(0, NbSamples)
                                    .Select(_ => (int) Beta.Sample(ran, MinSubTasks, AvgSubtasks, MaxSubTasks))
                                    .ToArray();

        durationSamples_ = Enumerable.Range(0, NbSamples)
                                     .Select(_ => (int) Beta.Sample(ran, MinDurationMs, AvgDurationMs, MaxDurationMs))
                                     .ToArray();
      }

      Console.WriteLine($"{nameof(TotalCalculationTime)}={TotalCalculationTime}");
      Console.WriteLine($"{nameof(TotalNbSubTasks)}={TotalNbSubTasks}");
      Console.WriteLine($"{nameof(Data)}={Data}");
      Console.WriteLine($"{nameof(Memory)}={Memory}");
      Console.WriteLine($"{nameof(SubTasksLevels)}={SubTasksLevels}");
      Console.WriteLine($"{nameof(SubTaskFraction)}={SubTaskFraction}");
      Console.WriteLine($"{nameof(MinSubTasks)}={MinSubTasks}");
      Console.WriteLine($"{nameof(MaxSubTasks)}={MaxSubTasks}");
      Console.WriteLine($"{nameof(AvgSubtasks)}={AvgSubtasks}");
      Console.WriteLine($"{nameof(AvgDurationMs)}={AvgDurationMs}");
      Console.WriteLine($"{nameof(MinDurationMs)}={MinDurationMs}");
      Console.WriteLine($"{nameof(MaxDurationMs)}={MaxDurationMs}");
      Console.WriteLine($"{nameof(seed_)}={seed_}");

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

    public bool IsLeaf(string taskId) => Math.Abs(taskId.GetCryptoHashCode()) % SubTaskFractionPrec > SubTaskFraction * SubTaskFractionPrec;

    public int GetNbSubtasks(string taskId) => subTaskSamples_[Math.Abs(taskId.GetCryptoHashCode()) % NbSamples];

    public int GetTaskDurationMs(string taskId) => durationSamples_[Math.Abs(taskId.GetCryptoHashCode()) % NbSamples];

    [PublicAPI]
    public static RunConfiguration Minimal => new(new TimeSpan(0, 0, 0, 0, 100),
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
                                                         minSubTasks: 1,
                                                         maxSubTasks: 20);

    [PublicAPI]
    public static RunConfiguration Small => new(new TimeSpan(0, 10, 0),
                                                        100,
                                                        1,
                                                        1,
                                                        2,
                                                        subTaskFraction: 0.1,
                                                        maxSubTasks: 100);

    [PublicAPI]
    public static RunConfiguration Medium => new(new TimeSpan(1, 0, 0),
                                                         10000,
                                                         1,
                                                         1,
                                                         3,
                                                         subTaskFraction: 0.15,
                                                         maxSubTasks: 100);

    [PublicAPI]
    public static RunConfiguration Large => new(new TimeSpan(36, 0, 0),
                                                4000000,
                                                1,
                                                2,
                                                5,
                                                subTaskFraction: 0.05,
                                                minSubTasks: 5);

    [PublicAPI]
    public static RunConfiguration XLarge => new(new TimeSpan(24000, 0, 0),
                                                 6000000,
                                                 1,
                                                 3,
                                                 7,
                                                 subTaskFraction: 0.1,
                                                 minSubTasks: 5);
  }
}
