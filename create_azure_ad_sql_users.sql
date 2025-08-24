-- Run this in the fdxdb database as Azure AD admin
-- Create Azure AD user for the application
CREATE USER [fdxadmin@fdx.trading] FROM EXTERNAL PROVIDER;

-- Grant necessary permissions
ALTER ROLE db_datareader ADD MEMBER [fdxadmin@fdx.trading];
ALTER ROLE db_datawriter ADD MEMBER [fdxadmin@fdx.trading];
ALTER ROLE db_ddladmin ADD MEMBER [fdxadmin@fdx.trading];

-- Create user for managed identity (if using App Service)
-- CREATE USER [your-app-service-name] FROM EXTERNAL PROVIDER;
-- ALTER ROLE db_datareader ADD MEMBER [your-app-service-name];
-- ALTER ROLE db_datawriter ADD MEMBER [your-app-service-name];

PRINT 'Azure AD users configured successfully';
