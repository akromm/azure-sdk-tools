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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Commands.ScenarioTest.Common;

namespace Microsoft.WindowsAzure.Commands.ScenarioTest.CloudServiceTests
{
    [TestClass]
    public class ReverseDnsFqdnAzureServiceScenarioTests : AzurePowerShellCertificateTest
    {
        public ReverseDnsFqdnAzureServiceScenarioTests()
            : base("CloudService\\Common.ps1",
                   "CloudService\\CloudServiceTests.ps1")
        {

        }

        [TestInitialize]
        public override void TestSetup()
        {
            base.TestSetup();
            powershell.AddScript("Initialize-CloudServiceTest");
        }

        [TestMethod]
        [TestCategory(Category.All)]
        [TestCategory(Category.CloudService)]
        [TestCategory(Category.BVT)]
        public void TestNewAzureServiceReverseDnsFqdn()
        {
            RunPowerShellTest("Test-NewAzureServiceWithReverseDnsFqdn");
        }

        [TestMethod]
        [TestCategory(Category.All)]
        [TestCategory(Category.CloudService)]
        [TestCategory(Category.BVT)]
        public void TestSetAzureServiceReverseDnsFqdn()
        {
            RunPowerShellTest("Test-SetAzureServiceWithReverseDnsFqdn");
        }

        [TestMethod]
        [TestCategory(Category.All)]
        [TestCategory(Category.CloudService)]
        [TestCategory(Category.BVT)]
        public void TestSetAzureServiceWithEmptyReverseDnsFqdn()
        {
            RunPowerShellTest("Test-SetAzureServiceWithEmptyReverseDnsFqdn");
        }
   }
}