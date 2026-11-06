# MASTER PROMPT: Build a Production-Ready .NET 10 Modular Monolith API Platform

## ROLE AND RESPONSIBILITY

Act as a Principal Software Architect, Senior .NET Backend Engineer, Application Security Engineer, Database Architect, and DevOps Engineer.

Your task is to design, implement, test, document, and deliver a complete, reusable, production-ready **.NET 10 Modular Monolith API Platform**.

You are responsible for making architectural decisions, implementing features, maintaining code quality, enforcing security, and ensuring that the entire solution is functional.

Do not treat this as a tutorial or a sample CRUD application.

Build a reusable backend foundation that can serve as the starting point for almost any real-world application.

The platform should be suitable for:

- SaaS applications.
- ERP and CRM systems.
- E-commerce applications.
- Inventory management.
- Booking and reservation systems.
- Enterprise management software.
- Financial management applications.
- Healthcare management systems.
- Project management platforms.
- Multi-tenant applications.
- Internal business applications.
- Third-party integration platforms.

The final platform must allow developers to create new business modules without rebuilding common infrastructure.

---

# 1. MANDATORY ARCHITECTURAL DECISIONS

These requirements are non-negotiable.

## Architecture

Use Modular Monolith Architecture with clear module boundaries.

## Application Design

Use the Application Service Pattern.

Do not introduce CQRS or unnecessary mediator abstractions.

## API

Use ASP.NET Core Minimal APIs exclusively.

Do not use MVC Controllers.

## Database

Use Entity Framework Core 10 directly.

Do not create generic repositories that unnecessarily wrap DbContext and DbSet.

## Configuration

Use the native ASP.NET Core Options Pattern for all configurable application settings.

Use strongly typed configuration classes and startup validation.

## Mapping

Do not use AutoMapper.

Use explicit DTO construction, LINQ projections, and manual mapping.

## Dependency Injection

Use native ASP.NET Core dependency injection with clear module registration extensions.

## Validation

Use FluentValidation for request validation.

## Core Infrastructure

Use PostgreSQL, Redis, Serilog, OpenTelemetry, background jobs, and Docker.

## Explicitly prohibited

- CQRS.
- MediatR.
- AutoMapper.
- Controller-based APIs.
- Unnecessary generic repositories.
- Unnecessary abstraction layers.
- Overengineered event-driven architecture.
- Business logic inside endpoint handlers.
- Direct cross-module access to internal implementation details.
- Hardcoded application configuration.

Do not introduce prohibited technologies indirectly through alternative packages.

---

# 2. TECHNOLOGY STACK

Use compatible, stable packages for .NET 10.

| Category | Technology |
|---|---|
| Runtime | .NET 10 |
| Language | C# |
| API | ASP.NET Core Minimal APIs |
| ORM | Entity Framework Core 10 |
| Database | PostgreSQL |
| Cache | Redis |
| Validation | FluentValidation |
| Authentication | ASP.NET Core Identity |
| Authorization | RBAC + Permissions |
| Tokens | JWT |
| Logging | Serilog |
| Observability | OpenTelemetry |
| Metrics | Prometheus |
| Dashboards | Grafana |
| Tracing | Jaeger or compatible OTLP backend |
| Background Jobs | Hangfire |
| Real-time | SignalR |
| API Documentation | OpenAPI / Swagger UI |
| Object Storage | S3-compatible / Azure Blob / Local |
| Testing | xUnit |
| Assertions | FluentAssertions or equivalent |
| Mocking | NSubstitute |
| Integration Testing | WebApplicationFactory |
| Infrastructure Testing | Testcontainers |
| Containers | Docker / Docker Compose |

Use packages only when they provide real value.

Verify package compatibility with .NET 10 before introducing dependencies.

---

# 3. SOLUTION STRUCTURE

Create a maintainable solution with explicit module boundaries.

Use this structure as the starting point:

```text
ModularMonolith.sln

src/
├── Api/
│   ├── Extensions/
│   ├── Middleware/
│   ├── Configuration/
│   ├── Filters/
│   ├── OpenApi/
│   └── Program.cs
│
├── BuildingBlocks/
│   ├── Common/
│   ├── Domain/
│   ├── Contracts/
│   ├── Infrastructure/
│   └── Observability/
│
├── Modules/
│   ├── Identity/
│   │   ├── Domain/
│   │   ├── Application/
│   │   ├── Infrastructure/
│   │   └── Endpoints/
│   │
│   ├── Users/
│   │   ├── Domain/
│   │   ├── Application/
│   │   ├── Infrastructure/
│   │   └── Endpoints/
│   │
│   ├── Organizations/
│   │   ├── Domain/
│   │   ├── Application/
│   │   ├── Infrastructure/
│   │   └── Endpoints/
│   │
│   ├── Roles/
│   │   ├── Domain/
│   │   ├── Application/
│   │   ├── Infrastructure/
│   │   └── Endpoints/
│   │
│   ├── Permissions/
│   │   ├── Domain/
│   │   ├── Application/
│   │   ├── Infrastructure/
│   │   └── Endpoints/
│   │
│   ├── Monitoring/
│   ├── AuditLogs/
│   ├── Notifications/
│   ├── FileStorage/
│   ├── BackgroundJobs/
│   ├── Webhooks/
│   ├── Settings/
│   ├── FeatureManagement/
│   ├── ApiKeys/
│   ├── Analytics/
│   └── Localization/
│
└── Tests/
    ├── UnitTests/
    ├── IntegrationTests/
    ├── ArchitectureTests/
    └── FunctionalTests/

infrastructure/
├── docker/
├── monitoring/
│   ├── prometheus/
│   ├── grafana/
│   └── otel/
└── scripts/

docs/
├── architecture/
├── modules/
├── api/
├── deployment/
└── development/
```

