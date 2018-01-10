using ReAct.unity;

using UnityEngine;



namespace ReAct
{
    public class POSHCore : POSHController
	{
        void Awake()
        {
            InitPOSH();
            Input.gyro.enabled = false;
         }

        void Start()
        {
            
        }

        void Update()
        {
            //Loom.QueueOnMainThread(() =>
            //{
                if (Application.isPlaying)
                    RunPOSH();
            //});
        }

    }
}
