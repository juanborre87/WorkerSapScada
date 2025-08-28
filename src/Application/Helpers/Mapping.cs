using Newtonsoft.Json;

namespace Application.Helpers;

public static class Mapping
{
    /// <summary>
    /// Mapea una entidad
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    public static T MapTo<T>(this object value)
    {
        var settings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore
        };
        return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(value, settings));
    }

    /// <summary>
    /// Mapea a una entidad ya existente
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <param name="destination"></param>
    public static void MapToExisting<T>(this object value, T destination)
    {
        var json = JsonConvert.SerializeObject(value, new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore
        });

        JsonConvert.PopulateObject(json, destination);
    }
}
