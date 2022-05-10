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
using System.Reflection;
using System.Runtime.Loader;

using QuantLib.Serialization;

namespace Services
{

  public class SimpleServiceContainer
  {
    public static double[] ComputePricing(object inputs)
    {
      var localConfigParameters = new ConfigParameters();
      
      string currentAssemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
      //var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath( Path.Combine(currentAssemblyDirectory, "QuantLibUtils.dll"));
      var assembly = Assembly.LoadFile( Path.Combine(currentAssemblyDirectory, "QuantLibUtils.dll"));
      //var assembly = Assembly.LoadFrom(Path.Combine(currentAssemblyDirectory, "QuantLibUtils.dll"));

      var instance = assembly.CreateInstance("QuantLibUtils.MatrixConvertor");

      var methodInfo = instance?.GetType().GetMethod("Deserialize");

      var configParameters = (ConfigParameters)methodInfo?.Invoke(instance, new object[] { inputs });
      configParameters ??= localConfigParameters;

      return new double[] { configParameters.DefaultValue, configParameters.PricingParameters.Spot, 0.0 };
    }

  }
}