﻿// This file is part of the ArmoniK project
// 
// Copyright (C) ANEO, 2021-2021. All rights reserved.
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

using ArmoniK.Core.gRPC.V1;

using Grpc.Core;
using Grpc.Net.Client;

using JetBrains.Annotations;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ArmoniK.Samples.HtcMock.Adapter
{
  public static class ServiceCollectionExt
  {
    [PublicAPI]
    public static IServiceCollection AddComponents(
      this IServiceCollection serviceCollection,
      IConfiguration          configuration
    )
    {
      serviceCollection.Configure<Options.Grpc>(configuration.GetSection(Options.Grpc.SettingSection))
                       .AddSingleton(sp =>
                       {
                         var options = sp.GetRequiredService<IOptions<Options.Grpc>>();
                         return GrpcChannel.ForAddress(options.Value.Endpoint);
                       })
                       .AddTransient(sp =>
                       {
                         ChannelBase channel = sp.GetRequiredService<GrpcChannel>();
                         return new ClientService.ClientServiceClient(channel);
                       })
                       .AddTransient<GridClient>();

      return serviceCollection;
    }
  }
}