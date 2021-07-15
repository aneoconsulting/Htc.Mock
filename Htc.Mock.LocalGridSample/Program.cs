/* Program.cs is part of the Htc.Mock.LocalGridSample solution.
    
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

using Htc.Mock.Core;

using JetBrains.Annotations;

namespace Htc.Mock.LocalGridSample
{
  [PublicAPI]
  public class Program
  {
    public static void Main()
    {
      // To provide a new client, one need to provide a dataClient and a gridClient
      var dataClient = new DataClient();
      var gridClient = new GridClient(dataClient);

      // Code below is standard.
      var client = new Client(gridClient, dataClient);
      
      client.Start();
    }
  }
}
