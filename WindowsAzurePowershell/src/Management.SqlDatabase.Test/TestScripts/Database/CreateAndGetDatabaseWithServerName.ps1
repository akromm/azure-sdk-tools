# ----------------------------------------------------------------------------------
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
    $Name,
    [Parameter(Mandatory=$true, Position=1)]
    [ValidateNotNullOrEmpty()]
    [string]
    $ManageUrl,
    [Parameter(Mandatory=$true, Position=2)]
    [ValidateNotNullOrEmpty()]
    [string]
    $SubscriptionID,
    [Parameter(Mandatory=$true, Position=3)]
    [ValidateNotNullOrEmpty()]
    [string]
    $SerializedCert
)

$IsTestPass = $False

Write-Output "`$Name=$Name"
Write-Output "`$ManageUrl=$ManageUrl"
Write-Output "`$SubscriptionID=$SubscriptionID"
Write-Output "`$SerializedCert=$SerializedCert"
$NameStartWith = $Name
. .\CommonFunctions.ps1


Try
{
	Init-TestEnvironment
	Init-AzureSubscription $SubscriptionId $SerializedCert "https://management.dev.mscds.com:12346/MockRDFE/"

	$server = Get-AzureSqlDatabaseServer

    Assert {$server[0]} "There are no servers to connect to"

    $ServerName = $server[0].ServerName

    $defaultCollation = "SQL_Latin1_General_CP1_CI_AS"
    $defaultEdition = "Web"
    $defaultMaxSizeGB = "1"
    $defaultIsReadOnly = $false
    $defaultIsFederationRoot = $false
    $defaultIsSystemObject = $false
    
    #############################################################
    # Create Database with only required parameters
    #############################################################
    Write-Output "Creating Database $Name ..."
    $database = New-AzureSqlDatabase -ServerName $ServerName -DatabaseName $Name
    Write-Output "Done"
    Validate-SqlDatabase -Actual $database -ExpectedName $Name -ExpectedCollationName $defaultCollation -ExpectedEdition `
            $defaultEdition -ExpectedMaxSizeGB $defaultMaxSizeGB -ExpectedIsReadOnly $defaultIsReadOnly `
            -ExpectedIsFederationRoot $defaultIsFederationRoot -ExpectedIsSystemObject $defaultIsSystemObject
    
    
    #############################################################
    #Get Database by database name
    #############################################################
    $database = Get-AzureSqlDatabase -ServerName $ServerName -DatabaseName $Name
    Validate-SqlDatabase -Actual $database -ExpectedName $Name -ExpectedCollationName $defaultCollation -ExpectedEdition `
            $defaultEdition -ExpectedMaxSizeGB $defaultMaxSizeGB -ExpectedIsReadOnly $defaultIsReadOnly `
            -ExpectedIsFederationRoot $defaultIsFederationRoot -ExpectedIsSystemObject $defaultIsSystemObject
    
    
    #############################################################
    # Create Database with all optional parameters
    #############################################################
    $Name = $Name + "1"
    Write-Output "Creating Database $Name ..."
    $database2 = New-AzureSqlDatabase -ServerName $ServerName $Name -Collation "SQL_Latin1_General_CP1_CS_AS" -Edition "Business" `
            -MaxSizeGB 20 -Force
    Write-Output "Done"
    
    Validate-SqlDatabase -Actual $database2 -ExpectedName $Name -ExpectedCollationName "SQL_Latin1_General_CP1_CS_AS" `
            -ExpectedEdition "Business" -ExpectedMaxSizeGB "20" -ExpectedIsReadOnly $defaultIsReadOnly `
            -ExpectedIsFederationRoot $defaultIsFederationRoot -ExpectedIsSystemObject $defaultIsSystemObject

            
    #############################################################
    #Get Database by database object
    #############################################################
    $database2 = Get-AzureSqlDatabase -ServerName $ServerName -Database $database2
    Validate-SqlDatabase -Actual $database2 -ExpectedName $Name -ExpectedCollationName "SQL_Latin1_General_CP1_CS_AS" `
            -ExpectedEdition "Business" -ExpectedMaxSizeGB "20" -ExpectedIsReadOnly $defaultIsReadOnly `
            -ExpectedIsFederationRoot $defaultIsFederationRoot -ExpectedIsSystemObject $defaultIsSystemObject
            
            
    #############################################################
    #Get Databases with no filter
    #############################################################
    $databases = (Get-AzureSqlDatabase -ServerName $ServerName) | Where-Object {$_.Name.StartsWith($NameStartWith)}
    $count = $databases.Count
    Assert {$count -eq 2} "Get database should have returned 2 database, but returned $count"
    
    $IsTestPass = $True
}
Finally
{
    if($database)
    {
        # Drop Database
        Drop-DatabasesWithServerName $ServerName $NameStartWith
    }
}

Write-TestResult $IsTestPass