namespace Core.Domain.Models.Cache
{
    public class CacheResult<T>
    {
        public bool IsHit { get; }
        public T Data { get; }
        public string ContentHash { get; }

        private CacheResult(bool isHit, T data, string contentHash)
        {
            IsHit = isHit;
            Data = data;
            ContentHash = contentHash;
        }

        public static CacheResult<T> Hit(T data, string contentHash)
            => new CacheResult<T>(true, data, contentHash);

        public static CacheResult<T> Miss()
            => new CacheResult<T>(false, default, null);
    }
}
