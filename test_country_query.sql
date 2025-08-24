-- Test querying users with Country column
SELECT TOP 5 
    Id,
    UserName,
    Email,
    FirstName,
    LastName,
    Country,
    PhoneNumber
FROM AspNetUsers