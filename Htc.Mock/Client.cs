﻿/* Client.cs is part of the Htc.Mock solution.
    
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
using System.Text;

using Htc.Mock.Core;

using JetBrains.Annotations;

namespace Htc.Mock
{
  [PublicAPI]
  public class Client
  {
    private readonly IGridClient gridClient;
    private readonly IDataClient dataClient;

    public Client(IGridClient gridClient, IDataClient dataClient)
    {
      this.gridClient = gridClient;
      this.dataClient = dataClient;
    }

    [PublicAPI]
    public void Start(RunConfiguration runConfiguration)
    {
      var session = gridClient.CreateSession();

      var request = runConfiguration.DefaultHeadRequest();

      var taskId = gridClient.SubmitTask(session, DataAdapter.BuildPayload(runConfiguration, request));

      gridClient.WaitSubtasksCompletion(taskId);

      // the mock project has been designed so that output contains dummy data
      _ = gridClient.GetResult(taskId);

      // the proper result is stored in the data cache
      var result = dataClient.GetData(request.Id);

      Console.WriteLine($"[Htc.Mock] Final result is {Encoding.ASCII.GetString(result)}");
      Console.WriteLine($"[Htc.Mock] If run with configuration = Medium, result should be Aggregate_3926158863_result");
    }


    [PublicAPI]
    public void Start() => Start(RunConfiguration.Medium);
  }
}

