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

namespace Microsoft.WindowsAzure.Management.Websites
{
    using System.Management.Automation;
    using Microsoft.WindowsAzure.Management.Utilities.Websites;
    using Microsoft.WindowsAzure.Management.Utilities.Websites.Common;
    using Microsoft.WindowsAzure.Management.Utilities.Websites.Services;
    using Microsoft.WindowsAzure.Management.Utilities.Websites.Services.DeploymentEntities;

    [Cmdlet(VerbsLifecycle.Disable, "AzureWebsiteApplicationDiagnostic"), OutputType(typeof(bool))]
    public class DisableAzureWebsiteApplicationDiagnosticCommand : WebsiteContextBaseCmdlet
    {
        private const string FileParameterSetName = "FileParameterSet";

        private const string StorageParameterSetName = "StorageParameterSet";

        public IWebsitesClient WebsitesClient { get; set; }

        [Parameter(Mandatory = false)]
        public SwitchParameter PassThru { get; set; }

        [Parameter(Mandatory = true, ParameterSetName = FileParameterSetName)]
        public SwitchParameter File { get; set; }

        [Parameter(Mandatory = true, ParameterSetName = StorageParameterSetName)]
        public SwitchParameter Storage { get; set; }

        public override void ExecuteCmdlet()
        {
            WebsitesClient = WebsitesClient ?? new WebsitesClient(CurrentSubscription, WriteDebug);

            if (File.IsPresent)
            {
                WebsitesClient.DisableApplicationDiagnostic(Name, WebsiteDiagnosticOutput.FileSystem);
            }
            else if (Storage.IsPresent)
            {
                WebsitesClient.DisableApplicationDiagnostic(Name, WebsiteDiagnosticOutput.StorageTable);
            }

            if (PassThru.IsPresent)
            {
                WriteObject(true);
            }
        }
    }
}
