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
      => configuration.SubTasksLevels == 0
           ? new FinalRequest()
           : new ComputeRequest(configuration.SubTasksLevels, configuration.TotalNbSubTasks);
  }

  [Serializable]
  public class RunConfiguration
  {
    private const int NbSamples           = 100000;

    private readonly int[] durationSamples_;

    private readonly int seed_;

    public RunConfiguration(TimeSpan totalCalculationTime,
                            int      totalNbSubTasks,
                            int      data,
                            int      memory,
                            int      subTasksLevels,
                            int      minDurationMs   = -1,
                            int      maxDurationMs   = -1)
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
      AvgDurationMs = TotalCalculationTime.TotalMilliseconds / (TotalNbSubTasks + 1);
      MinDurationMs = minDurationMs == -1 ? (int) (AvgDurationMs / 3) : minDurationMs;
      MaxDurationMs = maxDurationMs == -1 ? (int) (AvgDurationMs * 30) : maxDurationMs;

      seed_ = (int) TotalCalculationTime.Ticks +
              2 * TotalNbSubTasks +
              3 * Data +
              4 * Memory +
              5 * SubTasksLevels +
              6 * MinDurationMs +
              7 * MaxDurationMs +
              8 * NbSamples;

      var ran = new Random(seed_);

      durationSamples_ = Enumerable.Range(0, NbSamples)
                                   .Select(_ => (int) Beta.Sample(ran, MinDurationMs, AvgDurationMs, MaxDurationMs))
                                   .ToArray();
      
      Console.WriteLine($"{nameof(TotalCalculationTime)}={TotalCalculationTime}");
      Console.WriteLine($"{nameof(TotalNbSubTasks)}={TotalNbSubTasks}");
      Console.WriteLine($"{nameof(Data)}={Data}");
      Console.WriteLine($"{nameof(Memory)}={Memory}");
      Console.WriteLine($"{nameof(SubTasksLevels)}={SubTasksLevels}");
      Console.WriteLine($"{nameof(AvgDurationMs)}={AvgDurationMs}");
      Console.WriteLine($"{nameof(MinDurationMs)}={MinDurationMs}");
      Console.WriteLine($"{nameof(MaxDurationMs)}={MaxDurationMs}");
      Console.WriteLine($"{nameof(seed_)}={seed_}");
    }

    public TimeSpan TotalCalculationTime { get; }

    public int TotalNbSubTasks { get; }

    public int Data { get; }

    public int Memory { get; }

    public int SubTasksLevels { get; }

    public int MinDurationMs { get; }

    public int MaxDurationMs { get; }

    public double AvgDurationMs { get; }
    
    public int GetTaskDurationMs(string taskId)
    {
      Debug.Assert(durationSamples_ is not null, nameof(durationSamples_) + " != null");
      var hash   = taskId.GetCryptoHashCode();
      var result = durationSamples_[hash % NbSamples];
      return result;
    }

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
                                                 1);

    [PublicAPI]
    public static RunConfiguration Small => new(new TimeSpan(0, 10, 0),
                                                100,
                                                1,
                                                1,
                                                2);

    [PublicAPI]
    public static RunConfiguration Medium => new(new TimeSpan(1, 0, 0),
                                                 10000,
                                                 1,
                                                 1,
                                                 3);

    [PublicAPI]
    public static RunConfiguration Large => new(new TimeSpan(36, 0, 0),
                                                4000000,
                                                1,
                                                2,
                                                5);

    [PublicAPI]
    public static RunConfiguration XLarge => new(new TimeSpan(24000, 0, 0),
                                                 6000000,
                                                 1,
                                                 3,
                                                 7);
  }
}
