// This file is part of the ArmoniK project
// 
// Copyright (C) ANEO, 2021-2025. All rights reserved.
//   W. Kirschenmann   <wkirschenmann@aneo.fr>
//   J. Gurhem         <jgurhem@aneo.fr>
//   D. Dubuc          <ddubuc@aneo.fr>
//   L. Ziane Khodja   <lzianekhodja@aneo.fr>
//   F. Lemaitre       <flemaitre@aneo.fr>
//   S. Djebbar        <sdjebbar@aneo.fr>
//   J. Fonseca        <jfonseca@aneo.fr>
//   D. Brasseur       <dbrasseur@aneo.fr>
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY, without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace ArmoniK.Samples.Common
{
  public class ClientPayload
  {
    public enum TaskType
    {
      ComputeSquare,
      ComputeCube,
      Result,
      Sleep,
      JobOfNTasks,
      NJobOf1Tasks,
      Aggregation,
      SubTask,
      Undefined,
    }

    public bool      IsRootTask { get; set; }
    public TaskType  Type       { get; set; }
    public List<int> numbers    { get; set; }
    public int       result     { get; set; }
    public string    SubTaskId  { get; set; }
    public int       sleep      { get; set; }

    public byte[] serialize()
    {
      var jsonString = JsonSerializer.Serialize(this);
      return Encoding.ASCII.GetBytes(stringToBase64(jsonString));
    }

    public static ClientPayload deserialize(byte[] payload)
    {
      if (payload == null || payload.Length == 0)
      {
        return new ClientPayload
               {
                 Type    = TaskType.Undefined,
                 numbers = new List<int>(),
                 result  = 0,
               };
      }

      var str = Encoding.ASCII.GetString(payload);
      return JsonSerializer.Deserialize<ClientPayload>(Base64ToString(str));
    }

    private static string stringToBase64(string serializedJson)
    {
      var serializedJsonBytes       = Encoding.UTF8.GetBytes(serializedJson);
      var serializedJsonBytesBase64 = Convert.ToBase64String(serializedJsonBytes);
      return serializedJsonBytesBase64;
    }

    private static string Base64ToString(string base64)
    {
      var c = Convert.FromBase64String(base64);
      return Encoding.ASCII.GetString(c);
    }
  }
}
