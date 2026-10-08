using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using UrlShortener.Api.Models;

namespace UrlShortener.Api.Validators
{
    public class CreateLinkRequestValidator : AbstractValidator<CreateLinkRequest>
    {
        public CreateLinkRequestValidator()
        {
            RuleFor(x => x.OriginalUrl)
            .NotEmpty().WithMessage("URL is required.")
            .MaximumLength(2048).WithMessage("URL must not exceed 2048 characters.")
            .Must(BeAValidUrl).WithMessage("URL must be a valid URL.");

            RuleFor(x => x.ExpiresAt)
            .Must(BeInTheFuture).WithMessage("Expiration date must be in the future.")
            .When(x => x.ExpiresAt.HasValue);

            RuleFor(x => x.ExpiresAt)
            .Must(BeWithinOneYear).WithMessage("Expiration date must not be more than 1 year in the future.")
            .When(x => x.ExpiresAt.HasValue);
        }

        private bool BeAValidUrl(string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                return false;
            }

            return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps;
        }

        private static bool BeInTheFuture(DateTime? expiresAt)
        {
            if (!expiresAt.HasValue) return true;
            return expiresAt.Value > DateTime.UtcNow;
        }

        private static bool BeWithinOneYear(DateTime? expiresAt)
        {
            if (!expiresAt.HasValue) return true;
            return expiresAt.Value <= DateTime.UtcNow.AddYears(1);
        }
    }
}