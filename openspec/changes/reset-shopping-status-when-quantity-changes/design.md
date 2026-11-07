## Matching contract

The existing normalized ingredient-and-unit key defines the candidate group. Within it, canonicalize numeric quantities, normalize descriptive text, and include the persisted descriptive contribution context in the amount signature.

Reserve only unique exact-amount matches before assigning remaining rows, so changed items cannot steal identities needed by unchanged items. Non-unique matches reset state instead of carrying over an arbitrary MAM. Atomicity remains under existing plan mutation transaction. Manual duplicates are never grouped or removed.
