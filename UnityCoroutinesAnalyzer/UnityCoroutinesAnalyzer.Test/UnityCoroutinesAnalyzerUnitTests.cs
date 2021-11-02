using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using VerifyCS = UnityCoroutinesAnalyzer.Test.CSharpCodeFixVerifier<
    UnityCoroutinesAnalyzer.CoroutineInvocationAnalyzer,
    UnityCoroutinesAnalyzer.CoroutineInvocationFixProvider>;

namespace UnityCoroutinesAnalyzer.Test
{
    [TestClass]
    public class UnityCoroutinesAnalyzerUnitTest
    {
        //[TestMethod]
        public async Task TestMethod1()
        {
            var test = @"";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

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
    using System.Collections;

    namespace ConsoleApplication1
    {
        class TypeName
        {   
            IEnumerator Some()
            {
                while(true)
                {
                    int s = 0;
                }
            }
        }
    }";

            var fixtest = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using System.Collections;

    namespace ConsoleApplication1
    {
        class TypeName
        {   
            IEnumerator Some()
            {
                while(true)
                {
                    int s = 0;
                    yield return null;
                }
            }
        }
    }";

            await VerifyCS.VerifyCodeFixAsync(test, fixtest);
        }
    }
}
