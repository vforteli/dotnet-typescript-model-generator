using TypescriptModelGenerator;

namespace TypescriptModelGeneratorTests.Tests;

public record NullableTypesModel
{
    public required bool? SomeNullableBoolean { get; init; }
    public required string? SomeNullableString { get; init; }
    public required int? SomeNullableInt { get; init; }
    public required DateTime? SomeNullableDateTime { get; init; }
    public required DateTimeOffset? SomeNullableDateTimeOffset { get; init; }
    public required Guid? SomeNullableGuid { get; init; }
}

public record TypesModel
{
    public required bool SomeBoolean { get; init; }
    public required string SomeString { get; init; }
    public required int SomeInt { get; init; }
    public required DateTime SomeDateTime { get; init; }
    public required DateTimeOffset SomeDateTimeOffset { get; init; }
    public required Guid SomeGuid { get; init; }
}

public record NullableBooleanModel
{
    public required bool? SomeNullableBoolean { get; init; }
}

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

public class TypeScriptModelGeneratorTests
{
    private static string TestConvertSingleType(Type model)
    {
        var types = new Dictionary<string, string>();
        TypeScriptModelGenerator.ParseTypeRecursively(model, types, false);
        return types.Single().Value;
    }


    [TestCase]
    public void NullableBoolean()
    {
        var actual = TestConvertSingleType(typeof(NullableBooleanModel));

        const string expected =
            """
            export type NullableBooleanModel = {
              someNullableBoolean: boolean | null;
            };
            """;

        Assert.That(actual, Is.EqualTo(expected));
    }


    [TestCase]
    public void Properties()
    {
        var actual = TestConvertSingleType(typeof(TypesModel));

        const string expected =
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

        Assert.That(actual, Is.EqualTo(expected));
    }


    [TestCase]
    public void NullableProperties()
    {
        var actual = TestConvertSingleType(typeof(NullableTypesModel));

        const string expected =
            """
            export type NullableTypesModel = {
              someNullableBoolean: boolean | null;
              someNullableString: string | null;
              someNullableInt: number | null;
              someNullableDateTime: string | null;
              someNullableDateTimeOffset: string | null;
              someNullableGuid: string | null;
            };
            """;

        Assert.That(actual, Is.EqualTo(expected));
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
}