Each module must have its own logical boundaries and ownership.

Modules can use separate projects where doing so improves dependency enforcement. Do not create excessive projects simply to reproduce the folder structure.

Each module must provide:

- Domain entities.
- Value objects where appropriate.
- Domain rules.
- Application services.
- Application interfaces.
- Request and response DTOs.
- FluentValidation validators.
- Module-owned EF Core DbContext.
- Entity configurations.
- Database migrations.
- Minimal API endpoint definitions.
- Dependency registration.
- Configuration options.
- Tests.

---

# 4. MODULAR MONOLITH ARCHITECTURE

Implement a modular monolith with a single deployable application.

The application must have:

- One primary API host.
- Multiple independent modules.
- Clear module ownership.
- Shared infrastructure.
- Module-specific business logic.
- Controlled communication between modules.

## Module isolation

A module must not directly access:

- Another module's private repositories.
- Another module's internal DbContext.
- Another module's internal services.
- Another module's domain entities.

Cross-module communication must use:

1. Public interfaces.
2. Public contracts.
3. Domain events where appropriate.
4. Integration events where asynchronous communication is necessary.

Use synchronous service contracts for operations that require an immediate result.

Use asynchronous integration events for independent operations such as notifications, webhooks, and audit processing.

Implement reliable event publishing with a Transactional Outbox where required.

Do not build an unnecessary message broker or event bus.

## Module lifecycle

Every module should support:

- Service registration.
- Endpoint registration.
- Configuration registration.
- Database registration.
- Optional enable/disable configuration.
- Health check registration where applicable.

Create a consistent module registration convention.

Example:

```csharp
services.AddIdentityModule(configuration);
services.AddUserModule(configuration);
services.AddMonitoringModule(configuration);
services.AddNotificationModule(configuration);

app.MapIdentityModule();
app.MapUserModule();
app.MapMonitoringModule();
app.MapNotificationModule();
```

The application should not require modifications to unrelated modules when a new module is introduced.

---

# 5. APPLICATION SERVICE PATTERN

Use straightforward application services.

Do not use CQRS handlers.

Example:

```csharp
public interface IUserService
{
    Task<UserDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task<PagedResult<UserDto>> GetAllAsync(
        UserQuery query,
        CancellationToken cancellationToken);

    Task<UserDto> CreateAsync(
        CreateUserRequest request,
        CancellationToken cancellationToken);

    Task UpdateAsync(
        Guid id,
        UpdateUserRequest request,
        CancellationToken cancellationToken);

    Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken);
}
```

Services should:

- Implement application use cases.
- Coordinate domain operations.
- Enforce application-level business rules.
- Use dependency injection.
- Access module-owned persistence.
- Handle transaction boundaries.
- Return DTOs.
- Support cancellation tokens.
- Avoid unnecessary dependencies.

Domain entities must enforce their own business invariants.

Do not create service interfaces automatically for every class. Introduce interfaces where they provide a meaningful abstraction, module boundary, or testing benefit.

---

# 6. MINIMAL API IMPLEMENTATION

Use Minimal APIs exclusively.

Each module must define its own endpoint groups.

Example:

```csharp
public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(
        this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/users")
            .WithTags("Users")
            .RequireAuthorization();

        group.MapGet("/{id:guid}", GetByIdAsync)
            .WithName("GetUserById")
            .WithOpenApi();

        group.MapGet("/", GetAllAsync)
            .WithName("GetUsers")
            .WithOpenApi();

        group.MapPost("/", CreateAsync)
            .RequireAuthorization("users.create");

        group.MapPut("/{id:guid}", UpdateAsync)
            .RequireAuthorization("users.update");

        group.MapDelete("/{id:guid}", DeleteAsync)
            .RequireAuthorization("users.delete");

        return app;
    }

    private static async Task<Results<Ok<UserDto>, NotFound>>
        GetByIdAsync(
            Guid id,
            IUserService service,
            CancellationToken cancellationToken)
    {
        var user = await service.GetByIdAsync(
            id,
            cancellationToken);

        return user is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(user);
    }
}
```

Requirements:

- Use MapGroup.
- Use typed endpoint responses where practical.
- Use endpoint filters where appropriate.
- Use authorization policies.
- Add OpenAPI descriptions.
- Apply request validation.
- Use CancellationToken.
- Keep endpoint methods small.
- Delegate business logic to application services.
- Avoid direct DbContext usage inside endpoints.

Implement reusable extensions for:

- Validation.
- Authorization.
- Pagination.
- Problem Details.
- API response conventions.
- Correlation IDs.

---

# 7. OPTIONS PATTERN AND CONFIGURATION

The Options Pattern is mandatory throughout the application.

Do not scatter configuration access across the codebase.

Use:

- IOptions<T>.
- IOptionsSnapshot<T>.
- IOptionsMonitor<T>.

Create strongly typed option classes for:

- ApplicationOptions.
- DatabaseOptions.
- JwtOptions.
- SecurityOptions.
- RedisOptions.
- MonitoringOptions.
- EmailOptions.
- SmsOptions.
- FileStorageOptions.
- BackgroundJobOptions.
- NotificationOptions.
- WebhookOptions.
- RateLimitingOptions.
- CorsOptions.
- OpenTelemetryOptions.
- MultiTenancyOptions.
- LocalizationOptions.
- FeatureManagementOptions.
- RetentionOptions.

Example:

