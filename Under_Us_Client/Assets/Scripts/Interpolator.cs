
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interpolator : MonoBehaviour
{
    [SerializeField] private float timeElapsed = 0f;
    [SerializeField] private float timeToReachTarget = 0.05f;
    [SerializeField] private float movementThreshold = 0.05f;

    private readonly List<TransformUpdate> futureTransformUpdates = new List<TransformUpdate>();
    private float squareMovementThreshold;
    private TransformUpdate to;
    private TransformUpdate from;
    private TransformUpdate previous;

    private void Start()
    {
        squareMovementThreshold = movementThreshold * movementThreshold;
        to = new TransformUpdate(NetworkManager.Singleton.ServerTick, transform.position);
        from = new TransformUpdate(NetworkManager.Singleton.InterpolationTick, transform.position);
        previous = new TransformUpdate(NetworkManager.Singleton.InterpolationTick, transform.position);
    }

    private void Update()
    {
        for (int i = 0; i < futureTransformUpdates.Count; i++)
        {
            if (NetworkManager.Singleton.ServerTick >= futureTransformUpdates[i].Tick)
            {
                previous = to;
                to = futureTransformUpdates[i];
                from = new TransformUpdate(NetworkManager.Singleton.InterpolationTick, transform.position);

                futureTransformUpdates.RemoveAt(i);
                i--;
                timeElapsed = 0f;
                timeToReachTarget = Mathf.Max(to.Tick - from.Tick, 1) * Time.fixedDeltaTime; ;
            }
        }

        timeElapsed += Time.deltaTime;
        InterpolatePosition(timeElapsed / timeToReachTarget);
    }

    private void InterpolatePosition(float lerpAmount)
    {
        if ((to.Position - previous.Position).sqrMagnitude < squareMovementThreshold)
        {
            if (to.Position != from.Position)
                transform.position = Vector3.Lerp(from.Position, to.Position, lerpAmount);

            return;
        }

        transform.position = Vector3.LerpUnclamped(from.Position, to.Position, lerpAmount);

        Vector3 lerpedPos = Vector3.LerpUnclamped(from.Position, to.Position, lerpAmount);
        if (float.IsInfinity(lerpedPos.x) || float.IsNaN(lerpedPos.x) || float.IsInfinity(lerpedPos.y) || float.IsNaN(lerpedPos.y) || float.IsInfinity(lerpedPos.z) || float.IsNaN(lerpedPos.z))
            Debug.Log($"From: {from.Position} To: {to.Position} Lerp: {lerpAmount}");
    }

    public void NewUpdate(ushort tick, Vector3 position)
    {
        for (int i = 0; i < futureTransformUpdates.Count; i++)
        {
            if (tick < futureTransformUpdates[i].Tick)
            {
                futureTransformUpdates.Insert(i, new TransformUpdate(tick, position));
                return;
            }
        }

        futureTransformUpdates.Add(new TransformUpdate(tick, position));
    }
}
