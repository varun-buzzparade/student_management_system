# Changelog - Student Management System

## Latest Changes (Redis, AJAX, Pagination)

### 1. Redis-Backed Caching

**What changed:**
- Admin students list is cached using **Redis** (or Redis-compatible server such as **Memurai** on Windows).
- `IDistributedCache` backed by `Microsoft.Extensions.Caching.StackExchangeRedis`.
- Cache key includes query params (page, page size, filters); version bumped when a new student registers to invalidate all list caches.

**Configuration:**
- `appsettings.json`: `ConnectionStrings:Redis` (e.g. `localhost:6379`).
- Redis/Memurai must be running separately; the app does not start it.

**Files touched:**
- `Program.cs` – `AddStackExchangeRedisCache`, `InstanceName = "StudentMgmt_"`.
- `Controllers/AdminController.cs` – cache get/set, `ClearStudentListCacheAsync`.
- `Controllers/AccountController.cs` – calls clear cache after registration.

---

### 2. AJAX Profile Updates (No Page Reload)

**What changed:**
- **Admin Edit** (`/Admin/Edit/{id}`) and **Student Profile** (`/Student/Profile`): "Save Changes" submits via AJAX.
- Each **changed field** triggers a **separate POST** to a per-field update API; requests are **queued** to avoid conflicting updates on the same record.
- **Toastr** toasts show **success only for updated fields**; errors shown on failure.

**Files touched:**
- `Views/Admin/Edit.cshtml`, `Views/Student/Profile.cshtml` – fetch-based AJAX, Toastr, queue logic.
- `AdminController` / `StudentController` – POST actions (e.g. `UpdateFullName`, `UpdateEmail`, `UpdateDateOfBirth`, etc.).

---

### 3. Pagination Bar

**What changed:**
- Pagination bar truncated (e.g. `1 2 3 … 500`), ~15 page slots, centered below the table.
- Consistent width from initial load (no layout shift when changing pages).

**Files touched:**
- `Views/Admin/Index.cshtml` – pagination markup and logic.

---

### 4. Date of Birth (Registration & Profile)

**What changed:**
- Registration and profile use **Date of Birth**; age is **auto-calculated** and read-only.
- Date picker **year range** starts at **1900** (max today).

**Files touched:**
- Registration, Admin Edit, Student Profile views and ViewModels; `ApplicationUser.DateOfBirth`.

---

## Changes Made on 2026-01-27

### 1. ✅ Changed Age Input to Date of Birth

**What Changed:**
- Registration now asks for **Date of Birth** instead of Age
- Age is automatically calculated from Date of Birth
- Date picker UI for easy date selection

**Files Modified:**
- `Models/ApplicationUser.cs` - Added `DateOfBirth` field
- `ViewModels/StudentRegistrationViewModel.cs` - Changed from Age to DateOfBirth
- `ViewModels/AdminStudentEditViewModel.cs` - Added DateOfBirth field
- `ViewModels/StudentProfileViewModel.cs` - Added DateOfBirth field
- `Views/Account/Register.cshtml` - Date picker for DateOfBirth
- `Views/Admin/Edit.cshtml` - Date picker + readonly Age display
- `Views/Student/Profile.cshtml` - Date picker + readonly Age display
- `Controllers/AccountController.cs` - Auto-calculate age from DOB
- `Controllers/AdminController.cs` - Auto-calculate age from DOB
- `Controllers/StudentController.cs` - Auto-calculate age from DOB

**Database Changes:**
- Created migration: `AddDateOfBirth`
- Added `DateOfBirth` column to `AspNetUsers` table (nullable date field)

**How It Works:**
```csharp
// Age calculation logic
var today = DateOnly.FromDateTime(DateTime.Today);
var age = today.Year - dateOfBirth.Year;
if (dateOfBirth > today.AddYears(-age)) age--;
```

**User Experience:**
- 📅 Modern date picker with calendar dropdown
- 🔒 Maximum date is today (can't select future dates)
- ✨ Age automatically updates when DOB is saved
- 👁️ Age field is readonly (calculated, not editable)

---

### 2. ⚙️ SMTP Configuration Updated

**What Changed:**
- SMTP host changed from `smtp.gmail.com` to `smtp.office365.com`
- This matches your `@buzzparade.com` email domain better

**Files Modified:**
- `appsettings.json` - Updated SMTP Host to Office365

**Current SMTP Settings:**
```json
{
  "Host": "smtp.office365.com",
  "Port": 587,
  "EnableSsl": true,
  "UserName": "varun.t@buzzparade.com",
  "Password": "VNew2026!!"
}
```

**⚠️ Important Note:**
Emails may still NOT work because:
1. Office 365 requires proper authentication (your organization's IT setup)
2. Your organization may use a different SMTP server
3. You may need an app-specific password

**What To Do:**
- See `SMTP_SETUP.md` for detailed instructions
- Contact your IT department for correct SMTP settings
- Or use Gmail with an app password for testing

**Good News:**
- ✅ App won't crash if email fails
- ✅ Credentials shown on-screen if email fails
- ✅ Students can manually save their login info

---

## Testing the Changes

### Test Date of Birth Feature:

1. **Register a new student:**
   ```
   http://localhost:5146/Account/Register
   ```
   - Select a date of birth using the date picker
   - Submit the form
   - Age should be calculated automatically

2. **Edit existing student (as Admin):**
   ```
   http://localhost:5146/Admin/Index
   ```
   - Click "Edit" on any student
   - Change the Date of Birth
   - Notice Age updates automatically

3. **Edit profile (as Student):**
   ```
   http://localhost:5146/Student/Profile
   ```
   - Update your Date of Birth
   - Age field shows calculated age

### Test Email:

1. Register a new student with a real email you can access
2. Check if email arrives (check spam folder too!)
3. If no email:
   - Credentials should display on success screen
   - Check terminal for error messages
   - Follow `SMTP_SETUP.md` guide

---

## Database Migration

**To apply this update to another database:**

```bash
# Apply the migration
dotnet ef database update

# Or if starting fresh
dotnet ef database drop --force
dotnet ef database update
```

**Migration File:**
- `Migrations/[timestamp]_AddDateOfBirth.cs`

---

## Rollback Instructions

**If you need to undo these changes:**

```bash
# Rollback the database migration
dotnet ef database update [previous_migration_name]

# Or remove the migration entirely
dotnet ef migrations remove
```

Then restore the previous versions of the modified files from version control.

---

## Known Issues & Future Improvements

### Current Limitations:
1. **SMTP not verified** - Needs proper business email configuration
2. **Existing students** - Have `null` DateOfBirth (needs manual update by admin)
3. **No DOB validation** - Accepts very old dates (e.g., 1900)

### Potential Improvements:
1. Add DOB validation (minimum age 5, maximum age 100)
2. Add bulk-update tool for existing students
3. Implement OAuth for Office 365 email
4. Add email queue/retry mechanism
5. Show "email pending" status in admin panel

---

## Support

**For SMTP Issues:**
- Read `SMTP_SETUP.md`
- Contact your IT department
- Use Gmail for testing (see guide)

**For Date of Birth Issues:**
- Ensure database migration was applied
- Check existing students have DOB filled in
- Admin can edit student DOB if needed
