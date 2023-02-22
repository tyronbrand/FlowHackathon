param name string
param location string = resourceGroup().location
param tags object = {}

param databaseName string = ''
param keyVaultName string

@secure()
param sqlAdminPassword string

// Because databaseName is optional in main.bicep, we make sure the database name is set here.
var defaultDatabaseName = 'Hackathon'
var actualDatabaseName = !empty(databaseName) ? databaseName : defaultDatabaseName

module pgSqlServer '../core/database/postgresql/flexibleserver.bicep' = {
  name: 'postgreserver'
  params: {
    name: name
    location: location
    tags: tags
    sku: {
      name: 'Standard_B1ms'
      tier: 'Burstable'
    }
    version: '14'
    administratorLogin: 'postgres'
    administratorLoginPassword: sqlAdminPassword
    databaseName: actualDatabaseName
    storage: {
      storageSizeGB: 32
    }
    keyVaultName: keyVaultName
  }
}

output connectionStringKey string = pgSqlServer.outputs.connectionStringKey
output databaseName string = pgSqlServer.outputs.databaseName
