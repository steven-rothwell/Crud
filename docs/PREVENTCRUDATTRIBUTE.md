# PreventCrud Attribute

The [PreventCrud](/Crud.Api/Attributes/PreventCrudAttribute.cs) attribute is optional. This is used to prevent some or all [CRUD operations](/Crud.Api/Enums/CrudOperation.cs) on a model.

The following example will prevent all CRUD operations on the [Address](/Crud.Api/Models/Address.cs) model. This is useful for models that will always be stored within another model.

```c#
[PreventCrud]
public class Address
```

The following example will prevent [Updating](#update), all forms of [Partial Updating](#partial-update), and all forms of [Deleting](#delete) the `CreationMetadata`. This is useful when limiting CRUD operations is required. In this example `CreationMetadata` is required to be a readonly model.

```c#
[PreventCrud(CrudOperation.Update, CrudOperation.PartialUpdate, CrudOperation.Delete)]
public class CreationMetadata
```