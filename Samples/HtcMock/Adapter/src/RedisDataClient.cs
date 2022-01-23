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
using System.IO;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

using ArmoniK.Samples.HtcMock.Adapter.Options;

using Htc.Mock;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

using StackExchange.Redis;

namespace ArmoniK.Samples.HtcMock.Adapter
{
  public class RedisDataClient : IDataClient, IDisposable
  {
    private readonly ConnectionMultiplexer con_;

    public RedisDataClient(Redis options)
    {
      var configurationRoot = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).
                                                         AddJsonFile(options.CredentialsPath).Build();
      var credentials = configurationRoot.GetValue<RedisCredentials>(RedisCredentials.SettingSection);
      con_ = this.CreateConnection(options.EndpointUrl,
                            credentials.SslHost,
                            options.Timeout,
                            credentials.Ssl,
                            credentials.User,
                            credentials.Password);
    }

    private ConnectionMultiplexer CreateConnection(string endpointUrl, string sslHost, int timeout, bool Ssl, string user, string password)
    {
      var configurationOptions = new ConfigurationOptions
      {
        EndPoints =
        {
          endpointUrl,
        },
        Ssl            = Ssl,
        SslHost        = sslHost,
        ConnectTimeout = timeout,
        Password       = password,
        User           = user,
      };

      var connection = ConnectionMultiplexer.Connect(configurationOptions);
      connection.GetDatabase().Ping();
      return connection;
    }

    public byte[] GetData(string key)
    {
      var data = con_.GetDatabase().StringGet(key);
      if (data.IsNullOrEmpty)
        throw new Exception($"Key {key} is not associated to values");
      return data;
    }

    public void StoreData(string key, byte[] data)
    {
      var b = con_.GetDatabase().StringSet(key,
                            data);
      if (!b)
        throw new Exception($"Data not stored for key {key}");
    }

    public void Dispose()
    {
      con_.Close();
    }
  }
}