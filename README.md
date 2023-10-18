# Summary

The goal of this application is to solve the problem of needing to write boilerplate code when creating a microservice. Code necessary to be written should be as simple as possible while still allowing flexibilty for complex use cases. In addition to this, unlike a project template, microservices created from this application should be able to pull in versioned enhancements and fixes as desired.

# Quick Start

1. Start off by [forking](https://docs.github.com/en/get-started/quickstart/fork-a-repo#forking-a-repository) this repository. Check out [versions](#versions) and [branching strategy](#branching-strategy) to decide what branch to start from.
2. Clone the new repo locally.
3. Create a new branch.
4. Open the [solution](/Crud.sln) in an IDE.
5. Add any [models](#models) if desired or use existing example models.
6. Ensure data store is running.
7. Add connection information to [appsettings](/Crud.Api/appsettings.Development.json).
8. Start the application.
9. Use Postman or similar application to start calling the [C](#create)[R](#read)[U](#update)[D](#delete) routes. (See example [Postman requests](/Postman/Crud.postman_collection.json).)

# Models

Models are POCOs located in the [Models](/Crud.Api/Models/) folder. These map directly to a collection/table in the data store.

Examples in the following documentation use the [User](/Crud.Api/Models/User.cs) and [Address](/Crud.Api/Models/Address.cs) models that come defaultly in this project. These are used soley for examples and may be removed. Do not remove IExternalEntity or [ExternalEntity](#externalentity).

## Attributes

### Table Data Annotation

The [Table](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations.schema.tableattribute?view=net-7.0) data annotation is optional. It may be used to specify the name of the collection/table that the model will be stored in. Otherwise, the name that will be used as the collection/table will default to the pluralized version of the class name.

The following example will specify that the name of the collection/table to store the model in should be "users".

```c#
[Table("users")]
public class User : ExternalEntity
```

### Validator Data Annotations

Standard and custom [data annotation](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations?view=net-7.0) validators may be added to properties in the model. These will automatically be used to validate the model without adding any additional code to the [Validator](/Crud.Api/Validators/Validator.cs).

### JSON Attributes

Standard System.Text.Json attributes, like [JsonPropertyNameAttribute](https://learn.microsoft.com/en-us/dotnet/api/system.text.json.serialization.jsonpropertynameattribute?view=net-7.0), can be added to the properties in the model to customize the JSON serialization and deserialization.

### PreventCrud Attribute

The [PreventCrud](/Crud.Api/Attributes/PreventCrudAttribute.cs) attribute is optional. This is used to prevent some or all [CRUD operations](/Crud.Api/Enums/CrudOperation.cs) on a model. See details [here](/docs/PREVENTCRUDATTRIBUTE.md).

### PreventQuery Attribute

The [PreventQuery](/Crud.Api/Attributes/PreventQueryAttribute.cs) attribute is optional. This is used to prevent some or all [Query operators](/Crud.Api/QueryModels/Operator.cs) on a model's property. See details [here](/docs/PREVENTQUERYATTRIBUTE.md).

## ExternalEntity

This class and IExternalEntity interface should not be removed from the application. Although not necessary, it is highly suggested to inherit from for [models](#models) that map directly to a collection/table. Example: [User](/Crud.Api/Models/User.cs) maps to the `Users` collection while [Address](/Crud.Api/Models/Address.cs) is stored within a document in that collection. The purpose of this class is to give each document/row a unique "random" identifier so that it may be safely referenced by external applications. Sequential identifiers are not as safe to use as they can be easily manipulated and without the proper checks, allow access to other data. They do make for better clustered indexes, so they should continue to be used within the data store.

# Routing

This application uses a RESTful naming convention for the routes. In the examples below, replace `{typeName}` with the pluralized name of the model the action will be on. For example, when acting on [User](/Crud.Api/Models/User.cs), replace `{typeName}` with "users".

# Create

## api/{typeName} - HttpPost

Add the JSON describing the model to be created in the request body.

# Read

## api/{typeName}/{id:guid} - HttpGet

Replace `{id:guid}` with the `Id` of the model to be retrieved.

## api/{typeName}{?prop1=val1...&propN=valN} - HttpGet

Replace `{?prop1=val1...&propN=valN}` with [query parameter filtering](#query-parameter-filtering). By default, at least one query parameter is required. To allow returning all, the [validator](/Crud.Api/Validators/Validator.cs) check for this will need to be removed. All documents/rows that match the filter will be retrieved.

## api/query/{typeName} - HttpPost

Add the JSON [query filtering](#body-query-filtering) to the body of the request. All documents/rows that match the filter will be retrieved.

## api/query/{typeName}/count - HttpPost

This returns the `number` of documents/rows that the [query filtering](#body-query-filtering) filtered. The forseen utility of this route is for pagination.

# Update

## api/{typeName}/{id:guid} - HttpPut

Replace `{id:guid}` with the `Id` of the model to be updated. The document/row that this `Id` matches will be replaced by the JSON object in the body of the request.

# Partial Update

## api/{typeName}/{id:guid} - HttpPatch

Replace `{id:guid}` with the `Id` of the model to be updated. The document/row that this `Id` matches will have only the fields/columns updated that are in the JSON object in the body of the request.

## api/{typeName}{?prop1=val1...&propN=valN} - HttpPatch

Replace `{?prop1=val1...&propN=valN}` with [query parameter filtering](#query-parameter-filtering). By default, at least one query parameter is required. To allow updating all, the [validator](/Crud.Api/Validators/Validator.cs) check for this will need to be removed. All documents/rows that match the filter will have only the fields/columns updated that are in the JSON object in the body of the request.

*Note: Unable to do [query filtering](#body-query-filtering) and partial update as both require JSON in the body of the request.*

# Delete

## api/{typeName}/{id:guid} - HttpDelete

Replace `{id:guid}` with the `Id` of the model to be deleted.

## api/{typeName}{?prop1=val1...&propN=valN} - HttpDelete

Replace `{?prop1=val1...&propN=valN}` with [query parameter filtering](#query-parameter-filtering). By default, at least one query parameter is required. To allow deleting all, the [validator](/Crud.Api/Validators/Validator.cs) check for this will need to be removed. All documents/rows that match the filter will be deleted.

## api/query/{typeName} - HttpDelete

Add the JSON [query filtering](#body-query-filtering) to the body of the request. All documents/rows that match the filter will be deleted.

# Query Parameter Filtering

Properties of the model may be added as a query parameter to filter the documents/rows acted on in the data store. The operator is limited to equality for filtering. The underscore delimiter `parent_child` may be used to refer to child properties.

The following example will filter on [Users](/Crud.Api/Models/User.cs) with `age` equal to 42 and `city` equal to "Tampa".

```
api/users?age=42&address_city=Tampa
```

# Body Query Filtering

Queries can be added to the body of a request in JSON format. This will then be used to filter the documents/rows acted on in the data store. The dot delimiter `parent.child` may be used to refer to child properties.

## Includes

Fields/columns that will be returned from the data store. If this and [Excludes](#excludes) are null, all fields/columns are returned.

The following example will only return the `age`, `name`, and `city` for all [Users](/Crud.Api/Models/User.cs) retrieved.

```json
{
  "includes": ["age", "name", "address.city"]
}
```

Example returned JSON:

```json
[
  {
    "name": "Bill Johnson",
    "address": {
      "city": "Pittsburgh"
    },
    "age": 25
  },
  {
    "name": "John Billson",
    "address": {
      "city": "Dallas"
    },
    "age": 31
  },
  {
    "name": "Johnny Bill",
    "address": {
      "city": "Tampa"
    },
    "age": 42
  }
]
```

## Excludes

Fields/columns that will not be returned from the data store. If this and [Includes](#includes) are null, all fields/columns are returned.

The following example will return all properties except `hairColor`, `age`, `formerAddresses`, and `state` for all [Users](/Crud.Api/Models/User.cs) retrieved.

```json
{
  "excludes": ["hairColor", "age", "formerAddresses", "address.state"]
}
```

Example returned JSON:

```json
[
  {
    "id": "6cd6f392-8271-49bb-8564-e584ddf48890",
    "name": "Bill Johnson",
    "address": {
      "street": "44 Maple Street",
      "city": "Pittsburgh"
    },
    "favoriteThings": ["Steelers", "Pirates", "Penguins"]
  },
  {
    "id": "c7b1ebaf-4ac1-4fe0-b066-1282e072585a",
    "name": "John Billson",
    "address": {
      "street": "101 Elm Street",
      "city": "Dallas"
    },
    "favoriteThings": ["Cowboys", "Stars", "Mavericks"]
  },
  {
    "id": "f4064c6b-e41a-4c34-a0b2-9e7a233b8310",
    "name": "Johnny Bill",
    "address": {
      "street": "75 Oak Street",
      "city": "Tampa"
    },
    "favoriteThings": ["Buccaneers", "Lightning"]
  }
]
```

## Where

### Condition

Constrains what documents/rows are filtered on in the data store.

| JSON Type | Name | Description |
| --------- | ---- | ----------- |
| `String?` | Field  | Name of the field/column side being evaluated. <br/>Should be null if [GroupedConditions](#grouped-conditions) is populated. |
| `String?` | ComparisonOperator | The operator used in the evaluation. <br/>Should be null if [GroupedConditions](#grouped-conditions) is populated. |
| `String?` | Value | Value that the [ComparisonOperator](#comparison-operators) will compare the `Field` against in the evaluation. <br/>Should be null if `Values` or [GroupedConditions](#grouped-conditions) is populated. |
| `Array[String]?` | Values | Values that the [ComparisonOperator](#comparison-operators) will compare the `Field` against in the evaluation. <br/>Should be null if `Value` or [GroupedConditions](#grouped-conditions) is populated. |
| `Array[GroupedCondition]?` | GroupedConditions | Groups of conditions used for complex logic to constrain what documents/rows are filtered on in the data store. For more details, see the [GroupedConditions](#grouped-conditions) section. |

The following example will filter on [Users](/Crud.Api/Models/User.cs) with an age less than 30.

```json
{
  "where": {
    "field": "age",
    "comparisonOperator": "<",
    "value": "30"
  }
}
```

### Grouped Conditions

Groups of conditions used for complex logic to constrain what documents/rows are filtered on in the data store.<br/>
*Note: Top level Grouped Conditions default to an AND [LogicalOperator](#logical-operators).*

| JSON Type | Name | Description |
| --------- | ---- | ----------- |
| `String?` | LogicalOperator | The operator applied between each condition in `Conditions`. |
| `Array[Condition]` | Conditions | All conditions have the same [LogicalOperator](#logical-operators) applied between each condition. |

The following example will filter on [Users](/Crud.Api/Models/User.cs) with `city` equal to "Dallas" or an `age` equal to 25.

```json
{
    "where": {
        "groupedConditions": [{
            "logicalOperator": "||",
            "conditions": [{
                "field": "address.city",
                "comparisonOperator": "==",
                "value": "Dallas"
            },
            {
                "field": "age",
                "comparisonOperator": "==",
                "value": "25"
            }]
        }]
    }
}
```

### Comparison Operators

The aliases are put in a [Condition](#condition)'s `ComparisonOperator`. Aliases are not case sensitive. Some operators have multiple aliases for the same operator. These may be mixed and matched to fit any style.

| Name | Aliases | Description |
| ---- | ------- | ----------- |
| Equality | `==`<br/>`Equals`<br/>`EQ` |  |
| Inequality | `!=`<br/>`NotEquals`<br/>`NE` |  |
| GreaterThan | `>`<br/>`GreaterThan`<br/>`GT` |  |
| GreaterThanOrEquals | `>=`<br/>`GreaterThanOrEquals`<br/>`GTE` |  |
| LessThan | `<`<br/>`LessThan`<br/>`LT` |  |
| LessThanOrEquals | `<=`<br/>`LessThanOrEquals`<br/>`LTE` |  |
| In | `IN` | If any value in `Field` matches any value in `Values`. |
| NotIn | `NotIn`<br/>`NIN` | If all values in `Field` do not match any value in `Values`. |
| All | `All` | If all values in `Values` match any value in `Field`. |
| Contains | `Contains` | For use with `Field` properties of type `String`. If value in `Field` contains the value in `Value`. There may be hits to performance when using this operator. All [queries](#body-query-filtering) may be prevented from using this operator by setting `PreventAllQueryContains` to `true` in the [appsettings.json](/Crud.Api/appsettings.json). Instead of preventing all, individual properties may be prevented from being being [queried](#body-query-filtering) on using this operator by decorating it with the [PreventQuery](/Crud.Api/Attributes/PreventQueryAttribute.cs)([Operator](/Crud.Api/QueryModels/Operator.cs).Contains). |
| StartsWith | `StartsWith` | For use with `Field` properties of type `String`. If value in `Field` starts with the value in `Value`. There may be hits to performance when using this operator. All [queries](#body-query-filtering) may be prevented from using this operator by setting `PreventAllQueryStartsWith` to `true` in the [appsettings.json](/Crud.Api/appsettings.json). Instead of preventing all, individual properties may be prevented from being being [queried](#body-query-filtering) on using this operator by decorating it with the [PreventQuery](/Crud.Api/Attributes/PreventQueryAttribute.cs)([Operator](/Crud.Api/QueryModels/Operator.cs).StartsWith). |
| EndsWith | `EndsWith` | For use with `Field` properties of type `String`. If value in `Field` ends with the value in `Value`. There may be hits to performance when using this operator. All [queries](#body-query-filtering) may be prevented from using this operator by setting `PreventAllQueryEndsWith` to `true` in the [appsettings.json](/Crud.Api/appsettings.json). Instead of preventing all, individual properties may be prevented from being being [queried](#body-query-filtering) on using this operator by decorating it with the [PreventQuery](/Crud.Api/Attributes/PreventQueryAttribute.cs)([Operator](/Crud.Api/QueryModels/Operator.cs).EndsWith). |

### Logical Operators

The aliases are put in a [GroupedCondition](#grouped-conditions)'s `LogicalOperator`. This `LogicalOperator` is applied between each condition in `Conditions`. Aliases are not case sensitive. Some operators have multiple aliases for the same operator. These may be mixed at matched to fit any style.

| Name | Aliases |
| ---- | ------- |
| And | `&&`<br/>`AND` |
| Or | `\|\|`<br/>`OR` |

## Order By

In what order the documents/rows will be returned from the data store.

| JSON Type | Name | Description |
| --------- | ---- | ----------- |
| `String?` | Field | Name of the field/column being sorted. |
| `Boolean?` | IsDescending | If the `Field` will be in descending order.<br/>*Default: false* |

The following example will return all [Users](/Crud.Api/Models/User.cs) ordered first by their `city` ascending, then `age` descending, then by `name` ascending.

```json
{
  "orderby": [
    {
      "field": "address.city"
    },
    {
      "field": "age",
      "isDescending": true
    },
    {
      "field": "name"
    }
  ]
}
```

## Limit

Sets the max number of documents/rows that will be returned from the data store.

The following example limits the max number of [Users](/Crud.Api/Models/User.cs) returned to 2.

```json
{
  "limit": 2
}
```

## Skip

Sets how many documents/rows to skip over.

The following example skips over the first 3 [Users](/Crud.Api/Models/User.cs) that would have been returned and returns the rest.

```json
{
  "skip": 3
}
```

## Complex Query Example

The following example will only return `name`, `age`, and `favoriteThings` of [Users](/Crud.Api/Models/User.cs) with a `name` that ends with "Johnson" or `favoriteThings` that are in ["Steelers", "Lightning"] and a `city` equal to "Pittsburgh" and `age` less than or equal to 42. The result will be ordered by `name` in ascending order, then `age` in descending order. The first two that would have returned are skipped over. The max number of [Users](/Crud.Api/Models/User.cs) returned is ten.

```json
{
  "includes": ["name", "age", "favoriteThings"],
  "where": {
    "groupedConditions": [
      {
        "logicalOperator": "&&",
        "conditions": [
          {
            "groupedConditions": [
              {
                "logicalOperator": "||",
                "conditions": [
                  {
                    "field": "name",
                    "comparisonOperator": "ENDSWITH",
                    "value": "Johnson"
                  },
                  {
                    "field": "favoriteThings",
                    "comparisonOperator": "IN",
                    "values": ["Steelers", "Lightning"]
                  }
                ]
              },
              {
                "logicalOperator": "&&",
                "conditions": [
                  {
                    "field": "address.city",
                    "comparisonOperator": "==",
                    "value": "Pittsburgh"
                  },
                  {
                    "field": "age",
                    "comparisonOperator": "<=",
                    "value": "42"
                  }
                ]
              }
            ]
          }
        ]
      }
    ]
  },
  "orderby": [
    {
      "field": "name"
    },
    {
      "field": "age",
      "isDescending": true
    }
  ],
  "limit": 10,
  "skip": 2
}
```

To help get a better understanding, the following is an equivalent C# logical statement of the [where](#where) condition in the JSON above.

```c#
if (
    (user.Name.EndsWith("Johnson", StringComparison.OrdinalIgnoreCase)
    || user.FavoriteThings.Any(favoriteThing => new List<string> { "Steelers", "Lightning" }.Any(x => x == favoriteThing)))
  &&
    (user.Address.City == "Pittsburgh"
    && user.Age <= 42)
   )
```

# Validation

These methods may be used to prevent a CRUD operation and optionally return a message stating why the operation was invalid.

| Signature | Description |
| --------- | ----------- |
| `Task<ValidationResult> ValidateCreateAsync(Object model)` | Validates the model when creating. By default, data annotations on the model are validated. |
| `Task<ValidationResult> ValidateReadAsync(Object model, IDictionary<String, String>? queryParams)` | Validates the model when reading with [query parameter filtering](#query-parameter-filtering). By default, all query parameters are ensured to be properties of the model. |
| `Task<ValidationResult> ValidateUpdateAsync(Object model, Guid id)` | Validates the model when replacement updating with an Id. By default, data annotations on the model are validated. |
| `Task<ValidationResult> ValidatePartialUpdateAsync(Object model, Guid id, IReadOnlyCollection<String>? propertiesToBeUpdated)` | Validates the model when partially updating with an Id. By default, all properties to be updated are ensured to be properties of the model and data annotations on the model are validated. |
| `Task<ValidationResult> ValidatePartialUpdateAsync(Object model, IDictionary<String, String>? queryParams, IReadOnlyCollection<String>? propertiesToBeUpdated)` | Validates the model when partially updating with [query parameter filtering](#query-parameter-filtering). By default, all query parameters are ensured to be properties of the model, all properties to be updated are ensured to be properties of the model, and data annotations on the model are validated. |
| `Task<ValidationResult> ValidateDeleteAsync(Object model, IDictionary<String, String>? queryParams)` | Validates the model when deleting with [query parameter filtering](#query-parameter-filtering). By default, all query parameters are ensured to be properties of the model. |
| `ValidationResult ValidateQuery(Object model, Query query)` | Validates the model when using [body query filtering](#body-query-filtering). |

Each signature above may be overloaded by replacing the `Object model` parameter with a specific model type. There are many examples using the [User](/Crud.Api/Models/User.cs) model to override the validating method in the [Validator](/Crud.Api/Validators/Validator.cs) class. These may be removed as they are solely there as examples. 

The following example overrides the `Task<ValidationResult> ValidateCreateAsync(Object model)` validating method and also calls the `Object model` version of the method to reuse the logic.

```c#
public async Task<ValidationResult> ValidateCreateAsync(User user)
{
    if (user is null)
        return new ValidationResult(false, $"{nameof(User)} cannot be null.");

    var objectValidationResult = await ValidateCreateAsync((object)user);
    if (!objectValidationResult.IsValid)
        return objectValidationResult;

    return new ValidationResult(true);
}
```

# Preprocessing

[Preprocessing](/Crud.Api/Services/PreprocessingService.cs) is optional. These methods may be used to do any sort of preprocessing actions. See details [here](/docs/PREPROCESSING.md).

# Postprocessing

[Postprocessing](/Crud.Api/Services/PostprocessingService.cs) is optional. These methods may be used to do any sort of postprocessing actions. See details [here](/docs/POSTPROCESSING.md).

# Metrics

CRUD operations on models has been simplified. But at what cost? The following metrics were obtained by running the exact same [Postman requests](/Postman/Crud.postman_collection.json) against this application versus running them against an application that does the same operations, but without the dynamic model capabilities, called [CrudMetrics](https://github.com/steven-rothwell/CrudMetrics).

The following is the average of each request which was run with 100 iterations and no indexes on the collections.

| Request | CrudMetrics (baseline) | Crud (dynamic models) |
| ------- | ---------------------: | --------------------: |
| CreateUser  | 4 ms | 4 ms |
| ReadUser_Id  | 3 ms | 3 ms |
| ReadUser_Name  | 3 ms | 3 ms |
| UpdateUser_Id  | 3 ms | 3 ms |
| PartialUpdateUser_Id  | 3 ms | 4 ms |
| PartialUpdateUser_Name  | 3 ms | 3 ms |
| DeleteUser_Id  | 3 ms | 3 ms |
| DeleteUser_Name  | 3 ms | 3 ms |

# Mutable Code

The following are files and folders that may be altered when using this code to create a microservice. All other files and folders should only be modified by [contributors](#contributing).

- [Models](/Crud.Api/Models/) - See details [here](#models).
- [Validator](/Crud.Api/Validators/Validator.cs) - See details [here](#validation).
- [PreprocessingService](/Crud.Api/Services/PreprocessingService.cs) - See details [here](#preprocessing).
- [PostprocessingService](/Crud.Api/Services/PostprocessingService.cs) - See details [here](#postprocessing).

# Versions

Pattern: #.#.# - *breaking-change*.*new-features*.*maintenance*
<br/>Incrementing version zeroes-out version numbers to the right.
<br/>Example: Current version is 1.0.3, new features are added, new version is 1.1.0.

# Updating Versions

If a new version is released and these updates would be useful in a forked application:

1. At minimum, read all [release notes](#release-notes) for each breaking change since the last fetch. (Example: Last forked from v1.0.3. Desired updated version is v4.2.6. At least read release notes of v2.0.0, v3.0.0, and v4.0.0 as code changes may be required.)
1. Fetch the desired v#.#.# branch from this repository into the forked repository.
2. Create a new branch.
3. Merge existing forked application code and v#.#.# branch.
3. Fix any merge conflicts.
4. Test.

# Branching Strategy

| Name | Description |
| ---- | ----------- |
| v#.#.# | Standard branches to create a forked application from. |
| v#.#.#-beta | Used when the next version requires burn in testing. This may be forked from to test out new features, but should not be used in a production environment. |

# Release Notes

| Number | Available Preservers | Framework | Notes |
| ------ | -------------------- | --------- | ----- |
| 1.0.0 | MongoDB | .NET 7 | See details [here](/docs/release-notes/RELEASE-1.0.0.md). |

# Contributing

Please see detailed documentation on how to contribute to this project [here](/docs/CONTRIBUTING.md).

^ [Back to top](#summary)
