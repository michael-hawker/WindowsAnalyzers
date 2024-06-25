using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using VerifyCS = Microsoft.WindowsAppSDK.Analyzers.Test.CSharpCodeFixVerifier<
    Microsoft.WindowsAppSDK.Analyzers.DependencyPropertyNameOfAnalyzer,
    Microsoft.WindowsAppSDK.Analyzers.DependencyPropertyNameOfAnalyzerCodeFixProvider>;

namespace Microsoft.WindowsAppSDK.Analyzers.Test;

[TestClass]
public class DependencyPropertyNameOfAnalyzerUnitTests
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
    public async Task TestMethodNameOfCorrect()
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
                    DependencyProperty.Register({|#0:"MyProperty"|}, typeof(int), typeof(MyClass), new PropertyMetadata(0));
            }
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
            """;

        var expected = VerifyCS.Diagnostic("DependencyPropertyNameOfAnalyzer").WithLocation(0).WithArguments("MyProperty");
        await VerifyCS.VerifyCodeFixAsync(test, expected, fixtest);
    }

    // TODO: Create another analyzer for mismatched owner class type (i.e. typeof(MyOtherClass) when contained within MyClass...
}
