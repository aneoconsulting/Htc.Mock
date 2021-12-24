// IRequestRunnerFactory.cs is part of the Htc.Mock solution.
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

using Htc.Mock.Core;

using Microsoft.Extensions.Logging;

namespace Htc.Mock.RequestRunners
{
  /// <summary>
  ///   The <c>IRequestRunnerFactory</c> is used to build a new <c>IRequestRunner</c>
  ///   when a new session start to be executed.
  /// </summary>
  public interface IRequestRunnerFactory
  {
    IRequestRunner Create(RunConfiguration runConfiguration);
  }

  public class DistributedRequestRunnerFactory : IRequestRunnerFactory
  {
    private readonly IGridClient                       gridClient_;
    private readonly ILogger<DistributedRequestRunner> logger_;
    private readonly bool                              fastCompute_;
    private readonly bool                              useLowMem_;
    private readonly bool                              smallOutput_;

    public DistributedRequestRunnerFactory(IGridClient                       gridClient,
                                           ILogger<DistributedRequestRunner> logger,
                                           bool                              fastCompute = false,
                                           bool                              useLowMem   = false,
                                           bool                              smallOutput = false)
    {
      gridClient_  = gridClient;
      logger_      = logger;
      fastCompute_ = fastCompute;
      useLowMem_   = useLowMem;
      smallOutput_ = smallOutput;
    }

    /// <inheritdoc />
    public IRequestRunner Create(RunConfiguration runConfiguration) => new DistributedRequestRunner(gridClient_, 
                                                                                                    runConfiguration, 
                                                                                                    logger_, 
                                                                                                    fastCompute_,
                                                                                                    useLowMem_,
                                                                                                    smallOutput_);
  }
}
