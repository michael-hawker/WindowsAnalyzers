using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using VerifyCS = Microsoft.WindowsAppSDK.Analyzers.Test.CSharpCodeFixVerifier<
    Microsoft.WindowsAppSDK.Analyzers.DependencyPropertyOwnerTypeAnalyzer,
    Microsoft.WindowsAppSDK.Analyzers.DependencyPropertyOwnerTypeAnalyzerCodeFixProvider>;
using static Microsoft.WindowsAppSDK.Analyzers.DiagnosticDescriptors;

namespace Microsoft.WindowsAppSDK.Analyzers.Test;

[TestClass]
public class DependencyPropertyOwnerTypeAnalyzerUnitTests
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
    public async Task TestOwnerTypeCorrect()
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
    public async Task TestOwnerTypeIncorrect()
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
                    DependencyProperty.Register(nameof(MyProperty), typeof(int), typeof({|#0:MyOtherClass|}), new PropertyMetadata(0));
            }

            class MyOtherClass { }
            """;

        var expected = VerifyCS.Diagnostic(DependencyPropertyOwnerTypeId).WithLocation(0).WithArguments("MyOtherClass", "MyClass");
        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [TestMethod]
    public async Task TestMethodNameOfCodeFix()
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
                    DependencyProperty.Register(nameof(MyProperty), typeof(int), typeof({|#0:MyOtherClass|}), new PropertyMetadata(0));
            }

            class MyOtherClass { }
            """;

        var fixtest =
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

            class MyOtherClass { }
            """;

        var expected = VerifyCS.Diagnostic(DependencyPropertyOwnerTypeId).WithLocation(0).WithArguments("MyOtherClass", "MyClass");
        await VerifyCS.VerifyCodeFixAsync(test, expected, fixtest);
    }

    // TODO: Next analyzer I think is checking for declaration of ____Property DP and seeing if a "____" property exists in class...
    // Codefixer could maybe just generate the standard/blank property template for DP with that name...?
}
