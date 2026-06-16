# Tasks: HomeContentLock 01 — Desktop Base

**Repository**: `home-content-lock-net` — https://github.com/wesxavolimon/home-content-lock-net.git  
**Solution Name**: `HomeContentLock`  
**Status**: Ready for implementation  
**Scope**: MVP v1.0 — CLI interface, SQLite persistence, status monitoring, no protection yet  
**Tech Stack**: .NET 8.0, System.CommandLine 2.0+, Entity Framework Core, SQLite, Serilog, xUnit  
**Total Groups**: 5 | **Estimated Duration**: 4-5 weeks (1 developer)

---

## 0. Local Setup & Branch

### 0.1 Clone Repository

```bash
cd C:\Users\User\Documents\TI\Lavi\Web\Genova\01_TRABALHO_EXTERNO

# Clone if not exists
git clone https://github.com/wesxavolimon/home-content-lock-net.git HomeContentLockNet
cd HomeContentLockNet

# Configure git
git config user.name "Your Name"
git config user.email "your.email@example.com"
```

**Verification**:
- [x] Repository cloned to correct location
- [x] Git configured locally
- [x] Working directory: `01_TRABALHO_EXTERNO/HomeContentLockNet`

### 0.2 Create Feature Branch

```bash
git checkout develop
git pull origin develop
git checkout -b feature/home-content-lock-01-desktop-base
```

**Verification**:
- [x] Branch created locally
- [x] Branch name: `feature/home-content-lock-01-desktop-base`
- [x] Base: `develop` (current HEAD)

---

## 1. Project Setup

### 1.1 Create Solution Structure

**Create directory**:
```
src/
  HomeContentLock.Domain/
  HomeContentLock.Application/
  HomeContentLock.Infrastructure/
  HomeContentLock.Presentation/
  HomeContentLock.sln
tests/
  HomeContentLock.Domain.Tests/
  HomeContentLock.Application.Tests/
  HomeContentLock.Infrastructure.Tests/
  HomeContentLock.Presentation.Tests/
docker-compose.yml
Dockerfile
```

**Tasks**:
- [x] Create solution file: `HomeContentLock.sln`
- [x] Create 4 class libraries (.NET 8.0):
  - `HomeContentLock.Domain` (no dependencies except System)
  - `HomeContentLock.Application` (depends on Domain)
  - `HomeContentLock.Infrastructure` (depends on Domain)
  - `HomeContentLock.Presentation` (depends on Application + Infrastructure)
- [x] Add NuGet packages (common):
  - System.CommandLine (2.0+)
  - Entity.Framework.Core (8.0)
  - Entity.Framework.Core.SQLite (8.0)
  - Serilog (3.0+)
  - Serilog.Sinks.Console
  - Serilog.Sinks.File
  - xUnit (2.4+)
  - Moq (4.16+)
  - Testcontainers (3.0+)
- [x] Create `.gitignore` (standard .NET)
- [x] Create `README.md` with project overview

**Verification**:
- [x] Solution compiles: `dotnet build`
- [x] All 4 projects reference correctly
- [x] No circular dependencies

### 1.2 Create Database Schema

**File**: `HomeContentLock.Infrastructure/Persistence/schema.sql`

```sql
CREATE TABLE IF NOT EXISTS blocker_logs (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    timestamp TEXT NOT NULL,
    action TEXT NOT NULL,
    status TEXT NOT NULL,
    details TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS blocker_config (
    key TEXT PRIMARY KEY,
    value TEXT NOT NULL
);

INSERT OR IGNORE INTO blocker_config VALUES ('initialized', '1');
INSERT OR IGNORE INTO blocker_config VALUES ('current_status', 'DISABLED');
INSERT OR IGNORE INTO blocker_config VALUES ('last_password_change', '2026-06-15T00:00:00Z');
```

**Tasks**:
- [x] Create schema.sql file
- [x] Create database migration script
- [ ] Test schema with TestContainers.Sqlite

**Verification**:
- [ ] Schema creates tables successfully
- [ ] Config table has seed data
- [ ] Can execute INSERT/SELECT queries

---

## 2. Domain Layer

### 2.1 Create Domain Entities

**File**: `HomeContentLock.Domain/Entities/`

**Tasks**:
- [x] `BlockerStatus.cs` — enum: Enabled, Disabled, Error
- [x] `LogEntry.cs` — entity: timestamp, action, status, details
- [x] `ActivationSecret.cs` — entity: password_hash, created_at, algorithm
- [x] `CustomBlockedSite.cs` — entity: domain, added_at, is_active

