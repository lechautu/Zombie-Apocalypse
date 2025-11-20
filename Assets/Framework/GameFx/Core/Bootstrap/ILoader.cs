using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace GameFx.Core.Bootstrap
{
    public interface ILoader
    {
        UniTask Load();

        bool IsLoaded { get; }
        List<ILoader> Dependencies { get; }
    }
}
