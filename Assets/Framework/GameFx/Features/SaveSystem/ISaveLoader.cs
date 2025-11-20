
using GameFx.Core;

namespace GameFx.Features.SaveSystem
{
    public interface ISaveLoader
    {
        Result<bool> LoadProfile();
        Result<bool> SaveProfile();
    }
}