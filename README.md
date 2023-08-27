# Summary

The goal of this application is to solve the problem of needing to write boilerplate code when creating a microservice. Code necessary to be written should be as simple as possible while still allowing flexibilty for complex use cases. In addition to this, unlike a project template, microservices created from this application should be able to pull in versioned enhancements and fixes as desired.

# Quick Start

Examples in the following documentation use the [User](/Crud.Api/Models/User.cs) and [Address](/Crud.Api/Models/Address.cs) models that come defaultly in this project. These are used soley for examples and may be removed.

This application uses a RESTful naming convention for the routes. In the examples below, replace `{typeName}` with the pluralized name of the model the action will be on. For example, when acting on [User](/Crud.Api/Models/User.cs), replace `{typeName}` with "users".

TODO: write how to setup and quickly get entries in db.

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

## Replacement Update

### api/{typeName}/{id:guid} - HttpPut

Replace `{id:guid}` with the `Id` of the model to be updated. The document/row that this `Id` matches will be replaced by the JSON object in the body of the request.

## Partial Update

### api/{typeName}/{id:guid} - HttpPatch

Replace `{id:guid}` with the `Id` of the model to be updated. The document/row that this `Id` matches will have only the fields/columns updated that are in the JSON object in the body of the request.

### api/{typeName}{?prop1=val1...&propN=valN} - HttpPatch

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
| `Array[GroupedCondition]?` | GroupedConditions | Groups of conditions used for complex logic to constrain what documents/rows are filtered on in the data store. For more details, check out the [GroupedConditions](#grouped-conditions) section. |

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

The aliases are put in a [Condition](#condition)'s `ComparisonOperator`. Aliases are not case sensitive. Some operators have multiple aliases for the same operator. These may be mixed at matched to fit any style.

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
| Contains | `Contains` | For use with `Field` properties of type `String`. If value in `Field` contains the value in `Value`. There may be hits to performance when using this operator. All [queries](#body-query-filtering) may be prevented from using this operator by setting `PreventAllQueryContains` to `true` in the [appsettings.json](/Crud.Api/appsettings.json). Instead of preventing all, individual properties may be prevented from being being [queried](#body-query-filtering) on using this operator by decorating it with the [PreventQueryContainsAttribute](/Crud.Api/Validators/Attributes/PreventQueryContainsAttribute.cs). |
| StartsWith | `StartsWith` | For use with `Field` properties of type `String`. If value in `Field` starts with the value in `Value`. There may be hits to performance when using this operator. All [queries](#body-query-filtering) may be prevented from using this operator by setting `PreventAllQueryStartsWith` to `true` in the [appsettings.json](/Crud.Api/appsettings.json). Instead of preventing all, individual properties may be prevented from being being [queried](#body-query-filtering) on using this operator by decorating it with the [PreventQueryStartsWithAttribute](/Crud.Api/Validators/Attributes/PreventQueryStartsWithAttribute.cs). |
| EndsWith | `EndsWith` | For use with `Field` properties of type `String`. If value in `Field` ends with the value in `Value`. There may be hits to performance when using this operator. All [queries](#body-query-filtering) may be prevented from using this operator by setting `PreventAllQueryEndsWith` to `true` in the [appsettings.json](/Crud.Api/appsettings.json). Instead of preventing all, individual properties may be prevented from being being [queried](#body-query-filtering) on using this operator by decorating it with the [PreventQueryEndsWithAttribute](/Crud.Api/Validators/Attributes/PreventQueryEndsWithAttribute.cs). |

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

# Versions

| Number | Available Preservers | Framework |
| ------ | -------------------- | --------- |
| 1.0.0  | MongoDB              | .NET 7    |

# Release Notes

| Number | Notes                                                                                        |
| ------ | -------------------------------------------------------------------------------------------- |
| 1.0.0  | Initial release with basic CRUD operations, filtering by query parameters, and body queries. |

# Contributing

Please see detailed documentation on how to contribute to this project [here](/docs/CONTRIBUTING.md).

^ [Back to top](#summary)