**Specification**:
```csharp
// BlockerStatus.cs
public enum BlockerStatus
{
    Disabled,
    Enabled,
    Error
}

// LogEntry.cs
public class LogEntry
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string Action { get; set; }        // ENABLE, DISABLE, SITE_ADDED, etc
    public string Status { get; set; }        // SUCCESS, FAILED, PENDING
    public string Details { get; set; }       // JSON
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

// CustomBlockedSite.cs
public class CustomBlockedSite
{
    public string Domain { get; set; }
    public DateTime AddedAt { get; set; }
    public bool IsActive { get; set; } = true;
}
```

**Verification**:
- [x] All entities compile
- [x] No external dependencies
- [x] Properties are public + property initialization

### 2.2 Create Domain Exceptions

**File**: `HomeContentLock.Domain/Exceptions/`

**Tasks**:
- [x] `BlockerException.cs` — base exception
- [x] `InvalidPasswordException.cs` — password validation failure
- [x] `DatabaseException.cs` — database operation failure

**Verification**:
- [x] All exceptions inherit from `Exception`
- [x] All have meaningful constructors
- [x] Can be thrown and caught

### 2.3 Create Domain Interfaces

**File**: `HomeContentLock.Domain/Interfaces/`

**Tasks**:
- [x] `IBlockerRepository.cs` — Save/load logs, config, custom sites
- [x] `IPasswordValidator.cs` — Validate password hash

**Specification**:
```csharp
public interface IBlockerRepository
{
    Task<List<LogEntry>> GetLogsAsync(int limit = 100);
    Task<List<LogEntry>> GetLogsWithFilterAsync(string action, int limit = 100);
    Task SaveLogAsync(LogEntry log);
    Task<BlockerStatus> GetCurrentStatusAsync();
    Task UpdateStatusAsync(BlockerStatus status);
    Task<List<CustomBlockedSite>> GetCustomSitesAsync();
    Task SaveCustomSiteAsync(CustomBlockedSite site);
    Task DeleteCustomSiteAsync(string domain);
}

public interface IPasswordValidator
{
    Task<bool> ValidateAsync(string password);
    Task SavePasswordHashAsync(string passwordHash);
}
```

**Verification**:
- [x] All interfaces are clean and cohesive
- [x] Methods are async-friendly
- [x] Return types are clear

---

## 3. Infrastructure Layer

### 3.1 Implement SQLite Repository

**File**: `HomeContentLock.Infrastructure/Persistence/SqliteBlockerRepository.cs`

**Tasks**:
- [ ] Create `BlockerDatabase` (DbContext)
- [ ] Create `DbSet` for LogEntry, CustomBlockedSite
- [ ] Implement `IBlockerRepository`:
  - `GetLogsAsync()` — SELECT with ordering
  - `SaveLogAsync()` — INSERT with transaction
  - `GetCurrentStatusAsync()` — SELECT from config
  - `UpdateStatusAsync()` — UPDATE config
  - `GetCustomSitesAsync()` — SELECT WHERE isActive=1
  - `SaveCustomSiteAsync()` — INSERT OR REPLACE
  - `DeleteCustomSiteAsync()` — soft delete (set isActive=0)

**Verification**:
- [ ] Compiles without errors
- [ ] DbContext initializes
- [ ] Migrations work: `dotnet ef migrations add InitialCreate`
- [ ] Database can be created from schema

### 3.2 Implement Password Validator (SHA-256)

**File**: `HomeContentLock.Infrastructure/Security/FilePasswordValidator.cs`

**Tasks**:
- [ ] Create `SecretsFile` class (read/write secrets.json)
- [ ] Implement `IPasswordValidator`:
  - `ValidateAsync()` — hash input, compare with stored
  - `SavePasswordHashAsync()` — create/update secrets.json
- [ ] File location: `~/.config/HomeContentLock/secrets.json`

**Specification**:
```json
{
  "version": "1.0",
  "password_hash": "abc123...",
  "created_at": "2026-06-15T10:00:00Z",
  "algorithm": "SHA256"
}
```

**Verification**:
- [ ] Secrets file created in correct location
- [ ] Hash is reproducible (same password = same hash)
- [ ] Stored hash cannot be easily reversed

### 3.3 Create Test Infrastructure

**File**: `HomeContentLock.Infrastructure.Tests/`

**Tasks**:
- [ ] Create `TestDatabaseFixture` using TestContainers.Sqlite
- [ ] Create `RepositoryTests` with real SQLite:
  - Test CRUD operations
  - Test queries with filters
  - Test transactions
- [ ] Create `PasswordValidatorTests` with real file I/O:
  - Test hash creation
  - Test validation (valid/invalid passwords)
  - Test file updates

