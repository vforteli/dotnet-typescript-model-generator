namespace TypescriptModelGenerator;

public static class TypescriptTemplates
{
    public const string TypeTemplate =
        """
        export type {{typeName}} = {
        {{properties}}
        };
        """;

    public const string EnumTemplate = "export type {{typeName}} = {{values}};";
}