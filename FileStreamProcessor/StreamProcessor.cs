using PatternMatchLogicProvider.Contracts;
using System;
using System.IO;

namespace FileStreamProcessor
{
    public class StreamProcessor
    {
        public IPatternMatchLogicProvider PatternMatchLogicProvider { get; }

        /// <summary>
        /// Reads the stream as line strings and invokes pattern matching logic provider instance to check if it matches a line
        /// </summary>
        /// <param name="fileStream"></param>
        /// <param name="patternMatchingLogicProvider"></param>
        public StreamProcessor(IPatternMatchLogicProvider patternMatchLogicProvider)
        {
            PatternMatchLogicProvider = patternMatchLogicProvider;
        }

        public bool IsMatchFoundInStream(Stream fileStream)
        {
            using (StreamReader sr = new StreamReader(fileStream))
            {
                while (sr.Peek() != 0)
                {
                    string line = sr.ReadLine();

                    if (PatternMatchLogicProvider.IsMatch(line))
                        return true;
                }
            }

            return false;
        }

    }
}
