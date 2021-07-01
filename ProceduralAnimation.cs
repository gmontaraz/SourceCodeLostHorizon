using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralAnimation : MonoBehaviour
{
    // Start is called before the first frame update
    int j, lastLeg;
    bool onStep;
    float cooldown;
    private class LastPoint
    {
        public Vector3 pos;
        public bool ready;
        public bool grounded;
        public LastPoint next;
        public float distance;
        public LastPoint(Vector3 pos, Transform target)
        {
            this.pos = pos;
            grounded = false;
            ready = false;
            distance = 0;
        }
    }

    void Start()
    {
        onStep = false;
        distance = 0;
        lastPoints = new List<LastPoint>();
        lastBodyPos = body.position;
        for (int i = 0; i < 4; i++)
        {
            lastPoints.Add(new LastPoint(targets[i].position, targets[i]));
        }

        lastPoints[0].next = lastPoints[1];
        lastPoints[1].next = lastPoints[2];
        lastPoints[2].next = lastPoints[3];
        lastPoints[3].next = lastPoints[0];
        j = 0;
        lastLeg = 0;
        Invoke("FirstStep", 1f);
    }
    void FirstStep()
    {
        Step(0);
        Step(1);
    }

    // Update is called once per frame
    void Update()
    {
        if (planet.isPlayer)
        {
            float max_distance = 0;
            velocity = body.position - lastBodyPos;

            Vector3 averageLegPos = Vector3.zero;
            for (int i = 0; i < 4; i++)
            {
                targets[i].position = lastPoints[i].pos;
                RaycastHit hit1;
                Debug.DrawRay(sensors[i].position + (velocity * velocityMultiplier), -body.up);
                if (Physics.Raycast(sensors[i].position, -body.up, out hit1, Mathf.Infinity, ground))
                {
                    safeAreas[i].position = hit1.point;
                }
                lastPoints[i].distance = Vector3.Distance(targets[i].position, safeAreas[i].position);
                lastPoints[i].grounded = Physics.CheckSphere(lastPoints[i].pos, 0.2f, ground);
                averageLegPos += lastPoints[i].pos;
            }
            root.position = Vector3.Lerp(root.position, (averageLegPos / 4) + (body.up * offset), Time.deltaTime * smooth);

            if (((lastPoints[j].distance > treshold) || (lastPoints[j + 1].distance > treshold)) && cooldown <= 0)
            {
                cooldown = 0.2f;
                bool grounded = false;

                if (j == 0)
                {
                    if (lastPoints[2].grounded && lastPoints[3].grounded)
                    {
                        grounded = true;
                    }
                }
                else
                {
                    if (lastPoints[0].grounded && lastPoints[1].grounded)
                    {
                        grounded = true;
                    }
                }

                if (grounded)
                {
                    Step(j);
                    Step(j + 1);
                }
                j += 2;
                if (j > 2)
                {
                    j = 0;
                }
            }
            if (cooldown > 0)
            {
                cooldown -= Time.deltaTime;
            }
            else
            {
                cooldown = 0;
            }
            lastBodyPos = body.position;
        }
        
    }
    void Step(int j)
    {
        RaycastHit hit;
        if (Physics.Raycast(sensors[j].position + (velocity * velocityMultiplier *Time.deltaTime), -body.up, out hit, Mathf.Infinity, ground))
        {
            StartCoroutine(Animation(lastPoints[j], hit.point, Time.time));
        }
    }
    IEnumerator Animation(LastPoint point, Vector3 newPos, float time)
    {
        float percent = 0;
        while (percent < 1)
        {

            percent = Mathf.Clamp01((Time.time - time) / stepDuration);
            point.pos = Vector3.Lerp(point.pos, newPos, percent) + body.up * animationCurve.Evaluate(percent) * stepHeightMultiplier;
            yield return null;
        }

        onStep = false;
        point.ready = false;

    }
    List<LastPoint> lastPoints;
    float distance;
    [SerializeField, Range(0f, 1f)]
    float stepDuration, stepHeightMultiplier;

    float lastStep;
    public Transform[] targets;
    public Transform[] foots;
    public LayerMask ground;
    public Transform body;
    public Transform[] sensors;
    public Transform[] safeAreas;
    Vector3 lastBodyPos, velocity;
    public float treshold;
    public AnimationCurve animationCurve;
    public float velocityMultiplier;
    public Transform root;
    public float offset;
    public float smooth;
    public AstronomicalObject planet;
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(sensors[j].position, treshold);
        Gizmos.DrawSphere(sensors[j+1].position, treshold);
        foreach (Transform sensor in safeAreas)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(sensor.position, treshold);
        }
    }

}
