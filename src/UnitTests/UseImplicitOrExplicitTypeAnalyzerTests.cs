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
        {|Y0001:string s1 = """"|};
        {|Y0001:string s1Interpolated = $""""|};
        {|Y0001:string s1Verbatim = @""""|};
        {|Y0001:string s1VerbatimInterpolated = @$""""|};
        {|Y0001:string s1InterpolatedVerbatim = $@""""|};

        {|Y0001:int i1 = 0|};
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
            await VerifyCS.VerifyCodeFixAsync(code, fixedCode);
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

        [TestMethod]
        public async Task AsExpression_ShouldUseVar()
        {
            var code = @"
using System;
class C
{
    public void Process()
    {
        {|Y0001:A a = new A()|};
        {|Y0001:IInterface s = a as IInterface|};
        IInterface i = a;
    }
}
class A : IInterface
{
}
interface IInterface
{
}
";
            var fixedCode = @"
using System;
class C
{
    public void Process()
    {
        var a = new A();
        var s = a as IInterface;
        IInterface i = a;
    }
}
class A : IInterface
{
}
interface IInterface
{
}
";
            await VerifyCS.VerifyCodeFixAsync(code, fixedCode);
        }

        [TestMethod]
        public async Task DefaultExpression_ShouldUseVar()
        {
            var code = @"
using System;
class C
{
    public void Process()
    {
        {|#0:int x = default(int)|};
        int y = default;
        var z = default(int);
    }
}
";
            var fixedCode = @"
using System;
class C
{
    public void Process()
    {
        var x = default(int);
        int y = default;
        var z = default(int);
    }
}
";
            await VerifyCS.VerifyCodeFixAsync(code, VerifyCS.Diagnostic(UseImplicitOrExplicitTypeAnalyzer.UseImplicitTypeDiagnosticId).WithLocation(0), fixedCode);
        }

        [TestMethod]
        public async Task ForEach_ShouldUseVar()
        {
            var code = @"
class C
{
    public void Process(string[] items)
    {
        {|#0:foreach (var item in items) {}|}
    }
}
";
            var fixedCode = @"
class C
{
    public void Process(string[] items)
    {
        foreach (string item in items) {}
    }
}
";
            await VerifyCS.VerifyCodeFixAsync(code, VerifyCS.Diagnostic(UseImplicitOrExplicitTypeAnalyzer.UseExplicitTypeDiagnosticId).WithLocation(0), fixedCode);
        }

        [TestMethod]
        public async Task OutVarArgument_ShouldUseExplicitType()
        {
            var code = @"
class C
{
    public void Process(string s)
    {
        if (int.TryParse(s, out {|#0:var i|}))
        {
            System.Console.WriteLine(i);
        }
    }
}
";
            var fixedCode = @"
class C
{
    public void Process(string s)
    {
        if (int.TryParse(s, out int i))
        {
            System.Console.WriteLine(i);
        }
    }
}
";
            await VerifyCS.VerifyCodeFixAsync(code, VerifyCS.Diagnostic(UseImplicitOrExplicitTypeAnalyzer.UseExplicitTypeDiagnosticId).WithLocation(0), fixedCode);
        }

        [TestMethod]
        public async Task TestAddUsing()
        {
            var code = @"
using System.Linq;

class C
{
    public void M(string[] s)
    {
        {|#0:var x = s.ToList()|};
    }
}
";
            var fixedCode = @"
using System.Collections.Generic;
using System.Linq;

class C
{
    public void M(string[] s)
    {
        List<string> x = s.ToList();
    }
}
";
            await VerifyCS.VerifyCodeFixAsync(code, VerifyCS.Diagnostic(UseImplicitOrExplicitTypeAnalyzer.UseExplicitTypeDiagnosticId).WithLocation(0), fixedCode);
        }
    }
}
