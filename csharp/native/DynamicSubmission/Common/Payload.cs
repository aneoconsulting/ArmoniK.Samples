// This file is part of the ArmoniK project
//
// Copyright (C) ANEO, 2021-$CURRENT_YEAR$. All rights reserved.
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

using System.Text;
using System.Text.Json;

namespace ArmoniK.Samples.DynamicSubmission.Common
{
  /// <summary>
  ///   Represents a Table with a sequence of numerical values.
  ///   This class is used by both the worker and the client to manage and manipulate table data.
  /// </summary>
  public record Table
  {
    /// <summary>
    ///   Constructor to create a Table with Values equal to a sequence of numbers from 1 to size.
    /// </summary>
    /// <param name="size">Size of Table Values.</param>
    /// <param name="threshold">Threshold to split the Table.</param>
    public Table(uint size,
                 uint threshold)
    {
      Size      = size;
      Threshold = threshold;
      Values    = new uint[size];
      for (uint i = 0; i < size; i++)
      {
        Values[i] = i + 1;
      }
    }

    /// <summary>
    ///   Default constructor.
    /// </summary>
    public Table()
    {
      Size      = 0;
      Threshold = 0;
      Values    = new uint[1];
    }

    /// <summary>
    ///   Constructor with a specified start value.
    /// </summary>
    /// <param name="start">First int to put in Values.</param>
    /// <param name="size">Size of Values.</param>
    /// <param name="threshold">Number of Values in a threshold.</param>
    public Table(uint start,
                 uint size,
                 uint threshold)
    {
      Size      = size;
      Threshold = threshold;
      Values    = new uint[size];
      for (uint i = 0; i < size; i++)
      {
        Values[i] = i + start;
      }
    }

    /// <summary>
    ///   Gets the size of Table Values.
    /// </summary>
    public uint Size { get; init; }

    /// <summary>
    ///   Gets the threshold to split the Table.
    /// </summary>
    public uint Threshold { get; init; }

    /// <summary>
    ///   Gets or sets the int array with each Table value.
    /// </summary>
    public uint[] Values { get; init; }

    /// <summary>
    ///   Deserializes the parameter into a Table object.
    /// </summary>
    /// <param name="payload">The byte array to deserialize.</param>
    /// <returns>A Table object deserialized from the payload.</returns>
    public static Table Deserialize(byte[] payload)
    {
      var res = JsonSerializer.Deserialize<Table>(Encoding.ASCII.GetString(payload));
      return res ?? new Table();
    }

    /// <summary>
    ///   Serializes the current Table object into a byte array.
    /// </summary>
    /// <returns>A byte array representing the serialized Table object.</returns>
    public byte[] Serialize()
    {
      var res = Encoding.ASCII.GetBytes(JsonSerializer.Serialize(this));
      return res;
    }
  }
}
