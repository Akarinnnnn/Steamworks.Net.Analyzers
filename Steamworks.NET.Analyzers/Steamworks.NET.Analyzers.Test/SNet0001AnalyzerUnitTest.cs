using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Buffers;
using System.IO;
using System.Text;
using System.Threading.Tasks;
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
            const string testSource = """
                namespace Steamworks
                {
                    static class Internal
                    {
                        void InternalMethod()
                        {
                            var test = new CallResult<AsyncCallResult>();
                            var h = SteamAsyncTest.TestCallResult();
                        test.Set(h, (a, b) => {});
                        }
                    }
                }

                using Steamworks;

                class Main0
                {
                    static void Main()
                    {
                        CallResult<AsyncCallResult_t> testNot0001 = new();
                        var h = SteamAsyncTest.TestCallResult();
                        testNot0001.Set(h, (a, b) => {});

                        CallResult<AsyncCallback_t> test0001 = new();
                        test0002.

                        Callback<AsyncCallback_t> testNot0002 = new 

                        CallResult<AsyncCallResult> test0002 = new((r) => {});
                    }

                    static CallRes TestProperty { get {} }
                }
                """;

            await VerifyCS.VerifyAnalyzerAsync(CreateTestCode(testSource));
        }

        private static string CreateTestCode(string testSource)
        {
			Stream commonDef = typeof(SNet0001AnalyzerUnitTest).Assembly.GetManifestResourceStream("CommonDef.cs");
			int commonLength = (int)commonDef.Length;
			var buffer = ArrayPool<byte>.Shared.Rent(commonLength);
            commonDef.Read(buffer, 0, commonLength);

			return Encoding.UTF8.GetString(buffer) + testSource;
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
