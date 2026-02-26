# Fake Intra — Infrastructure as Code Specification

## 1. Overview

This document specifies the infrastructure as code (IaC) design for deploying **Fake Intra** to Azure using [Bicep](https://learn.microsoft.com/en-us/azure/azure-resource-manager/bicep/overview) and [Azure Verified Modules (AVM)](https://azure.github.io/Azure-Verified-Modules/). The infrastructure covers three isolated environments — **Dev**, **Test**, and **Prod** — each deployed into its own Azure resource group.

### 1.1 Deployed Components

| Component | Azure Service | AVM Module |
|-----------|--------------|------------|
| Environment container | Azure Resource Group | `br/public:avm/res/resources/resource-group` |
| React SPA (Frontend) | Azure App Service | `br/public:avm/res/web/site` |
| ASP.NET API (Backend) | Azure App Service | `br/public:avm/res/web/site` |
| App Service hosting plan | Azure App Service Plan | `br/public:avm/res/web/serverfarm` |
| Relational database | Azure SQL Database | `br/public:avm/res/sql/server` |
| Application monitoring | Application Insights | `br/public:avm/res/insights/component` |
| Telemetry workspace | Log Analytics Workspace | `br/public:avm/res/operational-insights/workspace` |
| Secret store | Azure Key Vault | `br/public:avm/res/key-vault/vault` |

### 1.2 Environments

| Environment | Purpose | SKU tier |
|-------------|---------|----------|
| `dev` | Developer integration, feature branches | Free / Basic |
| `test` | QA, acceptance testing, staging | Standard |
| `prod` | Live production traffic | Standard / Premium |

---

## 2. Repository Layout

All IaC files live under `infra/` at the repository root:

```
infra/
├── main.bicep                   # Root orchestration template
├── modules/
│   └── webapp.bicep             # Shared module: App Service Plan + two App 
├── environments/
│   ├── dev.bicepparam           # Dev parameter file
│   ├── test.bicepparam          # Test parameter file
│   └── prod.bicepparam         # Prod parameter file
└── README.md                    # Deployment instructions
```

---

## 3. Naming Conventions

All resource names follow the [Microsoft Cloud Adoption Framework (CAF) abbreviations](https://learn.microsoft.com/en-us/azure/cloud-adoption-framework/ready/azure-best-practices/resource-abbreviations).

| Resource | Name pattern | Example (dev) |
|----------|-------------|---------------|
| Resource group | `rg-ipsagent-{env}` | `rg-ipsagent-dev` |
| App Service Plan | `asp-ipsagent-{env}` | `asp-ipsagent-dev` |
| App Service — API | `app-ipsagent-api-{env}` | `app-ipsagent-api-dev` |
| App Service — Web | `app-ipsagent-app-{env}` | `app-ipsagent-app-dev` |
| SQL Server | `sql-ipsagent-{env}` | `sql-ipsagent-dev` |
| SQL Database | `sqldb-ipsagent-{env}` | `sqldb-ipsagent-dev` |
| Application Insights | `appi-ipsagent-{env}` | `appi-ipsagent-dev` |
| Log Analytics Workspace | `log-ipsagent-{env}` | `log-ipsagent-dev` |
| Key Vault | `kv-ipsagent-{env}` | `kv-ipsagent-dev` |

---

## 4. Resource Architecture

```
Resource Group: rg-ipsagent-{env}
│
├── log-ipsagent-{env}           (Log Analytics Workspace)
│   └── appi-ipsagent-{env}     (Application Insights — linked to workspace)
│
├── kv-ipsagent-{env}            (Key Vault — secrets referenced by App Services)
│
├── asp-ipsagent-{env}           (App Service Plan — shared by both apps)
│   ├── app-ipsagent-api-{env}  (API App Service — ASP.NET 10, system-assigned MI)
│   └── app-ipsagent-app-{env}  (Web App Service — Node 22 / static files, system-assigned MI)
│
└── sql-ipsagent-{env}           (Azure SQL Logical Server — SQL authentication)
    └── sqldb-ipsagent-{env}    (Azure SQL Database)
```

Both App Service managed identities are granted:
- `Key Vault Secrets User` role on the Key Vault (to read Key Vault references).
---

## 5. Bicep File Specifications

### 5.1 `main.bicep`

Root template. Targets the **subscription** scope (`targetScope = 'subscription'`). Creates the resource group first, then deploys all environment resources into it via a nested module scoped to that resource group.

**Parameters**

| Parameter | Type | Description |
|-----------|------|-------------|
| `env` | `string` | Environment tag: `dev`, `test`, or `prod` |
| `location` | `string` | Azure region for all resources |
| `appServicePlanSku` | `object` | SKU object for the App Service Plan |
| `sqlDatabaseSku` | `object` | SKU object for the SQL Database (unused in dev — Free offer used instead) |
| `sqlUseFreeLimit` | `bool` | Enable the Azure SQL Free serverless offer (dev only) |
| `sqlAllowPublicAccess` | `bool` | Allow public internet access to the SQL Server (`true` in dev, `false` in test/prod) |
| `sqlAdminLogin` | `string` | SQL Server administrator login name |
| `sqlAdminPassword` | `securestring` | SQL Server administrator password |
| `entraIdTenantId` | `string` | Entra ID Tenant ID for app authentication config |
| `entraIdClientId` | `string` | Entra ID Client ID for the API |
| `tags` | `object` | Resource tags applied to all resources |

> `sqlAdminLogin` and `sqlAdminPassword` must **never** be stored in parameter files. They are passed at deploy time via GitHub Actions secrets and written to Key Vault so the API App Service can retrieve the connection string securely at runtime.

**Resources declared (via AVM)**

1. Resource Group — `br/public:avm/res/resources/resource-group` *(subscription scope)*
2. Log Analytics Workspace — `br/public:avm/res/operational-insights/workspace`
3. Application Insights — `br/public:avm/res/insights/component`
4. Key Vault — `br/public:avm/res/key-vault/vault`
5. App Service Plan — `br/public:avm/res/web/serverfarm`
6. API App Service — `br/public:avm/res/web/site` (system-assigned managed identity enabled)
7. Web App Service — `br/public:avm/res/web/site` (system-assigned managed identity enabled)
8. Azure SQL Server — `br/public:avm/res/sql/server`
   - SQL Database as a nested child resource
9. Key Vault secrets (declared as `Microsoft.KeyVault/vaults/secrets` child resources):
   - `appi-connection-string` — Application Insights connection string (from App Insights module output)
   - `sql-connection-string` — ADO.NET connection string (constructed from `sqlAdminLogin`, `sqlAdminPassword`, SQL server FQDN)
   - `auth-tenant-id` — Entra ID Tenant ID (from `entraIdTenantId` parameter)
   - `auth-client-id` — Entra ID Client ID (from `entraIdClientId` parameter)

Resources 2–9 are declared inside a `module` block with `scope: resourceGroup(resourceGroupName)`, ensuring the resource group exists before any child resources are deployed.

**Role assignments (declared inline in Bicep)**

| Principal | Role | Scope |
|-----------|------|-------|
| API App Service MI | `Key Vault Secrets User` | Key Vault |
| Web App Service MI | `Key Vault Secrets User` | Key Vault |

---

### 5.2 `modules/webapp.bicep`

Optional shared module if the App Service Plan + site pattern is reused. Accepts the plan SKU and site-specific settings (name, runtime, app settings) and outputs the site hostname and principal ID (for RBAC assignments).

---

### 5.3 Environment Parameter Files

Each `.bicepparam` file lives under `infra/environments/` and uses `using '../main.bicep'`.

#### `infra/environments/dev.bicepparam`

```bicep
using '../main.bicep'

param env = 'dev'
param location = 'swedencentral'

param appServicePlanSku = {
  name: 'B1'
  tier: 'Basic'
}

// Dev uses the Azure SQL Free serverless offer — sqlDatabaseSku is ignored
param sqlUseFreeLimit = true

// Dev allows public internet access to SQL Server for local development
param sqlAllowPublicAccess = true

param tags = {
  environment: 'dev'
  application: 'ipsagent'
  managedBy: 'bicep'
}
```

#### `infra/environments/test.bicepparam`

```bicep
using '../main.bicep'

param env = 'test'
param location = 'swedencentral'

param appServicePlanSku = {
  name: 'S1'
  tier: 'Standard'
}

param sqlUseFreeLimit = false

param sqlAllowPublicAccess = false

param sqlDatabaseSku = {
  name: 'S1'
  tier: 'Standard'
  capacity: 20
}

param tags = {
  environment: 'test'
  application: 'ipsagent'
  managedBy: 'bicep'
}
```

#### `infra/environments/prod.bicepparam`

```bicep
using '../main.bicep'

param env = 'prod'
param location = 'swedencentral'

param appServicePlanSku = {
  name: 'P1v3'
  tier: 'PremiumV3'
}

param sqlUseFreeLimit = false

param sqlAllowPublicAccess = false

param sqlDatabaseSku = {
  name: 'S1'
  tier: 'Standard'
  capacity: 20
}

param tags = {
  environment: 'prod'
  application: 'ipsagent'
  managedBy: 'bicep'
}
```

> **Note:** Sensitive parameters (`sqlAdminLogin`, `sqlAdminPassword`, `entraIdTenantId`, `entraIdClientId`) must **not** be stored in parameter files. Provide them at deploy time via GitHub Actions secrets.

---

## 6. App Service Configuration

Both App Services have **system-assigned managed identity** enabled. This identity is used to authenticate to Key Vault (via Key Vault references).

### 6.1 API App Service (`app-ipsagent-api-{env}`)

| Setting | Value |
|---------|-------|
| Runtime stack | `DOTNETCORE\|10.0` |
| OS | Linux |
| HTTPS only | `true` |
| Always On | `true` (dev, test, prod) |
| Health check path | `/health` |
| Managed identity | System-assigned |

**App Settings injected by Bicep**

Secrets are stored in Key Vault and surfaced to the app via [Key Vault references](https://learn.microsoft.com/en-us/azure/app-service/app-service-key-vault-references) using the syntax `@Microsoft.KeyVault(VaultName=kv-ipsagent-{env};SecretName=<name>)`.

| Key | Value source |
|-----|-------------|
| `APPLICATIONINSIGHTS_CONNECTION_STRING` | Key Vault reference → secret `appi-connection-string` |
| `AzureAd__TenantId` | Key Vault reference → secret `auth-tenant-id` |
| `AzureAd__ClientId` | Key Vault reference → secret `auth-client-id` |
| `ConnectionStrings__DefaultConnection` | Key Vault reference → secret `sql-connection-string` |

### 6.2 Web App Service (`app-ipsagent-app-{env}`)

The React SPA is built as static files and served by the App Service using a Node.js runtime or a custom startup command.

| Setting | Value |
|---------|-------|
| Runtime stack | `NODE\|22-lts` |
| OS | Linux |
| HTTPS only | `true` |
| Startup command | `npx serve -s dist -l 8080` |
| Managed identity | System-assigned |

**App Settings injected by Bicep**

| Key | Value source |
|-----|-------------|
| `WEBSITE_NODE_DEFAULT_VERSION` | `~22` |

---

## 7. Azure SQL Configuration

### 7.1 SQL Server (`sql-ipsagent-{env}`)

- **Authentication:** SQL authentication enabled. The admin login and password are passed to Bicep as `sqlAdminLogin` / `sqlAdminPassword`, stored in Key Vault as secret `sql-connection-string`, and referenced by the API App Service at runtime.
- **Firewall:** Environment-dependent:
  - **dev:** Public network access enabled (direct internet access allowed for local development convenience).
  - **test / prod:** Public network access disabled; only Azure-internal traffic allowed (`Allow Azure services` rule enabled).
- **TLS:** Minimum TLS version `1.2`.

### 7.2 SQL Database (`sqldb-ipsagent-{env}`)

| Environment | SKU / Offer | Max size | Notes |
|-------------|------------|----------|-------|
| dev | **Free serverless** (GP_S_Gen5_1) | 32 GB | `useFreeLimit: true`, `freeLimitExhaustionBehavior: AutoPause` |
| test | Standard S1 (20 DTU) | 250 GB | |
| prod | Standard S1 (20 DTU) | 250 GB | |

- **Collation:** `SQL_Latin1_General_CP1_CI_AS`
- **Zone redundancy:** disabled (dev, test, prod)
- **Backup storage redundancy:** Local (dev, test), Geo (prod)

---

## 8. Application Insights Configuration

- **Workspace-based:** All Application Insights instances are linked to their environment's Log Analytics Workspace.
- **Sampling rate:** 100% (dev, test), configurable (prod — start at 100%, tune down under load).
- **Retention:** 30 days (dev), 90 days (test), 180 days (prod).
- The Application Insights connection string is written to Key Vault as secret `appi-connection-string` by the Bicep template immediately after the `insights/component` module completes. Both App Services read it via a Key Vault reference.
- The .NET SDK auto-detects `APPLICATIONINSIGHTS_CONNECTION_STRING` without code changes.

---

## 9. Azure Key Vault Configuration

### 9.1 Key Vault (`kv-ipsagent-{env}`)

- **SKU:** Standard
- **Soft delete:** enabled, 7-day retention (dev), 30-day retention (test, prod)
- **Purge protection:** disabled (dev), enabled (test, prod)
- **Public network access:** enabled (required for App Service Key Vault references over the Azure backbone)
- **RBAC authorization model** — no access policies; roles assigned via Bicep:

| Principal | Role | Notes |
|-----------|------|-------|
| API App Service MI | `Key Vault Secrets User` | Read secrets at runtime |
| Web App Service MI | `Key Vault Secrets User` | Read secrets at runtime |

### 9.2 Secrets Stored in Key Vault

| Secret name | Content | Populated by |
|-------------|---------|-------------|
| `appi-connection-string` | Application Insights connection string | Bicep — from App Insights module output |
| `sql-connection-string` | Full ADO.NET connection string for the SQL Database | Bicep — constructed from `sqlAdminLogin`, `sqlAdminPassword`, and SQL server FQDN |
| `auth-tenant-id` | Entra ID Tenant ID | Bicep — from `entraIdTenantId` parameter |
| `auth-client-id` | Entra ID Client ID | Bicep — from `entraIdClientId` parameter |

---

## 10. Security Considerations

| Concern | Mitigation |
|---------|----------|
| SQL credentials | Stored in Key Vault as `sql-connection-string`; never in source code or parameter files |
| Entra ID secrets | Stored in Key Vault; App Services access via Key Vault references over managed identity |
| Secret rotation | SQL password and Entra tokens rotated via pipeline; App Services pick up new values on next deployment |
| HTTPS enforcement | `httpsOnly: true` on all App Services |
| SQL firewall | Dev: public access enabled for local development; test/prod: only Azure-internal traffic allowed |
| Key Vault access | RBAC model; managed identities granted least-privilege `Key Vault Secrets User` role |
| Purge protection | Enabled on test and prod Key Vaults to prevent accidental permanent deletion |

---

## 11. CI/CD Integration

### 11.1 GitHub Actions Workflow (per environment)

Each environment has a dedicated **infrastructure** workflow triggered on changes to `infra/`, workflow file, or via manual dispatch. Application build and deployment are handled by separate workflows.

```
.github/
└── workflows/
    ├── infra.yml
```

**Deployment steps per workflow**

1. **Lint** — `az bicep lint infra/main.bicep`
2. **What-if** — `az deployment sub what-if --location swedencentral --template-file infra/main.bicep --parameters infra/environments/{env}.bicepparam` (dry-run diff)
3. **Deploy infra** — `az deployment sub create --location swedencentral --template-file infra/main.bicep --parameters infra/environments/{env}.bicepparam --parameters entraIdTenantId=... entraIdClientId=... sqlAdminLogin=... sqlAdminPassword=...`

### 11.2 Required GitHub Secrets

| Secret | Used in |
|--------|---------|
| `AZURE_CLIENT_ID` | Federated OIDC identity for `az login` |
| `AZURE_TENANT_ID` | Federated OIDC identity for `az login` |
| `AZURE_SUBSCRIPTION_ID` | Target subscription |
| `SQL_ADMIN_LOGIN` | Passed to Bicep as `sqlAdminLogin`; used to construct `sql-connection-string` in Key Vault |
| `SQL_ADMIN_PASSWORD` | Passed to Bicep as `sqlAdminPassword`; used to construct `sql-connection-string` in Key Vault |
| `AUTH_TENANT_ID` | Written to Key Vault as `auth-tenant-id`; also passed to Bicep as `entraIdTenantId` |
| `AUTH_CLIENT_ID` | Written to Key Vault as `auth-client-id`; also passed to Bicep as `entraIdClientId` |

---

## 12. Resource Group Strategy

Each environment gets its own resource group, created and managed by Bicep as part of the subscription-scoped deployment. This enables full lifecycle management (delete dev without touching prod) and ensures the resource group is always in the expected state.

| Resource Group | Subscription |
|----------------|-------------|
| `rg-ipsagent-dev` | Dev/Test subscription (recommended) |
| `rg-ipsagent-test` | Dev/Test subscription (recommended) |
| `rg-ipsagent-prod` | Production subscription (recommended) |

Because `main.bicep` targets the **subscription** scope, the deploying identity requires at minimum the **Contributor** role at subscription level (to create resource groups and deploy resources into them).

---

## 13. Implementation Checklist

- [ ] Create `infra/main.bicep` with `targetScope = 'subscription'` using AVM modules listed in §5.1
- [ ] Create `infra/modules/webapp.bicep` shared module (optional refactor)
- [ ] Create `infra/environments/dev.bicepparam`
- [ ] Create `infra/environments/test.bicepparam`
- [ ] Create `infra/environments/prod.bicepparam`
- [ ] Create `infra/README.md` with local deployment instructions
- [ ] Grant the GitHub Actions federated identity **Contributor** role at subscription level for each target subscription (manual step).
- [ ] Create `.github/workflows/infra.yml` to deploy Dev environment
