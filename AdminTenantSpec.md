# Admin Tenant Specification

## Overview
The Admin Tenant is a specialized tenant configuration that allows platform owners to manage other tenants within the SaaS application. This tenant must be configured with a specific flag in the application and correspond to a dedicated Application in the Identity Provider (FusionAuth).

## Configuration Correlation

### 1. Identity Provider (FusionAuth) Configuration
In `kickstart.json`, a specific Application is defined for the Admin Tenant. 

**Variable Definitions (`kickstart.json`):**
```json
"applicationIdAdmin": "8672d1ac-68db-425a-81e6-4748f39c42ab",
"clientSecretAdmin": "ce6beef5-c6bf-4661-bd06-38f5dd3d8c2c",
"defaultTenantId": "d7d09513-a3f5-401c-9685-34ab6c552453"
```

**Application Definition (`kickstart.json`):**
The application is created with specific OAuth configuration matching the application's routing.
```json
{
    "method": "POST",
    "url": "/api/application/#{applicationIdAdmin}",
    "tenantId": "#{defaultTenantId}",
    "body": {
      "application": {
        "name": "Admin Tenant",
        "oauthConfiguration" : {
            "authorizedRedirectURLs": ["https://localhost:5001/admin/signin-oidc"],
            "logoutURL": "https://localhost:5001/admin",
            "clientSecret": "#{clientSecretAdmin}",
            "enabledGrants": ["authorization_code", "refresh_token"]
        },
        // ... jwtConfiguration omitted
      }
    }
}
```

### 2. Application Code Configuration (`InitialTenantData.cs`)
The application requires a `BlastTenant` entry that strictly maps to the variables defined above.

| Field | Source / Value | Notes |
|-------|---------------|-------|
| `Id` | `"unique-id-admin"` | Fixed ID for the admin record in local DB. |
| `Identifier` | `"admin"` | *Critical*: Corresponds to the URL path segment in FusionAuth `authorizedRedirectURLs`. |
| `OpenIdConnectClientId` | `configuration["TenantAdminClientId"]` | Maps to `applicationIdAdmin` in FusionAuth. |
| `OpenIdConnectClientSecret` | `configuration["TenantAdminSecret"]` | Maps to `clientSecretAdmin` in FusionAuth. |
| `IdentityTenantId` | `"d7d09513-a3f5-401c-9685-34ab6c552453"` | Maps to `defaultTenantId` in FusionAuth. |
| `AdminTenant` | `true` | *Critical*: Flags this tenant as having elevated privileges in the SaaS platform. |

## Tenant Management API

The platform includes a dedicated API for automating tenant management. This API is secured and only accessible when targeting the **Admin Tenant** with a specific API Key.

### Authorization
*   **Target Tenant**: The request must be directed to the Admin Tenant's domain/paths.
*   **Header**: `ApiKey` must be provided.
*   **Validation**: The system validates that the `ApiKey` matches the configured `TenantAdminKey` AND that the current tenant context has `AdminTenant: true`.

### Endpoints
Defined in `TenantController`, these endpoints facilitate automated onboarding.

| Method | Endpoint | Description |
|--------|----------|-------------|
| `PUT` | `/api/tenant/` | Creates or updates a tenant configuration. Payload: `PutTenant.Command`. |
| `GET` | `/api/tenant/{id}` | Checks if a tenant exists by ID. |

## Tenant Administration UI

The application includes administrative screens for managing tenants, which are conditionally available.

### Access Control
*   **Visibility**: The "Tenant Configuration" menu item in the navigation sidebar is controlled by the `IsAdminTenant()` check.
*   **Logic**: The `CustomTenantInfo` object (hydrated from the tenant store) must have the `AdminTenant` property set to `true` for the current session's tenant.

### Screens
*   **List View** (`/tenants`): Lists all registered tenants in the system.
*   **Edit View** (`/tenant/edit/{id}`): Allows configuration of tenant details (Name, Identifier, OAuth settings).
    *   *Note*: The Admin Tenant itself is rendered as "Readonly" in the edit view.

## Implementation Steps for New SaaS Application

1.  **Define FusionAuth Application**: Create a new Application in FusionAuth for the admin portal. Convert the generated `Client Id` and `Client Secret` into configuration variables.
2.  **Configure Redirects**: Ensure the `Authorized Redirect URLs` in FusionAuth end in `/{admin-identifier}/signin-oidc` (e.g., `/admin/signin-oidc`).
3.  **Seed Data**: In the application startup/seeding logic, insert a Tenant record where:
    *   `AdminTenant` boolean is set to `true`.
    *   `Identifier` matches the redirect URL path used in step 2.
    *   `ClientId` and `ClientSecret` match the FusionAuth Application credentials.
4.  **Secure API**: Implement the `ApiKeyTenantAttribute` (or equivalent) to secure the tenant management endpoints using the admin tenant flag and a secure key.
5.  **Secure UI**: Gate the administration navigation items using the `AdminTenant` flag.
