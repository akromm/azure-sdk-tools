﻿// ----------------------------------------------------------------------------------
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

namespace Microsoft.WindowsAzure.Management.SqlDatabase.Test.UnitTests.Database.Cmdlet
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Management.SqlDatabase.Database.Cmdlet;
    using Microsoft.WindowsAzure.Management.SqlDatabase.Services;
    using Microsoft.WindowsAzure.Management.SqlDatabase.Services.ImportExport;
    using Microsoft.WindowsAzure.Management.Test.Utilities.Common;
    using Microsoft.WindowsAzure.Management.Utilities.Common;

    /// <summary>
    /// Test class for testing the Get-AzureSqlDatabaseImportExportStatus cmdlet
    /// </summary>
    [TestClass]
    public class GetAzureSqlDatabaseImportExportStatusTests : TestBase
    {
        /// <summary>
        /// Initializes the test environment
        /// </summary>
        [TestInitialize]
        public void SetupTest()
        {
            CmdletSubscriptionExtensions.SessionManager = new InMemorySessionManager();
        }

        /// <summary>
        /// Tests the ExportAzureSqlDatabaseProcess function 
        /// </summary>
        [TestMethod]
        public void GetAzureSqlDatabaseImportExportStatusProcessTest()
        {
            string serverName = "TestServer";
            string userName = "testUser";
            string password = "testPassword";
            string requestId = "00000000-0000-0000-0000-000000000000";
            string blobUri = "test.dummy.blob/container/blob.bacpac";
            string databaseName = "dummyDB";
            DateTime lastModified = DateTime.UtcNow;
            DateTime queuedTime = new DateTime(1, 2, 3, 4, 5, 6);
            string requestType = "Export";
            string ieRequestStatus = "Complete";

            MockCommandRuntime commandRuntime = new MockCommandRuntime();
            SimpleSqlDatabaseManagement channel = new SimpleSqlDatabaseManagement();
            channel.GetImportExporStatusThunk = ar =>
            {
                Assert.AreEqual(serverName, (string)ar.Values["serverName"]);
                Assert.AreEqual(userName, (string)ar.Values["userName"]);
                Assert.AreEqual(password, (string)ar.Values["password"]);
                Assert.AreEqual(requestId, (string)ar.Values["requestId"]);

                StatusInfo status = new StatusInfo();
                status.BlobUri = blobUri;
                status.DatabaseName = databaseName;
                status.LastModifiedTime = lastModified;
                status.QueuedTime = queuedTime;
                status.RequestId = requestId;
                status.RequestType = requestType;
                status.ServerName = serverName;
                status.Status = ieRequestStatus;

                ArrayOfStatusInfo operationResult = new ArrayOfStatusInfo();
                operationResult.Add(status);

                return operationResult;
            };

            
            GetAzureSqlDatabaseImportExportStatus getImportExportStatus = 
                new GetAzureSqlDatabaseImportExportStatus(channel) { ShareChannel = true };
            getImportExportStatus.CurrentSubscription = UnitTestHelper.CreateUnitTestSubscription();
            getImportExportStatus.CommandRuntime = commandRuntime;
            var result = getImportExportStatus.GetAzureSqlDatabaseImportExportStatusProcess(
                serverName,
                userName, 
                password, 
                requestId);

            Assert.AreEqual(blobUri, result[0].RequestId);
            Assert.AreEqual(databaseName, result[0].RequestId);
            Assert.AreEqual("00000000-0000-0000-0000-000000000000", result[0].RequestId);
            Assert.AreEqual("00000000-0000-0000-0000-000000000000", result[0].RequestId);
            Assert.AreEqual("00000000-0000-0000-0000-000000000000", result[0].RequestId);
            Assert.AreEqual("00000000-0000-0000-0000-000000000000", result[0].RequestId);
            Assert.AreEqual("00000000-0000-0000-0000-000000000000", result[0].RequestId);
            Assert.AreEqual("00000000-0000-0000-0000-000000000000", result[0].RequestId);

            Assert.AreEqual(0, commandRuntime.ErrorStream.Count);
        }
    }
}
