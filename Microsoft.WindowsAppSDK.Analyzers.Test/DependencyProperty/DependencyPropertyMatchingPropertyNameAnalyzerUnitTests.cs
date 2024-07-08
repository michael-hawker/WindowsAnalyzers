using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using static Microsoft.WindowsAppSDK.Analyzers.DiagnosticDescriptors;
using VerifyCS = Microsoft.WindowsAppSDK.Analyzers.Test.CSharpCodeFixVerifier<
    Microsoft.WindowsAppSDK.Analyzers.DependencyPropertyMatchingPropertyNameAnalyzer,
    Microsoft.WindowsAppSDK.Analyzers.DependencyPropertyMatchingPropertyNameAnalyzerCodeFixProvider>;

namespace Microsoft.WindowsAppSDK.Analyzers.Test;

[TestClass]
public class DependencyPropertyMatchingPropertyNameUnitTests
{
    //No diagnostics expected to show up
    [TestMethod]
    public async Task TestMethodEmpty()
    {
        var test = @"";

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    // TODO: This particular scenario probably needs a few different test cases/scenarios?
    // e.g. in a codefix where the proper dependencyproperty is hooked in GetValue/SetValue, but the property container name is incorrect...
    // another could be we have a hint at the property name from the first argument registration of DependencyProperty.Register

    //No diagnostics expected to show up
    [TestMethod]
    public async Task TestMatchingPropertyExistsCorrect()
    {
        var test =
            """
            using System;
            using Microsoft.UI.Xaml;
            
            namespace TestApplication;

            class MyClass : DependencyObject
            {
                public int FooBar
                {
                    get { return (int)GetValue(FooBarProperty); }
                    set { SetValue(FooBarProperty, value); }
                }

                // Using a DependencyProperty as the backing store for FooBar.  This enables animation, styling, binding, etc...
                public static readonly DependencyProperty FooBarProperty =
                    DependencyProperty.Register(nameof(FooBar), typeof(int), typeof(MyClass), new PropertyMetadata(0));
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [TestMethod]
    public async Task TestMatchingPropertyDoesNotExistAtAll()
    {
        var test =
            """
            using System;
            using Microsoft.UI.Xaml;
            
            namespace TestApplication;

            class MyClass : DependencyObject
            {
                // Using a DependencyProperty as the backing store for FooBar.  This enables animation, styling, binding, etc...
                public static readonly DependencyProperty {|#0:FooBarProperty|} =
                    DependencyProperty.Register("FooBarProperty", typeof(int), typeof(MyClass), new PropertyMetadata(0));
            }
            """;

        var expected = VerifyCS.Diagnostic(DependencyPropertyMatchingPropertyNameId).WithLocation(0).WithArguments("FooBarProperty", "FooBar");
        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [TestMethod]
    public async Task TestMatchingPropertyDoesNotExistWithField()
    {
        // TODO: A future code-fix helper here would probably want to identify this as a field and replace with a property
        var test =
            """
            using System;
            using Microsoft.UI.Xaml;
            
            namespace TestApplication;

            class MyClass : DependencyObject
            {
                public int FooBar = 0;

                // Using a DependencyProperty as the backing store for FooBar.  This enables animation, styling, binding, etc...
                public static readonly DependencyProperty {|#0:FooBarProperty|} =
                    DependencyProperty.Register("FooBarProperty", typeof(int), typeof(MyClass), new PropertyMetadata(0));
            }
            """;        

        var expected = VerifyCS.Diagnostic(DependencyPropertyMatchingPropertyNameId).WithLocation(0).WithArguments("FooBarProperty", "FooBar");
        await VerifyCS.VerifyAnalyzerAsync(test, expected);
    }

    [TestMethod]
    public async Task TestMatchingPropertyAddedFix()
    {
        var test =
            """
            using System;
            using Microsoft.UI.Xaml;
            
            namespace TestApplication;
            
            class MyClass : DependencyObject
            {
                // Using a DependencyProperty as the backing store for FooBar.  This enables animation, styling, binding, etc...
                public static readonly DependencyProperty {|#0:FooBarProperty|} =
                    DependencyProperty.Register("FooBarProperty", typeof(float), typeof(MyClass), new PropertyMetadata(0));
            }
            """;

        var fixtest =
            """
            using System;
            using Microsoft.UI.Xaml;
            
            namespace TestApplication;

            class MyClass : DependencyObject
            {
                public float FooBar
                {
                    get { return (float)GetValue(FooBarProperty); }
                    set { SetValue(FooBarProperty, value); }
                }            
                
                // Using a DependencyProperty as the backing store for FooBar.  This enables animation, styling, binding, etc...
                public static readonly DependencyProperty FooBarProperty =
                    DependencyProperty.Register("FooBarProperty", typeof(float), typeof(MyClass), new PropertyMetadata(0));
            }
            """;

        var expected = VerifyCS.Diagnostic(DependencyPropertyMatchingPropertyNameId).WithLocation(0).WithArguments("FooBarProperty", "FooBar");
        await VerifyCS.VerifyCodeFixAsync(test, expected, fixtest);
    }

    // TODO: Create analyzers which check for GetValue/SetValue within the property are gettting/setting the correct DependencyProperty value...
}
