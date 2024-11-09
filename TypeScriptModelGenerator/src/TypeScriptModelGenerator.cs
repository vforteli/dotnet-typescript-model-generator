using System.Collections;
using System.Collections.Immutable;
using System.Reflection;

namespace TypescriptModelGenerator;

public static class TypeScriptModelGenerator
{
    private static readonly NullabilityInfoContext NullabilityInfoContext = new NullabilityInfoContext();


    /// <summary>
    /// Parse a method parameter and return its typescript type
    /// </summary>
    /// <returns>Either the primitive typescript type, or the name of complex type created</returns>
    public static TsType ParseParameterInfo(ParameterInfo parameterInfo, Dictionary<string, string> processedTypes) =>
        ParseTypeRecursively(parameterInfo.ParameterType, processedTypes,
            NullabilityInfoContext.Create(parameterInfo).WriteState is NullabilityState.Nullable);


    /// <summary>
    /// Parse a property and return its typescript type
    /// </summary>
    /// <returns>Either the primitive typescript type, or the name of complex type created</returns>
    private static TsType ParsePropertyInfo(PropertyInfo propertyInfo, Dictionary<string, string> processedTypes) =>
        ParseTypeRecursively(propertyInfo.PropertyType, processedTypes,
            NullabilityInfoContext.Create(propertyInfo).WriteState is NullabilityState.Nullable);


    /// <summary>
    /// Recursively convert a type into typescript types
    /// </summary>
    /// <returns>The primitive type or the name of the complex type</returns>
    public static TsType ParseTypeRecursively(Type type, Dictionary<string, string> processedTypes,
        bool nullableRefType)
    {
        var isNullableType = TryFromNullableType(type, out type) || nullableRefType;
        var isCollectionType = TryFromCollectionType(type, out type);

        if (TryToTypescriptPrimitiveType(type, out var primitiveType))
        {
            return new PrimitiveType(primitiveType, isNullableType, isCollectionType);
        }

        // If we already have seen this type before, just return the name
        if (processedTypes.ContainsKey(type.Name))
        {
            return new ComplexType(type.Name, isNullableType, isCollectionType);
        }

        // New type which hasnt been seen before, recursively add bits of it to the processed types
        if (type.IsEnum)
        {
            var tsTypeDefinition = TypescriptTemplates.Enum
                .Replace("{{values}}", string.Join(" | ", type.GetEnumNames().Select(v => $"\"{v}\"")))
                .Replace("{{typeName}}", type.Name);

            processedTypes.Add(type.Name, tsTypeDefinition);
        }
        else if (type.IsClass)
        {
            var types = type.GetProperties()
                .Select(o =>
                    new KeyValuePair<string, TsType>(o.Name.ToCamelCase(), ParsePropertyInfo(o, processedTypes)))
                .ToImmutableList();

            var tsProperties = types.Select(p => $"  {p.Key}: {p.Value};");

            var imports = types.Select(o => o.Value).OfType<ComplexType>()
                .DistinctBy(o => o.Name)
                .Select(o => TypescriptTemplates.Import.Replace("{{typeName}}", o.Name))
                .ToList();

            var tsTypeDefinition = (imports.Count != 0 ? TypescriptTemplates.TypeWithImports : TypescriptTemplates.Type)
                .Replace("{{imports}}", string.Join("\n", imports))
                .Replace("{{properties}}", string.Join("\n", tsProperties))
                .Replace("{{typeName}}", type.Name);

            processedTypes.Add(type.Name, tsTypeDefinition);
        }
        else
        {
            throw new ArgumentException($"What do we do with this?");
        }

        return new ComplexType(type.Name, isNullableType, isCollectionType);
    }


    /// <summary>
    /// Get the typescript primitive type from c# type if possible
    /// </summary>
    private static bool TryToTypescriptPrimitiveType(Type type, out string primitiveType)
    {
        primitiveType = type switch
        {
            // todo more?
            not null when type == typeof(bool) => "boolean",
            not null when type == typeof(byte) => "number",
            not null when type == typeof(sbyte) => "number",
            not null when type == typeof(short) => "number",
            not null when type == typeof(ushort) => "number",
            not null when type == typeof(int) => "number",
            not null when type == typeof(uint) => "number",
            not null when type == typeof(long) => "number",
            not null when type == typeof(ulong) => "number",
            not null when type == typeof(float) => "number",
            not null when type == typeof(double) => "number",
            not null when type == typeof(decimal) => "number",
            not null when type == typeof(string) => "string",
            not null when type == typeof(char) => "string",
            not null when type == typeof(DateTime) => "string",
            not null when type == typeof(DateTimeOffset) => "string",
            not null when type == typeof(Guid) => "string",
            _ => "unknown"
        };

        return primitiveType != "unknown";
    }


    /// <summary>
    /// Tries to figure out if this is something that should be converted to a collection
    /// </summary>
    /// <returns>True if collection type</returns>
    private static bool TryFromCollectionType(Type type, out Type underlyingType)
    {
        if (typeof(IList).IsAssignableFrom(type)
            || (typeof(IEnumerable).IsAssignableFrom(type) && type != typeof(string)))
        {
            underlyingType = type.GenericTypeArguments.First();
            return true;
        }

        underlyingType = type;
        return false;
    }


    /// <summary>
    /// Tries to figure out if this is some kind of nullable value type
    /// </summary>
    /// <returns>True if nullable</returns>
    private static bool TryFromNullableType(Type type, out Type underlyingType)
    {
        var maybeUnderlyingType = Nullable.GetUnderlyingType(type);

        if (maybeUnderlyingType != null)
        {
            underlyingType = maybeUnderlyingType;
            return true;
        }

        underlyingType = type;
        return false;
    }
}