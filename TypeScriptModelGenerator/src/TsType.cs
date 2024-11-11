namespace TypescriptModelGenerator;

public abstract record TsType(string Name, bool IsNullable, bool IsArray)
{
    public sealed override string ToString() => Name + (IsArray ? "[]" : "") + (IsNullable ? " | null" : "");
}

public record PrimitiveType(string Name, bool IsNullable, bool IsArray) : TsType(Name, IsNullable, IsArray);

public record ComplexType(string Name, bool IsNullable, bool IsArray) : TsType(Name, IsNullable, IsArray);

public record GenericPrimitiveType(
    string Name,
    bool IsNullable,
    bool IsArray,
    List<TsType> GenericTypes
) : PrimitiveType(Name, IsNullable, IsArray);