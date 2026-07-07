using EnglishTutor.Learning.Domain.Aggregates.LanguageDefinitions.ValueObjects;
using FluentAssertions;
using Xunit;

namespace EnglishTutor.Learning.UnitTests;

public sealed class LanguageCodeTests
{
    [Theory]
    [InlineData("VI", "vi")]
    [InlineData("zh-cn", "zh-CN")]
    [InlineData("en-us", "en-US")]
    [InlineData("sr-latn-rs", "sr-Latn-RS")]
    public void Create_ShouldCanonicalizeBoundedBcp47Code(string input, string expected)
    {
        LanguageCode.Create(input).Value.Should().Be(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData("e")]
    [InlineData("en-@")]
    public void Create_WithInvalidCode_ShouldThrow(string input)
    {
        var action = () => LanguageCode.Create(input);

        action.Should().Throw<ArgumentException>();
    }
}
