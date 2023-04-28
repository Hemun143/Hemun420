using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Claims;

namespace Integrations.HKTDC.Webforms.Middleware
{
    /// <summary>
    /// 
    /// </summary>
    public class JsonClaimConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Claim);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var claim = (Claim)value;

            var jo = new JObject
            {
                { "Type", claim.Type },
                { "Value", claim.Value },
                { "ValueType", claim.ValueType },
                { "Issuer", claim.Issuer },
                { "OriginalIssuer", claim.OriginalIssuer }
            };

            jo.WriteTo(writer);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
            string type = (string)jo["Type"];
            JToken token = jo["Value"];
            string value = token.Type == JTokenType.String ? (string)token : token.ToString(Formatting.None);
            string valueType = (string)jo["ValueType"];
            string issuer = (string)jo["Issuer"];
            string originalIssuer = (string)jo["OriginalIssuer"];
            return new Claim(type, value, valueType, issuer, originalIssuer);
        }
    }
}
