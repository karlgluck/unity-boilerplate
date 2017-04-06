using FullSerializer;

public static partial class Util
{
    private static readonly fsSerializer _serializer = new fsSerializer();

    public static object DeepCopy(object originalObject)
    {
        fsData data;
        _serializer.TrySerialize(originalObject.GetType(), originalObject, out data).AssertSuccessWithoutWarnings();
        object deserialized = null;
        _serializer.TryDeserialize(data, originalObject.GetType(), ref deserialized).AssertSuccessWithoutWarnings();
        return deserialized;
    }

    public static string ObjectToJson(object originalObject)
    {
        fsData data;
        _serializer.TrySerialize(originalObject.GetType(), originalObject, out data).AssertSuccessWithoutWarnings();
        return fsJsonPrinter.CompressedJson(data);
    }

    public static string ObjectToJsonPretty(object originalObject)
    {
        fsData data;
        _serializer.TrySerialize(originalObject.GetType(), originalObject, out data).AssertSuccessWithoutWarnings();
        return fsJsonPrinter.PrettyJson(data);
    }

    public static object JsonToObject(string jsonString, System.Type type)
    {
        fsData data = fsJsonParser.Parse(jsonString);
        object deserialized = null;
        _serializer.TryDeserialize(data, type, ref deserialized).AssertSuccessWithoutWarnings();
        return deserialized;
    }

    public static object JsonToObjectIgnoreErrors(string jsonString, System.Type type)
    {
        fsData data = fsJsonParser.Parse(jsonString);
        object deserialized = null;
        _serializer.TryDeserialize(data, type, ref deserialized);
        return deserialized;
    }
}