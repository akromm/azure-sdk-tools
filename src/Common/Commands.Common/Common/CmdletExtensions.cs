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

namespace Microsoft.WindowsAzure.Commands.Utilities.Common
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Services.Client;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Management.Automation;
    using System.Runtime.Serialization;
    using System.Xml;
    using System.Xml.Linq;

    public static class CmdletExtensions
    {
        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        public static void WriteVerboseOutputForObject(this PSCmdlet powerShellCmdlet, object obj)
        {
            bool verbose = powerShellCmdlet.MyInvocation.BoundParameters.ContainsKey("Verbose") && ((SwitchParameter)powerShellCmdlet.MyInvocation.BoundParameters["Verbose"]).ToBool();
            if (verbose == false)
            {
                return;
            }

            string deserializedobj;
            var serializer = new DataContractSerializer(obj.GetType());

            using (var backing = new StringWriter())
            {
                using (var writer = new XmlTextWriter(backing))
                {
                    writer.Formatting = Formatting.Indented;

                    serializer.WriteObject(writer, obj);
                    deserializedobj = backing.ToString();
                }
            }

            deserializedobj = deserializedobj.Replace("/d2p1:", string.Empty);
            deserializedobj = deserializedobj.Replace("d2p1:", string.Empty);
            powerShellCmdlet.WriteVerbose(powerShellCmdlet.CommandRuntime.ToString());
            powerShellCmdlet.WriteVerbose(deserializedobj);
        }

        public static string TryResolvePath(this PSCmdlet psCmdlet, string path)
        {
            try
            {
                return psCmdlet.ResolvePath(path);
            }
            catch
            {
                return path;
            }
        }

        public static string ResolvePath(this PSCmdlet psCmdlet, string path)
        {
            if (path == null)
            {
                return null;
            }

            if (psCmdlet.SessionState == null)
            {
                return path;
            }

            path = path.Trim('"', '\'', ' ');
            var result = psCmdlet.SessionState.Path.GetResolvedPSPathFromPSPath(path);
            string fullPath = string.Empty;

            if (result != null && result.Count > 0)
            {
                fullPath = result[0].Path;
            }

            return fullPath;
        }

        public static Exception ProcessExceptionDetails(this PSCmdlet cmdlet, Exception exception)
        {
            if ((exception is DataServiceQueryException) && (exception.InnerException != null))
            {
                var dscException = FindDataServiceClientException(exception.InnerException);

                if (dscException == null)
                {
                    return new InnerDataServiceException(exception.InnerException.Message);
                }

                var message = dscException.Message;
                try
                {
                    XNamespace ns = "http://schemas.microsoft.com/ado/2007/08/dataservices/" +
                                    "metadata";
                    XDocument doc = XDocument.Parse(message);
                    if (doc.Root != null)
                    {
                        return new InnerDataServiceException(doc.Root.Element(ns + "message").Value);
                    }
                }
                catch
                {
                    return new InnerDataServiceException(message);
                }
            }

            return exception;
        }

        private static Exception FindDataServiceClientException(Exception ex)
        {
            if (ex is DataServiceClientException)
            {
                return ex;
            }

            return ex.InnerException != null ? FindDataServiceClientException(ex.InnerException) : null;
        }

        public static void ExecuteScript(this PSCmdlet cmdlet, string contents)
        {
            ExecuteScript<object>(cmdlet, contents);
        }

        public static void ExecuteScriptFile(this PSCmdlet cmdlet, string absolutePath)
        {
            ExecuteScriptFile<object>(cmdlet, absolutePath);
        }

        public static List<T> ExecuteScript<T>(this PSCmdlet cmdlet, string contents)
        {
            List<T> output = new List<T>();

            using (PowerShell powershell = PowerShell.Create(RunspaceMode.CurrentRunspace))
            {
                powershell.AddScript(contents);
                Collection<T> result = powershell.Invoke<T>();

                if (cmdlet.SessionState != null)
                {
                    powershell.Streams.Error.ForEach(e => cmdlet.WriteError(e));
                    powershell.Streams.Verbose.ForEach(r => cmdlet.WriteVerbose(r.Message));
                    powershell.Streams.Warning.ForEach(r => cmdlet.WriteWarning(r.Message));
                }

                if (result != null && result.Count > 0)
                {
                    output.AddRange(result);
                }
            }

            return output;
        }

        public static List<T> ExecuteScriptFile<T>(this PSCmdlet cmdlet, string absolutePath)
        {
            string contents = File.ReadAllText(absolutePath);
            return ExecuteScript<T>(cmdlet, contents);
        }

        #region PowerShell Commands

        public static void RemoveModule(this PSCmdlet cmdlet, string moduleName)
        {
            string contents = string.Format("Remove-Module {0}", moduleName);
            ExecuteScript<object>(cmdlet, contents);
        }

        public static List<PSModuleInfo> GetLoadedModules(this PSCmdlet cmdlet)
        {
            return ExecuteScript<PSModuleInfo>(cmdlet, "Get-Module");
        }

        public static void ImportModule(this PSCmdlet cmdlet, string modulePath)
        {
            string contents = string.Format("Import-Module '{0}'", modulePath);
            ExecuteScript<object>(cmdlet, contents);
        }

        public static void RemoveAzureAliases(this PSCmdlet cmdlet)
        {
            string contents = "Get-Alias | where { $_.Description -eq 'AzureAlias' } | foreach { Remove-Item alias:\\$($_.Name) }";
            ExecuteScript<object>(cmdlet, contents);
        }

        #endregion
    }
}
