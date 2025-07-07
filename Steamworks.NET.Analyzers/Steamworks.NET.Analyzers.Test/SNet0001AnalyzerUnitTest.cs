using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = Steamworks.NET.Analyzers.Test.CSharpCodeFixVerifier<
    Steamworks.NET.Analyzers.SNetCallbacksAnalyzer,
    Steamworks.NET.Analyzers.SNet0001CodeFixProvider>;

namespace Steamworks.NET.Analyzers.Test
{
    [TestClass]
    public class SNet0001AnalyzerUnitTest
    {
        //No diagnostics expected to show up
        [TestMethod]
        public async Task NotToFix()
        {
            var test =
				CommonSourceDef.AsyncCallDefiniations + """
                using Steamworks;

                class Main0
                {
                    static void Main()
                    {
                        CallResult<AsyncCallResult> test = new();
                        var h = SteamAsyncTest.TestCallResult();
                        test.Set(h, (a, b) => {});
                    }
                }
                """;

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public async Task TestMethod2()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class {|#0:TypeName|}
        {   
        }
    }";

            var fixtest = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TYPENAME
        {   
        }
    }";

            var expected = VerifyCS.Diagnostic("SNET0001");
            await VerifyCS.VerifyCodeFixAsync(test, expected, fixtest);
        }
    }
}
