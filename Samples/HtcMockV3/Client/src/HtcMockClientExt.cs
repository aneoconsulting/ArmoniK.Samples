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

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Htc.Mock.Core;

namespace ArmoniK.Samples.HtcMock.Client
{
  public static class HtcMockClientExt
  {
    public static void ParallelExec(this Htc.Mock.Client client, RunConfiguration runConfiguration, int nRun)
    {
      var sw = Stopwatch.StartNew();
      var tasks = Enumerable.Repeat(runConfiguration,
                                    nRun)
                            .Select(configuration => Task.Run(() => client.Start(configuration)))
                            .ToList();
      Task.WhenAll(tasks).Wait();
      var elapsedMilliseconds = sw.ElapsedMilliseconds;
      var stat = new SimpleStats
      {
        EllapsedTime = elapsedMilliseconds,
        Test         = "AsyncExec",
        NRun         = nRun,
      };
      Console.WriteLine("JSON Result : " + stat.ToJson());
    }

    public static void SeqExec(this Htc.Mock.Client client, RunConfiguration runConfiguration, int nRun)
    {
      var sw = Stopwatch.StartNew();
      for (var i = 0; i < nRun; i++)
        client.Start(runConfiguration);

      var elapsedMilliseconds = sw.ElapsedMilliseconds;
      var stat = new SimpleStats
      {
        EllapsedTime = elapsedMilliseconds,
        Test         = "SeqExec",
        NRun         = nRun,
      };
      Console.WriteLine("JSON Result : " + stat.ToJson());
    }
  }
}