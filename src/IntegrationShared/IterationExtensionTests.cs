//using System.Collections.Generic;
//using FluentAssertions;
//using ivaldez.Sql.SqlBulkLoader.Core;
//using Xunit;
//using Xunit.Abstractions;

//namespace ivaldez.Sql.IntegrationShared
//{
//    public class IterationExtensionTests
//    {
//        private readonly ITestOutputHelper _output;

//        public IterationExtensionTests(ITestOutputHelper output)
//        {
//            _output = output;
//        }

//        [Fact]
//        public void ShouldBatchEnumerable()
//        {
//            var list = new List<string>()
//            {
//                "1", "2", "3", "4", "5", "6",
//                "1a", "2a", "3a", "4a", "5a", "6a",
//                "1b", "2b", "3b", "4b", "5b", "6b",
//                "1c", "2c", "3c", "4c", "5c", "6c"
//            };

//            var batches = list.Batch(6);

//            var assertionStrings = new[]
//            {
//                "1 2 3 4 5 6 ",
//                "1a 2a 3a 4a 5a 6a ",
//                "1b 2b 3b 4b 5b 6b ",
//                "1c 2c 3c 4c 5c 6c "
//            };

//            var index = 0;
//            foreach (var batch in batches)
//            {
//                var outputLine = "";
//                foreach (var item in batch)
//                {
//                    var iterItem = item;

//                    outputLine += iterItem + " ";
//                }

//                _output.WriteLine($"{index} - {outputLine}");

//                outputLine.Should().Be(assertionStrings[index++]);
//            }
//        }
//    }
//}
