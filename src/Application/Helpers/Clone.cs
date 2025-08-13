namespace Application.Helpers;

public class Clone
{
    public static T WithoutId<T>(T source) where T : new()
    {
        var clone = new T();
        var properties = typeof(T).GetProperties();

        foreach (var prop in properties)
        {
            if (prop.Name == "Id") continue; // ignorar Id
            if (prop.CanWrite)
            {
                var value = prop.GetValue(source);
                prop.SetValue(clone, value);
            }
        }
        return clone;
    }
}
