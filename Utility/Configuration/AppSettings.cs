using System.Collections.ObjectModel;

namespace Utility.Configuration;

public class AppSettings
{
    public IReadOnlyDictionary<string, object> ToDictionary()
    {
        var data = GetType().GetProperties()
            .Select(info => (info.Name, Value: info.GetValue(this, null) ?? ""));

        var result = data.ToDictionary(item => item.Name, item => (item.Value is Enum ? item.Value.GetHashCode() : item.Value), StringComparer.OrdinalIgnoreCase);

        return new ReadOnlyDictionary<string, object>(result);
    }
}