**Verification**:
- [ ] All infrastructure tests pass
- [ ] 100% coverage of repository methods
- [ ] TestContainers starts/stops cleanly

---

## 4. Application Layer

### 4.1 Create Use Cases

**File**: `HomeContentLock.Application/UseCases/`

**Tasks**:
- [ ] `GetStatusUseCase.cs` — Query current status + log count
- [ ] `EnableBlockerUseCase.cs` — Mark as enabled, create log entry
- [ ] `DisableBlockerUseCase.cs` — Require password, mark as disabled
- [ ] `QueryLogsUseCase.cs` — Fetch logs with limit/filter
- [ ] `SetPasswordUseCase.cs` — Update password, create log entry
- [ ] `AddCustomSiteUseCase.cs` — Add site to blocklist
- [ ] `RemoveCustomSiteUseCase.cs` — Remove site from blocklist

**Specification** (example):
```csharp
public class EnableBlockerUseCase
{
    private readonly IBlockerRepository _repository;
    private readonly IPasswordValidator _validator;

    public async Task<Result> ExecuteAsync(string password)
    {
        if (string.IsNullOrEmpty(password))
            throw new InvalidPasswordException("Password required");

        var isValid = await _validator.ValidateAsync(password);
        if (!isValid)
            throw new InvalidPasswordException("Invalid password");

        var log = new LogEntry
        {
            Timestamp = DateTime.UtcNow,
            Action = "ENABLE",
            Status = "SUCCESS"
        };

        await _repository.SaveLogAsync(log);
        await _repository.UpdateStatusAsync(BlockerStatus.Enabled);

        return Result.Success();
    }
}
```

**Verification**:
- [ ] All use cases compile
- [ ] Each use case is focused (single responsibility)
- [ ] Dependencies injected via constructor

### 4.2 Create Application Service

**File**: `HomeContentLock.Application/Services/BlockerService.cs`

**Tasks**:
- [ ] Orchestrate all use cases
- [ ] Provide single entry point for business logic
- [ ] Handle cross-cutting concerns (logging, error handling)

**Verification**:
- [ ] Service can call each use case
- [ ] Errors are propagated correctly

### 4.3 Create Application Tests

**File**: `HomeContentLock.Application.Tests/`

**Tasks**:
- [ ] Create `EnableBlockerUseCaseTests` with mocks:
  - Valid password → creates log
  - Invalid password → throws exception
- [ ] Create `DisableBlockerUseCaseTests`:
  - Valid password → updates status
  - Invalid password → throws exception
- [ ] Create `AddCustomSiteUseCaseTests`:
  - Valid domain → saved to repo
  - Duplicate domain → handled correctly
- [ ] Similar for all other use cases

**Verification**:
- [ ] All application tests pass
- [ ] 100% coverage of use case logic
- [ ] Mocks are used correctly

---

## 5. Presentation Layer (CLI)

### 5.1 Create Commands

**File**: `HomeContentLock.Presentation/Commands/`

**Tasks**:
- [ ] `StatusCommand.cs` — Display blocker status
- [ ] `EnableCommand.cs` — Enable blocker (requires --password)
- [ ] `DisableCommand.cs` — Disable blocker (prompts for password)
- [ ] `LogsCommand.cs` — Show logs (supports --lines, --output json|table)
- [ ] `PasswordCommand.cs` — Update password
- [ ] `SitesAddCommand.cs` — Add custom blocked site
- [ ] `SitesRemoveCommand.cs` — Remove custom blocked site
- [ ] `SitesListCommand.cs` — List blocked sites
- [ ] `SitesClearCommand.cs` — Clear all custom sites

**Specification** (example):
```csharp
public class StatusCommand : Command
{
    public StatusCommand() : base("status", "Show blocker status")
    {
        AddAlias("st");
    }

    public override async Task<int> InvokeAsync(InvocationContext context)
    {
        var service = ServiceProvider.GetService<BlockerService>();
        var status = await service.GetStatusAsync();
        Console.WriteLine($"Status: {status.State}");
        Console.WriteLine($"Logs: {status.LogCount} entries");
        return 0;
    }
}
```

### 5.2 Create Output Formatters

**File**: `HomeContentLock.Presentation/Output/`

**Tasks**:
- [ ] `ConsoleFormatter.cs` — Pretty print tables + text
- [ ] `JsonFormatter.cs` — Output as JSON
- [ ] `MarkdownFormatter.cs` — Output as markdown table

**Verification**:
- [ ] Formatters produce clean output
- [ ] JSON is valid
- [ ] Tables are aligned

### 5.3 Create Program Entry Point

**File**: `HomeContentLock.Presentation/Program.cs`

