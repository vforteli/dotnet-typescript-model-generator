using TypescriptModelGenerator;

namespace TypescriptModelGeneratorTests.Tests;

public record SomeGenericModel<TFoo, TBar, THurr>
{
    public required List<TFoo> SomeGenericFooList { get; init; }
    public required List<TBar> SomeGenericBarList { get; init; }
    public required THurr SomeGenericHurr { get; init; }
    public required string SomeString { get; init; }
}

public record SomeGenericContainingModel
{
    public required SomeGenericModel<int, string, string> SomeGenericThing { get; init; }
}

public class GenericTypesTests
{
    [TestCase]
    public void GenericType()
    {
        var types = new Dictionary<string, string>();
        var typeName = TypeScriptModelGenerator.ParseTypeRecursively(typeof(SomeGenericContainingModel), types, false);

        const string expectedType =
            """
            import type { SomeGenericModel } from "./SomeGenericModel";

            export type SomeGenericContainingModel = {
              someGenericThing: SomeGenericModel<number, string, string>;
            };
            """;

        const string expectedGenericType =
            """
            export type SomeGenericModel<TFoo, TBar, THurr> = {
              someGenericFooList: TFoo[];
              someGenericBarList: TBar[];
              someGenericHurr: THurr;
              someString: string;
            };
            """;

        var derp = typeof(SomeGenericModel<int,string, int>);
        Assert.Multiple(() =>
        {
            Assert.That(types, Has.Count.EqualTo(2));
            Assert.That(typeName.Name, Is.EqualTo("SomeGenericContainingModel"));
            
            Assert.That(types["SomeGenericContainingModel"], Is.EqualTo(expectedType));
            Assert.That(types["SomeGenericModel"], Is.EqualTo(expectedGenericType));
        });
    }
}