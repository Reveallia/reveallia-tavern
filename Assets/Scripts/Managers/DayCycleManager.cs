using HelperManagers;
using Tools;

namespace Managers
{
    public class DayCycleManager: IManagerComponent
    {
        public TimeOfDay CurrentState;
        
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