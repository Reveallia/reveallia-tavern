using Managers;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class GameUI : MonoBehaviour
    {
        public static GameUI Instance;
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        [Header("Bottom Right Panel")]
        public Button TakeOrderButton;
        
        [Header("Top Left Panel")]
        public RectTransform ClockProgressBar;
        
        private bool _isInitialized;
        
        
        public void SetActiveTakeOrderButton(bool isActive)
        {
            TakeOrderButton.gameObject.SetActive(isActive);
        }

        public void Initialize()
        {
            SetActiveTakeOrderButton(false);
            _isInitialized = true;
        }

        public void Update()
        {
            if (!_isInitialized) return;
            UpdateClock();
        }

        private void UpdateClock()
        {
            float dayProgress = GameManagerContext.DayCycleManager.GetDayProgress();
            float rotatedAngle = dayProgress * 360f;
            ClockProgressBar.localRotation = Quaternion.Euler(0f, 0f, rotatedAngle);
        }
    }

    
}