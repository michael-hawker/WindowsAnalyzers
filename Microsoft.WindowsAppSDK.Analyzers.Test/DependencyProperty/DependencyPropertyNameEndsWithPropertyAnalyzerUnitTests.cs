using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using VerifyCS = Microsoft.WindowsAppSDK.Analyzers.Test.CSharpCodeFixVerifier<
    Microsoft.WindowsAppSDK.Analyzers.DependencyPropertyNameEndsWithPropertyAnalyzer,
    Microsoft.WindowsAppSDK.Analyzers.DependencyPropertyNameEndsWithPropertyAnalyzerCodeFixProvider>;
using static Microsoft.WindowsAppSDK.Analyzers.DiagnosticDescriptors;

namespace Microsoft.WindowsAppSDK.Analyzers.Test;

[TestClass]
public class DependencyPropertyNameEndsWithPropertyUnitTests
{
    //No diagnostics expected to show up
    [TestMethod]
    public async Task TestMethodEmpty()
    {
        var test = @"";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    //No diagnostics expected to show up
    [TestMethod]
    public async Task TestPropertyNameIsCorrect()
    {
        var test =
            """
            using System;
            using Microsoft.UI.Xaml;
            
            namespace TestApplication;

            class MyClass : DependencyObject
            {
                public int MyProperty
                {
                    get { return (int)GetValue(MyPropertyProperty); }
                    set { SetValue(MyPropertyProperty, value); }
                }

                // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
                public static readonly DependencyProperty MyPropertyProperty =
                    DependencyProperty.Register(nameof(MyProperty), typeof(int), typeof(MyClass), new PropertyMetadata(0));
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [TestMethod]
    public async Task TestPropertyNameIsIncorrect()
    {
        var test =
            """
            using System;
            using Microsoft.UI.Xaml;
            
            namespace TestApplication;

            class MyClass : DependencyObject
            {
                public int MyProperty
                {
                    get { return (int)GetValue(MyPropertyIncorrect); }
                    set { SetValue(MyPropertyIncorrect, value); }
                }

                // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
                public static readonly DependencyProperty {|#0:MyPropertyIncorrect|} =
                    DependencyProperty.Register(nameof(MyProperty), typeof(int), typeof(MyClass), new PropertyMetadata(0));
            }
            """;

        var expected = VerifyCS.Diagnostic(DependencyPropertyNameEndsWithPropertyId).WithLocation(0).WithArguments("MyPropertyIncorrect");
        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [TestMethod]
    public async Task TestDependencyPropertyNameIdentifierCodeFix()
    {
        var test =
            """
            using System;
            using Microsoft.UI.Xaml;
            
            namespace TestApplication;

            class MyClass : DependencyObject
            {
                public int MyProperty
                {
                    get { return (int)GetValue(MyPropertyIncorrect); }
                    set { SetValue(MyPropertyIncorrect, value); }
                }

                // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
                public static readonly DependencyProperty {|#0:MyPropertyIncorrect|} =
                    DependencyProperty.Register(nameof(MyProperty), typeof(int), typeof(MyClass), new PropertyMetadata(0));
            }
            """;

        // Note: The code even after this fix still isn't proper, but we have the other analyzer to detect/fix that
        // LINK: DependencyPropertyMatchingPropertyNameAnalyzer.cs
        var fixtest =
            """
            using System;
            using Microsoft.UI.Xaml;
            
            namespace TestApplication;

            class MyClass : DependencyObject
            {
                public int MyProperty
                {
                    get { return (int)GetValue(MyPropertyIncorrectProperty); }
                    set { SetValue(MyPropertyIncorrectProperty, value); }
                }

                // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
                public static readonly DependencyProperty MyPropertyIncorrectProperty =
                    DependencyProperty.Register(nameof(MyProperty), typeof(int), typeof(MyClass), new PropertyMetadata(0));
            }
            """;

        var expected = VerifyCS.Diagnostic(DependencyPropertyNameEndsWithPropertyId).WithLocation(0).WithArguments("MyPropertyIncorrect");
        await VerifyCS.VerifyCodeFixAsync(test, expected, fixtest);
    }
}
