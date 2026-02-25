# Infrastructure Deployment

This directory contains the Bicep Infrastructure as Code (IaC) for deploying the Fake Intra application to Azure.

## Prerequisites

- [Azure CLI](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli) (v2.61+)
- [Bicep CLI](https://learn.microsoft.com/en-us/azure/azure-resource-manager/bicep/install) (v0.27+, bundled with Azure CLI)
- An Azure subscription with **Contributor** role at subscription level
- The following values ready for deployment:
  - SQL admin login and password
  - Entra ID Tenant ID and Client ID

## Repository Structure

```
infra/
├── main.bicep                       # Root orchestration template (subscription scope)
├── modules/
│   └── keyvault-secrets.bicep       # Key Vault secrets child resources
├── environments/
│   ├── dev.bicepparam               # Dev parameter file
│   ├── test.bicepparam              # Test parameter file
│   └── prod.bicepparam              # Prod parameter file
└── README.md                        # This file
```

## Environments

| Environment | Parameter File | Description |
|-------------|---------------|-------------|
| `dev` | `environments/dev.bicepparam` | Developer integration — Basic SKU, free SQL |
| `test` | `environments/test.bicepparam` | QA / acceptance testing — Standard SKU |
| `prod` | `environments/prod.bicepparam` | Production — Premium SKU |

## Local Deployment

### 1. Sign in to Azure

```bash
az login
az account set --subscription <SUBSCRIPTION_ID>
```

### 2. Lint the template

```bash
az bicep lint --file infra/main.bicep
```

### 3. Preview changes (what-if)

```bash
az deployment sub what-if \
  --location swedencentral \
  --template-file infra/main.bicep \
  --parameters infra/environments/dev.bicepparam \
  --parameters \
    sqlAdminLogin=<SQL_ADMIN_LOGIN> \
    sqlAdminPassword=<SQL_ADMIN_PASSWORD> \
    entraIdTenantId=<ENTRA_ID_TENANT_ID> \
    entraIdClientId=<ENTRA_ID_CLIENT_ID>
```

### 4. Deploy

```bash
az deployment sub create \
  --location swedencentral \
  --template-file infra/main.bicep \
  --parameters infra/environments/dev.bicepparam \
  --parameters \
    sqlAdminLogin=<SQL_ADMIN_LOGIN> \
    sqlAdminPassword=<SQL_ADMIN_PASSWORD> \
    entraIdTenantId=<ENTRA_ID_TENANT_ID> \
    entraIdClientId=<ENTRA_ID_CLIENT_ID>
```

Replace `dev.bicepparam` with `test.bicepparam` or `prod.bicepparam` for other environments.

> **Important:** Never store `sqlAdminLogin`, `sqlAdminPassword`, `entraIdTenantId`, or `entraIdClientId` in parameter files. Pass them at deploy time.

## CI/CD

Infrastructure is deployed automatically via GitHub Actions (`.github/workflows/infra.yml`). The workflow uses OIDC federated identity for authentication and reads sensitive values from GitHub repository secrets.

### Required GitHub Secrets

| Secret | Description |
|--------|-------------|
| `AZURE_CLIENT_ID` | App registration Client ID for OIDC login |
| `AZURE_TENANT_ID` | Azure AD Tenant ID for OIDC login |
| `AZURE_SUBSCRIPTION_ID` | Target Azure subscription |
| `SQL_ADMIN_LOGIN` | SQL Server administrator login name |
| `SQL_ADMIN_PASSWORD` | SQL Server administrator password |
| `AUTH_TENANT_ID` | Entra ID Tenant ID (stored in Key Vault) |
| `AUTH_CLIENT_ID` | Entra ID Client ID (stored in Key Vault) |

## Teardown

To delete an environment's resources, remove the resource group:

```bash
az group delete --name rg-ipsagent-dev --yes --no-wait
```
