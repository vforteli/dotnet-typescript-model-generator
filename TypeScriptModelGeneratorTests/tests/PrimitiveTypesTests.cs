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

public class PrimitiveTypesTests
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
}