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

namespace Microsoft.WindowsAzure.Management.SqlDatabase.Test.FunctionalTests
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Xml.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Management.SqlDatabase.Test.Utilities;

    [TestClass]
    public class DatabaseTest
    {
        private string userName;
        private string password;
        private string manageUrl;
        private string serverLocation;
        private string subscriptionId;
        private string serializedCert;
        private string containerName;
        private string storageName;
        private string accessKey;
        private string ieServerLocation;

        private const string CreateContextScript = @"Database\CreateContext.ps1";
        private const string CreateScript = @"Database\CreateAndGetDatabase.ps1";
        private const string UpdateScript = @"Database\UpdateDatabase.ps1";
        private const string DeleteScript = @"Database\DeleteDatabase.ps1";
        private const string FormatValidationScript = @"Database\FormatValidation.ps1";
        private const string ImportExportScript = @"Database\ImportExportDatabase.ps1";

        [TestInitialize]
        public void Setup()
        {
            XElement root = XElement.Load("SqlDatabaseSettings.xml");
            this.userName = root.Element("SqlAuthUserName").Value;
            this.password = root.Element("SqlAuthPassword").Value;
            this.manageUrl = root.Element("ManageUrl").Value;
            this.serverLocation = root.Element("ServerLocation").Value;
            this.subscriptionId = root.Element("SubscriptionId").Value;
            this.serializedCert = root.Element("SerializedCert").Value;
            this.containerName = root.Element("ContainerName").Value;
            this.storageName = root.Element("StorageName").Value;
            this.accessKey = root.Element("AccessKey").Value;
            this.ieServerLocation = root.Element("IEServerLocation").Value;
        }

        [TestMethod]
        [TestCategory("Functional")]
        public void CreateContext()
        {
            string arguments = string.Format(
                CultureInfo.InvariantCulture,
                "-ManageUrl \"{0}\" -UserName \"{1}\" -Password \"{2}\"",
                this.manageUrl,
                this.userName,
                this.password);
            bool testResult = PSScriptExecutor.ExecuteScript(
                DatabaseTest.CreateContextScript,
                arguments);
            Assert.IsTrue(testResult);
        }

        [TestMethod]
        [TestCategory("Functional")]
        public void CreateDatabase()
        {
            string arguments = string.Format(
                CultureInfo.InvariantCulture,
                "-Name \"{0}\" -ManageUrl \"{1}\" -UserName \"{2}\" -Password \"{3}\"",
                "testcreatedbfromcmdlet",
                this.manageUrl,
                this.userName,
                this.password);
            bool testResult = PSScriptExecutor.ExecuteScript(DatabaseTest.CreateScript, arguments);
            Assert.IsTrue(testResult);
        }

        [TestMethod]
        [TestCategory("Functional")]
        public void UpdateDatabase()
        {
            string arguments = string.Format(
                CultureInfo.InvariantCulture,
                "-Name \"{0}\" -ManageUrl \"{1}\" -UserName \"{2}\" -Password \"{3}\"",
                "testupdatedbfromcmdlet",
                this.manageUrl,
                this.userName,
                this.password);
            bool testResult = PSScriptExecutor.ExecuteScript(DatabaseTest.UpdateScript, arguments);
            Assert.IsTrue(testResult);
        }

        [TestMethod]
        [TestCategory("Functional")]
        public void DeleteDatabase()
        {
            string arguments = string.Format(
                CultureInfo.InvariantCulture,
                "-Name \"{0}\" -ManageUrl \"{1}\" -UserName \"{2}\" -Password \"{3}\"",
                "testDeletedbfromcmdlet",
                this.manageUrl,
                this.userName,
                this.password);
            bool testResult = PSScriptExecutor.ExecuteScript(DatabaseTest.DeleteScript, arguments);
            Assert.IsTrue(testResult);
        }

        [TestMethod]
        [TestCategory("Functional")]
        public void OutputObjectFormatValidation()
        {
            string outputFile = Path.Combine(Directory.GetCurrentDirectory(), Guid.NewGuid() + ".txt");
            string arguments = string.Format(
                CultureInfo.InvariantCulture,
                "-Name \"{0}\" -ManageUrl \"{1}\" -UserName \"{2}\" -Password \"{3}\" -OutputFile \"{4}\"",
                "testFormatdbfromcmdlet",
                this.manageUrl,
                this.userName,
                this.password,
                outputFile);
            bool testResult = PSScriptExecutor.ExecuteScript(DatabaseTest.FormatValidationScript, arguments);
            Assert.IsTrue(testResult);

            OutputFormatValidator.ValidateOutputFormat(outputFile, @"Database\ExpectedFormat.txt");
        }

        /// <summary>
        /// Runs the script to test the import and export functionality
        /// </summary>
        [TestMethod]
        [TestCategory("Functional")]
        public void ImportExportDatabase()
        {
            string outputFile = Path.Combine(Directory.GetCurrentDirectory(), Guid.NewGuid() + ".txt");

            string cmdlineArgs =
                "-UserName \"{0}\" -Password \"{1}\" -SubscriptionId \"{2}\" -SerializedCert \"{3}\" "
                + "-ContainerName \"{4}\" -StorageName \"{5}\" -StorageAccessKey \"{6}\" "
                + "-ServerLocation \"{7}\"";

            string arguments = string.Format(
                CultureInfo.InvariantCulture,
                cmdlineArgs,
                this.userName,
                this.password,
                this.subscriptionId,
                this.serializedCert,
                this.containerName,
                this.storageName,
                this.accessKey,
                this.ieServerLocation);
            bool testResult = PSScriptExecutor.ExecuteScript(DatabaseTest.ImportExportScript, arguments);
            Assert.IsTrue(testResult);
        }
    }
}
