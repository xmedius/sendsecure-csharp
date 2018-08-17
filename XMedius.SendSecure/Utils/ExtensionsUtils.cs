using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

public static class ExtensionsUtils
{
    public static JToken RemoveEntry(this JToken token, string keytoRemove)
    {
        if (token.Type == JTokenType.Array)
        {
            foreach (JToken p in token)
            {
                p.RemoveEntry(keytoRemove);
            }
        }
        else
        {
            if (token[keytoRemove] != null)
            {
                token[keytoRemove].Parent.Remove();
            }
        }
        return token;
    }

    public static JToken RemoveEntries(this JToken token, List<string> keystoRemove)
    {
        if (token.Type == JTokenType.Array)
        {
            foreach (JToken p in token)
            {
                p.RemoveEntries(keystoRemove);
            }
        }
        else
        {
            keystoRemove.ForEach(property => { token.RemoveEntry(property); });
        }
        return token;
    }

    public static string ToJson(this JObject jObject)
    {
        return jObject.ToString(Formatting.None);
    }
}