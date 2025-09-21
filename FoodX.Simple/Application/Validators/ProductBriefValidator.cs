using FluentValidation;
using FoodX.Simple.Application.DTOs;

namespace FoodX.Simple.Application.Validators
{
    public class CreateProductBriefValidator : AbstractValidator<CreateProductBriefDto>
    {
        public CreateProductBriefValidator()
        {
            RuleFor(x => x.ProductName)
                .NotEmpty().WithMessage("Product name is required")
                .MaximumLength(200).WithMessage("Product name must not exceed 200 characters");

            RuleFor(x => x.Category)
                .NotEmpty().WithMessage("Category is required")
                .MaximumLength(100).WithMessage("Category must not exceed 100 characters");

            RuleFor(x => x.BenchmarkBrandReference)
                .MaximumLength(200).WithMessage("Benchmark brand reference must not exceed 200 characters");

            RuleFor(x => x.BenchmarkWebsiteUrl)
                .MaximumLength(500).WithMessage("URL must not exceed 500 characters")
                .Must(BeAValidUrl).WithMessage("Invalid URL format")
                .When(x => !string.IsNullOrEmpty(x.BenchmarkWebsiteUrl));

            RuleFor(x => x.PackageSize)
                .MaximumLength(100).WithMessage("Package size must not exceed 100 characters");

            RuleFor(x => x.CountryOfOrigin)
                .MaximumLength(100).WithMessage("Country must not exceed 100 characters");

            RuleFor(x => x.KosherOrganization)
                .NotEmpty().WithMessage("Kosher organization is required when product is kosher certified")
                .When(x => x.IsKosherCertified);

            RuleFor(x => x.AdditionalNotes)
                .MaximumLength(2000).WithMessage("Additional notes must not exceed 2000 characters");

            RuleFor(x => x.ImageUrl)
                .Must(BeAValidUrl).WithMessage("Invalid image URL format")
                .When(x => !string.IsNullOrEmpty(x.ImageUrl));
        }

        private bool BeAValidUrl(string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return true;

            return Uri.TryCreate(url, UriKind.Absolute, out var result)
                && (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
        }
    }

    public class UpdateProductBriefValidator : AbstractValidator<UpdateProductBriefDto>
    {
        public UpdateProductBriefValidator()
        {
            Include(new CreateProductBriefValidator());

            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Invalid product brief ID");

            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("Status is required")
                .Must(BeAValidStatus).WithMessage("Invalid status value");
        }

        private bool BeAValidStatus(string status)
        {
            var validStatuses = new[] { "Draft", "Active", "Sourcing", "Completed", "Cancelled" };
            return validStatuses.Contains(status);
        }
    }
}