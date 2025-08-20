# FoodX B2B Platform - Development Server Credentials

## Server Access Information
- **Application URL**: http://localhost:5193
- **Environment**: Development
- **Generated Date**: 2025-01-20

## Test User Credentials

### Quick Access Accounts
These are the primary accounts for quick testing:

| Role | Email | Password | Dashboard URL |
|------|-------|----------|---------------|
| **Admin** | admin1@test.com | Admin1@Pass123 | /dashboard/admin |
| **Buyer** | buyer1@test.com | Buyer1@Pass123 | /dashboard/buyer |
| **Supplier** | supplier1@test.com | Supplier1@Pass123 | /dashboard/supplier |

---

## All Test Accounts by Role

### 🛡️ SYSTEM ADMINISTRATORS (5 users)
Full system access, user management, and configuration capabilities.

| Email | Password | Access Level |
|-------|----------|--------------|
| admin1@test.com | Admin1@Pass123 | Full Admin |
| admin2@test.com | Admin2@Pass123 | Full Admin |
| admin3@test.com | Admin3@Pass123 | Full Admin |
| admin4@test.com | Admin4@Pass123 | Full Admin |
| admin5@test.com | Admin5@Pass123 | Full Admin |

### 🛒 BUYERS (5 users)
Can browse products, place orders, and manage purchases.

| Email | Password | Company |
|-------|----------|---------|
| buyer1@test.com | Buyer1@Pass123 | Test Company |
| buyer2@test.com | Buyer2@Pass123 | Test Company |
| buyer3@test.com | Buyer3@Pass123 | Test Company |
| buyer4@test.com | Buyer4@Pass123 | Test Company |
| buyer5@test.com | Buyer5@Pass123 | Test Company |

### 📦 SUPPLIERS (5 users)
Can manage products, inventory, and process orders.

| Email | Password | Company |
|-------|----------|---------|
| supplier1@test.com | Supplier1@Pass123 | Test Supplier |
| supplier2@test.com | Supplier2@Pass123 | Test Supplier |
| supplier3@test.com | Supplier3@Pass123 | Test Supplier |
| supplier4@test.com | Supplier4@Pass123 | Test Supplier |
| supplier5@test.com | Supplier5@Pass123 | Test Supplier |

### 🤝 AGENTS (5 users)
Can facilitate transactions and manage client relationships.

| Email | Password | Territory |
|-------|----------|-----------|
| agent1@test.com | Agent1@Pass123 | Region 1 |
| agent2@test.com | Agent2@Pass123 | Region 2 |
| agent3@test.com | Agent3@Pass123 | Region 3 |
| agent4@test.com | Agent4@Pass123 | Region 4 |
| agent5@test.com | Agent5@Pass123 | Region 5 |

### 🎓 EXPERTS (5 users)
Provide specialized knowledge and consultation services.

| Email | Password | Specialization |
|-------|----------|----------------|
| expert1@test.com | Expert1@Pass123 | Food Safety |
| expert2@test.com | Expert2@Pass123 | Quality Control |
| expert3@test.com | Expert3@Pass123 | Logistics |
| expert4@test.com | Expert4@Pass123 | Compliance |
| expert5@test.com | Expert5@Pass123 | Market Analysis |

### 📋 BACK OFFICE (5 users)
Handle administrative tasks and support operations.

| Email | Password | Department |
|-------|----------|------------|
| backoffice1@test.com | BackOffice1@Pass123 | Operations |
| backoffice2@test.com | BackOffice2@Pass123 | Finance |
| backoffice3@test.com | BackOffice3@Pass123 | HR |
| backoffice4@test.com | BackOffice4@Pass123 | Support |
| backoffice5@test.com | BackOffice5@Pass123 | Admin |

---

## Password Requirements
All passwords follow these security requirements:
- ✅ Minimum 8 characters
- ✅ At least one uppercase letter
- ✅ At least one lowercase letter
- ✅ At least one number
- ✅ At least one special character (@)

## Password Pattern
All test passwords follow the pattern: `{Role}{Number}@Pass123`
- Example: Admin1@Pass123, Buyer2@Pass123, etc.

---

## How to Login

1. **Navigate to the application**:
   ```
   http://localhost:5193
   ```

2. **Go to the login page**:
   ```
   http://localhost:5193/Account/Login
   ```

3. **Enter credentials**:
   - Use any email/password combination from above
   - Click the eye icon to reveal/hide password while typing
   - Check "Remember me" to stay logged in

4. **After successful login**:
   - You'll be redirected to your role-specific dashboard
   - Admin users go to `/dashboard/admin`
   - Buyers go to `/dashboard/buyer`
   - Suppliers go to `/dashboard/supplier`
   - And so on...

---

## Troubleshooting Login Issues

### If login fails:
1. **Check the application is running**:
   ```bash
   cd FoodX.Admin
   dotnet run
   ```

2. **Verify database connection**:
   - Ensure Azure SQL Database is accessible
   - Check connection string in `appsettings.json`

3. **Reset password** (if needed):
   - Navigate to: http://localhost:5193/reset-test-passwords
   - This endpoint resets passwords for admin1, buyer1, and supplier1

4. **Check application logs**:
   - Console output shows detailed error messages
   - Check for database connection issues

---

## Security Notes

⚠️ **DEVELOPMENT ONLY**: These credentials are for development/testing purposes only.

🔒 **Production Environment**: 
- Never use these credentials in production
- Implement proper invitation-based registration
- Use strong, unique passwords
- Enable two-factor authentication
- Regular password rotation policy

---

## Database Information

- **Server**: fdx-sql-prod.database.windows.net
- **Database**: fdxdb
- **Authentication**: Azure AD Integrated
- **Tables**: AspNetUsers, AspNetRoles, AspNetUserRoles

---

## Additional Features

### Password Visibility Toggle
- All password fields have an eye icon
- Click to reveal/hide password
- Helps prevent typing errors

### Role-Based Access
Each role has different permissions:
- **Admin**: Full system access
- **Buyer**: Purchase and order management
- **Supplier**: Product and inventory management
- **Agent**: Transaction facilitation
- **Expert**: Consultation and advisory
- **BackOffice**: Administrative support

---

## SendGrid Email Configuration

### API Credentials
- **API Key Name**: FDX.Trading.emails
- **API Key**: [Stored securely - do not commit to repository]
- **Sender Identity**: udi@fdx.trading
- **Sender Name**: Udi-Stryk
- **Domain**: fdx.trading (verified)

### Configuration Location
- File: `FoodX.Admin/appsettings.json`
- Section: `SendGrid`

### Email Service Features
- Magic link authentication
- Password reset emails
- System notifications
- Order confirmations

---

## Contact & Support

For issues with credentials or login:
1. Check this documentation first
2. Review application logs
3. Verify database connectivity
4. Contact system administrator if issues persist

---

*Last Updated: 2025-01-20*
*Total Test Users: 30*
*Roles: 6*