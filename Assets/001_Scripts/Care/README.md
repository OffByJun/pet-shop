# Care / Reception architecture

The reception and care feature is split into application ports, domain state, and Unity adapters.

- `CustomerReceptionScene`: coordinates the reception use case through interfaces only.
- `ReceptionUIComponent`: passive view; emits user intent and renders `ReceptionViewModel`.
- `ReceptionDialogueSession`: owns conversation state; wording comes from `IReceptionDialogueComposer`.
- `ReceptionHandoff`: owns only handoff timing.
- `ReceptionCareSceneTransition`: persists the visit and performs Unity scene navigation.
- `ReceptionCustomerActor`: presents customer/carrier movement using pre-authored scene objects.
- `ReceptionOrderSource`: adapts the existing `ServiceOrder` system and filters by serialized supported actions.
- `CareSession`: owns care condition progress, tool rules, wetness, completion, and byproducts.
- `CarePlayScene`: coordinates the care use case without rendering responsibilities.
- `CareUIComponent`: passive uGUI view backed by serialized Canvas objects.
- `CareStageInput`: uGUI pointer adapter for direct treatment gestures.

## Extension points

- Add or replace reception adapters through the interfaces in `ReceptionContracts.cs`.
- Change dialogue tone/localization by implementing `IReceptionDialogueComposer`.
- Add condition layouts by implementing `ICareConditionSource`.
- Change cross-scene condition mapping by implementing `ICareConditionIdMapper`.
- Add supported reception actions through `ReceptionOrderSource.supportedActions` in the Inspector.

The scene builders run only in the Editor. Runtime code does not build either UI hierarchy.
