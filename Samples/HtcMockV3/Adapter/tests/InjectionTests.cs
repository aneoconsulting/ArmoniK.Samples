// This file is part of the ArmoniK project
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

using System.Collections.Generic;

using ArmoniK.Samples.HtcMock.Adapter;
using ArmoniK.Samples.HtcMock.Adapter.Options;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using NUnit.Framework;

namespace ArmoniK.Samples.HtcMock.Adapters.Tests
{
  [TestFixture]
  internal class InjectionTests
  {
    [SetUp]
    public void SetUp()
    {
      Dictionary<string, string> baseConfig = new()
      {
        { "Redis:EndpointUrl", "127.0.0.1:6789" },
      };

      var configSource = new MemoryConfigurationSource
      {
        InitialData = baseConfig,
      };

      var builder = new ConfigurationBuilder()
        .Add(configSource);

      configuration_ = builder.Build();
    }

    private IConfigurationRoot configuration_;

  }
}