using System;
using System.Collections.Generic;
using System.Linq;

namespace ArmoniK.Samples.GridServer.Common
{
  public static class SelectExtensions
  {
    public static IEnumerable<double> ConvertToArray(this IEnumerable<byte> arr)
    {
      var bytes = arr as byte[] ?? arr.ToArray();

      var values = new double[bytes.Count() / sizeof(double)];

      var i = 0;
      for (; i < values.Length; i++)
      {
        values[i] = BitConverter.ToDouble(bytes.ToArray(),
                                          i * 8);
      }

      return values;
    }
  }
}
