# HomeContentLock — Desktop Base (v1.0)

**A content blocking solution for Windows with local persistence and activity logging.**

---

## 🏗️ Architecture

\\\
┌─────────────────────────────────────────────────────────┐
│         Presentation Layer (CLI)                        │
│  ┌───────────────────────────────────────────────────┐  │
│  │  Program.cs                                       │  │
│  │  ├─ status     → get blocker status              │  │
│  │  ├─ enable     → activate content blocker        │  │
│  │  ├─ disable    → deactivate content blocker      │  │
│  │  └─ logs       → view activity history           │  │
│  └───────────────────────────────────────────────────┘  │
│         System.CommandLine (2.0+)                       │
└────────────┬────────────────────────────────────────────┘
             │ depends on
             ▼
┌─────────────────────────────────────────────────────────┐
│      Application Layer (Use Cases)                      │
│  ┌───────────────────────────────────────────────────┐  │
│  │ • GetStatusUseCase                                │  │
│  │ • EnableBlockerUseCase                            │  │
│  │ • DisableBlockerUseCase                           │  │
│  │ • QueryLogsUseCase                                │  │
│  └───────────────────────────────────────────────────┘  │
│         Orchestrates domain logic & repositories        │
└────────────┬────────────────────────────────────────────┘
             │ depends on
             ▼
┌─────────────────────────────────────────────────────────┐
│    Infrastructure Layer (Repositories & Services)       │
│  ┌───────────────────────────────────────────────────┐  │
│  │ • SqliteBlockerRepository (IBlockerRepository)   │  │
│  │   ├─ SaveLog, GetLogs                            │  │
│  │   ├─ GetStatus, UpdateStatus                     │  │
│  │   ├─ SaveCustomSite, DeleteCustomSite            │  │
│  │   └─ BlockerDatabase (DbContext)                 │  │
│  │                                                   │  │
│  │ • PasswordValidator (IPasswordValidator)         │  │
│  │   ├─ ValidateAsync                               │  │
│  │   └─ SavePasswordHashAsync                       │  │
│  └───────────────────────────────────────────────────┘  │
│   Entity Framework Core + SQLite                        │
└────────────┬────────────────────────────────────────────┘
             │ implements contracts from
             ▼
┌─────────────────────────────────────────────────────────┐
│         Domain Layer (Contracts & Entities)             │
│  ┌───────────────────────────────────────────────────┐  │
│  │ Interfaces:                                       │  │
│  │  • IBlockerRepository                             │  │
│  │  • IPasswordValidator                             │  │
│  │                                                   │  │
│  │ Entities:                                         │  │
│  │  • BlockerStatus (enum)                           │  │
│  │  • LogEntry                                       │  │
│  │  • CustomBlockedSite                              │  │
│  │  • ActivationSecret                               │  │
│  │                                                   │  │
│  │ Exceptions:                                       │  │
│  │  • BlockerException (base)                        │  │
│  │  • InvalidPasswordException                       │  │
│  │  • DatabaseException                              │  │
│  └───────────────────────────────────────────────────┘  │
│         Zero external dependencies                      │
└─────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────┐
│      CrossCutting Layer (DI Configuration)              │
│  • ServiceCollectionExtensions                          │
│  • Registers repositories, use cases, database          │
│  • Serilog logging configuration                        │
└─────────────────────────────────────────────────────────┘
\\\

---

## 📁 Project Structure

\\\
HomeContentLockNet/
├── src/
│   ├── HomeContentLock.sln              # Solution file
│   ├── HomeContentLock.Domain/          # Pure domain contracts
│   ├── HomeContentLock.Application/     # Use cases & orchestration
│   ├── HomeContentLock.Infrastructure/  # Data access & services
│   ├── HomeContentLock.Presentation/    # CLI entry point
│   └── HomeContentLock.CrossCutting/    # DI & utilities
│
├── tests/
│   ├── HomeContentLock.Domain.Tests/
│   ├── HomeContentLock.Application.Tests/
│   ├── HomeContentLock.Infrastructure.Tests/
│   └── HomeContentLock.Presentation.Tests/
│
└── README.md (this file)
\\\

---

## 🛠️ Tech Stack

| Component | Technology | Version |
|-----------|-----------|---------|
| Runtime | .NET | 10.0+ |
| ORM | Entity Framework Core | 10.0+ |
| Database | SQLite | Latest |
| CLI | System.CommandLine | 2.0+ |
| Logging | Serilog | 3.0+ |
| Testing | xUnit + Moq | Latest |

---

## 🚀 Quick Start

### Build
\\\ash
dotnet build -c Release
\\\

### Run Tests
\\\ash
dotnet test
\\\

### Run CLI
\\\ash
dotnet run --project src/HomeContentLock.Presentation -- status
\\\

---

## 📋 CLI Commands

- **status** — Get current blocker status
- **enable** — Activate the content blocker
- **disable** — Deactivate the content blocker
- **logs** — View activity history (last 50 entries)

---

## 🏢 Clean Architecture

`
Domain (Entities, Interfaces, Exceptions)
    ↑
Application (Use Cases)
    ↑
Infrastructure (Repositories, Services)
    ↑
Presentation (CLI)
`

**Dependency Rule:** Inner layers never depend on outer layers.

---

**Built with ❤️ using .NET 10.0 Clean Architecture**
