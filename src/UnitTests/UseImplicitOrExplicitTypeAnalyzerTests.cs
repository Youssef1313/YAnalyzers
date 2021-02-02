using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VerifyCS = YAnalyzers.Test.CSharpCodeFixVerifier<
    YAnalyzers.CSharp.CSharpUseImplicitOrExplicitTypeAnalyzer,
    YAnalyzers.CSharp.UseImplicitOrExplicitTypeCodeFix>;

namespace YAnalyzers.Test
{
    // Tests are taken from Roslyn:
    // https://github.com/dotnet/roslyn/blob/6bf3920b9170f201c9c2fb8301d2a07e22870756/src/Analyzers/CSharp/Tests/UseImplicitOrExplicitType/UseImplicitTypeTests.cs
    [TestClass]
    public class UseImplicitOrExplicitTypeAnalyzerTests
    {
        [TestMethod]
        public async Task FieldDeclaration_NoDiagnostic()
        {
            var code = @"
class Program
{
    int _myfield = 5;
}
";

            await VerifyCS.VerifyCodeFixAsync(code, code);
        }

        [TestMethod]
        public async Task EventFieldDeclaration_NoDiagnostic()
        {
            var code = @"
using System;
class Program
{
    public delegate void D();
    public event D _myevent = Console.WriteLine;
}
";

            await VerifyCS.VerifyCodeFixAsync(code, code);
        }

        [TestMethod]
        public async Task ConstantDeclaration_NoDiagnostic()
        {
            var code = @"
class Program
{
    void Method()
    {
        const int x = 5;
    }
}
";
            await VerifyCS.VerifyCodeFixAsync(code, code);
        }

        [TestMethod]
        public async Task NullDeclaration_NoDiagnostic()
        {
            var code = @"
using System;
class Program
{
    void Method()
    {
        Program x = null;
    }
}
";
            await VerifyCS.VerifyCodeFixAsync(code, code);
        }

        [TestMethod]
        public async Task Dynamic_NoDiagnostic()
        {
            var code = @"
using System;
class Program
{
    void Method()
    {
        dynamic x = 1;
    }
}
";
            await VerifyCS.VerifyCodeFixAsync(code, code);
        }

        [TestMethod]
        public async Task AnonymousMethod_NoDiagnostic()
        {
            var code = @"
using System;
class Program
{
    void Method()
    {
        Func<string, bool> comparer = delegate (string value) {
            return value != ""0"";
        };
    }
}
";
            await VerifyCS.VerifyCodeFixAsync(code, code);
        }

        [TestMethod]
        public async Task LambdaExpression_NoDiagnostic()
        {
            var code = @"
using System;
class Program
{
    void Method()
    {
        Func<int, int> x = y => y * y;
    }
}
";
            await VerifyCS.VerifyCodeFixAsync(code, code);
        }

        [TestMethod]
        public async Task MethodGroup_NoDiagnostic()
        {
            var code = @"
using System;
class Program
{
    void Method()
    {
        Func<string, string> copyStr = string.Copy;
    }
}
";
            await VerifyCS.VerifyCodeFixAsync(code, code);
        }

        [TestMethod]
        public async Task MultipleDeclarators_NoDiagnostic()
        {
            var code = @"
using System;
class Program
{
    void Method()
    {
        int x = 5, y = x;
    }
}
";
            await VerifyCS.VerifyCodeFixAsync(code, code);
        }

        [TestMethod]
        public async Task DeclarationWithoutInitializer_NoDiagnostic()
        {
            var code = @"
using System;
class Program
{
    void Method()
    {
        Program x;
    }
}
";
            await VerifyCS.VerifyCodeFixAsync(code, code);
        }

        [TestMethod]
        public async Task WillHaveConflicts_NoDiagnostic()
        {
            var code = @"
using System;
class Program
{
    void Method()
    {
        Program p = new Program();
    }
    class var
    {
    }
}
";
            await VerifyCS.VerifyCodeFixAsync(code, code);
        }


