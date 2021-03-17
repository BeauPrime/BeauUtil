using UnityEngine;
using BeauUtil;

public class KinematicTestObject : MonoBehaviour
{
    private const float DistanceEpsilon = 1f / 64f;
    private const float FloorDistance = DistanceEpsilon;

    [Inline(InlineAttribute.DisplayType.HeaderLabel)] public KinematicConfig2D Config;
    [Inline(InlineAttribute.DisplayType.HeaderLabel)] public KinematicState2D State;
    public Rigidbody2D Body;
    public LayerMask SolidMask;
    public float BounceFactor = 1;

    private void FixedUpdate()
    {
        Vector2 offset = KinematicMath2D.Integrate(ref State, ref Config, Time.deltaTime);

        RaycastHit2D closestHit;
        if (Body.IsCastOverlapping(SolidMask, offset, out closestHit))
        {
            float distance = closestHit.distance;

            Debug.LogFormat("colliding with floor {0} {1}", closestHit.normal, closestHit.distance);
            offset.Normalize();
            offset.x *= distance;
            offset.y *= distance;
            State.Velocity = Vector2.Reflect(State.Velocity, closestHit.normal) * BounceFactor;
        }

        Body.position += offset;

        Collider2D penetrating;
        int ticks = 16;
        while(Body.IsOverlapping(SolidMask, out penetrating) && ticks-- > 0)
        {
            var distance = Body.Distance( penetrating);
            if (distance.distance < 0)
            {
                Vector2 vec = distance.normal;
                float dist = distance.distance;
                Body.position += new Vector2(vec.x * dist, vec.y * dist);
                Debug.LogFormat("Repositioning on {0} by {1}", vec, dist);
            }
            else
                break;
        }

        transform.position = Body.position;

        if (Body.IsOverlapping(SolidMask, new Vector2(0, -FloorDistance)))
        {
            State.GravityMultiplier = 0;
            if (State.Velocity.y < 0.05f)
                State.Velocity.y = 0;
        }
        else
        {
            State.GravityMultiplier = 1;
        }
    }
}