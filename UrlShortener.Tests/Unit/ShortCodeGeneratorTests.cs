using FluentAssertions;
using UrlShortener.Core.Services;

namespace UrlShortener.Tests.Unit;

public class ShortCodeGeneratorTests
{
    [Fact]
    public void Generate_ReturnsSevenCharacters()
    {
        var code = ShortCodeGenerator.Generate();
        code.Should().HaveLength(7);
    }

    [Fact]
    public void Generate_ReturnsOnlyBase62Characters()
    {
        var code = ShortCodeGenerator.Generate();
        code.Should().MatchRegex("^[a-zA-Z0-9]{7}$");
    }

    [Fact]
    public void Generate_ProducesUniqueCodes()
    {
        var codes = Enumerable.Range(0, 1000)
            .Select(_ => ShortCodeGenerator.Generate())
            .ToHashSet();

        // With 3.5 trillion possibilities, 1000 codes should be unique
        codes.Should().HaveCount(1000);
    }
}