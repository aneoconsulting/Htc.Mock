/* IRequestRunnerFactory.cs is part of the Htc.Mock solution.
    
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


using Htc.Mock.Core;

namespace Htc.Mock.RequestRunners
{
  /// <summary>
  /// The <c>IRequestRunnerFactory</c> is used to build a new <c>IRequestRunner</c>
  /// when a new session start to be executed.
  /// </summary>
  public interface IRequestRunnerFactory
  {
    IRequestRunner Create(RunConfiguration runConfiguration, string session);
  }
}
