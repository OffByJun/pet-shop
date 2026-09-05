using System;
using _001_Scripts.Core;
using _001_Scripts.Core.Pipes;
using _001_Scripts.Core.Pipes.Msgs;
using _001_Scripts.UI.UILib;
using UnityEngine;
using UnityEngine.UI;

namespace _001_Scripts.UI.Components
{
    /// <summary>Passive uGUI view. It emits intent and renders a view model.</summary>
    public sealed class ReceptionUIComponent : GameBehaviour
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
        // Slot -> question index. An order can carry more requests than there are buttons, so the
        // remaining questions are packed into the slots instead of being pinned to a fixed index.
        private readonly System.Collections.Generic.List<int> openQuestions = new System.Collections.Generic.List<int>();

        private void Awake()
        {
            for (var i = 0; i < questionButtons.Length; i++)
            {
                var captured = i;
                questionButtons[i].onClick.AddListener(() =>
                {
                    if (captured >= openQuestions.Count) return;
                    GamePipe.Publish(new ReceptionInputRequest(this, ReceptionInput.Ask, openQuestions[captured]));
                });
            }
            acceptButton.onClick.AddListener(() => GamePipe.Publish(new ReceptionInputRequest(this, ReceptionInput.Accept, -1)));
            rejectButton.onClick.AddListener(() => GamePipe.Publish(new ReceptionInputRequest(this, ReceptionInput.Reject, -1)));
            nextButton.onClick.AddListener(() => GamePipe.Publish(new ReceptionInputRequest(this, ReceptionInput.Next, -1)));
            careButton.onClick.AddListener(() => GamePipe.Publish(new ReceptionInputRequest(this, ReceptionInput.EnterCare, -1)));
        }

        public void Render(ReceptionViewModel model)
        {
            if (model?.Order == null) return;
            speakerText.text = string.IsNullOrEmpty(model.BondLabel) || model.VisitNumber <= 0
                ? model.Speaker
                : $"{model.Speaker}   ·   {model.BondLabel} {model.VisitNumber}회차";
            dialogueText.text = model.Line;
            patienceSlider.value = model.Patience;
            var animalName = model.Order.Pet.BaseAnimal == null ? string.Empty : model.Order.Pet.BaseAnimal.DisplayName;
            petText.text = $"{model.Order.Pet.DisplayName} · {animalName}";
            requestCountText.text = $"필수 {model.Order.RequiredRequests.Count} · 선택 {model.Order.OptionalRequests.Count}";

            var memo = string.Empty;
            for (var i = 0; i < model.Questions.Count; i++)
                if (model.Questions[i].Asked) memo += "• " + model.Questions[i].Reveal + "\n";
            memoText.text = string.IsNullOrEmpty(memo) ? "추가 질문으로 정확한 상태를 확인할 수 있어요." : memo;

            openQuestions.Clear();
            if (model.CanInteract)
                for (var i = 0; i < model.Questions.Count; i++)
                    if (!model.Questions[i].Asked) openQuestions.Add(i);
            for (var i = 0; i < questionButtons.Length; i++)
            {
                var visible = i < openQuestions.Count;
                questionButtons[i].gameObject.SetActive(visible);
                if (visible) questionLabels[i].text = "?  " + model.Questions[openQuestions[i]].Prompt;
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
