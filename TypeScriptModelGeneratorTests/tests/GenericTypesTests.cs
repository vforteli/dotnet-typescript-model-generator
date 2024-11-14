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
    public required SomeGenericModel<int, string, TypesModel> SomeGenericThing { get; init; }
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
            import type { TypesModel } from "./TypesModel";

            export type SomeGenericContainingModel = {
              someGenericThing: SomeGenericModel<number, string, TypesModel>;
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
        
        const string expectedTypesModel =
            """
            export type TypesModel = {
              someBoolean: boolean;
              someString: string;
              someInt: number;
              someDateTime: string;
              someDateTimeOffset: string;
              someGuid: string;
            };
            """;

        var derp = typeof(SomeGenericModel<int,string, int>);
        Assert.Multiple(() =>
        {
            Assert.That(types, Has.Count.EqualTo(3));
            Assert.That(typeName.Name, Is.EqualTo("SomeGenericContainingModel"));
            
            Assert.That(types["SomeGenericContainingModel"], Is.EqualTo(expectedType));
            Assert.That(types["SomeGenericModel"], Is.EqualTo(expectedGenericType));
            Assert.That(types["TypesModel"], Is.EqualTo(expectedTypesModel));
        });
    }
}