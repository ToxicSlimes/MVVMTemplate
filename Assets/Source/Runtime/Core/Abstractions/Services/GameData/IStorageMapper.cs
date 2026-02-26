using System.Collections.Generic;

namespace Core.Abstractions.Services.GameData
{
    public interface IStorageMapper<out TResult>
    {
        TResult Map(Dictionary<string, object> storage);
    }
}
