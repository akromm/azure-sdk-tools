﻿// ----------------------------------------------------------------------------------
//
// Copyright Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ----------------------------------------------------------------------------------

namespace Microsoft.WindowsAzure.Commands.CloudService.Development
{
    using System.IO;
    using System.Management.Automation;
    using System.Security.Permissions;
    using Utilities.CloudService;
    using Utilities.CloudService.AzureTools;
    using Utilities.Common;
    using Utilities.Properties;

    /// <summary>
    /// Packages the service project into cloud or local package.
    /// </summary>
    [Cmdlet(VerbsData.Save, "AzureServiceProjectPackage"), OutputType(typeof(PSObject))]
    public class SaveAzureServiceProjectPackageCommand : CmdletBase
    {
        [Parameter(Mandatory = false)]
        [Alias("l")]
        public SwitchParameter Local { get; set; }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public override void ExecuteCmdlet()
        {
            AzureTool.Validate();
            string rootPath = CommonUtilities.GetServiceRootPath(CurrentPath());
            string packagePath;

            CloudServiceProject service = new CloudServiceProject(rootPath, null);

            if (!Local.IsPresent)
            {
                service.CreatePackage(DevEnv.Cloud);
                packagePath = Path.Combine(rootPath, Resources.CloudPackageFileName);
            }
            else
            {
                service.CreatePackage(DevEnv.Local);
                packagePath = Path.Combine(rootPath, Resources.LocalPackageFileName);
            }

            
            WriteVerbose(string.Format(Resources.PackageCreated, packagePath));
            SafeWriteOutputPSObject(typeof(PSObject).FullName, Parameters.PackagePath, packagePath);
        }
    }
}
