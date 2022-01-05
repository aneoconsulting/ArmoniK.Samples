﻿// This file is part of the ArmoniK project
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

using System;

namespace ArmoniK.Samples.HtcMock.Adapter
{
  public static class SessionIdExt
  {
    public static string ToHtcMockId(this SessionId sessionId)
    {
      return $"{sessionId.Session}_{sessionId.SubSession}";
    }

    public static SessionId ToArmoniKId(this string id)
    {
      var split = id.Split('_');
      if (split.Length != 2)
        throw new ArgumentException("Id is not a valid SessionId",
                                    nameof(id));
      return new SessionId
      {
        Session    = split[0],
        SubSession = split[1],
      };
    }
  }
}