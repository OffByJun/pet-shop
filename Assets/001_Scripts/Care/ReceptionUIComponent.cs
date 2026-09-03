using System;
using UnityEngine;
using UnityEngine.UI;

namespace PetShop.Care
{
    /// <summary>Passive uGUI view. It emits intent and renders a view model.</summary>
    public sealed class ReceptionUIComponent : MonoBehaviour, IReceptionView
    {
        [Header("Dialogue")]
        [SerializeField] private Text speakerText;
        [SerializeField] private Text dialogueText;
        [SerializeField] private Slider patienceSlider;
        [SerializeField] private Button[] questionButtons;
        [SerializeField] private Text[] questionLabels;
        [Header("Order memo")]
        [SerializeField] private Text petText;
        [SerializeField] private Text requestCountText;
        [SerializeField] private Text memoText;
        [Header("Actions")]
        [SerializeField] private Button acceptButton;
        [SerializeField] private Button rejectButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button careButton;

        public event Action<int> QuestionRequested;
        public event Action AcceptRequested;
        public event Action RejectRequested;
        public event Action NextRequested;
        public event Action CareRequested;

        private void Awake()
        {
            for (var i = 0; i < questionButtons.Length; i++)
            {
                var captured = i;
                questionButtons[i].onClick.AddListener(() => QuestionRequested?.Invoke(captured));
            }
            acceptButton.onClick.AddListener(() => AcceptRequested?.Invoke());
            rejectButton.onClick.AddListener(() => RejectRequested?.Invoke());
            nextButton.onClick.AddListener(() => NextRequested?.Invoke());
            careButton.onClick.AddListener(() => CareRequested?.Invoke());
        }

        public void Render(ReceptionViewModel model)
        {
            if (model?.Order == null) return;
            speakerText.text = model.Speaker;
            dialogueText.text = model.Line;
            patienceSlider.value = model.Patience;
            var animalName = model.Order.Pet.BaseAnimal == null ? string.Empty : model.Order.Pet.BaseAnimal.DisplayName;
            petText.text = $"{model.Order.Pet.DisplayName} · {animalName}";
            requestCountText.text = $"필수 {model.Order.RequiredRequests.Count} · 선택 {model.Order.OptionalRequests.Count}";

            var memo = string.Empty;
            for (var i = 0; i < model.Questions.Count; i++)
                if (model.Questions[i].Asked) memo += "• " + model.Questions[i].Reveal + "\n";
            memoText.text = string.IsNullOrEmpty(memo) ? "추가 질문으로 정확한 상태를 확인할 수 있어요." : memo;

            for (var i = 0; i < questionButtons.Length; i++)
            {
                var visible = model.CanInteract && i < model.Questions.Count && !model.Questions[i].Asked;
                questionButtons[i].gameObject.SetActive(visible);
                if (visible) questionLabels[i].text = "?  " + model.Questions[i].Prompt;
            }
            acceptButton.gameObject.SetActive(model.CanInteract);
            rejectButton.gameObject.SetActive(model.CanInteract);
            nextButton.gameObject.SetActive(model.CanInteract);
            careButton.gameObject.SetActive(model.CanEnterCare);
        }

        public void Configure(
            Text speaker, Text line, Slider patience, Button[] questions, Text[] labels,
            Text pet, Text requestCount, Text memo, Button accept, Button reject, Button next, Button care)
        {
            speakerText = speaker;
            dialogueText = line;
            patienceSlider = patience;
            questionButtons = questions;
            questionLabels = labels;
            petText = pet;
            requestCountText = requestCount;
            memoText = memo;
            acceptButton = accept;
            rejectButton = reject;
            nextButton = next;
            careButton = care;
        }
    }
}
