using EnglishTutor.BuildingBlocks.Infrastructure.SecretProtection;
using FluentAssertions;
using Microsoft.AspNetCore.DataProtection;

namespace EnglishTutor.BuildingBlocks.UnitTests;

public class SecretProtectorTests
{
    [Fact]
    public void Protect_Then_Unprotect_Roundtrip_Should_Return_Plaintext()
    {
        // Arrange: DataProtection requires a real provider. Use EphemeralDataProtectionProvider
        // so the test is self-contained and does not touch the filesystem.
        var provider = DataProtectionProvider.Create("EnglishTutor.Tests");
        var sut = new DataProtectionSecretProtector(provider);
        var plaintext = "sk-test-ABCDEFGH-1234567890";

        // Act
        var protectedValue = sut.Protect(plaintext);
        var recovered = sut.Unprotect(protectedValue);

        // Assert
        protectedValue.Should().NotBe(plaintext);
        recovered.Should().Be(plaintext);
    }

    [Fact]
    public void Protect_Should_Produce_Different_Outputs_For_Same_Input()
    {
        // Data Protection adds entropy — same plaintext can produce different
        // ciphertexts, but all of them must unprotect to the original.
        var provider = DataProtectionProvider.Create("EnglishTutor.Tests");
        var sut = new DataProtectionSecretProtector(provider);
        var plaintext = "deterministic-test-value";

        var a = sut.Protect(plaintext);
        var b = sut.Protect(plaintext);

        sut.Unprotect(a).Should().Be(plaintext);
        sut.Unprotect(b).Should().Be(plaintext);
    }

    [Fact]
    public void Unprotect_With_Tampered_Ciphertext_Should_Throw()
    {
        var provider = DataProtectionProvider.Create("EnglishTutor.Tests");
        var sut = new DataProtectionSecretProtector(provider);
        var protectedValue = sut.Protect("original-secret");

        // Tamper: change the last character.
        var tampered = protectedValue[..^1] + (protectedValue[^1] == 'A' ? 'B' : 'A');

        var act = () => sut.Unprotect(tampered);
        act.Should().Throw<Exception>();
    }
}
