# set variables
$resourceGroupName = "serverside-assignment-rg"
$location = "westeurope"
$templateFile = "./template.bicep"
$functionAppName = "server-side-assignment"
$deploymentName = "serverside-assignment-deployment"

# check if user is logged in to Azure
$accountInfo = az account show 2>$null
if (-not $accountInfo) {
    Write-Host "Login to Azure first..."
    az login
}

# spin up a new resource group if it doesn't exist
$resourceGroup = az group show --name $resourceGroupName --output none 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "Resource group $resourceGroupName does not exist. Creating it..."
    az group create --name $resourceGroupName --location $location
} else {
    Write-Host "Resource group $resourceGroupName already exists."
}

# deploy the Bicep template
Write-Host "Deploying Bicep template..."
$deployment = az deployment group create --resource-group $resourceGroupName --template-file $templateFile --parameters location=$location --name $deploymentName

if ($LASTEXITCODE -eq 0) {
    Write-Host "Deployment succeeded."
} else {
    Write-Host "Deployment failed. Checking logs..."
    az deployment operation group list --resource-group $resourceGroupName --name $deploymentName
    exit 1
}

# check if the function app is running
$functionAppCheck = az functionapp show --name $functionAppName --resource-group $resourceGroupName --query "state" -o tsv
if ($functionAppCheck -ne "Running") {
    Write-Host "Starting function app..."
    az functionapp start --name $functionAppName --resource-group $resourceGroupName
}

# publish the Azure Functions
Write-Host "Publishing Azure Functions..."
func azure functionapp publish $functionAppName --build-native-deps --force

if ($LASTEXITCODE -eq 0) {
    Write-Host "Azure functions published successfully."
} else {
    Write-Host "Error publishing Azure functions."
    exit 1
}

# verify the functions
Write-Host "Verifying published Azure Functions..."
$functionsList = az functionapp function list --name $functionAppName --resource-group $resourceGroupName -o table

if ($functionsList) {
    Write-Host "Functions successfully published to Azure:"
    Write-Host $functionsList
} else {
    Write-Host "Error verifying functions."
    exit 1
}
