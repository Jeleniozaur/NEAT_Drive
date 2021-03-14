using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class carController : MonoBehaviour
{
    [System.Serializable]
    public class checkpoint
    {
        public Transform tr;
        public bool check = false;
    }
    public float acc, turn;
    public float fitness=0;
    Rigidbody2D rb;
    Vector2 inp;
    NeuralNet brain;
    public List<checkpoint> checkpoints = new List<checkpoint>();
    FitnessData fd;
    float timeSinceLastCheckpoint;
    int nextCPint = 0;
    public float smooth = 2f;

    void checkCheckpoint(Transform checkpoint)
    {
        for(int i = 0; i < checkpoints.Count; i++)
        {
            if(checkpoints[i].tr == checkpoint)
            {
                if (!checkpoints[i].check)
                {
                    checkpoints[i].check = true;
                    fitness += 10*GameObject.Find("NEAT").GetComponent<NEAT>().t;
                }
            }
        }
    }

    private void Start()
    {
        brain = gameObject.GetComponent<NeuralNet>();
        GameObject cH = GameObject.Find("checkpointsHandler");
        for(int i = 0; i < checkpoints.Count; i++)
        {
            checkpoints[i].tr = cH.GetComponent<checkpointsHandler>().points[i];
        }
        rb = gameObject.GetComponent<Rigidbody2D>();
        fd = gameObject.GetComponent<FitnessData>();
    }
    

    private void FixedUpdate()
    {
        timeSinceLastCheckpoint -= Time.fixedDeltaTime;
        fd.fitness = fitness;
        manageRays();
        inp = new Vector2(brain.getOutputValue(-1f, 1f, brain.outputLayer[0]), brain.getOutputValue(-1f, 1f, brain.outputLayer[1]));
        rb.velocity += new Vector2(transform.right.x, transform.right.y) * acc * inp.y;
        rb.rotation -= turn * inp.x;
    }

    void manageRays()
    {
        //
        RaycastHit2D fHit = Physics2D.Raycast(transform.position, transform.right, 10f);
        RaycastHit2D lHit = Physics2D.Raycast(transform.position, transform.up, 10f);
        RaycastHit2D rHit = Physics2D.Raycast(transform.position, -transform.up, 10f);
        RaycastHit2D lfHit = Physics2D.Raycast(transform.position, (transform.right+transform.up).normalized, 10f);
        RaycastHit2D rfHit = Physics2D.Raycast(transform.position, (transform.right-transform.up).normalized, 10f);

        Debug.DrawRay(transform.position, transform.right);
        Debug.DrawRay(transform.position, transform.up);
        Debug.DrawRay(transform.position, -transform.up);
        Debug.DrawRay(transform.position, (transform.right+transform.up).normalized);
        Debug.DrawRay(transform.position, (transform.right-transform.up).normalized);

        brain.setInputValue(0f, 10f, fHit.distance, brain.inputLayer[0]);
        brain.setInputValue(0f, 10f, lHit.distance, brain.inputLayer[1]);
        brain.setInputValue(0f, 10f, rHit.distance, brain.inputLayer[2]);
        brain.setInputValue(0f, 10f, lfHit.distance, brain.inputLayer[3]);
        brain.setInputValue(0f, 10f, rfHit.distance, brain.inputLayer[4]);
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        checkCheckpoint(col.transform);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        fitness -= Time.deltaTime*40f;
    }
}
