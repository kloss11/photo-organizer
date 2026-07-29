using NetArchTest.Rules;
using PhotoOrganizer.Domain.Model;

namespace PhotoOrganizer.ArchitectureTests;

/// <summary>
/// Egzekwuje złote zasady Clean Architecture. Testy same w sobie są tanią „polisą" —
/// wychwycą przypadkowe naruszenie granic warstw w trakcie dalszego rozwoju.
/// </summary>
public sealed class LayerDependencyTests
{
    private static readonly System.Reflection.Assembly Domain = typeof(MediaFile).Assembly;
    private static readonly System.Reflection.Assembly Application = typeof(PhotoOrganizer.Application.DependencyInjection).Assembly;
    private static readonly System.Reflection.Assembly Infrastructure = typeof(PhotoOrganizer.Infrastructure.DependencyInjection).Assembly;

    [Fact]
    public void Domain_depends_on_nothing_outside_itself()
    {
        var result = Types.InAssembly(Domain)
            .Should()
            .NotHaveDependencyOnAny(
                "PhotoOrganizer.Application",
                "PhotoOrganizer.Infrastructure",
                "PhotoOrganizer.Platform",
                "PhotoOrganizer.Presentation",
                "Microsoft.Extensions",
                "MetadataExtractor")
            .GetResult();

        Assert.True(result.IsSuccessful, Describe(result));
    }

    [Fact]
    public void Application_does_not_depend_on_Infrastructure_or_platforms()
    {
        var result = Types.InAssembly(Application)
            .Should()
            .NotHaveDependencyOnAny(
                "PhotoOrganizer.Infrastructure",
                "PhotoOrganizer.Platform",
                "PhotoOrganizer.Presentation",
                "MetadataExtractor")
            .GetResult();

        Assert.True(result.IsSuccessful, Describe(result));
    }

    [Fact]
    public void Infrastructure_does_not_depend_on_presentation_or_platforms()
    {
        var result = Types.InAssembly(Infrastructure)
            .Should()
            .NotHaveDependencyOnAny(
                "PhotoOrganizer.Platform",
                "PhotoOrganizer.Presentation")
            .GetResult();

        Assert.True(result.IsSuccessful, Describe(result));
    }

    private static string Describe(TestResult result) =>
        result.IsSuccessful
            ? string.Empty
            : "Naruszenia zależności: " + string.Join(", ", result.FailingTypeNames ?? []);
}
