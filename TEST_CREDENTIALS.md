# FDX Trading Test User Credentials

## Overview
30 test users have been created across 6 roles. Each user can login to the website using their email and password.

## Login Credentials by Role

### BUYERS (5 users)
| Email | Password | Role |
|-------|----------|------|
| buyer1@test.com | Buyer1@Pass123 | Buyer |
| buyer2@test.com | Buyer2@Pass123 | Buyer |
| buyer3@test.com | Buyer3@Pass123 | Buyer |
| buyer4@test.com | Buyer4@Pass123 | Buyer |
| buyer5@test.com | Buyer5@Pass123 | Buyer |

### SUPPLIERS (5 users)
| Email | Password | Role |
|-------|----------|------|
| supplier1@test.com | Supplier1@Pass123 | Seller |
| supplier2@test.com | Supplier2@Pass123 | Seller |
| supplier3@test.com | Supplier3@Pass123 | Seller |
| supplier4@test.com | Supplier4@Pass123 | Seller |
| supplier5@test.com | Supplier5@Pass123 | Seller |

### EXPERTS (5 users)
| Email | Password | Role |
|-------|----------|------|
| expert1@test.com | Expert1@Pass123 | Expert |
| expert2@test.com | Expert2@Pass123 | Expert |
| expert3@test.com | Expert3@Pass123 | Expert |
| expert4@test.com | Expert4@Pass123 | Expert |
| expert5@test.com | Expert5@Pass123 | Expert |

### AGENTS (5 users)
| Email | Password | Role |
|-------|----------|------|
| agent1@test.com | Agent1@Pass123 | Agent |
| agent2@test.com | Agent2@Pass123 | Agent |
| agent3@test.com | Agent3@Pass123 | Agent |
| agent4@test.com | Agent4@Pass123 | Agent |
| agent5@test.com | Agent5@Pass123 | Agent |

### SYSTEM ADMINISTRATORS (5 users)
| Email | Password | Role |
|-------|----------|------|
| admin1@test.com | Admin1@Pass123 | Admin |
| admin2@test.com | Admin2@Pass123 | Admin |
| admin3@test.com | Admin3@Pass123 | Admin |
| admin4@test.com | Admin4@Pass123 | Admin |
| admin5@test.com | Admin5@Pass123 | Admin |

### BACK OFFICE (5 users)
| Email | Password | Role |
|-------|----------|------|
| backoffice1@test.com | BackOffice1@Pass123 | BackOffice |
| backoffice2@test.com | BackOffice2@Pass123 | BackOffice |
| backoffice3@test.com | BackOffice3@Pass123 | BackOffice |
| backoffice4@test.com | BackOffice4@Pass123 | BackOffice |
| backoffice5@test.com | BackOffice5@Pass123 | BackOffice |

## Password Pattern
All passwords follow the pattern: **RoleName#@Pass123**
- Where # is the user number (1-5)
- All passwords meet complexity requirements:
  - At least 8 characters
  - Contains uppercase and lowercase letters
  - Contains numbers
  - Contains special characters

## Database Structure
Each user role has:
1. An entry in the AspNetUsers table (for authentication)
2. An entry in the Users table (business data)
3. An entry in their respective role table (Buyers, Suppliers, Experts, Agents, SystemAdmins, BackOffice)

## Testing the Login
1. Run the application: `dotnet run` in the FoodX.Admin folder
2. Navigate to the login page
3. Use any of the above credentials to login
4. Each role will have different access permissions based on their role

## Quick Test Accounts
For quick testing, use these accounts:
- **Buyer**: buyer1@test.com / Buyer1@Pass123
- **Supplier**: supplier1@test.com / Supplier1@Pass123
- **Admin**: admin1@test.com / Admin1@Pass123