using System.Threading;
using Core.Abstractions.Results;
using Cysharp.Threading.Tasks;

namespace Core.Abstractions.Services.Save
{
    public interface IRawSaveStore
    {
        UniTask<bool> ExistsAsync(string pathKey, CancellationToken ct = default);
        UniTask<OperationResult<RawSaveCopies>> LoadCopiesAsync(
            string pathKey,
            CancellationToken ct = default);
        UniTask<OperationResult> SaveAsync(string pathKey, byte[] data, CancellationToken ct = default);
        UniTask<OperationResult> DeleteAsync(string pathKey, CancellationToken ct = default);
    }
}
