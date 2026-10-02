using System.Threading;
using Core.Abstractions.Results;
using Cysharp.Threading.Tasks;

namespace Core.Abstractions.Services.Save
{
    public interface ISaveService
    {
        UniTask<bool> ExistsAsync(string slot, CancellationToken ct = default);
        UniTask<OperationResult> SaveAsync<TSnapshot>(string slot, TSnapshot snapshot, CancellationToken ct = default);
        UniTask<OperationResult<TSnapshot>> LoadAsync<TSnapshot>(string slot, CancellationToken ct = default);
        UniTask<OperationResult> DeleteAsync(string slot, CancellationToken ct = default);
    }
}
