using Newtonsoft.Json.Linq;

namespace Core.Application.Services.Save.Migrations
{
    public interface IJsonSnapshotMigration
    {
        int FromVersion { get; }
        int ToVersion { get; }
        JObject Apply(JObject snapshot);
    }
}
