using UnityEngine;

namespace _001_Scripts.Data.Customers
{
    /// <summary>접수대 대화의 인내심과 표정 규칙입니다. 수치는 모두 이 에셋에서 조정합니다.</summary>
    [CreateAssetMenu(fileName = "ReceptionSettings", menuName = "PetShop/Customers/Reception Settings")]
    public sealed class ReceptionSettings : ScriptableObject
    {
        [Header("Patience")]
        [Tooltip("손님이 도착했을 때의 인내심입니다.")]
        [SerializeField, Range(0f, 1f)] private float startingPatience = 1f;
        [Tooltip("응대를 기다리는 동안 1초에 줄어드는 인내심입니다. 0이면 시간으로는 줄지 않습니다.")]
        [SerializeField, Min(0f)] private float drainPerSecond = .016f;
        [Tooltip("질문 한 번에 줄어드는 인내심입니다.")]
        [SerializeField, Range(0f, 1f)] private float questionCost = .12f;
        [Tooltip("질문만으로는 이 값 아래로 내려가지 않습니다. 시간 감소에는 적용되지 않습니다.")]
        [SerializeField, Range(0f, 1f)] private float questionFloor = .2f;

        [Header("Leaving")]
        [Tooltip("인내심이 0이 되면 손님이 그냥 돌아갑니다.")]
        [SerializeField] private bool leaveWhenPatienceEmpty = true;

        [Header("Lines")]
        [Tooltip("접수대 문구 테이블입니다. 비어 있으면 기본 문구로 동작합니다.")]
        [SerializeField] private ReceptionDialogueTable lines;

        [Header("Mood")]
        [Tooltip("이 값 이하부터 시무룩한 표정을 씁니다.")]
        [SerializeField, Range(0f, 1f)] private float uneasyPatience = .8f;
        [Tooltip("이 값 이하부터 울상 표정을 씁니다.")]
        [SerializeField, Range(0f, 1f)] private float upsetPatience = .65f;

        public float StartingPatience => startingPatience;
        public float DrainPerSecond => drainPerSecond;
        public float QuestionCost => questionCost;
        public float QuestionFloor => questionFloor;
        public bool LeaveWhenPatienceEmpty => leaveWhenPatienceEmpty;
        public ReceptionDialogueTable Lines => lines;

        public CustomerMood MoodFor(float patience)
        {
            if (patience <= upsetPatience) return CustomerMood.Upset;
            return patience <= uneasyPatience ? CustomerMood.Uneasy : CustomerMood.Calm;
        }

        private void OnValidate()
        {
            upsetPatience = Mathf.Min(upsetPatience, uneasyPatience);
            questionFloor = Mathf.Min(questionFloor, startingPatience);
        }
    }
}
