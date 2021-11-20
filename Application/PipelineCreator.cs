using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using TestGeneratormain.Context;

namespace Application
{
    class PipelineCreator
    {
        private readonly int max;

        public PipelineCreator(int _max)
        {
            max = _max;
        }

        private async Task<string> ReadAllTextAsync(string path) { return await File.ReadAllTextAsync(path); }
        private async void WriteAllAsync(Code code) { return await File.WriteAllTextAsync("")}
        public async Task PerformProcessing(IEnumerable<string> files)
        {

            var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };
            
            var readingBlock = new TransformBlock<string, string>(
                ReadAllTextAsync,new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = max });

            var processingBlock = new TransformBlock<FileWithContent, FileWithContent[]>(
                async fwc => await new TestClassGenerator().GetTestClassFiles(fwc),
                new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = _pipelineConfiguration.MaxProcessingTasks });

            var writingBlock = new ActionBlock<FileWithContent[]>(async fwc => await WriteFile(fwc),
                new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = _pipelineConfiguration.MaxWritingTasks });

            readingBlock.LinkTo(processingBlock, linkOptions);
            processingBlock.LinkTo(writingBlock, linkOptions);

            foreach (string file in files)
            {
                readingBlock.Post(file);
            }

            readingBlock.Complete();

            await writingBlock.Completion;
        }

        private async Task<string> ReadFile(string filePath)
        {
            string result;
            using (var streamReader = new StreamReader(filePath))
            {
                result = await streamReader.ReadToEndAsync();
            }
            return result;
        }

        private async Task WriteFile(FileWithContent[] filesWithContent)
        {
            foreach (var file in filesWithContent)
            {
                using (var streamWriter = new StreamWriter(file.Path))
                {
                    await streamWriter.WriteAsync(file.Content);
                }
            }
        }
    }
}
}
