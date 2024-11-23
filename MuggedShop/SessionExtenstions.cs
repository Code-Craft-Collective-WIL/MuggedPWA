using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace MuggedShop
{
    // Provides extension methods for managing complex objects in session state (Troeslen & Japikse, 2021)
    public static class SessionExtensions
    {
        // Retrieves an object from the session state, deserializing it from JSON format (Troeslen & Japikse, 2021)
        public static T GetObject<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default : JsonSerializer.Deserialize<T>(value);
        }

        // Stores an object in the session state, serializing it to JSON format (Troeslen & Japikse, 2021)
        public static void SetObject<T>(this ISession session, string key, T value)
        {
            session.SetString(key, JsonSerializer.Serialize(value));
        }
    }
}
