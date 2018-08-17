using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Linq;
using System.Runtime.Serialization;

namespace XMedius.SendSecure.Helpers
{
    public class SecurityEnums
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum TimeUnit
        {
            [EnumMember(Value = "hours")] Hours,
            [EnumMember(Value = "days")] Days,
            [EnumMember(Value = "weeks")] Weeks,
            [EnumMember(Value = "months")] Months,
            [EnumMember(Value = "years")] Years
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum RetentionPeriod
        {
            [EnumMember(Value = "discard_at_expiration")] DiscardAtExpiration,
            [EnumMember(Value = "retain_at_expiration")] RetainAtExpiration,
            [EnumMember(Value = "do_not_discard")] DoNotDiscard
        }

        public static TimeUnit StringToTimeUnit(string str)
        {
            foreach (var value in Enum.GetValues(typeof(TimeUnit)))
            {
                if (Enum.GetName(typeof(TimeUnit), value).Equals(str, StringComparison.CurrentCultureIgnoreCase))
                {
                    return (TimeUnit)value;
                }
            }

            throw new Exceptions.SendSecureException();
        }

        public static String TimeUnitToString(TimeUnit timeUnit)
        {
            return Enum.GetName(typeof(TimeUnit), timeUnit).ToLower();
        }

        public static RetentionPeriod StringToRetentionPeriod(string str)
        {
            System.Globalization.TextInfo info = new System.Globalization.CultureInfo("en-US", false).TextInfo;

            foreach (var value in Enum.GetValues(typeof(RetentionPeriod)))
            {
                if (Enum.GetName(typeof(RetentionPeriod), value).Equals(info.ToTitleCase(str).Replace("_", "")))
                {
                    return (RetentionPeriod)value;
                }
            }

            throw new Exceptions.SendSecureException();
        }

        public static String RetentionPeriodToString(RetentionPeriod retentionPeriod)
        {
            string enumName = Enum.GetName(typeof(RetentionPeriod), retentionPeriod);

            return string.Concat(enumName.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x.ToString() : x.ToString())).ToLower();
        }
    }
}
