using Microsoft.CodeAnalysis;

namespace Microsoft.WindowsAppSDK.Analyzers;

/// <summary>
/// Centralized location for tracking descriptors of analyzers available. Note: Strings are in Resources.resx.
/// </summary>
partial class DiagnosticDescriptors
{
    public const string WindowsAppSDKWinUICategory = "Microsoft.WindowsAppSDK.Analyzers";

    // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
    // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Localizing%20Analyzers.md for more on localization

    public const string DependencyPropertyNameOfId =        "WASDKWUI0001";
    public const string DependencyPropertyOwnerTypeId =     "WASDKWUI0002";

    /// <summary>
    /// Gets a <see cref="DiagnosticDescriptor"/> for a suggestion to change the literal parameter for a DependencyProperty property to use <c>nameof</c> instead.
    /// <para>
    /// Format: <c>Dependency Property registration for '{0}' can be made more robust using nameof()</c>
    /// </para>
    /// </summary>
    public static readonly DiagnosticDescriptor DependencyPropertyNameOf = new(
        id: DependencyPropertyNameOfId,
        title: new LocalizableResourceString(nameof(Resources.DependencyPropertyNameOfTitle), Resources.ResourceManager, typeof(Resources)),
        messageFormat: new LocalizableResourceString(nameof(Resources.DependencyPropertyNameOfMessageFormat), Resources.ResourceManager, typeof(Resources)),
        category: WindowsAppSDKWinUICategory,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: new LocalizableResourceString(nameof(Resources.DependencyPropertyNameOfDescription), Resources.ResourceManager, typeof(Resources)));

    /// <summary>
    /// Gets a <see cref="DiagnosticDescriptor"/> for a suggestion to change the owner type parameter for a DependencyProperty property to match the container class.
    /// <para>
    /// Format: <c>Dependency Property registration with ownerType parameter '{0}' should match the actual owning type of '{1}'</c>
    /// </para>
    /// </summary>
    public static readonly DiagnosticDescriptor DependencyPropertyOwnerType = new(
        id: DependencyPropertyOwnerTypeId,
        title: new LocalizableResourceString(nameof(Resources.DependencyPropertyOwnerTypeTitle), Resources.ResourceManager, typeof(Resources)),
        messageFormat: new LocalizableResourceString(nameof(Resources.DependencyPropertyOwnerTypeMessageFormat), Resources.ResourceManager, typeof(Resources)),
        category: WindowsAppSDKWinUICategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: new LocalizableResourceString(nameof(Resources.DependencyPropertyOwnerTypeDescription), Resources.ResourceManager, typeof(Resources)));
}
