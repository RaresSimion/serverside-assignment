// get the location of the resource group
param location string = resourceGroup().location

// get the name of the storage account
param storageAccountName string = 'ws${uniqueString(resourceGroup().id)}'

// create storage account
resource storageAccount 'Microsoft.Storage/storageAccounts@2022-09-01' = {
  name: storageAccountName
  location: location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
}

// get the storage account keys
var storageAccountKeys = listKeys(storageAccount.id, '2022-09-01')
var storageAccountConnectionString = 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${storageAccountKeys.keys[0].value};EndpointSuffix=core.windows.net'

// create the app service plan
resource appServicePlan 'Microsoft.Web/serverfarms@2022-03-01' = {
  name: 'server-side-assignment-plan'
  location: location
  sku: {
    tier: 'Dynamic'
    name: 'Y1'
  }
}

// create the function app
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
}

// define the queue services for the storage account
resource queueServices 'Microsoft.Storage/storageAccounts/queueServices@2022-09-01' = {
  name: 'default'
  parent: storageAccount
}

// define the queue
resource queue1 'Microsoft.Storage/storageAccounts/queueServices/queues@2022-09-01' = {
  name: 'weather-queue'
  parent: queueServices
}

// define the blob service for the storage account
resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2022-09-01' = {
  name: 'default'
  parent: storageAccount
}

// define the blob container
resource blobContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2022-09-01' = {
  name: 'weather-images'
  parent: blobService
  properties: {
    publicAccess: 'Blob'
  }
}
