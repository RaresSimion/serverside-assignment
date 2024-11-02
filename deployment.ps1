# # Variables
# $resourceGroupName = "serverside-assignment-rg"
# $location = "westeurope"
# $storageAccountName = "serverside-assignment-storage"
# $functionAppName = "serverside-assignment-func"
# $appInsightsName = "serverside-assignment-appinsights"

# # Login to Azure
# Write-Host "Logging in to Azure..."
# az login

# # Create Resource Group
# Write-Host "Creating resource group..."
# az group create --name $resourceGroupName --location $location

# # Deploy Bicep Template
# Write-Host "Deploying Bicep template..."
# az deployment group create --resource-group $resourceGroupName --template-file "./template.bicep" --parameters storageAccountName=$storageAccountName functionAppName=$functionAppName location=$location appInsightsName=$appInsightsName

# Write-Host "Deployment completed."

# Set variables
$resourceGroupName = "serverside-assignment-rg"
$location = "westeurope"
$templateFile = "./template.bicep"
$functionAppName = "serverside-assignment-func"
$deploymentName = "serverside-assignment-deployment"

# Log in if not already logged in
$accountInfo = az account show 2>$null
if (-not $accountInfo) {
    Write-Host "Not logged in to Azure. Please login first."
    az login
}

# Check if the resource group exists
$resourceGroup = az group show --name $resourceGroupName --output none 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "Resource group $resourceGroupName does not exist. Creating it..."
    az group create --name $resourceGroupName --location $location
} else {
    Write-Host "Resource group $resourceGroupName already exists."
}

# Deploy Bicep template
Write-Host "Deploying Bicep template..."
$deployment = az deployment group create --resource-group $resourceGroupName --template-file $templateFile --parameters location=$location --name $deploymentName

if ($LASTEXITCODE -eq 0) {
    Write-Host "Deployment succeeded."
} else {
    Write-Host "Deployment failed. Checking logs..."
    az deployment operation group list --resource-group $resourceGroupName --name $deploymentName
    exit 1
}

# Check function app status
$functionAppCheck = az functionapp show --name $functionAppName --resource-group $resourceGroupName --query "state" -o tsv
if ($functionAppCheck -ne "Running") {
    Write-Host "Function app is not running. Starting it..."
    az functionapp start --name $functionAppName --resource-group $resourceGroupName
}

# Publish the Azure Functions to the Function App
Write-Host "Publishing Azure Functions..."
func azure functionapp publish $functionAppName --build-native-deps --force

if ($LASTEXITCODE -eq 0) {
    Write-Host "Azure Functions published successfully."
} else {
    Write-Host "Error publishing Azure Functions. Check logs for details."
    exit 1
}

# Verify published Azure Functions
Write-Host "Verifying published Azure Functions..."
$functionsList = az functionapp function list --name $functionAppName --resource-group $resourceGroupName -o table

if ($functionsList) {
    Write-Host "Functions successfully published to Azure:"
    Write-Host $functionsList
} else {
    Write-Host "Error: No functions found after publishing. Check deployment logs."
    exit 1
}
