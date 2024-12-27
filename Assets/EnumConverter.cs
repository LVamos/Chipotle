using System;

using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

public class EnumConverter<T> : IYamlTypeConverter where T : struct, Enum
{
    public bool Accepts(Type type)
    {
        return type == typeof(T);
    }

    public object ReadYaml(IParser parser, Type type, ObjectDeserializer deserializer)
    {
        var scalar = parser.Consume<Scalar>();
        if (int.TryParse(scalar.Value, out int intValue))
        {
            return (T)(object)intValue;
        }
        if (Enum.TryParse(scalar.Value, true, out T enumValue))
        {
            return enumValue;
        }

        throw new InvalidOperationException($"Invalid value for enum {typeof(T)}: {scalar.Value}");
    }

    public void WriteYaml(IEmitter emitter, object value, Type type, ObjectSerializer serializer)
    {
        // Zapisujeme číselnou reprezentaci hodnoty enumu
        emitter.Emit(new Scalar(Convert.ToInt32(value).ToString()));
    }
}
