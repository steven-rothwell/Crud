# Preprocessing

Preprocessing is optional. These methods may be used to do any sort of preprocessing actions.

| Signature | Description |
| --------- | ----------- |
| `Task<MessageResult> PreprocessCreateAsync(Object model)` | Preprocessing when creating. |
| `Task<MessageResult> PreprocessReadAsync(Object model, Guid id)` | Preprocessing when reading with an Id. |
| `Task<MessageResult> PreprocessReadAsync(Object model, IDictionary<String, String>? queryParams)` | Preprocessing when reading with [query parameter filtering](#query-parameter-filtering). |
| `Task<MessageResult> PreprocessReadAsync(Object model, Query query)` | Preprocessing when reading with [body query filtering](#body-query-filtering). |
| `Task<MessageResult> PreprocessReadCountAsync(Object model, Query query)` | Preprocessing when reading the count with [body query filtering](#body-query-filtering). |
| `Task<MessageResult> PreprocessUpdateAsync(Object model, Guid id)` | Preprocessing when updating with an Id. |
| `Task<MessageResult> PreprocessPartialUpdateAsync(Object model, Guid id, IDictionary<String, JsonElement> propertyValues)` | Preprocessing when partially updating with an Id. |
| `Task<MessageResult> PreprocessPartialUpdateAsync(Object model, IDictionary<String, String>? queryParams, IDictionary<String, JsonElement> propertyValues)` | Preprocessing when partially updating with [query parameter filtering](#query-parameter-filtering). |
| `Task<MessageResult> PreprocessDeleteAsync(Object model, Guid id)` | Preprocessing when deleting with an Id. |
| `Task<MessageResult> PreprocessDeleteAsync(Object model, IDictionary<String, String>? queryParams)` | Preprocessing when deleting with [query parameter filtering](#query-parameter-filtering). |
| `Task<MessageResult> PreprocessDeleteAsync(Object model, Query query)` | Preprocessing when deleting with [body query filtering](#body-query-filtering). |