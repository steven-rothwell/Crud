# Postprocessing

Postprocessing is optional. These methods may be used to do any sort of postprocessing actions.

| Signature | Description |
| --------- | ----------- |
| `Task<MessageResult> PostprocessCreateAsync(Object createdModel)` | Postprocessing when creating. |
| `Task<MessageResult> PostprocessReadAsync(Object model, Guid id)` | Postprocessing when reading with an Id. |
| `Task<MessageResult> PostprocessReadAsync(IEnumerable<Object> models, IDictionary<String, String>? queryParams)` | Postprocessing when reading with [query parameter filtering](#query-parameter-filtering). |
| `Task<MessageResult> PostprocessReadAsync(IEnumerable<Object> models, Query query)` | Postprocessing when reading with [body query filtering](#body-query-filtering). |
| `Task<MessageResult> PostprocessReadCountAsync(Object model, Query query, Int64 count)` | Postprocessing when reading the count with [body query filtering](#body-query-filtering). |
| `Task<MessageResult> PostprocessUpdateAsync(Object updatedModel, Guid id)` | Postprocessing when updating with an Id. |
| `Task<MessageResult> PostprocessPartialUpdateAsync(Object updatedModel, Guid id, IDictionary<String, JsonElement> propertyValues)` | Postprocessing when partially updating with an Id. |
| `Task<MessageResult> PostprocessPartialUpdateAsync(Object model, IDictionary<String, String>? queryParams, IDictionary<String, JsonElement> propertyValues, Int64 updatedCount)` | Postprocessing when partially updating with [query parameter filtering](#query-parameter-filtering). |
| `Task<MessageResult> PostprocessDeleteAsync(Object model, Guid id, Int64 deletedCount)` | Postprocessing when deleting with an Id. |
| `Task<MessageResult> PostprocessDeleteAsync(Object model, IDictionary<String, String>? queryParams, Int64 deletedCount)` | Postprocessing when deleting with [query parameter filtering](#query-parameter-filtering). |
| `Task<MessageResult> PostprocessDeleteAsync(Object model, Query query, Int64 deletedCount)` | Postprocessing when deleting with [body query filtering](#body-query-filtering). |