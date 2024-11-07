param location string = resourceGroup().location
param storageAccountName string = 'ws${uniqueString(resourceGroup().id)}'

// Create the storage account resource and enable public access
resource storageAccount 'Microsoft.Storage/storageAccounts@2022-09-01' = {
  name: storageAccountName
  location: location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
  properties: {
    allowBlobPublicAccess: true  // Enable public access for blob containers
  }
}

// Retrieve the connection string dynamically after the storage account is created
var storageAccountKeys = listKeys(storageAccount.id, '2022-09-01')
var storageAccountConnectionString = 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${storageAccountKeys.keys[0].value};EndpointSuffix=core.windows.net'

// Define the App Service Plan (ServerFarm) - for Consumption plan
resource appServicePlan 'Microsoft.Web/serverfarms@2022-03-01' = {
  name: 'server-side-assignment-plan'
  location: location
  sku: {
    tier: 'Dynamic'
    name: 'Y1'
  }
}

// Define the Function App and use the storage connection string dynamically
resource functionApp 'Microsoft.Web/sites@2022-03-01' = {
  name: 'server-side-assignment'
  location: location
  kind: 'functionapp'
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: {
      appSettings: [
        {
          name: 'AzureWebJobsStorage'
          value: storageAccountConnectionString
        },{
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: 'dotnet-isolated'
        },{
          name: 'WEBSITE_RUN_FROM_PACKAGE'
          value: '1'
        }
      ]
    }
  }
  dependsOn: [
    appServicePlan
    storageAccount
  ]
}

// Define the storage queues as child resources of the storage account
resource queueServices 'Microsoft.Storage/storageAccounts/queueServices@2022-09-01' = {
  name: 'default'
  parent: storageAccount
}

resource queue1 'Microsoft.Storage/storageAccounts/queueServices/queues@2022-09-01' = {
  name: 'weather-queue'
  parent: queueServices
  dependsOn: [queueServices]
}

// Define the blob services for the storage account
resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2022-09-01' = {
  name: 'default'
  parent: storageAccount
}

// Define the blob container with public access
resource blobContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2022-09-01' = {
  name: 'weather-images'
  parent: blobService
  properties: {
    publicAccess: 'Blob'
  }
  dependsOn: [
    blobService
  ]
}

// Output the function app name and storage account blob URI
output functionAppName string = functionApp.name
output storageAccountUri string = storageAccount.properties.primaryEndpoints.blob
