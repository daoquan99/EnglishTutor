using System;
using System.Linq;
using System.Reflection;
using EnglishTutor.AiGateway.Contracts;
using EnglishTutor.AiGateway.Application.Abstractions;
using FluentAssertions;
using Xunit;

namespace EnglishTutor.IntegrationTests.AiGateway;

public class AiGatewayContractReflectionTests
{
    [Fact]
    public void Contracts_And_ApplicationAbstractions_ShouldNotExposeDecryptedOrRawKeys()
    {
        // Arrange
        var assemblies = new[]
        {
            Assembly.GetAssembly(typeof(IAiGatewayModule)),              // Contracts assembly
            Assembly.GetAssembly(typeof(IAiProviderExecutionGateway))   // Application assembly
        };

        var forbiddenSubstrings = new[]
        {
            "rawkey",
            "decryptedkey",
            "apikey",
            "secret",
            "tokenhash",
            "password"
        };

        // Act & Assert
        foreach (var assembly in assemblies)
        {
            if (assembly == null) continue;

            // Gather all types defined in Contracts and Application Abstractions namespaces
            var types = assembly.GetTypes()
                .Where(t => t.Namespace != null && 
                            (t.Namespace.StartsWith("EnglishTutor.AiGateway.Contracts") ||
                             t.Namespace.StartsWith("EnglishTutor.AiGateway.Application.Abstractions")));

            foreach (var type in types)
            {
                // Check public/internal properties
                var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
                foreach (var prop in properties)
                {
                    var propNameLower = prop.Name.ToLowerInvariant();
                    foreach (var forbidden in forbiddenSubstrings)
                    {
                        propNameLower.Should().NotContain(
                            forbidden, 
                            $"Type {type.FullName} exposes property {prop.Name} containing forbidden substring '{forbidden}' which violates secret boundaries.");
                    }
                }

                // Check public/internal fields
                var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
                foreach (var field in fields)
                {
                    var fieldNameLower = field.Name.ToLowerInvariant();
                    foreach (var forbidden in forbiddenSubstrings)
                    {
                        fieldNameLower.Should().NotContain(
                            forbidden, 
                            $"Type {type.FullName} exposes field {field.Name} containing forbidden substring '{forbidden}' which violates secret boundaries.");
                    }
                }

                // Check public method parameters
                var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
                foreach (var method in methods)
                {
                    var parameters = method.GetParameters();
                    foreach (var param in parameters)
                    {
                        var paramNameLower = param.Name!.ToLowerInvariant();
                        foreach (var forbidden in forbiddenSubstrings)
                        {
                            paramNameLower.Should().NotContain(
                                forbidden, 
                                $"Type {type.FullName} method {method.Name} exposes parameter {param.Name} containing forbidden substring '{forbidden}' which violates secret boundaries.");
                        }
                    }
                }
            }
        }
    }
}
