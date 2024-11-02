// @description('The name of the storage account')
// param storageAccountName string

// @description('The name of the function app')
// param functionAppName string

// @description('The location of the resources')
// param location string = resourceGroup().location

// @description('The name of the Application Insights instance')
// param appInsightsName string

// @description('The SKU of the App Service plan')
// param sku string = 'Y1'

// resource storageAccount 'Microsoft.Storage/storageAccounts@2022-09-01' = {
//   name: storageAccountName
//   location: location
//   sku: {
//     name: 'Standard_LRS'
//   }
//   kind: 'StorageV2'
//   properties: {
//     accessTier: 'Hot'
//   }
// }

// resource appServicePlan 'Microsoft.Web/serverfarms@2022-03-01' = {
//   name: '${functionAppName}-plan'
//   location: location
//   sku: {
//     name: sku
//     tier: 'Dynamic'
//   }
//   properties: {
//     reserved: true
//   }
// }

// resource functionApp 'Microsoft.Web/sites@2022-03-01' = {
//   name: functionAppName
//   location: location
//   kind: 'functionapp'
//   identity: {
//     type: 'SystemAssigned'
//   }
//   properties: {
//     serverFarmId: appServicePlan.id
//     siteConfig: {
//       appSettings: [
//         {
//           name: 'AzureWebJobsStorage'
//           value: storageAccount.properties.primaryEndpoints.blob
//         }
//         {
//           name: 'FUNCTIONS_EXTENSION_VERSION'
//           value: '~4'
//         }
//         {
//           name: 'FUNCTIONS_WORKER_RUNTIME'
//           value: 'dotnet-isolated'
//         }
//         {
//           name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
//           value: appInsights.properties.InstrumentationKey
//         }
//       ]
//     }
//   }
// }

// resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
//   name: appInsightsName
//   location: location
//   kind: 'web'
//   properties: {
//     Application_Type: 'web'
//   }
// }

// output storageAccountConnectionString string = storageAccount.properties.primaryEndpoints.blob
// output functionAppName string = functionApp.name
// output appInsightsInstrumentationKey string = appInsights.properties.InstrumentationKey


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
  name: 'assignment-plan'
  location: location
  sku: {
    tier: 'Dynamic'
    name: 'Y1'
  }
}

// Define the Function App and use the storage connection string dynamically
resource functionApp 'Microsoft.Web/sites@2022-03-01' = {
  name: 'weather-image-function-app'
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
// resource queue2 'Microsoft.Storage/storageAccounts/queueServices/queues@2022-09-01' = {
//   name: 'image-processing-queue'
//   parent: queueServices
//   dependsOn: [queueServices]
// }

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
