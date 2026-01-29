# Student Management System

A comprehensive web-based student management system built with ASP.NET Core and PostgreSQL, featuring role-based access control for administrators and students.

## Overview

The Student Management System provides a secure platform for educational institutions to manage student information. The system supports two distinct user roles with specific capabilities:

- **Administrators**: Full access to manage all student records
- **Students**: Self-service portal for viewing and updating personal information

## Quick Start

### Prerequisites

- .NET 10.0 SDK
- PostgreSQL 15 or higher
- Redis or a Redis-compatible server (e.g. [Memurai](https://www.memurai.com/) on Windows) for caching
- A modern web browser

### Installation & Setup

1. **Clone or download the project**

2. **Configure the database connection**
   
   Edit `appsettings.json` and update the connection strings:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Host=localhost;Port=5432;Database=student_management_system;Username=postgres;Password=your_password",
     "Redis": "localhost:6379"
   }
   ```

3. **Ensure Redis is running**
   
   The app uses Redis for caching the admin students list. Use Redis (Linux/macOS/Docker) or Memurai (Windows) listening on `localhost:6379`. The app does not start Redis; it must be running separately.

4. **Apply database migrations**
   ```bash
   dotnet ef database update
   ```

5. **Run the application**
   ```bash
   dotnet run
   ```

6. **Access the application**
   
   Open your browser and navigate to: `http://localhost:5146`

### Default Admin Credentials

- **Email**: `admin@school.local`
- **Password**: `Admin@12345`

⚠️ **Important**: Change the default admin password after first login.

## Documentation

- **[Features & Functionality](FEATURES.md)** - Detailed description of all features and user workflows
- **[Technical Stack](TECHSTACK.md)** - Architecture, technologies, and technical details

## Project Structure

```
student_management_system/
├── Controllers/           # MVC Controllers
│   ├── AccountController.cs
│   ├── AdminController.cs
│   ├── HomeController.cs
│   └── StudentController.cs
├── Data/                 # Database context and seeders
│   ├── ApplicationDbContext.cs
│   └── IdentitySeeder.cs
├── Models/               # Domain models
├── Services/             # Business logic services
├── ViewModels/           # Data transfer objects
├── Views/                # Razor views
├── Migrations/           # EF Core migrations
└── wwwroot/             # Static files

```

## Key Features

- ✅ Role-based authentication & authorization
- ✅ Student registration with auto-generated IDs (date of birth → auto-calculated age)
- ✅ Admin dashboard with advanced search and filtering
- ✅ Redis-backed caching for the students list (invalidated when a new student registers)
- ✅ Student profile management (admin & student) via AJAX updates—no page reload, Toastr notifications
- ✅ Secure password generation
- ✅ Email notification system (configurable)
- ✅ Responsive Bootstrap UI

## Configuration

### Email Settings (Optional)

To enable email notifications, configure SMTP settings in `appsettings.json`:

```json
"Smtp": {
  "Host": "smtp.gmail.com",
  "Port": 587,
  "EnableSsl": true,
  "UserName": "your_email@gmail.com",
  "Password": "your_app_password",
  "FromName": "Student Management System",
  "FromEmail": "your_email@gmail.com"
}
```

**Note**: If email is not configured, registration credentials will be displayed on-screen instead.

### Redis (Caching)

The admin students list is cached using Redis. Configure the Redis connection in `appsettings.json`:

```json
"ConnectionStrings": {
  "Redis": "localhost:6379"
}
```

- **Windows**: Use [Memurai](https://www.memurai.com/) (Redis-compatible) or Redis via WSL/Docker.
- **Linux/macOS**: Use Redis; ensure it is running before starting the app.
- Cache is cleared automatically when a new student registers.

## Security Features

- Password hashing using ASP.NET Core Identity
- Anti-forgery token validation
- Role-based authorization
- Secure session management
- SQL injection prevention via Entity Framework

## Support & Maintenance

For issues, questions, or contributions, please refer to the technical documentation in `TECHSTACK.md`.

## License

All rights reserved © 2026 Student Management System
