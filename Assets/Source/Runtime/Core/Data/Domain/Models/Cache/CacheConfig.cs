using System;

namespace Core.Data.Domain.Models.Cache
{
    public class CacheConfig
    {
        public TimeSpan DefaultTtl { get; set; } = TimeSpan.FromMinutes(30);
        public int SchemaVersion { get; set; } = 1;
        public int MaxAgeDays { get; set; } = 30;
    }
}
