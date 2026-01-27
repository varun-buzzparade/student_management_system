# Technical Stack Documentation

## Table of Contents

1. [Technology Overview](#technology-overview)
2. [Architecture](#architecture)
3. [Backend Technologies](#backend-technologies)
4. [Frontend Technologies](#frontend-technologies)
5. [Database](#database)
6. [Dependencies](#dependencies)
7. [Project Structure](#project-structure)
8. [Design Patterns](#design-patterns)
9. [Security Implementation](#security-implementation)
10. [Development & Deployment](#development--deployment)

---

## Technology Overview

### Core Framework
- **Platform**: .NET 10.0
- **Framework**: ASP.NET Core MVC
- **Language**: C# 13
- **Pattern**: Model-View-Controller (MVC)

### Database
- **Database Engine**: PostgreSQL 15+
- **ORM**: Entity Framework Core 10.0
- **Provider**: Npgsql.EntityFrameworkCore.PostgreSQL 10.0

### Identity & Authentication
- **Framework**: ASP.NET Core Identity
- **Package**: Microsoft.AspNetCore.Identity.EntityFrameworkCore 10.0

---

## Architecture

### Application Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                         Presentation Layer                   │
│                    (Views - Razor Pages)                     │
└────────────────────────────┬────────────────────────────────┘
                             │
┌────────────────────────────┴────────────────────────────────┐
│                      Controller Layer                        │
│  (AccountController, AdminController, StudentController)     │
└────────────────────────────┬────────────────────────────────┘
                             │
┌────────────────────────────┴────────────────────────────────┐
│                       Service Layer                          │
│      (EmailSender, StudentIdGenerator, PasswordGenerator)    │
└────────────────────────────┬────────────────────────────────┘
                             │
┌────────────────────────────┴────────────────────────────────┐
│                       Data Access Layer                      │
│        (ApplicationDbContext, Entity Framework Core)         │
└────────────────────────────┬────────────────────────────────┘
                             │
┌────────────────────────────┴────────────────────────────────┐
│                       Database Layer                         │
│                     (PostgreSQL Database)                    │
└─────────────────────────────────────────────────────────────┘
```

### MVC Pattern Implementation

**Models**:
- `ApplicationUser.cs` - Extended Identity user model
- `Gender.cs` - Enum for gender values
- `Roles.cs` - Static role constants
- `ErrorViewModel.cs` - Error handling model

**Views**:
- Razor Pages (.cshtml)
- Layout templates
- Partial views
- Shared components

**Controllers**:
- `AccountController` - Authentication & registration
- `AdminController` - Admin operations
- `StudentController` - Student self-service
- `HomeController` - Public pages

---

## Backend Technologies

### 1. ASP.NET Core MVC

**Version**: 10.0

**Key Features Used**:
- Model-View-Controller pattern
- Routing and URL generation
- Dependency Injection
- Middleware pipeline
- Model binding and validation
- Anti-forgery tokens

**Configuration**:
```csharp
// Program.cs
builder.Services.AddControllersWithViews();
```

### 2. ASP.NET Core Identity

**Version**: 10.0

**Purpose**: User authentication and authorization

**Configuration**:
```csharp
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireDigit = true;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();
```

**Features Implemented**:
- User registration
- Login/logout
- Password hashing (PBKDF2)
- Role-based authorization
- Cookie authentication
- User management

### 3. Entity Framework Core

**Version**: 10.0

**Provider**: Npgsql.EntityFrameworkCore.PostgreSQL 10.0

**Purpose**: Object-Relational Mapping (ORM)

**Features Used**:
- Code-First migrations
- LINQ queries
- Change tracking
- Relationship management
- Database seeding

**Database Context**:
```csharp
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
}
```

### 4. Custom Services

#### EmailSenderService
```csharp
public interface IEmailSenderService
{
    Task SendEmailAsync(string toEmail, string subject, string htmlBody);
}
```
- SMTP email delivery
- HTML email support
- Error handling
- Configurable settings

#### StudentIdGenerator
- Generates unique student IDs
- Format: STU + 5-digit number
- Collision detection
- Async database checking

#### PasswordGenerator
- Secure random password generation
- Meets complexity requirements
- Uses cryptographic random number generator

---

## Frontend Technologies

### 1. Razor View Engine

**Purpose**: Server-side HTML generation

**Features**:
- C# code in views
- Layout pages
- Partial views
- Tag helpers
- ViewModels binding

**Example**:
```cshtml
@model StudentRegistrationViewModel

@if (User.IsInRole("Admin"))
{
    <a asp-controller="Admin" asp-action="Index">Dashboard</a>
}
```

### 2. Bootstrap 5

**Version**: 5.x

**Purpose**: CSS framework for responsive design

**Components Used**:
- Grid system
- Forms and input groups
- Tables
- Navigation bars
- Buttons and alerts
- Cards

**Implementation**:
```html
<link rel="stylesheet" href="~/lib/bootstrap/dist/css/bootstrap.min.css" />
```

### 3. jQuery

**Version**: Latest

**Purpose**: Client-side JavaScript library

**Usage**:
- Form validation
- AJAX requests (potential)
- DOM manipulation
- Bootstrap dependencies

### 4. Client-Side Validation

**Technology**: jQuery Validation + Unobtrusive Validation

**Features**:
- Real-time form validation
- Custom validation rules
- Server-side validation fallback

---

## Database

### Database Management System

**DBMS**: PostgreSQL 15+

**Connection String Format**:
```
Host=localhost;Port=5432;Database=student_management_system;Username=postgres;Password=your_password
```

### Database Schema

#### AspNetUsers Table (Extended)
```sql
CREATE TABLE "AspNetUsers" (
    "Id" text PRIMARY KEY,
    "UserName" text,
    "NormalizedUserName" text,
    "Email" text,
    "NormalizedEmail" text,
    "EmailConfirmed" boolean,
    "PasswordHash" text,
    "SecurityStamp" text,
    "ConcurrencyStamp" text,
    "PhoneNumber" text,
    "PhoneNumberConfirmed" boolean,
    "TwoFactorEnabled" boolean,
    "LockoutEnd" timestamp,
    "LockoutEnabled" boolean,
    "AccessFailedCount" integer,
    -- Custom fields
    "FullName" text NOT NULL,
    "Age" integer NOT NULL,
    "HeightCm" decimal(5,2) NOT NULL,
    "Gender" text NOT NULL,
    "MobileNumber" text,
    "StudentId" text,
    "CreatedAt" timestamp NOT NULL
);
```

#### AspNetRoles Table
```sql
CREATE TABLE "AspNetRoles" (
    "Id" text PRIMARY KEY,
    "Name" text,
    "NormalizedName" text,
    "ConcurrencyStamp" text
);
```

#### AspNetUserRoles Table (Junction)
```sql
CREATE TABLE "AspNetUserRoles" (
    "UserId" text,
    "RoleId" text,
    PRIMARY KEY ("UserId", "RoleId"),
    FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id"),
    FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles" ("Id")
);
```

### Migrations

**Tool**: Entity Framework Core Migrations

**Commands**:
```bash
# Create new migration
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update

# Remove last migration (if not applied)
dotnet ef migrations remove
```

**Current Migrations**:
- `InitialCreate` - Creates all Identity tables and custom fields

---

## Dependencies

### NuGet Packages

```xml
<ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="10.0.2" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.2">
        <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
        <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="10.0.0" />
</ItemGroup>
```

### Package Details

| Package | Version | Purpose |
|---------|---------|---------|
| Microsoft.AspNetCore.Identity.EntityFrameworkCore | 10.0.2 | Identity system with EF Core |
| Microsoft.EntityFrameworkCore.Design | 10.0.2 | Design-time EF Core tools |
| Npgsql.EntityFrameworkCore.PostgreSQL | 10.0.0 | PostgreSQL provider for EF Core |

---

## Project Structure

```
student_management_system/
│
├── Controllers/                          # MVC Controllers
│   ├── AccountController.cs             # Auth & registration
│   ├── AdminController.cs               # Admin operations
│   ├── HomeController.cs                # Public pages
│   └── StudentController.cs             # Student self-service
│
├── Data/                                 # Database & seeding
│   ├── ApplicationDbContext.cs          # EF Core context
│   └── IdentitySeeder.cs                # Initial data seeding
│
├── Migrations/                           # EF Core migrations
│   ├── 20260126115729_InitialCreate.cs
│   ├── 20260126115729_InitialCreate.Designer.cs
│   └── ApplicationDbContextModelSnapshot.cs
│
├── Models/                               # Domain models
│   ├── ApplicationUser.cs               # Extended user model
│   ├── ErrorViewModel.cs                # Error handling
│   ├── Gender.cs                        # Gender enum
│   └── Roles.cs                         # Role constants
│
├── Properties/
│   └── launchSettings.json              # Development settings
│
├── Services/                             # Business logic
│   ├── IEmailSenderService.cs           # Email interface
│   ├── PasswordGenerator.cs             # Password generation
│   ├── SmtpEmailSender.cs               # SMTP implementation
│   ├── SmtpSettings.cs                  # SMTP configuration
│   └── StudentIdGenerator.cs            # Student ID generation
│
├── ViewModels/                           # Data transfer objects
│   ├── AdminStudentEditViewModel.cs
│   ├── AdminStudentListItemViewModel.cs
│   ├── AdminStudentListViewModel.cs
│   ├── PagedResult.cs
│   ├── StudentListQueryViewModel.cs
│   ├── StudentProfileViewModel.cs
│   └── StudentRegistrationViewModel.cs
│
├── Views/                                # Razor views
│   ├── Account/
│   │   ├── AccessDenied.cshtml
│   │   ├── Login.cshtml
│   │   └── Register.cshtml
│   ├── Admin/
│   │   ├── Details.cshtml
│   │   ├── Edit.cshtml
│   │   └── Index.cshtml
│   ├── Home/
│   │   ├── Index.cshtml
│   │   └── Privacy.cshtml
│   ├── Shared/
│   │   ├── _Layout.cshtml
│   │   ├── _ValidationScriptsPartial.cshtml
│   │   └── Error.cshtml
│   ├── Student/
│   │   └── Profile.cshtml
│   ├── _ViewImports.cshtml
│   └── _ViewStart.cshtml
│
├── wwwroot/                              # Static files
│   ├── css/
│   ├── js/
│   └── lib/
│
├── appsettings.json                      # Configuration
├── appsettings.Development.json          # Dev configuration
├── Program.cs                            # Application entry point
└── student_management_system.csproj      # Project file
```

---

## Design Patterns

### 1. Model-View-Controller (MVC)
- Separation of concerns
- Testable components
- Clear responsibility boundaries

### 2. Repository Pattern
- Abstracted via Entity Framework Core
- DbContext acts as repository
- Unit of Work pattern included

### 3. Dependency Injection
```csharp
// Service registration
builder.Services.AddScoped<IEmailSenderService, SmtpEmailSender>();

// Constructor injection
public class AccountController : Controller
{
    private readonly IEmailSenderService _emailSender;
    
    public AccountController(IEmailSenderService emailSender)
    {
        _emailSender = emailSender;
    }
}
```

### 4. ViewModel Pattern
- Separate DTOs for views
- Prevents over-posting
- Cleaner validation

### 5. Service Layer Pattern
- Business logic separation
- Reusable services
- Easier testing

---

## Security Implementation

### 1. Authentication
- Cookie-based authentication
- Secure session management
- Automatic logout on browser close

### 2. Authorization
```csharp
[Authorize(Roles = Roles.Admin)]
public class AdminController : Controller
{
    // Only admins can access
}
```

### 3. Password Security
- PBKDF2 hashing (via Identity)
- Salt included automatically
- Configurable iteration count

### 4. CSRF Protection
```csharp
[ValidateAntiForgeryToken]
public async Task<IActionResult> Register(...)
```

### 5. SQL Injection Prevention
- Parameterized queries via EF Core
- LINQ query translation
- No raw SQL commands

### 6. XSS Prevention
- Razor automatic HTML encoding
- Tag helpers
- Content Security Policy headers

---

## Development & Deployment

### Development Environment

**Requirements**:
- Visual Studio 2024 / VS Code / Rider
- .NET 10.0 SDK
- PostgreSQL 15+
- Git

**Setup**:
```bash
# Restore packages
dotnet restore

# Build project
dotnet build

# Run application
dotnet run

# Watch mode (auto-reload)
dotnet watch run
```

### Database Migrations

```bash
# Add new migration
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update

# Generate SQL script
dotnet ef migrations script

# Remove last migration
dotnet ef migrations remove
```

### Environment Configuration

**Development**: `appsettings.Development.json`
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

**Production**: `appsettings.json`
- Connection strings
- SMTP settings
- Admin credentials

### Deployment Options

1. **IIS**:
   - Publish as self-contained or framework-dependent
   - Configure application pool
   - Set environment variables

2. **Docker**:
   ```dockerfile
   FROM mcr.microsoft.com/dotnet/aspnet:10.0
   WORKDIR /app
   COPY published/ .
   ENTRYPOINT ["dotnet", "student_management_system.dll"]
   ```

3. **Linux with Kestrel**:
   - Systemd service
   - Nginx reverse proxy
   - Let's Encrypt SSL

4. **Cloud Platforms**:
   - Azure App Service
   - AWS Elastic Beanstalk
   - Google Cloud Run

### Configuration Management

**Connection Strings**:
- Stored in `appsettings.json`
- Override with environment variables
- Use User Secrets in development

**Example**:
```bash
# Set environment variable
export ConnectionStrings__DefaultConnection="Host=prod-server;..."
```

---

## Performance Considerations

### 1. Database Optimization
- Indexes on frequently queried fields
- AsNoTracking() for read-only queries
- Pagination for large datasets
- Lazy loading disabled

### 2. Caching
- Static file caching
- Response caching potential
- Distributed cache ready

### 3. Query Optimization
```csharp
// Efficient query with projection
var students = await _dbContext.Users
    .AsNoTracking()
    .Where(u => u.Role == "Student")
    .Select(u => new StudentViewModel { ... })
    .ToListAsync();
```

---

## Testing Considerations

### Testable Architecture
- Dependency injection enables mocking
- Interface-based services
- Separation of concerns

### Potential Test Types
- Unit tests for services
- Integration tests for controllers
- UI tests with Selenium
- Database tests with in-memory provider

---

## Future Technical Enhancements

### Potential Improvements
- API endpoints (RESTful API)
- SignalR for real-time features
- Background job processing (Hangfire)
- Advanced caching (Redis)
- Logging framework (Serilog)
- Health checks endpoint
- Swagger/OpenAPI documentation
- Docker containerization
- CI/CD pipeline
- Automated testing suite

---

## Troubleshooting

### Common Issues

**Port already in use**:
```bash
# Windows
netstat -ano | findstr :5146
taskkill /PID <pid> /F

# Linux
lsof -i :5146
kill -9 <pid>
```

**Database connection failed**:
- Check PostgreSQL is running
- Verify connection string
- Check firewall settings
- Verify credentials

**Migration errors**:
```bash
# Reset database
dotnet ef database drop
dotnet ef database update
```

---

## Version Information

- **.NET Version**: 10.0
- **C# Version**: 13
- **Entity Framework Core**: 10.0.2
- **PostgreSQL**: 15+
- **Bootstrap**: 5.x
- **jQuery**: 3.x

---

## Contact & Support

For technical issues or questions about the implementation, refer to:
- ASP.NET Core documentation: https://docs.microsoft.com/aspnet/core
- Entity Framework Core: https://docs.microsoft.com/ef/core
- PostgreSQL: https://www.postgresql.org/docs
