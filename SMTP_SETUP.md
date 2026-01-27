# SMTP Email Setup Guide

## Current Configuration

The application is currently configured to use **Office 365 SMTP** for sending emails from your `@buzzparade.com` email address.

### Current Settings (in `appsettings.json`):

```json
"Smtp": {
  "Host": "smtp.office365.com",
  "Port": 587,
  "EnableSsl": true,
  "UserName": "varun.t@buzzparade.com",
  "Password": "VNew2026!!",
  "FromName": "Student Management System",
  "FromEmail": "varun.t@buzzparade.com"
}
```

## Important: Why Emails May Not Be Sending

**Your emails are likely NOT sending because:**

1. **Office 365 requires authentication through your organization**
   - Your password alone may not be sufficient
   - Office 365 often requires app-specific passwords or OAuth authentication
   - Your organization may have security policies blocking programmatic email access

2. **Your organization may use a different SMTP server**
   - Many companies use custom mail servers
   - Contact your IT department to get the correct SMTP settings

## How to Fix SMTP Issues

### Option 1: Contact Your IT Department (RECOMMENDED)

Ask your IT team for:
- **SMTP Server Address** (e.g., `smtp.buzzparade.com` or `mail.buzzparade.com`)
- **SMTP Port** (usually 587 or 465)
- **Authentication Method** (username/password, app password, or OAuth)
- **SSL/TLS Requirements**

### Option 2: Use Gmail for Testing

If you want to test email functionality quickly, you can use Gmail:

1. **Create or use a Gmail account** (e.g., `your.test.email@gmail.com`)

2. **Enable 2-Factor Authentication** on your Gmail account:
   - Go to: https://myaccount.google.com/security
   - Enable 2-Step Verification

3. **Generate an App Password**:
   - Go to: https://myaccount.google.com/apppasswords
   - Select "Mail" and your device
   - Copy the 16-character password

4. **Update `appsettings.json`**:

```json
"Smtp": {
  "Host": "smtp.gmail.com",
  "Port": 587,
  "EnableSsl": true,
  "UserName": "your.test.email@gmail.com",
  "Password": "your-16-character-app-password",
  "FromName": "Student Management System",
  "FromEmail": "your.test.email@gmail.com"
}
```

### Option 3: Common Business Email Providers

**Microsoft 365 / Office 365:**
```json
{
  "Host": "smtp.office365.com",
  "Port": 587,
  "EnableSsl": true
}
```

**Google Workspace (G Suite):**
```json
{
  "Host": "smtp.gmail.com",
  "Port": 587,
  "EnableSsl": true
}
```

**Custom Domain (Contact IT):**
```json
{
  "Host": "smtp.yourdomain.com",
  "Port": 587,
  "EnableSsl": true
}
```

## Testing Email After Configuration

1. **Stop the application** (Ctrl+C in terminal)
2. **Update `appsettings.json`** with correct SMTP settings
3. **Restart the application**: `dotnet run`
4. **Register a test student** with a real email you can access
5. **Check your inbox** (and spam folder!)

## Application Behavior

**Good News:** The application won't crash if emails fail!

- If email sending fails, credentials are displayed on-screen
- Students can still save their login details manually
- The application continues to function normally

## Security Note

⚠️ **NEVER commit `appsettings.json` with real passwords to version control!**

Consider using:
- Environment variables
- Azure Key Vault
- User Secrets for development: `dotnet user-secrets set "Smtp:Password" "your-password"`

## Need Help?

If you're still having issues:
1. Check the application logs in the terminal
2. Verify firewall isn't blocking outbound port 587
3. Contact your IT department for organization-specific SMTP settings
