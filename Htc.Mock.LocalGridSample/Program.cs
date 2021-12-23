// Program.cs is part of the Htc.Mock solution.
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

using JetBrains.Annotations;

using Microsoft.Extensions.Logging;

using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;

namespace Htc.Mock.LocalGridSample
{
  [PublicAPI]
  public class Program
  {
    public static void Main()
    {

      Log.Logger = new LoggerConfiguration()
                  .MinimumLevel.Warning()
                  .Enrich.FromLogContext()
                  .WriteTo.Console()
                  .CreateLogger();

      var loggerFactory = LoggerFactory.Create(builder =>
                           {
                             builder.AddFilter("Microsoft", LogLevel.Warning)
                                    .AddSerilog();
                           });

      var logger = loggerFactory.CreateLogger(nameof(Program));

      logger.LogCritical("Critical:Hello Htc.Mock!");
      logger.LogError("Hello Htc.Mock!");
      logger.LogWarning("Hello Htc.Mock!");
      logger.LogInformation("Hello Htc.Mock!");
      logger.LogTrace("Hello Htc.Mock!");


      // To provide a new client, one need to provide a sessionClient
      var gridClient = new GridClient(loggerFactory);

      // Code below is standard.
      var client = new Client(gridClient, loggerFactory.CreateLogger<Client>());

      client.Start(new(TimeSpan.FromSeconds(1), 500, 1, 1, 5));
    }
  }
}
