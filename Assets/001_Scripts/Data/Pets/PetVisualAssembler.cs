using System.Collections.Generic;
using _001_Scripts.Core;
using UnityEngine;

namespace _001_Scripts.Data.Pets
{
    public sealed class PetVisualAssembler : GameBehaviour
    {
        [SerializeField] private Transform visualRoot;
        private readonly List<GameObject> spawnedObjects = new List<GameObject>();

        public GameObject ModelInstance { get; private set; }

        public bool Build(PetVariantDefinition variant)
        {
            Clear();
            if (variant == null || variant.BaseAnimal == null || variant.BaseAnimal.ModelPrefab == null) return false;
            var root = visualRoot == null ? transform : visualRoot;
            ModelInstance = Instantiate(variant.BaseAnimal.ModelPrefab, root, false);
            spawnedObjects.Add(ModelInstance);
            ApplyAnimator(variant.BaseAnimal);
            if (variant.Attribute == null) return true;
            ApplyMaterials(variant.Attribute);
            AddAttachments(variant.Attribute);
            return true;
        }

        public void Clear()
        {
            for (var i = spawnedObjects.Count - 1; i >= 0; i--)
                if (spawnedObjects[i] != null) Destroy(spawnedObjects[i]);
            spawnedObjects.Clear();
            ModelInstance = null;
        }

        private void ApplyAnimator(PetBase baseAnimal)
        {
            if (baseAnimal.AnimatorController == null) return;
            var animator = ModelInstance.GetComponentInChildren<Animator>(true);
            if (animator != null) animator.runtimeAnimatorController = baseAnimal.AnimatorController;
        }

        private void ApplyMaterials(PetAttributeDefinition attribute)
        {
            var overrides = attribute.MaterialOverrides;
            for (var i = 0; i < overrides.Count; i++)
            {
                var entry = overrides[i];
                if (entry.Material == null) continue;
                var target = string.IsNullOrWhiteSpace(entry.RendererPath)
                    ? ModelInstance.transform
                    : ModelInstance.transform.Find(entry.RendererPath);
                if (target == null) continue;
                var renderer = target.GetComponent<Renderer>();
                if (renderer == null) continue;
                var materials = renderer.sharedMaterials;
                if (entry.MaterialIndex < 0 || entry.MaterialIndex >= materials.Length) continue;
                materials[entry.MaterialIndex] = entry.Material;
                renderer.sharedMaterials = materials;
            }
        }

        private void AddAttachments(PetAttributeDefinition attribute)
        {
            var anchors = ModelInstance.GetComponentsInChildren<PetVisualSlotAnchor>(true);
            var attachments = attribute.Attachments;
            for (var i = 0; i < attachments.Count; i++)
            {
                var entry = attachments[i];
                if (entry.Prefab == null) continue;
                var parent = FindAnchor(anchors, entry.Slot);
                var instance = Instantiate(entry.Prefab, parent, false);
                instance.transform.localPosition = entry.LocalPosition;
                instance.transform.localRotation = Quaternion.Euler(entry.LocalEulerAngles);
                instance.transform.localScale = entry.LocalScale == Vector3.zero ? Vector3.one : entry.LocalScale;
                spawnedObjects.Add(instance);
            }
        }

        private Transform FindAnchor(PetVisualSlotAnchor[] anchors, PetVisualSlot slot)
        {
            for (var i = 0; i < anchors.Length; i++) if (anchors[i].Slot == slot) return anchors[i].transform;
            return ModelInstance.transform;
        }
    }
}
