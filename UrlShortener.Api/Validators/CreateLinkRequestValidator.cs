using System;
using System.Collections.Generic;
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
        }

        private bool BeAValidUrl(string url)
        {
            if(!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                return false;
            }

            return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps;
        }
    }
}