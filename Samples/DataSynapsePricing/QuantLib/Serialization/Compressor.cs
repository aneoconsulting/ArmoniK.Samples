
// This file is part of the ArmoniK project
// 
// Copyright (C) ANEO, 2021-2022. All rights reserved.
//   W. Kirschenmann   <wkirschenmann@aneo.fr>
//   J. Gurhem         <jgurhem@aneo.fr>
//   D. Dubuc          <ddubuc@aneo.fr>
//   L. Ziane Khodja   <lzianekhodja@aneo.fr>
//   F. Lemaitre       <flemaitre@aneo.fr>
//   S. Djebbar        <sdjebbar@aneo.fr>
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.IO;

namespace QuantLib.Serialization
{

  public static class Compressor
  {
    public static byte[] SerializeObject<T>(T objectToByteArray)
    {
      var serializer = new Slim.SlimSerializer() { SerializeForFramework = true };
      using var mem = new MemoryStream();
      
      serializer.Serialize(mem, objectToByteArray);

      return mem.ToArray();
    }


    public static T DeSerializeObject<T>(byte[] byteArrayToObject)
    {
      var serializer = new Slim.SlimSerializer() { SerializeForFramework = true };
      var obj = default(T);

      using (var mem = new MemoryStream(byteArrayToObject)
             {
               Position = 0
             })
      {
        obj = (T)serializer.Deserialize(mem);
      }

      return obj;
    }
  }
}