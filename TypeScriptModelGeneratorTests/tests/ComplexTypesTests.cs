using TypescriptModelGenerator;

namespace TypescriptModelGeneratorTests.Tests;


public record NestedTypesModel
{
    public required bool SomeNullableBoolean { get; init; }
    public required TypesModel Types { get; init; }
    public required TypesModel? AlsoTypesNullable { get; init; }
}

public record TypeWithList
{
    public required List<TypesModel> TypesList { get; init; }
}

public enum SomeEnum
{
    Hurr,
    Durr,
}

public record TypeWithEnum
{
    public required SomeEnum SomeEnumValue { get; init; }
    public required SomeEnum? SomeNullableEnumValue { get; init; }
}

public record TypeWithDictionary
{
    public required Dictionary<string, string> SomeStringDictionary { get; init; }
    public required Dictionary<SomeEnum, string> SomeEnumDictionary { get; init; }
}

public class SomeClass
{
    public Task<TypesModel> DoSomething(TypeWithEnum? someEnum)
    {
        throw new NotImplementedException("nothing to do");
    }
}

public class ComplexTypesTests
{
    private static string TestConvertSingleType(Type model)
    {
        var types = new Dictionary<string, string>();
        TypeScriptModelGenerator.ParseTypeRecursively(model, types, false);
        return types.Single().Value;
    }
    
    [TestCase]
    public void NestedTypesModel()
    {
        var types = new Dictionary<string, string>();
        var typeName = TypeScriptModelGenerator.ParseTypeRecursively(typeof(NestedTypesModel), types, false);

        const string expectedType =
            """
            import type { TypesModel } from "./TypesModel";

            export type NestedTypesModel = {
              someNullableBoolean: boolean;
              types: TypesModel;
              alsoTypesNullable: TypesModel | null;
            };
            """;

        const string expectedNestedType =
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

        Assert.Multiple(() =>
        {
            Assert.That(types, Has.Count.EqualTo(2));
            Assert.That(typeName.Name, Is.EqualTo("NestedTypesModel"));
            Assert.That(types["NestedTypesModel"], Is.EqualTo(expectedType));
            Assert.That(types["TypesModel"], Is.EqualTo(expectedNestedType));
        });
    }

    [TestCase]
    public void ListTypesModel()
    {
        var types = new Dictionary<string, string>();
        var typeName = TypeScriptModelGenerator.ParseTypeRecursively(typeof(TypeWithList), types, false);

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

        const string expectedTypeWithListModel =
            """
            import type { TypesModel } from "./TypesModel";

            export type TypeWithList = {
              typesList: TypesModel[];
            };
            """;

        Assert.Multiple(() =>
        {
            Assert.That(types, Has.Count.EqualTo(2));
            Assert.That(typeName.Name, Is.EqualTo("TypeWithList"));
            Assert.That(types["TypesModel"], Is.EqualTo(expectedTypesModel));
            Assert.That(types["TypeWithList"], Is.EqualTo(expectedTypeWithListModel));
        });
    }

    [TestCase]
    public void EnumTypesModel()
    {
        var types = new Dictionary<string, string>();
        var typeName = TypeScriptModelGenerator.ParseTypeRecursively(typeof(TypeWithEnum), types, false);

        const string expectedEnumType =
            """
            export type SomeEnum = "Hurr" | "Durr";
            """;

        const string expectedTypeWithEnum =
            """
            import type { SomeEnum } from "./SomeEnum";

            export type TypeWithEnum = {
              someEnumValue: SomeEnum;
              someNullableEnumValue: SomeEnum | null;
            };
            """;

        Assert.Multiple(() =>
        {
            Assert.That(types, Has.Count.EqualTo(2));
            Assert.That(typeName.Name, Is.EqualTo("TypeWithEnum"));
            Assert.That(types["SomeEnum"], Is.EqualTo(expectedEnumType));
            Assert.That(types["TypeWithEnum"], Is.EqualTo(expectedTypeWithEnum));
        });
    }

    [TestCase]
    public void DictionaryTypesModel()
    {
        var types = new Dictionary<string, string>();
        var typeName = TypeScriptModelGenerator.ParseTypeRecursively(typeof(TypeWithDictionary), types, false);

        const string expectedEnumType =
            """
            export type SomeEnum = "Hurr" | "Durr";
            """;

        const string expectedTypeWithDictionary =
            """
            import type { SomeEnum } from "./SomeEnum";

            export type TypeWithDictionary = {
              someStringDictionary: Record<string, string>;
              someEnumDictionary: Record<SomeEnum, string>;
            };
            """;

        Assert.Multiple(() =>
        {
            Assert.That(types, Has.Count.EqualTo(2));
            Assert.That(typeName.Name, Is.EqualTo("TypeWithDictionary"));
            Assert.That(types["SomeEnum"], Is.EqualTo(expectedEnumType));
            Assert.That(types["TypeWithDictionary"], Is.EqualTo(expectedTypeWithDictionary));
        });
    }


    [TestCase]
    public void ParameterInfo()
    {
        var types = new Dictionary<string, string>();

        var methodInfo = typeof(SomeClass).GetMethods().First();

        var tsType = TypeScriptModelGenerator.ParseParameterInfo(methodInfo.GetParameters().First(), types);

        Assert.Multiple(() =>
        {
            Assert.That(types, Has.Count.EqualTo(2));
            Assert.That(tsType.Name,
                Is.EqualTo("TypeWithEnum")); // Nullable reference type determined to be nullable here from parameterinfo
            Assert.That(tsType.IsNullable, Is.True);
        });
    }
}