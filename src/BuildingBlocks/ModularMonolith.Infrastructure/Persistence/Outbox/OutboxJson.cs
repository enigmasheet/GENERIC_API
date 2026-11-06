using System.Text.Json;
using System.Text.Json.Serialization;

namespace ModularMonolith.Infrastructure.Persistence.Outbox;

public static class OutboxJson
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
}
