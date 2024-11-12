# TypeScriptModelGenerator

Generate TypeScript types from dotnet models.
This thing probably wont work with all possible types, but for now covers most use cases.

## Usage

```csharp
// From a type
var types = new Dictionary<string, string>();
var typeName = TypeScriptModelGenerator.ParseTypeRecursively(typeof(SomeType), types, false);

// From method parameters using ParameterInfo
var types = new Dictionary<string, string>();
var type = TypeScriptModelGenerator.ParseParameterInfo(someParameterInfo, types);

// types will contain a dictionary of all the recognized TypeScript type names and their respective definition
```

See tests for more examples of output

Note that it is not possible to determine if a generic parameter is nullable through reflection.
Therefore cases such as Task<SomeReferenceType?> cannot be detected.

Nullability of reference types can be determined when used in method parameters or properties.

## todo
- consider namespaces when processing types
- more types