```csharp
public sealed class MonitoringOptions
{
    public const string SectionName = "Monitoring";

    public bool Enabled { get; set; } = true;

    public int CheckIntervalSeconds { get; set; } = 60;

    public int TimeoutSeconds { get; set; } = 30;

    public int FailureThreshold { get; set; } = 3;

    public int RecoveryThreshold { get; set; } = 2;

    public int RetentionDays { get; set; } = 90;
}
```

Registration:

```csharp
services.AddOptions<MonitoringOptions>()
    .BindConfiguration(MonitoringOptions.SectionName)
    .ValidateDataAnnotations()
    .Validate(
        options => options.CheckIntervalSeconds > 0,
        "Check interval must be positive.")
    .ValidateOnStart();
```

Configuration requirements:

- Strong typing.
- Validation at startup.
- Sensible defaults.
- Environment variable overrides.
- Development and production configuration.
- Secret management.
- Runtime reload where appropriate.
- Configuration documentation.

Use IOptionsMonitor for singleton services that need reloadable configuration.

Use IOptionsSnapshot for scoped services where per-scope configuration is appropriate.

Do not expose secrets through configuration APIs.

Example configuration:

```json
{
  "Application": {
    "Name": "ModularMonolith",
    "Environment": "Development"
  },
  "Monitoring": {
    "Enabled": true,
    "CheckIntervalSeconds": 60,
    "TimeoutSeconds": 30,
    "FailureThreshold": 3,
    "RecoveryThreshold": 2,
    "RetentionDays": 90
  },
  "RateLimiting": {
    "Enabled": true,
    "PermitLimit": 100,
    "WindowSeconds": 60
  },
  "Redis": {
    "Enabled": true,
    "InstanceName": "ModularMonolith:"
  }
}
```

Never commit actual production credentials or signing keys.

---

# 8. CORE MODULES

Implement the following modules as reusable platform functionality.

## MODULE 1: Identity and Authentication

Implement a complete authentication foundation.

Features:

- User registration.
- Login.
- Logout.
- JWT access token generation.
- Refresh token rotation.
- Refresh token revocation.
- Refresh token reuse detection.
- Password hashing.
- Password change.
- Forgot password.
- Password reset.
- Email verification.
- Account lockout.
- Failed login tracking.
- Session management.
- Revoke all sessions.
- Login history.
- Device/session information.
- Optional MFA.
- External authentication provider readiness.

Security requirements:

- Never store plaintext passwords.
- Never store refresh tokens in plaintext.
- Validate token issuer and audience.
- Use appropriate token expiration.
- Support signing-key rotation.
- Secure password reset workflows.
- Protect against brute-force attacks.
- Revoke compromised sessions.

Use ASP.NET Core Identity where appropriate.

Separate identity authentication from user profile management.

## MODULE 2: User Management

Features:

- Create users.
- Update users.
- Delete users.
- Soft deletion where appropriate.
- Get user details.
- Paginated listing.
- Search.
- Filtering.
- Sorting.
- Activate/deactivate users.
- User profiles.
- Profile pictures.
- User preferences.
- User status.
- User activity history.
- User roles.
- User permissions.
- Bulk operations.
- Import/export.

## MODULE 3: Role and Permission Management

Implement complete RBAC.

Features:

- Create roles.
- Update roles.
- Delete roles.
- Assign roles.
- Remove roles.
- Create permissions.
- Assign permissions to roles.
- Assign direct permissions to users where appropriate.
- Permission grouping by module.
- Permission policies.
- Dynamic permission checking.
- Role hierarchy readiness.
- Default system roles.
- Super administrator role.
- Permission seeding.

Use permission policies such as:

```csharp
.RequireAuthorization("users.create")
```

Do not rely only on role names for fine-grained authorization.

## MODULE 4: Organization and Multi-Tenancy

Implement optional multi-tenancy.

Features:

- Organization management.
- Tenant creation.
- Tenant identification.
- Organization members.
- Organization invitations.
- Tenant-specific configuration.
- Tenant-specific permissions.
- Tenant-level roles.
- Tenant activation/deactivation.
- Tenant subscription readiness.
- Tenant audit history.
- Tenant data isolation.

Support:

1. Single-tenant mode.
2. Shared database with tenant discriminator.
3. Database-per-tenant extension readiness.

Use tenant resolution middleware or an equivalent controlled mechanism.

Enforce tenant isolation at the persistence layer.

Ensure background jobs, caching, file access, and API authorization respect tenant context.

Never trust a tenant ID supplied by the client without validating membership and access rights.

Multi-tenancy must be optional.

## MODULE 5: API MONITORING SERVICE

Build a complete API Monitoring Service.

This is a first-class module.

The purpose is to monitor API availability, uptime, response times, latency, failures, and performance, and generate alerts when problems occur.

### A. Endpoint Management

Allow administrators to register monitored endpoints.

Features:

- Create monitoring endpoint.
- Update monitoring endpoint.
- Delete monitoring endpoint.
- Enable/disable monitoring.
- Endpoint name.
- Endpoint URL.
- HTTP method.
- Request headers.
- Authentication configuration.
- Request body where appropriate.
- Expected status codes.
- Expected response content.
- Timeout configuration.
- Check interval.
- Endpoint grouping.
- Environment tagging.
- Monitoring tags.
- Maintenance windows.

Never expose stored credentials in monitoring API responses.

### B. Uptime Monitoring

Features:

- Scheduled HTTP checks.
- Endpoint availability.
- Success/failure detection.
- Uptime percentage.
- Downtime percentage.
- Total downtime.
- Consecutive failures.
- Recovery detection.
- Historical uptime.
- Availability timeline.
- Incident start time.
- Incident end time.
- Maintenance window exclusion.

