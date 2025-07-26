using Data;
using HelperManagers;
using Tools;
using UnityEngine;

namespace Managers
{
    public class DayCycleManager: IManagerComponent
    {
        public TimeOfDay CurrentState;

        public float CurrentTime { get; private set; }
        private float _dayDuration => GameConfiguration.Instance.DayDuration;
        private float _eveningDuration => GameConfiguration.Instance.NightDuration;
        
        
        public void ChangeState(TimeOfDay newState)
        {
            CurrentState = newState;
            EventBus.Publish<TimeOfDay>(CurrentState);
        }

        public void Initialize()
        {
            CurrentState = TimeOfDay.Day;
            EventBus.Publish<TimeOfDay>(CurrentState);
            CustomLogger.LogGameLoop("DayCycleManager Initialized");
        }
        
        public void Update()
        {
            CurrentTime += Time.deltaTime;
            
            if (CurrentTime >= _dayDuration && CurrentState == TimeOfDay.Day)
            {
                ChangeState(TimeOfDay.Evening);
            }
            else if (CurrentTime >= _dayDuration + _eveningDuration && CurrentState == TimeOfDay.Evening)
            {
                ChangeState(TimeOfDay.Day);
                CurrentTime = 0f;
            }
        }
        
        public float GetDayProgress()
        {
            return CurrentTime / (_dayDuration + _eveningDuration);
        }

        public void Dispose()
        {
            
        }
    }
    
    public enum TimeOfDay
    {
        Day,
        Evening,
    }
}