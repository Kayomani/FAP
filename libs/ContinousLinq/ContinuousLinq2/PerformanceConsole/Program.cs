using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PerformanceConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            PerformanceTest performanceTest = new PerformanceTest();
            //performanceTest.GroupJoin();
            //performanceTest.MySkipListVsLomontLookups();
            //performanceTest.MySkipListVsLomontAdds();
            //performanceTest.SkipListVsSortedDictionaryAdds();
            //performanceTest.SkipListVsSortedDictionaryLookups();
            //performanceTest.LlrbtVsSortedDictionaryAdds();
            //performanceTest.LlrbtVsSortedDictionaryLookups();
            //performanceTest.RecursiveFunctionVsStack();
            //performanceTest.CompareQueryCreation();
            //performanceTest.GetInterfaceTest();
            //performanceTest.MemoryTest();
            //performanceTest.SortingTest();
            //performanceTest.TestDynamicProperty();
            //performanceTest.TestDynamicInvoke();
            //performanceTest.SelectTest();
            //performanceTest.SelectLinearUpdateTest();
            //performanceTest.SelectUnrelatedPropertyLinearUpdateTest();
            //performanceTest.WhereTest();
            //performanceTest.ContinuousSumWithoutPausing();
            //performanceTest.ContinuousSumWithPausing();
            //performanceTest.TakeTest();
            //performanceTest.SkipTest();
            performanceTest.SkipTakeCombineAssertTest();
        }
    }
}