Calculate uptime using explicitly defined rules and monitoring periods.

Distinguish monitoring failures from intentional maintenance.

### C. Performance Monitoring

Track:

- Response time.
- Average response time.
- Minimum response time.
- Maximum response time.
- P50 latency.
- P90 latency.
- P95 latency.
- P99 latency.
- Request timeout.
- HTTP status distribution.
- Error rate.
- Success rate.
- Response size where appropriate.

Provide historical metrics and date-range filtering.

Do not calculate percentiles from averages.

Use an efficient strategy for metric aggregation and retention.

### D. Incident Management

Implement a complete incident lifecycle.

Statuses:

- Healthy.
- Degraded.
- Down.
- Investigating.
- Acknowledged.
- Resolved.
- Maintenance.

Features:

- Automatic incident creation.
- Failure threshold.
- Recovery threshold.
- Incident deduplication.
- Incident acknowledgment.
- Incident assignment.
- Incident resolution.
- Incident comments.
- Incident timeline.
- Root cause notes.
- Incident history.
- Incident notifications.

Do not create a new incident for every consecutive failed check.

### E. Alert Management

Support:

- Email alerts.
- Webhook alerts.
- In-app alerts.
- SignalR real-time alerts.
- SMS provider integration readiness.

Features:

- Alert rules.
- Threshold configuration.
- Failure-based alerts.
- Latency-based alerts.
- Error-rate alerts.
- Recovery alerts.
- Alert escalation.
- Alert acknowledgment.
- Alert suppression.
- Notification deduplication.
- Alert history.
- Notification retry.

Deliver alerts through background processing.

### F. SSL Monitoring

Features:

- SSL certificate expiration detection.
- Certificate expiry alerts.
- Configurable warning periods.
- Certificate validity checks.

### G. Monitoring Dashboard APIs

Expose dashboard-ready endpoints:

- Overall monitoring status.
- Total monitored endpoints.
- Healthy endpoints.
- Degraded endpoints.
- Down endpoints.
- Current incidents.
- Recent incidents.
- Average response time.
- Slowest endpoints.
- Uptime percentage.
- Error rate.
- Historical latency.
- Historical uptime.

### H. Monitoring Architecture

Use:

- BackgroundService or Hangfire for scheduling.
- IHttpClientFactory.
- Configurable HttpClient timeouts.
- Cancellation tokens.
- Retry policies only where appropriate.
- Redis distributed locks when multiple workers may execute the same check.
- Database persistence.
- Aggregated metrics.
- Retention policies.

Avoid overlapping checks.

Prevent monitoring jobs from causing excessive traffic.

Support multiple application instances.

Make monitoring intervals configurable.

Do not allow arbitrary monitored URLs to become an SSRF vulnerability. Validate destinations, block unsafe internal and cloud metadata addresses, validate redirects, and apply network-level egress restrictions.

---

## MODULE 6: Audit Logging

Build centralized audit logging.

Track:

- User login.
- Failed login.
- Logout.
- User creation.
- User update.
- User deletion.
- Role changes.
- Permission changes.
- Administrative actions.
- Important business operations.
- Security events.
- Configuration changes.

Audit properties:

- ID.
- Tenant ID.
- User ID.
- Action.
- Module.
- Entity type.
- Entity ID.
- Before state where appropriate.
- After state where appropriate.
- IP address.
- User agent.
- Correlation ID.
- Timestamp.

Features:

- Search audit records.
- Filter.
- Sort.
- Pagination.
- Date-range queries.
- Export.
- Retention.
- Audit history.

Audit records should be append-only through normal application operations.

Never record passwords, secrets, authentication tokens, or unnecessary sensitive data.

## MODULE 7: Notification Management

Implement a reusable notification infrastructure.

Channels:

- Email.
- In-app.
- SignalR.
- SMS provider integration.
- Webhooks.

Features:

- Notification templates.
- Template variables.
- Notification preferences.
- User preferences.
- Tenant notification settings.
- Scheduled notifications.
- Immediate notifications.
- Bulk notifications.
- Read/unread status.
- Notification history.
- Delivery status.
- Failed delivery tracking.
- Retry policies.
- Provider abstraction.

Implement provider interfaces:

- IEmailSender.
- ISmsSender.
- INotificationSender.

Provide a development email provider for local testing.

Use background processing for delivery.

## MODULE 8: File and Media Storage

Implement a provider-independent file storage service.

Features:

- Upload.
- Download.
- Delete.
- File metadata.
- File categorization.
- File ownership.
- File size validation.
- MIME type validation.
- Extension validation.
- Private/public access.
- Secure download.
- File versioning readiness.
- Image metadata.
- File retention.
- File access permissions.

Providers:

- Local storage.
- Amazon S3-compatible storage.
- Azure Blob Storage.

Use:

```csharp
public interface IFileStorageProvider
{
    Task<StoredFile> UploadAsync(
        Stream stream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken);

    Task<Stream?> DownloadAsync(
        string fileKey,
        CancellationToken cancellationToken);

    Task DeleteAsync(
        string fileKey,
        CancellationToken cancellationToken);
}
```

Do not store large files directly in PostgreSQL.

Validate file contents rather than trusting client-provided MIME types.

## MODULE 9: Background Job Management

Use Hangfire for background processing.

Features:

- Immediate jobs.
- Delayed jobs.
- Recurring jobs.
- Scheduled jobs.
- Job execution history.
- Job status.
- Retry configuration.
- Failed job handling.
- Job cancellation where supported.
- Administrative job monitoring.
- Job dashboard.
- Job retention.

Use idempotency for jobs that can be retried.

Ensure tenant context is preserved for tenant-specific jobs.

