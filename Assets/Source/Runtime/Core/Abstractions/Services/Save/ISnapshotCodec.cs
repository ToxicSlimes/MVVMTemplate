using Core.Abstractions.Results;

namespace Core.Abstractions.Services.Save
{
    public interface ISnapshotCodec
    {
        OperationResult<byte[]> Encode<TSnapshot>(TSnapshot snapshot);
        OperationResult<TSnapshot> Decode<TSnapshot>(byte[] data);
    }
}
