using System.Collections.Immutable;

namespace TypescriptModelGenerator;

public abstract record TsType(string Name, bool IsNullable, bool IsArray, ImmutableList<TsType> GenericTypeArguments)
{
    public sealed override string ToString() =>
        Name
        + (GenericTypeArguments.Count != 0 ? $"<{string.Join(", ", GenericTypeArguments.Select(o => o.Name))}>" : "")
        + (IsArray ? "[]" : "")
        + (IsNullable ? " | null" : "");
}

public record PrimitiveType(string Name, bool IsNullable, bool IsArray, ImmutableList<TsType> GenericTypeArguments)
    : TsType(Name, IsNullable, IsArray, GenericTypeArguments);

public record ComplexType(string Name, bool IsNullable, bool IsArray, ImmutableList<TsType> GenericTypeArguments)
    : TsType(Name, IsNullable, IsArray, GenericTypeArguments);