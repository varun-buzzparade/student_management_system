# Features & Functionality Documentation

## Table of Contents

1. [User Roles](#user-roles)
2. [Authentication & Authorization](#authentication--authorization)
3. [Admin Features](#admin-features)
4. [Student Features](#student-features)
5. [Common Features](#common-features)
6. [User Workflows](#user-workflows)

---

## User Roles

The system supports two distinct user roles, each with specific permissions and capabilities:

### 1. Administrator Role
- Full access to all student records
- Ability to view, search, filter, and edit student information
- Access to administrative dashboard
- Cannot be created through the standard registration process

### 2. Student Role
- Self-service access to personal profile
- Ability to view and update own information
- Created through the public registration process
- Cannot access other students' information

---

## Authentication & Authorization

### Login System
- **URL**: `/Account/Login`
- **Access**: Public
- **Features**:
  - Email-based authentication
  - Secure password verification
  - Role-based redirection after login
  - Session management
  - Remember return URL for seamless navigation

### Logout
- **Access**: Authenticated users only
- **Action**: Secure session termination
- **Redirect**: Returns to login page

### Security Features
- Password complexity requirements:
  - Minimum 8 characters
  - At least 1 uppercase letter
  - At least 1 lowercase letter
  - At least 1 number
  - At least 1 special character
- Anti-forgery token validation
- Secure password hashing (ASP.NET Core Identity)
- Account lockout protection

---

## Admin Features

### 1. Student Management Dashboard

**URL**: `/Admin/Index`

**Access**: Administrators only

**Capabilities**:

#### Search & Filter Options
- **Name Search**: Case-insensitive partial match on full name
- **Age Filter**: Exact age match
- **Gender Filter**: Filter by Male, Female, or Unknown
- **Mobile Number**: Partial match search
- **Email Address**: Case-insensitive partial match

#### Display Features
- Paginated results (default: 20 students per page, configurable 5-100)
- Total student count
- Sortable columns
- Clean, responsive table layout

#### Displayed Information
- Student ID (unique identifier)
- Full Name
- Age
- Gender
- Mobile Number
- Email Address
- Action buttons (View Details, Edit)

### 2. View Student Details

**URL**: `/Admin/Details/{id}`

**Access**: Administrators only

**Information Displayed**:
- Student ID
- Full Name
- Email Address
- Age
- Height (in centimeters)
- Gender
- Mobile Number
- Account Creation Date

**Features**:
- Read-only view
- Navigation back to student list
- Option to edit student information

### 3. Edit Student Information

**URL**: `/Admin/Edit/{id}`

**Access**: Administrators only

**Editable Fields**:
- Full Name
- Email Address (with duplicate check)
- Age (numeric input)
- Height in cm (decimal input)
- Gender (dropdown: Male, Female, Unknown)
- Mobile Number

**Validation**:
- Required field validation
- Email format validation
- Unique email enforcement
- Numeric range validation for age and height

**Features**:
- Pre-populated form with current data
- Real-time validation
- Success/error messaging
- Automatic username update on email change

---

## Student Features

### 1. Student Registration

**URL**: `/Account/Register`

**Access**: Public (unauthenticated users)

**Registration Process**:

#### Required Information
- Full Name
- Email Address (must be unique)
- Age
- Height (in centimeters)
- Gender (Male, Female, Unknown)
- Mobile Number

#### Automatic Generation
- **Student ID**: Unique 8-digit ID (format: STU followed by 5 digits)
  - Example: `STU00001`, `STU00042`
  - Sequential generation
  - Collision prevention
- **Temporary Password**: Secure random password
  - Meets all complexity requirements
  - Auto-generated for security

#### Post-Registration
1. Student account created in database
2. Student role assigned automatically
3. Credentials delivery (2 options):
   - **Email sent** (if SMTP configured): Credentials emailed to student
   - **On-screen display** (if email fails): Credentials shown on success page
4. Redirect to login page

### 2. Student Profile Management

**URL**: `/Student/Profile`

**Access**: Authenticated students only

**View Mode**:
- Display all personal information
- Read-only Student ID
- Clean, organized layout

**Edit Mode**:
- Update Full Name
- Change Email Address (with duplicate check)
- Update Age
- Modify Height
- Change Gender
- Update Mobile Number

**Features**:
- Inline form validation
- Success messaging after updates
- Cannot modify Student ID (system-generated, immutable)
- Real-time validation feedback

---

## Common Features

### 1. Navigation Bar

**Dynamic Navigation** based on user role and authentication status:

#### Unauthenticated Users
- Home
- Login
- Register

#### Authenticated Students
- Home
- My Profile
- User greeting (displays email)
- Logout button

#### Authenticated Administrators
- Home
- Students (link to admin dashboard)
- User greeting (displays email)
- Logout button

### 2. Access Control

**URL Protection**:
- Unauthorized users redirected to login
- Role-based page access enforcement
- Access Denied page for unauthorized attempts

### 3. Data Validation

**Client-Side Validation**:
- HTML5 form validation
- Bootstrap validation styling
- Real-time feedback

**Server-Side Validation**:
- Model state validation
- Business rule enforcement
- Duplicate email detection
- Data type validation

---

## User Workflows

### Admin Workflow: Managing Students

```
1. Login as Administrator
   ↓
2. Click "Students" in navigation
   ↓
3. View student list (default: all students)
   ↓
4. [Optional] Apply filters/search
   ↓
5. Click "Details" to view student information
   OR
   Click "Edit" to modify student information
   ↓
6. Make changes (if editing)
   ↓
7. Save changes
   ↓
8. View success message
   ↓
9. Return to student list
```

### Student Workflow: Registration

```
1. Navigate to Register page
   ↓
2. Fill in registration form
   - Full Name
   - Email
   - Age
   - Height
   - Gender
   - Mobile Number
   ↓
3. Submit form
   ↓
4. System generates:
   - Unique Student ID
   - Secure temporary password
   ↓
5. Credentials delivered:
   - Via email (if configured)
   - OR displayed on screen
   ↓
6. Save credentials
   ↓
7. Navigate to Login
   ↓
8. Login with provided credentials
```

### Student Workflow: Update Profile

```
1. Login as Student
   ↓
2. Click "My Profile" in navigation
   ↓
3. View current profile information
   ↓
4. Scroll to edit form
   ↓
5. Update desired fields
   ↓
6. Submit changes
   ↓
7. View success message
   ↓
8. Verify updated information
```

### Admin Workflow: Search for Specific Student

```
1. Login as Administrator
   ↓
2. Navigate to Students page
   ↓
3. Apply search filters:
   - Enter name (partial match works)
   - Select age (optional)
   - Select gender (optional)
   - Enter mobile number (optional)
   - Enter email (optional)
   ↓
4. Click Search/Apply
   ↓
5. View filtered results
   ↓
6. Click on student for more details
```

---

## Error Handling

### User-Friendly Error Messages

**Login Errors**:
- "Email and password are required"
- "Invalid login attempt"

**Registration Errors**:
- "Email is already registered"
- Field-specific validation messages
- "Email delivery failed - please save these credentials now!"

**Update Errors**:
- "Email is already in use"
- Field-specific validation errors
- "Unable to update student information"

### Access Denied
- Custom Access Denied page
- Clear message indicating insufficient permissions
- Link back to appropriate page

---

## Special Features

### 1. Smart Email Handling

The system gracefully handles email configuration issues:

- **Email Configured**: Sends credentials via email
- **Email Not Configured**: Displays credentials on-screen with warning
- **No Application Crash**: Registration always succeeds

### 2. Student ID Generation

- Sequential numbering
- Format: STU + 5-digit number (padded with zeros)
- Collision detection and retry logic
- Guaranteed uniqueness

### 3. Responsive Design

- Mobile-friendly interface
- Bootstrap 5 framework
- Optimized for all screen sizes
- Touch-friendly controls

### 4. Data Privacy

- Students can only access their own data
- Administrators have full access
- No cross-student data exposure
- Secure session management

---

## Future Enhancement Possibilities

While not currently implemented, the system architecture supports:

- Password reset functionality
- Email verification
- Student document uploads
- Grade/course management
- Bulk student import
- Advanced reporting
- Student photo uploads
- Multi-factor authentication
- Audit logging
- Role-based admin hierarchy
