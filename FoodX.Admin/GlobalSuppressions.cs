using System.Diagnostics.CodeAnalysis;

// Suppress CA1515 for application types that don't need to be internal
[assembly: SuppressMessage("Design", "CA1515:Consider making public types internal", Justification = "Application types are intentionally public", Scope = "namespaceanddescendants", Target = "~N:FoodX.Admin.Models")]
[assembly: SuppressMessage("Design", "CA1515:Consider making public types internal", Justification = "Application types are intentionally public", Scope = "namespaceanddescendants", Target = "~N:FoodX.Admin.Data")]
[assembly: SuppressMessage("Design", "CA1515:Consider making public types internal", Justification = "Application types are intentionally public", Scope = "namespaceanddescendants", Target = "~N:FoodX.Admin.Themes")]

// Suppress CA1812 for Identity classes that are instantiated by DI
[assembly: SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated by DI", Scope = "type", Target = "~T:FoodX.Admin.Components.Account.IdentityRevalidatingAuthenticationStateProvider")]
[assembly: SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated by DI", Scope = "type", Target = "~T:FoodX.Admin.Components.Account.IdentityNoOpEmailSender")]
[assembly: SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated by DI", Scope = "type", Target = "~T:FoodX.Admin.Components.Account.IdentityRedirectManager")]
[assembly: SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated by DI", Scope = "type", Target = "~T:FoodX.Admin.Components.Account.IdentityUserAccessor")]

// Suppress CA2007 for ASP.NET Core code where ConfigureAwait(false) is not needed
[assembly: SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task", Justification = "Not needed in ASP.NET Core", Scope = "namespaceanddescendants", Target = "~N:FoodX.Admin")]

// Suppress CA1062 for migration methods
[assembly: SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "EF Core migration methods", Scope = "member", Target = "~M:FoodX.Admin.Data.Migrations.CreateIdentityTables.Up(Microsoft.EntityFrameworkCore.Migrations.MigrationBuilder)")]
[assembly: SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "EF Core migration methods", Scope = "member", Target = "~M:FoodX.Admin.Data.Migrations.CreateIdentityTables.Down(Microsoft.EntityFrameworkCore.Migrations.MigrationBuilder)")]

// Suppress CA1848 for simple logging
[assembly: SuppressMessage("Performance", "CA1848:Use the LoggerMessage delegates", Justification = "Simple logging is sufficient", Scope = "member", Target = "~M:FoodX.Admin.Components.Account.IdentityComponentsEndpointRouteBuilderExtensions.MapAdditionalIdentityEndpoints(Microsoft.AspNetCore.Routing.IEndpointRouteBuilder)~Microsoft.AspNetCore.Routing.IEndpointRouteBuilder")]

// Suppress CA1859 for IEmailSender interface usage
[assembly: SuppressMessage("Performance", "CA1859:Use concrete types when possible for improved performance", Justification = "Using interface for flexibility", Scope = "member", Target = "~F:FoodX.Admin.Components.Account.IdentityNoOpEmailSender.emailSender")]

// Suppress CA1056 for URL string properties (database compatibility)
[assembly: SuppressMessage("Design", "CA1056:URI-like properties should not be strings", Justification = "String URLs for database compatibility", Scope = "member", Target = "~P:FoodX.Admin.Models.Company.LogoUrl")]

// Suppress BL0007 for MudBlazor component parameters
[assembly: SuppressMessage("Usage", "BL0007:Component parameters should be auto properties", Justification = "MudBlazor components", Scope = "namespaceanddescendants", Target = "~N:FoodX.Admin.Components")]

// Suppress CS8618 for nullable references in Entity Framework models
[assembly: SuppressMessage("Nullable", "CS8618:Non-nullable field must contain a non-null value when exiting constructor", Justification = "Entity Framework models", Scope = "namespaceanddescendants", Target = "~N:FoodX.Admin.Models")]
[assembly: SuppressMessage("Nullable", "CS8618:Non-nullable field must contain a non-null value when exiting constructor", Justification = "Entity Framework models", Scope = "namespaceanddescendants", Target = "~N:FoodX.Admin.Data")]

// Suppress CS1998 for async methods without await (used for interface compatibility)
[assembly: SuppressMessage("Async", "CS1998:Async method lacks 'await' operators and will run synchronously", Justification = "Interface implementation", Scope = "namespaceanddescendants", Target = "~N:FoodX.Admin.Services")]

// Suppress CS0162 for unreachable code (used for debugging)
[assembly: SuppressMessage("Code Quality", "CS0162:Unreachable code detected", Justification = "Debugging code", Scope = "namespaceanddescendants", Target = "~N:FoodX.Admin")]