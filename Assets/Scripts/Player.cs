using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player instance;

    public static CustomGrabber[] hands;
    public CustomGrabber[] instanceHands;

    public Coroutine vibration;
    public float durationLeft;

    private void Awake()
    {
        instance = this;
        hands = instanceHands;
    }

    /**
     * Convenience method to vibrate either hand.
     * If interrupt is true, will stop any current vibration and start a new one. 
     */
    public static void Vibrate(float duration, float strength, CustomGrabber hand, bool interrupt)
    {
        if (instance.vibration != null && instance.durationLeft > 0)
        {
            if (interrupt)
            {
                instance.StopCoroutine(instance.vibration);
            }
            else
            {
                return;
            }
        }

        instance.vibration = instance.StartCoroutine(instance.Vibrate(duration, strength, (hand == hands[0]) ? OVRInput.Controller.LTouch : OVRInput.Controller.RTouch));
    }

    public IEnumerator Vibrate(float duration, float strength, OVRInput.Controller controller)
    {
        durationLeft = duration;
        while (durationLeft > 0)
        {
            OVRInput.SetControllerVibration(1, strength, controller);

            yield return new WaitForSeconds(Mathf.Min(durationLeft, 0.2f));
            durationLeft -= 0.2f;
        }

        OVRInput.SetControllerVibration(0, 0, controller);
    }
}