        [TestMethod]
        public async Task SeeminglyConflictingType_ShouldUseVar()
        {
            var code = @"
using System;
class var<T>
{
    void M()
    {
        {|#0:var<int> c = new var<int>()|};
        var c2 = new var<int>();
    }
}
";

            var fixedCode = @"
using System;
class var<T>
{
    void M()
    {
        var c = new var<int>();
        var c2 = new var<int>();
    }
}
";
            await VerifyCS.VerifyCodeFixAsync(code, VerifyCS.Diagnostic(UseImplicitOrExplicitTypeAnalyzer.UseImplicitTypeDiagnosticId).WithLocation(0), fixedCode);
        }

        [TestMethod]
        public async Task SeeminglyConflictingTypeNonGeneric_ShouldUseVar_NoDiagnostic()
        {
            var code = @"
using System;
class var
{
    void M()
    {
        var c = new var();
    }
}
";
            await VerifyCS.VerifyCodeFixAsync(code, code);
        }

        [TestMethod]
        public async Task PrimitiveTypeIsApparent_ShouldUseVar()
        {
            var code = @"
class Program
{
    void M()
    {
        {|#0:string s1 = """"|};
        {|#1:string s1Interpolated = $""""|};
        {|#2:string s1Verbatim = @""""|};
        {|#3:string s1VerbatimInterpolated = @$""""|};
        {|#4:string s1InterpolatedVerbatim = $@""""|};

        {|#5:int i1 = 0|};
        uint ui1 = 0;
    }

    void M2()
    {
        var s1 = """";
        var s1Interpolated = $"""";
        var s1Verbatim = @"""";
        var s1VerbatimInterpolated = @$"""";
        var s1InterpolatedVerbatim = $@"""";

        var i1 = 0;
    }
}
";

            var fixedCode = @"
class Program
{
    void M()
    {
        var s1 = """";
        var s1Interpolated = $"""";
        var s1Verbatim = @"""";
        var s1VerbatimInterpolated = @$"""";
        var s1InterpolatedVerbatim = $@"""";

        var i1 = 0;
        uint ui1 = 0;
    }

    void M2()
    {
        var s1 = """";
        var s1Interpolated = $"""";
        var s1Verbatim = @"""";
        var s1VerbatimInterpolated = @$"""";
        var s1InterpolatedVerbatim = $@"""";

        var i1 = 0;
    }
}
";
            var expectedDiagnostics = new[]
            {
                VerifyCS.Diagnostic(UseImplicitOrExplicitTypeAnalyzer.UseImplicitTypeDiagnosticId).WithLocation(0),
                VerifyCS.Diagnostic(UseImplicitOrExplicitTypeAnalyzer.UseImplicitTypeDiagnosticId).WithLocation(1),
                VerifyCS.Diagnostic(UseImplicitOrExplicitTypeAnalyzer.UseImplicitTypeDiagnosticId).WithLocation(2),
                VerifyCS.Diagnostic(UseImplicitOrExplicitTypeAnalyzer.UseImplicitTypeDiagnosticId).WithLocation(3),
                VerifyCS.Diagnostic(UseImplicitOrExplicitTypeAnalyzer.UseImplicitTypeDiagnosticId).WithLocation(4),
                VerifyCS.Diagnostic(UseImplicitOrExplicitTypeAnalyzer.UseImplicitTypeDiagnosticId).WithLocation(5),
            };
            await VerifyCS.VerifyCodeFixAsync(code, expectedDiagnostics, fixedCode);
        }

        [TestMethod]
        public async Task PrimitiveTypeIsNotApparent_ShouldNotUseVar()
        {
            var code = @"
class Program
{
    int GetOne() => 1;

    void M()
    {
        int one = GetOne();
        {|#0:var one2 = GetOne()|};
    }
}
";

            var fixedCode = @"
class Program
{
    int GetOne() => 1;

    void M()
    {
        int one = GetOne();
        int one2 = GetOne();
    }
}
";

            await VerifyCS.VerifyCodeFixAsync(code, VerifyCS.Diagnostic(UseImplicitOrExplicitTypeAnalyzer.UseExplicitTypeDiagnosticId).WithLocation(0), fixedCode);
        }
    }
}
