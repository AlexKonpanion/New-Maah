using UnityEngine;
using System.Collections;

namespace Game
{
    public class LastPlayerSighting : MonoBehaviour
    {
        public Vector3 position = new Vector3(1000f, 1000f, 1000f);         // The last global sighting of the player.
        public Vector3 resetPosition = new Vector3(1000f, 1000f, 1000f);    // The default position if the player is not in sight.
        public float lightHighIntensity = 0.25f;                            // The directional light's intensity when the alarms are off.
        public float lightLowIntensity = 0f;                                // The directional light's intensity when the alarms are on.
        public float fadeSpeed = 7f;                                        // How fast the light fades between low and high intensity.
        public float musicFadeSpeed = 1f;                                   // The speed at which the 


        public Light playerLight;                                            // Reference to the main light.
        public AudioSource[] playerVisibleAudio;                                       // Reference to the AudioSources of the megaphones.
        public float coolDownTime;

        private Vector3 sightedPosition;

        void Awake()
        {
            sightedPosition = resetPosition;
        }


        void Update()
        {
            // Switch the alarms and fade the music.
            if (sightedPosition != position)
            {
                StartCoroutine(CoolDown(position));
            }
            //SwitchAlarms();
            MusicFading();

        }


        void SwitchAlarms()
        {
            
            // Create a new intensity.
            float newIntensity;

            // If the position is not the reset position...
            if (position != resetPosition)
                // ... then set the new intensity to low.
                newIntensity = lightLowIntensity;
            else
                // Otherwise set the new intensity to high.
                newIntensity = lightHighIntensity;

            // Fade the directional light's intensity in or out.
            playerLight.intensity = Mathf.Lerp(playerLight.intensity, newIntensity, fadeSpeed * Time.deltaTime);

            // For all of the sirens...
            for (int i = 0; i < playerVisibleAudio.Length; i++)
            {
                // ... if alarm is triggered and the audio isn't playing, then play the audio.
                if (position != resetPosition && !playerVisibleAudio[i].isPlaying)
                    playerVisibleAudio[i].Play();
                // Otherwise if the alarm isn't triggered, stop the audio.
                else if (position == resetPosition)
                    playerVisibleAudio[i].Stop();
            }
        }




        IEnumerator CoolDown(Vector3 pos)
        {
            sightedPosition = pos;
            yield return new WaitForSeconds(coolDownTime);

            if (position == pos)
            {
                position = resetPosition;
                sightedPosition = resetPosition;
            }
        }

        void MusicFading()
        {
            // If the alarm is not being triggered...
            if (position != resetPosition)
            {
                // ... fade out the normal music...
                GetComponent<AudioSource>().volume = Mathf.Lerp(GetComponent<AudioSource>().volume, 0f, musicFadeSpeed * Time.deltaTime);

            }
            else
            {
                // Otherwise fade in the normal music and fade out the panic music.
                GetComponent<AudioSource>().volume = Mathf.Lerp(GetComponent<AudioSource>().volume, 0.5f, musicFadeSpeed * Time.deltaTime);
            }
        }
    }
}