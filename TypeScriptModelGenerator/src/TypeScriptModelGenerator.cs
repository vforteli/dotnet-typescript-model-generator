using System.Collections;
using System.Collections.Immutable;
using System.Reflection;

namespace TypescriptModelGenerator;

public static class TypeScriptModelGenerator
{
    private static readonly NullabilityInfoContext NullabilityInfoContext = new();


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
    public static TsType ParsePropertyInfo(PropertyInfo propertyInfo, Dictionary<string, string> processedTypes) =>
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

        if (TryIntoRecordType(type, out var keyType, out var valueType))
        {
            var keyTsType = ParseTypeRecursively(keyType, processedTypes, false);
            var valueTsType = ParseTypeRecursively(valueType, processedTypes, false);

            return new PrimitiveType("Record", isNullableType, false, [keyTsType, valueTsType]);
        }

        var isCollectionType = TryFromCollectionType(type, out type);

        if (TryIntoTypescriptPrimitiveType(type, out var primitiveType))
        {
            return new PrimitiveType(primitiveType, isNullableType, isCollectionType, []);
        }

        if (type.IsGenericParameter)
        {
            // todo this is all a bit sketchy...
            return new PrimitiveType(type.Name, false, isCollectionType, []);
        }

        // If we already have seen this type before, just return the name
        if (processedTypes.ContainsKey(type.Name))
        {
            return new ComplexType(type.Name, isNullableType, isCollectionType, []);
        }

        if (type.IsEnum)
        {
            var tsTypeDefinition = TypescriptTemplates.Enum
                .Replace("{{values}}", string.Join(" | ", type.GetEnumNames().Select(v => $"\"{v}\"")))
                .Replace("{{typeName}}", type.Name);

            processedTypes.Add(type.Name, tsTypeDefinition);
            return new ComplexType(type.Name, isNullableType, isCollectionType, []);
        }

        if (type.IsClass)
        {
            var typeName = type.Name.Split('`')[0]; // generic types include the arity as SomeType`n, get rid of it...
            var genericTypeArguments = type.GenericTypeArguments
                .Select(t =>
                    new KeyValuePair<Type, TsType>(t,
                        ParseTypeRecursively(t, processedTypes, false)))
                .ToImmutableList();

            var genericTypeName = typeName;
            if (type.IsGenericType)
            {
                genericTypeName = GetGenericTypeName(type);
            }

            var propertiesTypes = (type.IsGenericType
                    ? type.Assembly.DefinedTypes.Single(o => o.Name == type.Name).GetProperties()
                    : type.GetProperties())
                .Select(o =>
                    new KeyValuePair<string, TsType>(o.Name.ToCamelCase(), ParsePropertyInfo(o, processedTypes)))
                .ToImmutableList();

            var genericTypeImports = propertiesTypes
                .Select(o => o.Value)
                .SelectMany(o => o.GenericTypeArguments.OfType<ComplexType>());

            var imports = propertiesTypes.Select(o => o.Value).OfType<ComplexType>()
                .Concat(genericTypeImports)
                .DistinctBy(o => o.Name)
                .Select(o => TypescriptTemplates.Import.Replace("{{typeName}}", o.Name))
                .ToImmutableList();

            var tsTypeDefinition = (imports.Count != 0 ? TypescriptTemplates.TypeWithImports : TypescriptTemplates.Type)
                .Replace("{{imports}}", string.Join("\n", imports))
                .Replace("{{properties}}", string.Join("\n", propertiesTypes.Select(p => $"  {p.Key}: {p.Value};")))
                .Replace("{{typeName}}", genericTypeName);

            processedTypes.Add(typeName, tsTypeDefinition);

            return new ComplexType(
                typeName,
                isNullableType,
                isCollectionType,
                genericTypeArguments.Select(o => o.Value).ToImmutableList());
        }

        throw new ArgumentException($"What do we do with this?");
    }


    /// <summary>
    /// Get the name of the type with the generic parameter names eg SomeType&lt;TKey, TValue, TFoo&gt;
    /// </summary>
    private static string GetGenericTypeName(Type type)
    {
        var typeName = type.Name.Split('`')[0];
        var parameters = type.Assembly.DefinedTypes.Single(o => o.Name == type.Name).GenericTypeParameters
            .Select(o => new PrimitiveType(o.Name, false, false, [])).ToImmutableList();

        return parameters.IsEmpty
            ? typeName
            : $"{typeName}<{string.Join(", ", parameters)}>";
    }


    /// <summary>
    /// Get the typescript primitive type from c# type if possible, eg:
    /// string -> string
    /// int, float etc -> number
    /// </summary>
    private static bool TryIntoTypescriptPrimitiveType(Type type, out string primitiveType)
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
    /// Tries to figure out if this is something that should be converted to a Record
    /// </summary>
    /// <returns>True if Record type</returns>
    private static bool TryIntoRecordType(Type type, out Type keyType, out Type valueType)
    {
        if (typeof(IDictionary).IsAssignableFrom(type) && type.GenericTypeArguments.Length == 2)
        {
            keyType = type.GenericTypeArguments[0];
            valueType = type.GenericTypeArguments[1];
            return true;
        }

        keyType = typeof(void);
        valueType = typeof(void);
        return false;
    }


    /// <summary>
    /// Tries to figure out if this is something that should be converted to a collection
    /// </summary>
    /// <returns>True if array type</returns>
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