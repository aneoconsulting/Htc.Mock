/* Beta.cs is part of the Htc.Mock solution.
    
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

namespace Htc.Mock.Utils
{
  public static class Beta
  {
    public static double Sample(Random ran, double min, double avg, double max)
    {
      Debug.Assert(min < avg);
      Debug.Assert(avg < max);
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

      Debug.Assert(IsValidBetaParameterSet(a, b), $"Parameters a={a} and b={b} are invalid.");

      var x = BetaSample(ran, a, b);

      var result = (int)(x * width + min);
      Debug.Assert(result >= min, $"Result ({result}) for x={x} is smaller than min ({min})");
      Debug.Assert(result <= max, $"Result ({result}) for x={x} is bigger than min ({max})");
      return result;
    }

    private static bool IsValidBetaParameterSet(double a, double b) => a >= 0.0 && b >= 0.0;

    private static double BetaSample(Random ran, double a, double b)
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
      var    num3 = num1 - 1.0 / 3.0;
      var    num4 = 1.0 / Math.Sqrt(9.0 * num3);
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
}
