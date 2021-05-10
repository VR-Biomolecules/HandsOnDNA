using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ToScale
{
    [System.Serializable]
    public struct Transition
    {
        internal bool elapse;
        internal float elapsedTime;

        public AnimationCurve scaleCurve;
        public AnimationCurve curveMultiplier;

        internal Vector3 startPosition;
        internal Vector3 startScale;
        public Vector3 endPosition;
        public Vector3 endScale;

        public float endMultiplier;

        [Space]

        public bool modifyTransparency;
        public AnimationCurve transparencyCurve;
        internal Renderer renderer;
        public Material material;
    }
    public Transition[] transitions;
    internal int index;

    public Transform objectToScale;

    internal float startLightRange;
    internal List<Light> lightsToScale = new List<Light>();

    public void SetupTransition()
    {
        transitions[index].startPosition = objectToScale.localPosition;
        transitions[index].startScale = objectToScale.localScale;

        if (lightsToScale.Count == 0)
        {
            foreach(Light l in objectToScale.GetComponentsInChildren<Light>())
            {
                if (l.type == LightType.Point)
                {
                    lightsToScale.Add(l);
                }
            }

            if (lightsToScale.Count > 1) { startLightRange = lightsToScale[0].range; }
        }
        if (T().material)
        {
            transitions[index].renderer = objectToScale.GetComponentInChildren<Renderer>(true);

            Color color = T().material.color;
            color.a = T().transparencyCurve.Evaluate(0);
            T().material.color = color;
        }
    }

    public float Play()
    {
        transitions[index].elapse = true;

        return transitions[index].scaleCurve.keys[transitions[index].scaleCurve.keys.Length - 1].time;
    }

    public void Elapse()
    {
        if (index >= transitions.Length || !transitions[index].elapse)
        {
            return;
        }
        transitions[index].elapsedTime += Time.deltaTime;

        float curveValue = T().scaleCurve.Evaluate(T().elapsedTime)
         * T().curveMultiplier.Evaluate(T().elapsedTime / T().scaleCurve.keys[T().scaleCurve.keys.Length - 1].time);

        objectToScale.localPosition = Vector3.Lerp(T().startPosition, T().endPosition * T().endMultiplier, curveValue);
        objectToScale.localScale = Vector3.Lerp(T().startScale, T().endScale * T().endMultiplier, curveValue);

        foreach (Light l in lightsToScale)
        {
            l.range = startLightRange * objectToScale.localScale.x;
        }

        if (T().modifyTransparency)
        {
            if (T().renderer.material != T().material && T().elapsedTime > T().transparencyCurve.keys[0].time) { transitions[index].renderer.material = T().material; }

            Color color = T().material.color;
            color.a = T().transparencyCurve.Evaluate(T().elapsedTime);
            T().material.color = color;
        }

        if (T().elapsedTime > T().scaleCurve.keys[T().scaleCurve.keys.Length - 1].time)
        {
            transitions[index].elapse = false;
            index++;
            if (index < transitions.Length) { SetupTransition(); }
        }
    }

    public void Skip()
    {
        transitions[index].elapse = true;
        transitions[index].elapsedTime = T().scaleCurve.keys[T().scaleCurve.keys.Length - 1].time;
        Elapse();
    }

    Transition T()
    {
        return transitions[index];
    }
}