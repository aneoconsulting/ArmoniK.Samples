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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ArmoniK.DevelopmentKit.Client.Common;
using ArmoniK.DevelopmentKit.Client.Common.Exceptions;

using Microsoft.Extensions.Logging;

namespace CustomClientGUI.Submitter
{
  public class ResultForStressTestsHandler : IServiceInvocationHandler
  {
    private readonly ILogger<DemoTests> Logger_;

    public ResultForStressTestsHandler(ILoggerFactory loggerFactory)
      => Logger_ = loggerFactory.CreateLogger<DemoTests>();

    public int    NbResults  { get; set; } = 0;
    public double Total      { get; private set; } = 0;
    public int    NbResponse { get; set; } = 0;

    /// <summary>
    ///   The callBack method which has to be implemented to retrieve error or exception
    /// </summary>
    /// <param name="e">The exception sent to the client from the control plane</param>
    /// <param name="taskId">The task identifier which has invoke the error callBack</param>
    public void HandleError(ServiceInvocationException e,
                            string                     taskId)

    {
      if (e.StatusCode == ArmonikStatusCode.TaskCancelled)
      {
        Logger_.LogWarning($"Warning from {taskId} : " + e.Message);
      }
      else
      {
        Logger_.LogError($"Error from {taskId} : " + e.Message);

        SubActionError?.Invoke(e,
                               taskId);
      }
    }

    /// <summary>
    ///   The callBack method which has to be implemented to retrieve response from the server
    /// </summary>
    /// <param name="response">The object receive from the server as result the method called by the client</param>
    /// <param name="taskId">The task identifier which has invoke the response callBack</param>
    public void HandleResponse(object response,
                               string taskId)

    {
      switch (response)
      {
        case double[] doubles:
          Total += doubles.Sum();
          break;
        case null:
          Logger_.LogInformation("Task finished but nothing returned in Result");
          break;
      }

      NbResults++;

      SubActionResponse?.Invoke(response,
                                taskId);
    }

    public Action<object, string>                     SubActionResponse { get; set; }
    public Action<ServiceInvocationException, string> SubActionError    { get; set; }
  }
}
