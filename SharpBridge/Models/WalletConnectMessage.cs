#nullable enable
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

#pragma warning disable CS8618
#pragma warning disable CS8601
#pragma warning disable CS8603

namespace SharpBridge.Models;

public partial class WalletConnectMessage
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("topic")]
    public virtual Guid? Topic { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("type")]
    public virtual string Type { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("payload")]
    public virtual string Payload { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("silent")]
    public virtual bool? Silent { get; set; }
}

public partial class WalletConnectMessage
{
    public static WalletConnectMessage FromJson(string json)
    {
        return JsonSerializer.Deserialize<WalletConnectMessage>(json, Converter.Settings);
    }
}

public static class Serialize
{
    public static string ToJson(this WalletConnectMessage self)
    {
        return JsonSerializer.Serialize(self, Converter.Settings);
    }
}

internal static class Converter
{
    public static readonly JsonSerializerOptions Settings = new(JsonSerializerDefaults.General)
    {
        Converters =
        {
            new DateOnlyConverter(),
            new TimeOnlyConverter(),
            IsoDateTimeOffsetConverter.Singleton
        }
    };
}

public class DateOnlyConverter : JsonConverter<DateOnly>
{
    private readonly string serializationFormat;

    public DateOnlyConverter() : this(null)
    {
    }

    private DateOnlyConverter(string? serializationFormat)
    {
        this.serializationFormat = serializationFormat ?? "yyyy-MM-dd";
    }

    public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        return DateOnly.Parse(value!);
    }

    public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(serializationFormat));
    }
}

public class TimeOnlyConverter : JsonConverter<TimeOnly>
{
    private readonly string serializationFormat;

    public TimeOnlyConverter() : this(null)
    {
    }

    private TimeOnlyConverter(string? serializationFormat)
    {
        this.serializationFormat = serializationFormat ?? "HH:mm:ss.fff";
    }

    public override TimeOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        return TimeOnly.Parse(value!);
    }

    public override void Write(Utf8JsonWriter writer, TimeOnly value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(serializationFormat));
    }
}

internal class IsoDateTimeOffsetConverter : JsonConverter<DateTimeOffset>
{
    private const string DefaultDateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFFK";


    public static readonly IsoDateTimeOffsetConverter Singleton = new();
    private CultureInfo? _culture;
    private string? _dateTimeFormat;

    private DateTimeStyles DateTimeStyles { get; set; } = DateTimeStyles.RoundtripKind;

    public string? DateTimeFormat
    {
        get => _dateTimeFormat ?? string.Empty;
        set => _dateTimeFormat = string.IsNullOrEmpty(value) ? null : value;
    }

    private CultureInfo Culture
    {
        get => _culture ?? CultureInfo.CurrentCulture;
        set => _culture = value;
    }

    public override bool CanConvert(Type t)
    {
        return t == typeof(DateTimeOffset);
    }

    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
    {
        if ((DateTimeStyles & DateTimeStyles.AdjustToUniversal) == DateTimeStyles.AdjustToUniversal
            || (DateTimeStyles & DateTimeStyles.AssumeUniversal) == DateTimeStyles.AssumeUniversal)
            value = value.ToUniversalTime();

        var text = value.ToString(_dateTimeFormat ?? DefaultDateTimeFormat, Culture);

        writer.WriteStringValue(text);
    }

    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var dateText = reader.GetString();

        if (string.IsNullOrEmpty(dateText)) return default;
        return !string.IsNullOrEmpty(_dateTimeFormat)
            ? DateTimeOffset.ParseExact(dateText, _dateTimeFormat, Culture, DateTimeStyles)
            : DateTimeOffset.Parse(dateText, Culture, DateTimeStyles);
    }
}
#pragma warning restore CS8618
#pragma warning restore CS8601
#pragma warning restore CS8603