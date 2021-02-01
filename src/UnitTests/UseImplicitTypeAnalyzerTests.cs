using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VerifyCS = YAnalyzers.Test.CSharpCodeFixVerifier<
    YAnalyzers.CSharp.CSharpUseImplicitTypeAnalyzer,
    YAnalyzers.CSharp.UseImplicitTypeCodeFix>;

namespace YAnalyzers.Test
{
    // Tests are taken from Roslyn:
    // https://github.com/dotnet/roslyn/blob/6bf3920b9170f201c9c2fb8301d2a07e22870756/src/Analyzers/CSharp/Tests/UseImplicitOrExplicitType/UseImplicitTypeTests.cs
    [TestClass]
    public class UseImplicitTypeAnalyzerTests
    {
        [TestMethod]
        public async Task FieldDeclaration_NoDiagnostic()
        {
            var code = @"
using System;
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
using System;
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
    }
}
