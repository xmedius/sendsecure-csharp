using Newtonsoft.Json;
using System.Collections.Generic;

namespace XMedius.SendSecure.JsonObjects
{
    internal class GetFavoritesResponseSuccess
    {
        [JsonProperty(PropertyName = "favorites", Required = Required.AllowNull)]
        public List<Helpers.Favorite> Favorites { get; set; }
    }
}