Protect Hangfire administrative interfaces.

## MODULE 10: Caching

Implement a reusable caching infrastructure.

Support:

- In-memory caching.
- Redis distributed caching.
- Cache expiration.
- Cache invalidation.
- Cache key conventions.
- Cache grouping.
- Distributed locking.
- Cache health checks.

Create:

- ICacheService.
- IDistributedLockService.

Features must be configurable.

Do not cache sensitive user data without a defined isolation and security policy.

## MODULE 11: Application Settings

Build dynamic settings management.

Features:

- Global settings.
- Module settings.
- Tenant settings.
- User preferences.
- Feature configuration.
- Runtime configuration.
- Setting categories.
- Setting validation.
- Setting history.
- Setting permissions.

Use the Options Pattern for application infrastructure configuration.

Database-backed settings may be used for dynamic business configuration.

Do not mix secret storage with ordinary application settings.

## MODULE 12: API Management

Implement reusable API infrastructure.

Features:

- Versioning.
- Standard response conventions.
- Global exception handling.
- Request validation.
- Pagination.
- Sorting.
- Filtering.
- Searching.
- Rate limiting.
- Request correlation.
- Idempotency keys.
- CORS.
- Secure headers.
- Request size limits.
- OpenAPI documentation.
- API deprecation.
- API version compatibility.

Use RFC 9457 Problem Details for errors.

Implement API versioning with a supported .NET package if required.

## MODULE 13: Health Check Service

Provide:

- Application liveness.
- Application readiness.
- PostgreSQL health.
- Redis health.
- Background job health.
- File storage health.
- External service health.
- Module health.

Expose:

```text
GET /health
GET /health/live
GET /health/ready
```

Keep sensitive infrastructure details private.

Use readiness checks for deployment orchestration.

Do not make liveness depend on every external service.

## MODULE 14: Observability and Logging

Implement centralized observability.

### Logging

Use Serilog.

Features:

- Structured logs.
- Console logging.
- File logging where appropriate.
- Centralized log enrichment.
- Correlation IDs.
- Trace IDs.
- User context.
- Tenant context.
- Environment information.
- Exception details.

### Metrics

Use OpenTelemetry metrics.

Track:

- HTTP request count.
- HTTP duration.
- HTTP errors.
- Database duration.
- Background job execution.
- External API duration.
- Application resource utilization.
- Custom business metrics.

### Distributed Tracing

Use OpenTelemetry tracing.

Trace:

- Incoming HTTP requests.
- Outgoing HTTP calls.
- Database operations.
- Background jobs.
- Module interactions where useful.

### Monitoring Infrastructure

Provide:

- OpenTelemetry Collector configuration.
- Prometheus configuration.
- Grafana dashboards.
- Trace backend configuration.

Avoid exposing secrets or sensitive data through telemetry.

Distinguish infrastructure observability from the API Monitoring module, which checks the availability of registered external or internal endpoints.

## MODULE 15: Webhook Management

Implement a reusable webhook engine.

Features:

- Webhook registration.
- Webhook URL validation.
- Event subscription.
- Secret generation.
- HMAC signatures.
- Delivery history.
- Retry policies.
- Exponential backoff.
- Timeout configuration.
- Failed delivery management.
- Webhook enable/disable.
- Manual retry.
- Delivery logs.

Use background processing.

Sign payloads securely.

Prevent SSRF.

Do not expose webhook secrets in API responses.

## MODULE 16: Feature Management

Implement feature flags.

Features:

- Create feature flags.
- Enable/disable flags.
- Global feature flags.
- Tenant-specific feature flags.
- User-specific feature flags.
- Environment-specific flags.
- Scheduled activation readiness.
- Feature history.
- Permission-controlled management.

Use a reusable feature evaluation service.

## MODULE 17: API Key Management

Features:

- Generate API keys.
- Hash API keys before storage.
- API key name.
- Key permissions.
- Expiration.
- Revocation.
- Last-used timestamp.
- Usage tracking.
- Rate limits.
- IP restrictions where appropriate.
- Key rotation.

Never return complete API keys after initial generation.

## MODULE 18: Dashboard and Analytics

Provide reusable dashboard APIs.

Features:

- Total users.
- Active users.
- New registrations.
- User activity.
- System health.
- API uptime.
- Response performance.
- Error rates.
- Active incidents.
- Recent audit activity.
- Background job statistics.
- Notification delivery statistics.
- Storage statistics.

Support date-range filtering.

Use optimized aggregate queries.

Do not calculate dashboard metrics by loading entire database tables.

## MODULE 19: Localization and Time Zones

Features:

- Resource-based localization.
- Multiple languages.
- User language preference.
- Tenant language preference.
- Localized validation messages.
- UTC storage.
- Time zone preferences.
- Localized date formatting.

Use UTC for persisted timestamps.

## MODULE 20: Data Import and Export

Features:

- CSV import/export.
- Excel import/export.
- JSON export.
- Bulk operations.
- Background import jobs.
- Import validation.
- Import error reports.
- Export history.
- Large dataset streaming.

Use streaming and background processing for large operations.

Do not load huge datasets into memory unnecessarily.

---

# 9. ADDITIONAL REUSABLE INFRASTRUCTURE

Implement these cross-cutting capabilities.

### Domain Events

Provide a lightweight domain event mechanism.

Use it only when it improves separation of business responsibilities.

Do not introduce MediatR.

### Transactional Outbox

Implement an outbox for reliable delivery of events that must not be lost after a database transaction commits.

Include:

- Outbox records.
- Processing worker.
- Retry policy.
- Processing status.
- Error tracking.
- Idempotent processing.
- Retention.