**Tasks**:
- [ ] Setup dependency injection (IServiceCollection)
- [ ] Configure Serilog logging
- [ ] Register all use cases
- [ ] Setup System.CommandLine with all commands
- [ ] Handle global error handling

**Verification**:
- [ ] Application starts without errors
- [ ] Help text displays: `blocker --help`
- [ ] Each command shows help: `blocker status --help`

### 5.4 Create CLI Tests

**File**: `HomeContentLock.Presentation.Tests/`

**Tasks**:
- [ ] Create `ProgramTests`:
  - Test help output
  - Test command parsing
  - Test exit codes (0 for success, 1 for error)
- [ ] Create `CommandTests` for each command:
  - Status command returns exit code 0
  - Enable command with invalid password returns exit code 1

**Verification**:
- [ ] All CLI tests pass
- [ ] Exit codes are correct
- [ ] Help messages are clear

---

## 6. Integration Testing

### 6.1 Create End-to-End Test

**File**: `HomeContentLock.Tests.Integration/`

**Tasks**:
- [ ] Create `E2ETests` that:
  - Start app with `blocker status` → returns DISABLED
  - Run `blocker enable --password test123` → updates status
  - Run `blocker logs --output json` → returns valid JSON
  - Run `blocker sites add google.com --password test123` → saves site
  - Run `blocker sites list` → shows google.com
  - Run `blocker disable` (prompts and enters password) → updates status

**Verification**:
- [ ] Full flow works end-to-end
- [ ] Database persists across commands
- [ ] All output formats work

---

## 7. Documentation & Deployment

### 7.1 Docker Setup

**Tasks**:
- [ ] Create `Dockerfile` (multi-stage):
  - Build stage: .NET SDK 8.0
  - Runtime stage: .NET 8.0 runtime (slim)
  - Self-contained: no additional .NET installation required
- [ ] Create `docker-compose.yml`:
  - Service: blocker CLI
  - Volume: ~/.config/HomeContentLock/
  - Working dir: /app

**Verification**:
- [ ] Docker image builds: `docker build -t home-content-lock:latest .`
- [ ] Image size is reasonable (< 200MB)
- [ ] Container runs CLI: `docker run home-content-lock blocker status`

### 7.2 Create Documentation

**Tasks**:
- [ ] Update `README.md`:
  - Project overview
  - Installation (binary, Docker)
  - Usage examples (all 8 commands)
  - Configuration (secrets.json location)
  - Contributing guidelines
- [ ] Create `CHANGELOG.md` (v1.0 entry)
- [ ] Create `docs/ARCHITECTURE.md` (high-level overview)

**Verification**:
- [ ] README is clear and complete
- [ ] All commands are documented with examples
- [ ] Links work

### 7.3 Final Testing Checklist

**Tasks**:
- [ ] All unit tests pass: `dotnet test`
- [ ] All integration tests pass
- [ ] Code coverage > 85%: `dotnet test /p:CollectCoverage=true`
- [ ] No compiler warnings
- [ ] Markdown linting passes
- [ ] Docker image builds and runs
- [ ] Manual testing of all commands

**Verification**:
- [ ] `dotnet test --configuration Release` shows all green ✓
- [ ] Build produces no warnings
- [ ] Docker runs successfully

---

## 8. Commit & Push

**Tasks**:
- [ ] Commit all changes (grouped by functionality)
  - Commit 1: "feat: setup project structure + domain layer"
  - Commit 2: "feat: implement infrastructure (SQLite + password validator)"
  - Commit 3: "feat: implement application use cases"
  - Commit 4: "feat: implement CLI commands"
  - Commit 5: "feat: add Docker setup + documentation"
- [ ] Push to remote: `git push origin feature/home-content-lock-01-desktop-base`
- [ ] Create Pull Request (see `proposal-v1-0-overview.md` for PR template)

**Verification**:
- [ ] All commits have meaningful messages
- [ ] Git history is clean and linear
- [ ] Branch is up to date with develop before PR

---

## Summary

**This task list covers:**
- ✅ Complete Clean Architecture implementation
- ✅ CLI interface with 8+ commands
- ✅ SQLite persistence with migration
- ✅ Password validation (SHA-256)
- ✅ Custom site blocking foundation
- ✅ 100% test coverage
- ✅ Docker containerization
- ✅ Comprehensive documentation

**Not included (v2.0+):**
- ❌ Desktop UI (Electron)
- ❌ Web Dashboard
- ❌ API REST
- ❌ Windows protection (see #03)
- ❌ Android app (see #02)

**Estimated Effort**: 4-5 weeks (1 senior .NET developer)
