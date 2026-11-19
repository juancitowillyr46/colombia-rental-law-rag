using System.Reflection;

namespace RentalLawAssistant.Tests.Unit;

public sealed class ArchitectureTests
{
    [Fact]
    public void CoreDoesNotReferenceAspNetCore()
    {
        var core = Assembly.Load("RentalLawAssistant.Core");

        var aspNetReferences = core.GetReferencedAssemblies()
            .Where(reference => reference.Name?.StartsWith("Microsoft.AspNetCore", StringComparison.Ordinal) == true)
            .Select(reference => reference.Name)
            .ToList();

        Assert.Empty(aspNetReferences);
    }
}
