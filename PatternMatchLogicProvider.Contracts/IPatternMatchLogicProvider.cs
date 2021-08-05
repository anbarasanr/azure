using System;

namespace PatternMatchLogicProvider.Contracts
{
    public interface IPatternMatchLogicProvider
    {
        string Pattern { get; }
        bool IsMatch(string data);
        bool IsPatternValid();
    }
}
