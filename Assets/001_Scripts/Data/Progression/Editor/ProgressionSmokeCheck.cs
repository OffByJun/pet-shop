using System;
using _001_Scripts.Data.Economy;
using UnityEditor;
using UnityEngine;

namespace _001_Scripts.Data.Progression.Editor
{
    public static class ProgressionSmokeCheck
    {
        public static void RunBatch()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<ProgressionCatalog>(ProgressionStarterContentCreator.CatalogPath);
            if (catalog == null) throw new InvalidOperationException("Progression catalog was not created.");
            if (catalog.Unlocks.Count != 7 || catalog.EndingCandidates.Count != 2)
                throw new InvalidOperationException("Unexpected starter progression content count.");

            var wallet = new CurrencyWallet(10000);
            var progression = new ProgressionService(catalog, new EconomyPurchaseService(wallet));
            if (progression.IsContentUnlocked("rare_attribute_pets"))
                throw new InvalidOperationException("Rare pets must start locked.");

            for (var i = 0; i < catalog.Unlocks.Count; i++)
                if (!progression.TryUnlock(catalog.Unlocks[i]))
                    throw new InvalidOperationException($"Could not unlock {catalog.Unlocks[i].UnlockId} in catalog order.");

            if (!progression.IsContentUnlocked("expanded_pet_attribute_condition_pool") ||
                !progression.IsContentUnlocked("rare_attribute_pets") ||
                !progression.IsContentUnlocked("special_conditions"))
                throw new InvalidOperationException("Progression content gates did not open.");

            var goal = catalog.EndingCandidates[0];
            if (!progression.TryCompleteEnding(goal) || progression.CurrentStage != ProgressionStageId.Final)
                throw new InvalidOperationException("Ending completion did not reach the final stage.");
            if (!goal.ContinueBusinessAfterCompletion)
                throw new InvalidOperationException("Post-ending business must remain enabled.");

            var snapshot = progression.State.CreateSnapshot();
            var restored = new ProgressionState();
            restored.Restore(snapshot);
            if (!restored.IsEndingCompleted(goal.GoalId) || !restored.IsUnlocked(catalog.Unlocks[0].UnlockId))
                throw new InvalidOperationException("Progression snapshot restore failed.");

            Debug.Log($"Progression smoke check passed. Remaining balance: {wallet.Balance}");
        }
    }
}
