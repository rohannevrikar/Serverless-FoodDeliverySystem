[CmdletBinding()]
Param(

    [string] 
    [Parameter(Mandatory=$true)]
    $ResourceGroupName,

    [string] 
    $ResourceLocation = "centralindia",

    [string] 
    [Parameter(Mandatory=$true)]
    $TemplateFile

)

Function Set-ResourceGroup() {
   
    param(

        [Parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [string]$ResourceGroupName,

        [Parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [string]$ResourceLocation        
    
    )
    
    $ResourceGroup = az group show --name $ResourceGroupName
    if (-not($ResourceGroup)) {
        Write-Verbose "Creating resource group $ResourceGroupName"
        $ResourceGroup = az group create --name $ResourceGroupName -l $ResourceLocation
    }
    else {
        Write-Verbose "Resource group $ResourceGroupName found"
    }
    return $ResourceGroup
}

Set-ResourceGroup -ResourceGroupName $ResourceGroupName -ResourceLocation $ResourceLocation
az deployment group create --resource-group $ResourceGroupName --template-file $TemplateFile


$functionApps = (Get-ChildItem ../Solution -Filter "ServerlessFoodDelivery.FunctionApp.*" -Recurse -Directory).Fullname

foreach($functionApp in $functionApps){

    $projectName = Split-Path $functionApp -Leaf

    dotnet publish "$functionApp/$projectName.csproj" -c Release

    $publishFolder = "$functionApp/bin/Release/netcoreapp3.1/publish"

    New-Item -ItemType Directory -Force -Path "$PSScriptRoot/DeploymentZips"

    $publishZip = "DeploymentZips/$projectName.zip"

    if(Test-Path $publishZip) {Remove-item $publishZip}
    
    Add-Type -assembly "system.io.compression.filesystem"

    [io.compression.zipfile]::CreateFromDirectory($publishFolder, $publishZip)

    $functionAppName = ""
    if($projectName -eq "ServerlessFoodDelivery.FunctionApp.Mock") {$functionAppName = "fds-mock"}
    if($projectName -eq "ServerlessFoodDelivery.FunctionApp.Orchestrators") {$functionAppName = "fds-orchestrators"}
    if($projectName -eq "ServerlessFoodDelivery.FunctionApp.Orders") {$functionAppName = "fds-orders"}
    if($projectName -eq "ServerlessFoodDelivery.FunctionApp.Restaurants") {$functionAppName = "fds-restaurants"}

    az functionapp deployment source config-zip -g $ResourceGroupName -n $functionAppName --src $publishZip
   
}