### Idempotency

Implement idempotency support for selected endpoints.

Features:

- Idempotency key.
- Request fingerprint.
- Stored result.
- Expiration.
- Duplicate request handling.
- Concurrent request protection.

### Data Concurrency

Use optimistic concurrency where necessary.

Prevent lost updates.

Return appropriate conflict responses.

### Maintenance Mode

Provide:

- Global maintenance mode.
- Optional module maintenance.
- Configurable maintenance message.
- Administrative bypass.
- Health endpoint exceptions.
- Maintenance audit history.

### Data Retention

Support configurable retention for:

- Audit logs.
- Monitoring metrics.
- Monitoring checks.
- Notifications.
- Background job history.
- Webhook delivery history.
- Temporary files.

---

# 10. DATABASE ARCHITECTURE

Use PostgreSQL as the primary relational database.

Each module owns its DbContext or clearly defined persistence boundary.

Use EF Core directly.

Requirements:

- Entity configurations.
- Explicit relationships.
- Foreign keys.
- Indexes.
- Unique constraints.
- Check constraints where appropriate.
- Database transactions.
- Optimistic concurrency.
- Soft deletion where appropriate.
- UTC timestamps.
- CreatedAt.
- UpdatedAt.
- CreatedBy.
- UpdatedBy.
- TenantId where applicable.
- Migration management.
- Seed data.
- Query optimization.

Use PostgreSQL schemas to organize module-owned tables when appropriate.

Example:

```text
identity.users
identity.refresh_tokens

users.profiles
users.preferences

organizations.organizations
organizations.members

monitoring.endpoints
monitoring.checks
monitoring.incidents
monitoring.alert_rules

audit.audit_logs

notifications.notifications
notifications.templates

files.stored_files

webhooks.registrations
webhooks.deliveries
```

Use explicit database migration ownership.

Do not allow one module to modify another module's tables without an explicitly defined integration requirement.

For read-only queries, use AsNoTracking.

Use Select projections for DTOs.

Implement pagination at the database query level.

Avoid N+1 queries.

Do not expose EF Core entities directly through API responses.

---

# 11. API RESPONSE STANDARDS

Use a consistent API response contract.

Example success response:

```json
{
  "success": true,
  "message": "User retrieved successfully.",
  "data": {
    "id": "00000000-0000-0000-0000-000000000001",
    "email": "user@example.com"
  },
  "traceId": "trace-id"
}
```

Example paginated response:

```json
{
  "success": true,
  "data": [],
  "pagination": {
    "page": 1,
    "pageSize": 20,
    "totalCount": 100,
    "totalPages": 5
  }
}
```

Use RFC 9457 Problem Details for HTTP errors.

Do not wrap every response unnecessarily if doing so conflicts with HTTP semantics or streaming.

Use correct HTTP status codes:

- 200 OK.
- 201 Created.
- 202 Accepted.
- 204 No Content.
- 400 Bad Request.
- 401 Unauthorized.
- 403 Forbidden.
- 404 Not Found.
- 409 Conflict.
- 422 Unprocessable Content where appropriate.
- 429 Too Many Requests.
- 500 Internal Server Error.
- 503 Service Unavailable.

Document all endpoint responses in OpenAPI.

---

# 12. SECURITY REQUIREMENTS

Follow OWASP API Security Top 10 principles.

Implement:

- Secure authentication.
- Permission-based authorization.
- JWT validation.
- Refresh token rotation.
- Password security.
- Account lockout.
- Rate limiting.
- CORS restrictions.
- Input validation.
- SQL injection prevention.
- Secure file uploads.
- Secure webhook signing.
- SSRF prevention.
- Tenant isolation.
- Secret management.
- Security event logging.
- Secure HTTP headers.
- Request size limits.
- Sensitive information filtering.
- Secure error responses.
- Session revocation.

Use least-privilege access.

Never:

- Store plaintext passwords.
- Store plaintext refresh tokens.
- Log secrets.
- Expose signing keys.
- Return internal stack traces to clients.
- Trust tenant IDs without authorization.
- Allow arbitrary URL fetching.
- Expose administrative endpoints publicly.

Security must be implemented at the application and infrastructure levels.

---

# 13. TESTING STRATEGY

Implement real automated tests.

## Unit Tests

Test:

- Domain entities.
- Domain business rules.
- Application services.
- Validators.
- Permission evaluation.
- Monitoring state transitions.
- Incident creation.
- Alert rules.
- Retention logic.
- Idempotency logic.

## Integration Tests

Test:

- Authentication.
- Registration.
- Login.
- Refresh token rotation.
- Permission enforcement.
- User management.
- Database persistence.
- Tenant isolation.
- Monitoring execution.
- Incident lifecycle.
- Notification delivery.
- File storage.
- Background jobs.
- Webhook delivery.
- Caching.

Use PostgreSQL Testcontainers for database integration tests where practical.

## API Tests

Use WebApplicationFactory.

Test:

- Request validation.
- HTTP status codes.
- Authorization.
- Response contracts.
- Pagination.
- Filtering.
- Error handling.
- API versioning.

## Architecture Tests

Enforce:

- No circular dependencies.
- No unauthorized cross-module dependencies.
- Domain independence.
- Application dependency rules.
- Module ownership.
- No MVC Controllers.
- No CQRS/MediatR dependencies.
- No AutoMapper dependencies.

All critical functionality must have meaningful test coverage.

Do not write tests that simply verify that a mock was called without validating useful behavior.

---

# 14. DOCKER AND DEVELOPMENT ENVIRONMENT

Provide a complete Docker development environment.

Required services:

- API.
- PostgreSQL.
- Redis.
- Hangfire infrastructure using the configured database.
- OpenTelemetry Collector.
- Prometheus.
- Grafana.
- Trace backend.

Create:

```text
Dockerfile
docker-compose.yml
docker-compose.override.yml
.env.example
```

Requirements:

- Persistent database storage.
- Redis persistence where appropriate.
- Health checks.
- Service dependencies.
- Environment variables.
- Network isolation.
- Non-root containers where practical.
- Production-safe defaults.
- No committed secrets.

The developer should be able to run:

```bash
docker compose up -d
```

Then:

```bash
dotnet restore
dotnet build
dotnet test
```

Provide database migration commands and instructions.

Do not rely on automatically applying production migrations on every application startup without a controlled deployment strategy.

---

# 15. CONFIGURATION AND ENVIRONMENT MANAGEMENT

Support:

- Development.
- Testing.
- Staging.
- Production.

Use:

```text
appsettings.json
appsettings.Development.json
appsettings.Testing.json
appsettings.Staging.json
appsettings.Production.json
```

Keep secrets outside source control.

Use environment variables and secret managers.

Document configuration keys.

Validate mandatory settings at startup.

Use secure production defaults.

Optional modules must be independently configurable.

---

# 16. PERFORMANCE AND SCALABILITY

Design for maintainable performance.

Implement:

- Asynchronous database operations.
- CancellationToken propagation.
- Database pagination.
- Query projection.
- Appropriate indexes.
- Connection pooling.
- Redis caching.
- Efficient background processing.
- Bounded concurrency.
- HTTP client reuse through IHttpClientFactory.
- Streaming for large file operations.
- Database transaction boundaries.
- Efficient dashboard aggregation.
- Monitoring data aggregation.
- Log and metric retention.

Avoid:

- Unnecessary database round trips.
- Unbounded background queues.
- Excessive database tracking.
- Large in-memory data loads.
- Blocking asynchronous operations.
- Excessive logging.
- Unbounded monitoring history.

The application must support multiple API instances without duplicating scheduled work or creating inconsistent distributed state.

---

# 17. DOCUMENTATION

Create comprehensive documentation.

Required documents:

### README.md
- Project overview.
- Technology stack.
- Prerequisites.
- Installation.
- Configuration.
- Startup instructions.
- Test instructions.

### Architecture Documentation
- Modular monolith overview.
- Architecture diagram.
- Module boundaries.
- Dependency rules.
- Database ownership.
- Communication patterns.
- Transaction strategy.

### Module Documentation

Document every module:

- Purpose.
- Features.
- Endpoints.
- Services.
- Configuration.
- Database tables.
- Dependencies.
- Extension points.

### Developer Documentation

Explain:

- How to create a new module.
- How to register services.
- How to add Minimal API endpoints.
- How to create EF Core entities.
- How to add migrations.
- How to add options.
- How to validate requests.
- How to implement background jobs.
- How to add notification providers.
- How to integrate external services.

### Deployment Documentation

Include:

- Docker deployment.
- Environment configuration.
- Database migration.
- Redis configuration.
- Reverse proxy configuration.
- TLS.
- Monitoring infrastructure.
- Logging.
- Backup and recovery.
- Production security checklist.

---

# 18. IMPLEMENTATION PLAN

Implement the project in controlled phases.

Do not attempt to generate the entire application in one response.

Do not generate thousands of lines of unverified code.

Implement each phase, build it, test it, and fix errors before continuing.

## Phase 1 — Solution Foundation

Implement:

- Solution structure.
- Project references.
- Building blocks.
- Dependency injection.
- Minimal API host.
- Module registration.
- Options Pattern.
- PostgreSQL.
- EF Core.
- Redis.
- Serilog.
- Global exception handling.
- Problem Details.
- OpenAPI.
- Docker.

Deliverable: A working API foundation.

## Phase 2 — Identity and Authorization

Implement:

- ASP.NET Core Identity.
- Registration.
- Login.
- JWT.
- Refresh token rotation.
- Password reset.
- Email verification.
- Users.
- Roles.
- Permissions.
- Authorization policies.
- API keys.

Deliverable: A fully functional authentication and authorization system.

## Phase 3 — Core Platform Modules

Implement:

- Organizations.
- Optional multi-tenancy.
- Audit logging.
- Notifications.
- File storage.
- Background jobs.
- Caching.
- Settings.

Deliverable: Reusable core platform functionality.

## Phase 4 — Monitoring and Observability

Implement:

- Endpoint registration.
- Scheduled monitoring.
- Uptime checks.
- Response time metrics.
- Incident detection.
- Alert management.
- Email and webhook alerts.
- SSL monitoring.
- Monitoring dashboard APIs.
- OpenTelemetry.
- Prometheus.
- Grafana.

Deliverable: A fully functional API monitoring platform.

## Phase 5 — Integration Infrastructure

Implement:

- Webhooks.
- Transactional Outbox.
- Domain events.
- Idempotency.
- API key management improvements.
- Feature flags.
- External integration abstractions.

Deliverable: A reusable integration platform.

## Phase 6 — Enterprise Capabilities

Implement:

- Dashboard analytics.
- Data import/export.
- Localization.
- Maintenance mode.
- Retention policies.
- Advanced security features.
- Multi-tenant enhancements.

Deliverable: Complete enterprise-ready platform capabilities.

## Phase 7 — Quality Assurance

Implement:

- Unit tests.
- Integration tests.
- Functional tests.
- Architecture tests.
- Security tests.
- Performance tests where appropriate.
- Docker verification.
- Documentation review.

Deliverable: Verified and documented platform.

---

