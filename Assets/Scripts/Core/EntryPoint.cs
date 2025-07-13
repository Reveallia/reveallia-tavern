using Managers;
using Tools;
using UnityEngine;

namespace Core
{
    public class EntryPoint : MonoBehaviour
    {
        public GameManager GameManager;

        public void Start()
        {
            CustomLogger.LogGameLoop("EntryPoint.Start");
            GameManager.Initialize();
        }
    }
}