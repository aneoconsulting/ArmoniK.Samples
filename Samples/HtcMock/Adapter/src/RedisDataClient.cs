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

using Microsoft.Extensions.Options;

using StackExchange.Redis;

namespace ArmoniK.Samples.HtcMock.Adapter
{
  public class RedisDataClient : IDataClient
  {
    private readonly IDatabase db_;

    public RedisDataClient(IOptions<Redis> options) : this(options.Value.EndpointUrl,
                                                           options.Value.SslHost,
                                                           options.Value.Timeout,
                                                           options.Value.CaCertPath,
                                                           options.Value.ClientPfxPath)
    {
    }

    public RedisDataClient(string endpointUrl, string sslHost, int timeout, string caCertPath, string clientPfxPath)
    {
      var configurationOptions = new ConfigurationOptions
      {
        EndPoints =
        {
          endpointUrl,
        },
        Ssl            = true,
        SslHost        = sslHost,
        ConnectTimeout = timeout,
      };
      if (!File.Exists(caCertPath))
        throw new FileNotFoundException(caCertPath + " was not found !");

      if (!File.Exists(clientPfxPath))
        throw new FileNotFoundException(clientPfxPath + " was not found !");

      // method to validate the certificate
      // https://github.com/StackExchange/StackExchange.Redis/issues/1113
      configurationOptions.CertificateValidation += (sender, certificate, chain, sslPolicyErrors) =>
      {
        X509Certificate2 certificateAuthority = new(caCertPath);
        if (sslPolicyErrors == SslPolicyErrors.RemoteCertificateChainErrors)
        {
          var root = chain.ChainElements[^1].Certificate;
          return certificateAuthority.Equals(root);
        }

        return sslPolicyErrors == SslPolicyErrors.None;
      };

      configurationOptions.CertificateSelection += delegate
      {
        var cert = new X509Certificate2(clientPfxPath);
        return cert;
      };
      var connection = ConnectionMultiplexer.Connect(configurationOptions);
      db_ = connection.GetDatabase();
      db_.Ping();
    }

    public byte[] GetData(string key)
    {
      var data = db_.StringGet(key);
      if (data.IsNullOrEmpty)
        throw new Exception($"Key {key} is not associated to values");
      return data;
    }

    public void StoreData(string key, byte[] data)
    {
      var b = db_.StringSet(key,
                            data);
      if (!b)
        throw new Exception($"Data not stored for key {key}");
    }
  }
}