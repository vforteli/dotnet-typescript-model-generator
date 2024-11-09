namespace TypescriptModelGenerator;

public static class TypescriptTemplates
{
    public const string Type =
        """
        export type {{typeName}} = {
        {{properties}}
        };
        """;
    
    public const string TypeWithImports =
        """
        {{imports}}

        export type {{typeName}} = {
        {{properties}}
        };
        """;

    public const string Enum = "export type {{typeName}} = {{values}};";
    
    public const string Import = """import type { {{typeName}} } from "./{{typeName}}";""";
}