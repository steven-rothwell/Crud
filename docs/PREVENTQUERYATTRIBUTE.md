# PreventQuery Attribute

The [PreventQuery](/Crud.Api/Attributes/PreventQueryAttribute.cs) attribute is optional. This is used to prevent some or all [Query operators](/Crud.Api/QueryModels/Operator.cs) on a model's property.

The following example will prevent all [Query operators](/Crud.Api/QueryModels/Operator.cs) on the [User](/Crud.Api/Models/User.cs) `FormerAddress` property.

```c#
public class User
{
  [PreventQuery]
  public ICollection<Address>? FormerAddresses { get; set; }
}
```

The following example will prevent [Operator](/Crud.Api/QueryModels/Operator.cs).Contains on the [User](/Crud.Api/Models/User.cs) `Name` property. This is useful for operators that may cause performance hits or properties that are not indexed and may also lead to poor performance when fetching the data.

```c#
public class User
{
  [PreventQuery(Operator.Contains)]
  public String? Name { get; set; }
}
```