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
using System.Threading;
using System.Threading.Tasks;

namespace ArmoniK.Samples.Common
{
  public static class Utils
  {
    public static object[] ParamsHelper(params object[] elements)
      => elements;

    public static IDisposable PeriodicInfo(Action action,
                                           int    seconds)
      => new DisposablePeriodicInfo(action,
                                    seconds);


    private class DisposablePeriodicInfo : IDisposable
    {
      private readonly Action                  action_;
      private readonly CancellationTokenSource cts_;

      internal DisposablePeriodicInfo(Action action,
                                      int    seconds)
      {
        cts_    = new CancellationTokenSource();
        action_ = action;
        Task.Run(async () =>
                 {
                   while (!cts_.Token.IsCancellationRequested)
                   {
                     action();
                     await Task.Delay(TimeSpan.FromSeconds(seconds),
                                      cts_.Token);
                   }
                 },
                 cts_.Token);
      }

      /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
      public void Dispose()
      {
        cts_.Cancel();
        action_();
        cts_.Dispose();
      }
    }
  }
}
