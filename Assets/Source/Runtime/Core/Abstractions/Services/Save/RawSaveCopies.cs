using Core.Abstractions.Results;

namespace Core.Abstractions.Services.Save
{
    public readonly struct RawSaveCopies
    {
        public RawSaveCopies(
            OperationResult<byte[]> primary,
            OperationResult<byte[]> backup)
        {
            Primary = primary;
            Backup = backup;
        }

        public OperationResult<byte[]> Primary { get; }
        public OperationResult<byte[]> Backup { get; }
    }
}
