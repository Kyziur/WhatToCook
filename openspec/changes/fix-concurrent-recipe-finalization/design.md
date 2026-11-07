## Decision

Acquire PostgreSQL `FOR UPDATE` on the draft row inside a transaction *before* loading its state. Keep that transaction through recipe creation and state transition. `RecipeWriteService` already reuses `CurrentTransaction`, so no new transaction layer is needed. Concurrent callers return the pre-existing controlled `409 AlreadyFinalized` response with `recipeId`.

Avoid global advisory hash locks and cross-process in-memory semaphores. A non-existent draft returns `404`. Cancellation rolls back and releases the database row lock.