# 19. IMPLEMENTATION RULES FOR THE CODING AGENT

Follow these rules throughout the entire development process.

1. Inspect the existing repository before making changes.
2. Preserve existing working functionality unless a change is necessary.
3. Use .NET 10 and compatible stable packages.
4. Use Minimal APIs exclusively.
5. Use application services instead of CQRS.
6. Never introduce MediatR.
7. Never introduce AutoMapper.
8. Use Options Pattern for configuration.
9. Use EF Core directly.
10. Avoid unnecessary generic repositories.
11. Keep domain logic inside domain entities and domain services.
12. Keep endpoints thin.
13. Use DTOs for API contracts.
14. Map DTOs manually.
15. Use FluentValidation.
16. Apply dependency injection consistently.
17. Use CancellationToken for asynchronous operations.
18. Use explicit transaction boundaries.
19. Avoid circular dependencies.
20. Enforce module boundaries.
21. Avoid God classes.
22. Avoid unnecessary abstractions.
23. Do not hardcode secrets or environment-specific settings.
24. Do not leave empty service implementations.
25. Do not create fake responses to simulate working functionality.
26. Do not leave TODOs for required functionality.
27. Use meaningful error handling.
28. Add tests for critical behavior.
29. Build after each meaningful implementation phase.
30. Fix all build errors before moving forward.
31. Do not claim a feature is completed until it actually works.
32. Keep documentation synchronized with implementation.
33. Prefer native .NET capabilities over unnecessary third-party dependencies.
34. Make optional modules independently configurable.
35. Ensure disabling an optional module does not break unrelated modules.

---

# 20. DEFINITION OF DONE

The platform is considered complete only when the following conditions are met.

## Architecture
- [ ] Modular monolith architecture is implemented.
- [ ] Module boundaries are enforced.
- [ ] Minimal APIs are used exclusively.
- [ ] No CQRS or MediatR exists.
- [ ] No AutoMapper exists.
- [ ] Application Service Pattern is implemented.
- [ ] Options Pattern is implemented.
- [ ] EF Core persistence is configured.

## Authentication
- [ ] Registration works.
- [ ] Login works.
- [ ] JWT works.
- [ ] Refresh token rotation works.
- [ ] Password reset works.
- [ ] Role authorization works.
- [ ] Permission authorization works.
- [ ] Session revocation works.

## Core Features
- [ ] User management works.
- [ ] Audit logging works.
- [ ] Notifications work.
- [ ] File storage works.
- [ ] Background jobs work.
- [ ] Redis caching works.
- [ ] Settings management works.

## Monitoring
- [ ] Endpoints can be registered.
- [ ] Monitoring jobs execute.
- [ ] Uptime is calculated.
- [ ] Response times are recorded.
- [ ] Incidents are detected.
- [ ] Alerts are delivered.
- [ ] Recovery is detected.
- [ ] Historical metrics are available.
- [ ] Dashboard APIs work.

## Infrastructure
- [ ] PostgreSQL works.
- [ ] Redis works.
- [ ] Docker Compose works.
- [ ] Health checks work.
- [ ] Structured logging works.
- [ ] OpenTelemetry exports work.
- [ ] Prometheus metrics are available.
- [ ] Grafana dashboards are configured.

## Quality
- [ ] Solution builds.
- [ ] Database migrations work.
- [ ] Automated tests pass.
- [ ] Architecture tests pass.
- [ ] Security requirements are addressed.
- [ ] Documentation is complete.

---

# 21. FINAL DELIVERABLES

At the end of implementation, provide:

1. Complete source code.
2. Complete solution and project structure.
3. All module implementations.
4. PostgreSQL database schema.
5. EF Core migrations.
6. API endpoint documentation.
7. OpenAPI specification.
8. Docker and Docker Compose files.
9. Environment configuration examples.
10. Unit tests.
11. Integration tests.
12. Architecture tests.
13. Monitoring infrastructure configuration.
14. README.
15. Architecture documentation.
16. Module development guide.
17. Deployment guide.
18. Production readiness checklist.

Also provide a module implementation report with these statuses:

- Completed.
- Partially completed.
- Not implemented.
- Requires external credentials or infrastructure.

Never mark a feature completed simply because its folder, interface, or database entity exists.

---

# 22. FINAL INSTRUCTION

Start by inspecting the repository and identifying its current state.

If it is empty, create the solution from scratch.

If it already contains code, inspect and preserve reusable functionality.

Then:

1. Propose the final architecture.
2. Explain the project structure.
3. Create the solution.
4. Implement Phase 1.
5. Build the solution.
6. Run tests.
7. Fix all errors.
8. Document the completed phase.
9. Continue with the next phase.

Work incrementally and autonomously within the available repository and environment.

Do not stop after presenting architecture diagrams or code examples.

Actually create the files, implement the functionality, configure the dependencies, run the commands, and verify the results.

If a required external service or credential is unavailable, implement the real provider abstraction, provide a usable local development implementation where appropriate, and clearly document what remains.

Maintain an implementation checklist so progress is never confused with completion.

**THE ULTIMATE GOAL**

Build a reusable, secure, scalable, production-ready .NET 10 Modular Monolith API Platform.

A developer should be able to start a new project, enable the modules they need, implement project-specific business functionality, and immediately reuse the platform's authentication, authorization, users, monitoring, audit logging, notifications, file storage, caching, background jobs, webhooks, observability, and infrastructure.

The result must be practical, maintainable, testable, extensible, and ready to serve as the foundation of real-world applications.

**Architecture philosophy: Keep it modular, keep it simple, enforce boundaries, prefer native .NET features, and never introduce complexity without a concrete benefit.**