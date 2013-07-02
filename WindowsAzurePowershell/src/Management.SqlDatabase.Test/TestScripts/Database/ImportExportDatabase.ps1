﻿# ----------------------------------------------------------------------------------
#
# Copyright Microsoft Corporation
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
# http://www.apache.org/licenses/LICENSE-2.0
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.
# ----------------------------------------------------------------------------------

[CmdletBinding()]
Param
(
    [Parameter(Mandatory=$true, Position=0)]
    [ValidateNotNullOrEmpty()]
    [string]
    $UserName,
    [Parameter(Mandatory=$true, Position=1)]
    [ValidateNotNullOrEmpty()]
    [string]
    $Password,
    [Parameter(Mandatory=$true, Position=2)]
    [ValidateNotNullOrEmpty()]
    [string]
    $SubscriptionId,
    [Parameter(Mandatory=$true, Position=3)]
    [ValidateNotNullOrEmpty()]
    [string]
    $SerializedCert,
    [Parameter(Mandatory=$true, Position=4)]
    [ValidateNotNullOrEmpty()]
    [Uri]
    $ContainerName,
    [Parameter(Mandatory=$true, Position=4)]
    [ValidateNotNullOrEmpty()]
    [Uri]
    $StorageName,
    [Parameter(Mandatory=$true, Position=5)]
    [ValidateNotNullOrEmpty()]
    [string]
    $StorageAccessKey,
    [Parameter(Mandatory=$true, Position=6)]
    [ValidateNotNullOrEmpty()]
    [String]
    $ServerLocation
)

$IsTestPass = $False

Write-Output "`$UserName=$UserName"
Write-Output "`$Password=$Password"
Write-Output "`$SubscriptionId=$SubscriptionId"
Write-Output "`$SerializedCert=$SerializedCert"
Write-Output "`$ContainerName=$ContainerName"
Write-Output "`$StorageAccessKey=$StorageAccessKey"
Write-Output "`$ServerLocation=$ServerLocation"
    
    
. .\CommonFunctions.ps1
. .\Database\ExportTests.ps1
. .\Database\ImportTests.ps1

$ManageUrlPrefix = "https://"
$ManageUrlPostfix = ".database.windows.net/"
$DatabaseNamePrefix = "testIEDatabase"

Try
{
    ####################################################
    # Set up test
	Init-TestEnvironment
    Init-AzureSubscription -SubscriptionID $SubscriptionId -SerializedCert $SerializedCert
    
    ##########
    # create a server to use
    Write-Output "Creating server"
    $server = New-AzureSqlDatabaseServer -AdministratorLogin $UserName -AdministratorLoginPassword `
        $Password -Location $ServerLocation
    Assert {$server} "Failed to create a server"
    Write-Output "Server $($server.ServerName) created"
    
    ##########
	# set the firewall rules
	New-AzureSqlDatabaseServerFirewallRule -ServerName $server.ServerName -RuleName "AllowAll" `
		-StartIpAddress "0.0.0.0" -EndIpAddress "255.255.255.255"
    
    ##########
    # create a context to connect to the server.
    $ManageUrl = $ManageUrlPrefix + $server.ServerName + $ManageUrlPostfix
    $context = Get-ServerContextByManageUrlWithSqlAuth -ManageUrl $ManageUrl -UserName $UserName `
        -Password $Password
		
    ##########
    # Create a couple databases
    $DatabaseName1 = $DatabaseNamePrefix + (get-date).Ticks
    
    Write-Output "Creating Database $DatabaseName1 ..."
    $database = New-AzureSqlDatabase -Context $context -DatabaseName $DatabaseName1
    Assert {$database} "Failed to create a database"
    Write-Output "Done"

    $DatabaseName2 = $DatabaseNamePrefix + "2" + (get-date).Ticks
    
    Write-Output "Creating Database $DatabaseName2 ..."
    $database2 = New-AzureSqlDatabase -Context $context -DatabaseName $DatabaseName2
    Assert {$database2} "Failed to create a database"
    Write-Output "Done"

    ##########
    # Create the storage connection context.
	$StgCtx = New-AzureStorageContext -StorageAccountName $StorageName -StorageAccountKey $StorageAccessKey
	$container = Get-AzureStorageContainer -Name $ContainerName -Context $StgCtx
    
    ####################################################
    # Test export and get-export status
    try
    {
        TestExportWithRequestObject

        TestExportWithRequestId
    }
    finally
    {
        Drop-Databases $context $DatabaseNamePrefix
    }

    ####################################################
    # test import and get-import status
    try
    {
        TestImportWithRequestObject

        TestImportWithRequestObjectAndOptionalParameters

        TestImportWithRequestId
    }
    finally
    {
        Drop-Databases $context $DatabaseNamePrefix
    }

    $IsTestPass = $True
}
Finally
{
	if($server)
	{
		Drop-Server $server
	}
		
	if($StgCtx)
	{
		if($BlobName)
		{
			Remove-AzureStorageBlob -Container $ContainerName -Blob $BlobName -Context $StgCtx
		}
		if($BlobName2)
		{
			Remove-AzureStorageBlob -Container $ContainerName -Blob $BlobName2 -Context $StgCtx
		}
	}
}

Write-TestResult $IsTestPass
