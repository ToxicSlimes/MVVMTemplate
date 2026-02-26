namespace Core.Data.Domain.Models.Cache
{
    public class CacheEntry
    {
        public string ContentHash { get; set; }
        public string EncryptedData { get; set; }
        public string IV { get; set; }
        public long CreatedAtTicks { get; set; }
        public long ExpiresAtTicks { get; set; }
        public int SchemaVersion { get; set; }
    }
}
