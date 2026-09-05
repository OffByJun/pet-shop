using System;
using System.Collections.Generic;
using System.Linq;
using _001_Scripts.Data;
using _001_Scripts.Data.Customers;
using _001_Scripts.Data.Progression;
using _001_Scripts.Managers;
using UnityEditor;
using UnityEngine;

namespace _001_Scripts.UI.Editor
{
    public static class ShopRoutineValidation
    {
        [MenuItem("Tools/PetShop/Routine/Validate Data")]
        public static void ValidateMenu() => Debug.Log(Validate());

        public static string Validate()
        {
            var settings = AssetDatabase.LoadAssetAtPath<ShopRoutineSettings>(ShopRoutineCreator.Root + "/ShopRoutineSettings.asset");
            var days = AssetDatabase.LoadAssetAtPath<GameSettings>(ShopRoutineCreator.Root + "/BusinessDaySettings.asset");
            var catalog = AssetDatabase.LoadAssetAtPath<ServiceOrderCatalog>("Assets/002_Resources/ServiceOrders/ServiceOrderCatalog.asset");
            var progression = AssetDatabase.LoadAssetAtPath<ProgressionCatalog>(ShopRoutineCreator.Root + "/RoutineProgressionCatalog.asset");
            Require(settings != null && days != null && catalog != null && progression != null, "Missing routine assets");
            Require(days.MinimumCustomers > 0 && days.MaximumCustomers >= days.MinimumCustomers, "Invalid customer count");
            Require(days.BusinessDurationSeconds > 0f, "Invalid business duration");
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var decoration in settings.Decorations)
                Require(decoration != null && !string.IsNullOrWhiteSpace(decoration.DecorationId) && ids.Add(decoration.DecorationId), "Null or duplicate decoration ID");
            var conditions = new HashSet<PetConditionDefinition>();
            foreach (var rule in settings.CareRules)
            {
                Require(rule.Condition != null && conditions.Add(rule.Condition), "Null or duplicate care mapping");
                Require(rule.DomainTool != null && rule.DomainTool.CanProcess(rule.Condition), "Incompatible tool for " + rule.Condition.name);
                Require(rule.DomainTool.RewardAction == rule.Condition.ResolvedBy, "Wrong byproduct action for " + rule.Condition.name);
                Require(rule.CreateState().Zone.width > 0 && rule.CreateState().Zone.height > 0, "Empty care target zone");
            }
            foreach (var unlock in progression.Unlocks)
                foreach (var prerequisite in unlock.Prerequisites)
                    Require(prerequisite != null && progression.Unlocks.Contains(prerequisite), "Unreachable prerequisite");
            var randomState = UnityEngine.Random.state;
            var maximumRequests = 0;
            try
            {
                UnityEngine.Random.InitState(905);
                var generator = new ServiceOrderGenerator(catalog, null, _ => true, c => conditions.Contains(c));
                for (var i = 0; i < 1000; i++)
                {
                    var order = generator.CreateOrder();
                    maximumRequests = Math.Max(maximumRequests, order.Requests.Count);
                    Require(order.RequiredCount > 0 && order.Requests.All(r => conditions.Contains(r.Condition)), "Unplayable generated order");
                }
            }
            finally { UnityEngine.Random.state = randomState; }
            // The reused care scene currently authors five condition rows and target marks.
            Require(maximumRequests <= 5, "Care UI needs additional condition rows for this catalog");
            return $"PASS: {settings.CareRules.Count} care mappings, {settings.Decorations.Count} decorations, {progression.Unlocks.Count} upgrades; 1000 generated orders, maximum {maximumRequests} requests.";
        }
        private static void Require(bool condition, string message)
        { if (!condition) throw new InvalidOperationException(message); }
    }
}
