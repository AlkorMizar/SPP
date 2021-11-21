using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using TestGenerator;
using TestGeneratormain.Context;

namespace Application
{
    class Pipeline
    {
        private readonly int maxDegreeOfParall;
        private DecomposeCode decomposer;
        private CreateTestCode testGenerator;
        public Pipeline(int _maxDegreeOfParall)
        {
            maxDegreeOfParall = _maxDegreeOfParall;
            decomposer = new DecomposeCode();
            testGenerator = new CreateTestCode();

        }

        private async Task<string> ReadAllTextAsync(string path) { return await File.ReadAllTextAsync(path); }
        
        private async Task WriteAllAsync(IEnumerable<Code> codes) {
            List<Task> codesTask = new List<Task>();
            foreach (var code in codes)
            {
                Console.WriteLine(code.Path);
                codesTask.Add(File.WriteAllTextAsync(code.Path, code.CodeText));
            }
            await Task.WhenAll(codesTask);
        }


        private async Task<IEnumerable<Code>> CreateSeveralTests(IEnumerable<TypeContext> contexts) {
            List<Task<Code>> codesTask = new List<Task<Code>>();
            foreach (var context in contexts)
            {
                codesTask.Add(testGenerator.AsyncCreateTestClass(context));
            }
            var result=await Task.WhenAll(codesTask);
            return result;
        }
        
        public async Task PerformProcessing(IEnumerable<string> files)
        {

            var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };
            
            var readingBlock = new TransformBlock<string, string>(
                ReadAllTextAsync,new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = maxDegreeOfParall });

            var decompose = new TransformBlock<string,IEnumerable<TypeContext>>(
                decomposer.AsyncDecomposeType, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = maxDegreeOfParall });
            
            var conccurAndProcess = new TransformBlock<IEnumerable<TypeContext>, IEnumerable<Code>>(
                CreateSeveralTests, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = maxDegreeOfParall });

            var writingBlock = new ActionBlock<IEnumerable<Code>>(
                WriteAllAsync,new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = maxDegreeOfParall });

            readingBlock.LinkTo(decompose, linkOptions);
            decompose.LinkTo(conccurAndProcess, linkOptions);
            conccurAndProcess.LinkTo(writingBlock, linkOptions);

            foreach (string file in files)
            {
                readingBlock.Post(file);
            }

            readingBlock.Complete();

            await writingBlock.Completion;
        }
    }
}
