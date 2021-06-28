// This file is part of the Htc.Mock solution.
// 
// Copyright (c) ANEO. All rights reserved.
// * Wilfried KIRSCHENMANN (ANEO)
// 


using System;

using Htc.Mock.Common;

namespace Htc.Mock.GridClient
{
  class Program
  {
    static void Main()
    {
      var runConfiguration =
        new RunConfiguration(new TimeSpan(1, 0, 0),
                                   1000,
                                   1,
                                   1,
                                   3,
                                   minSubTasks: 10);

      var request = runConfiguration.DefaultHeadRequest();

      // TODO: create session

      // TODO: Submit request

      // TODO: wait for result

    }
  }
}
