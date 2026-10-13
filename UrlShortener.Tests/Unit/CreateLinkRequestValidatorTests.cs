using FluentAssertions;
using FluentValidation.TestHelper;
using UrlShortener.Api.Models;
using UrlShortener.Api.Validators;

namespace UrlShortener.Tests.Unit;

public class CreateLinkRequestValidatorTests
{
    private readonly CreateLinkRequestValidator _validator = new();

    [Theory]
    [InlineData("https://google.com")]
    [InlineData("http://example.com/path?q=1")]
    [InlineData("https://sub.domain.co.uk/deep/path")]
    public void ValidUrls_PassValidation(string url)
    {
        var result = _validator.TestValidate(new CreateLinkRequest { OriginalUrl = url });
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-a-url")]
    [InlineData("javascript:alert(1)")]
    [InlineData("data:text/html,<script>")]
    [InlineData("file:///etc/passwd")]
    [InlineData("ftp://example.com")]
    public void InvalidUrls_FailValidation(string url)
    {
        var result = _validator.TestValidate(new CreateLinkRequest { OriginalUrl = url });
        result.ShouldHaveValidationErrorFor(x => x.OriginalUrl);
    }

    [Fact]
    public void UrlLongerThan2048_Fails()
    {
        var longUrl = "https://example.com/" + new string('a', 2100);
        var result = _validator.TestValidate(new CreateLinkRequest { OriginalUrl = longUrl });
        result.ShouldHaveValidationErrorFor(x => x.OriginalUrl);
    }

    [Fact]
    public void PastExpiry_Fails()
    {
        var result = _validator.TestValidate(new CreateLinkRequest
        {
            OriginalUrl = "https://google.com",
            ExpiresAt = DateTime.UtcNow.AddDays(-1)
        });
        result.ShouldHaveValidationErrorFor(x => x.ExpiresAt);
    }

    [Fact]
    public void ExpiryMoreThanOneYearOut_Fails()
    {
        var result = _validator.TestValidate(new CreateLinkRequest
        {
            OriginalUrl = "https://google.com",
            ExpiresAt = DateTime.UtcNow.AddYears(2)
        });
        result.ShouldHaveValidationErrorFor(x => x.ExpiresAt);
    }

    [Fact]
    public void NullExpiry_Passes()
    {
        var result = _validator.TestValidate(new CreateLinkRequest
        {
            OriginalUrl = "https://google.com",
            ExpiresAt = null
        });
        result.ShouldNotHaveAnyValidationErrors();
    }
}