using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NEAT : MonoBehaviour
{
    public delegate void nG();
    public static event nG OnNextGeneration;
    [System.Serializable]
    public class creature
    {
         public GameObject go;
         public NeuralNet brain;
         public FitnessData fD;
    }
    public GameObject childPrefab;
    public int generationCount = 10;
    public float generationTime = 20f;
    public float t;
    [Range(0f, 100f)]
    public float chanceToMutate = 20f;
    [Range(0f, 1f)]
    public float mutationRange = 0.1f;
    [Range(0f,1f)]
    public float chanceToMutateOverGeneraionDiff = 0.1f;
    [Range(0f, 1f)]
    public float mutatioRangeOverGenerationDIff = 0.1f;
    public Vector3 startPos = Vector3.zero;
    public Vector3 startEulerAngles = Vector3.zero;
    public List<creature> creatures = new List<creature>();

    bool run = true;

    public GameObject genTxtPrefab;
    Text genTxt;
    int generation=0;
    public bool showLeaderMark;
    public Vector3 leaderMarkSize = new Vector3(3,3,3);
    public Transform leaderMarkPrefab;
    Transform leaderMark;

    private void Update()
    {
        if (run)
        {
            t -= Time.deltaTime;
            if (t <= 0)
            {
                run = false;
                createNextGeneration();
            }
        }
    }

    bool chance(float percentage)
    {
        var r = Random.Range(0f, 100f);
        return r < percentage;
    }

    private void Start()
    {
        t = generationTime;
        if(generationCount % 2 != 0)
        {
            generationCount++;
        }
        createRandomCreatures(generationCount);
        var gtt = Instantiate(genTxtPrefab);
        gtt.transform.parent = GameObject.Find("NeuralNet_UI").transform;
        gtt.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        genTxt = gtt.GetComponent<Text>();
        genTxt.text = "Generation: " + generation + "\n" + "Chance to mutate = " + Mathf.Round(chanceToMutate * 1000f) / 1000f + "%" + "\n" + "MutationRange = " + Mathf.Round((mutationRange * 100f) * 1000f) / 1000f + "%";
    }

    public void createRandomCreatures(int numOfCreatures)
    {
        for(int i = 0; i < numOfCreatures; i++)
        {
            var go = Instantiate(childPrefab);
            go.transform.position = startPos;
            go.transform.eulerAngles = startEulerAngles;
            creatures.Add(new creature());
            creatures[i].go = go;
            var brain = go.GetComponent<NeuralNet>();
            brain.setupNeuralNet();
            brain.DrawNeuralNet();
            brain.randomizeBias();
            brain.randomizeWeights();
            brain.showNeuralNet = false;
            creatures[i].brain = brain;
            creatures[i].fD = go.GetComponent<FitnessData>();
        }
        leaderMark = Instantiate(leaderMarkPrefab);
        leaderMark.localScale = leaderMarkSize;
        leaderMark.SetParent(creatures[0].go.transform);
        creatures[0].brain.showNeuralNet = true;
    }

    public void createNextGeneration()
    {
        //sort creatures by fitness
        //kill worse half of population
        //recreate better half and create rest out of their genes
        Destroy(leaderMark.gameObject);
        sortCreaturesByFitness();
        Debug.Log("Destroying half of the generation...");
        for (int i = generationCount-1; i >= generationCount/2 ; i--)
        {
            Destroy(creatures[i].go);
            //Debug.Log("Destroyed agent with " + creatures[i].fD.fitness + "fitness");
            creatures.Remove(creatures[creatures.Count - 1]);
        }
        Debug.Log("Destroyed");
        //getNextGenerationMinMaxValues();
        createNext();
    }

    void createNext()
    {
        //save brains
        Debug.Log("saving brains...");
        List<List<NeuralNet.Layer>> brains = new List<List<NeuralNet.Layer>>();
        for(int i = 0; i < generationCount/2; i++)
        {
            brains.Add(creatures[i].brain.layers);
            Destroy(creatures[i].go);
        }
        Debug.Log(brains.Count+" brains saved.");
        creatures = new List<creature>();
        //next gen
        Debug.Log("recreating half of last generation...");
        for (int i = 0; i < generationCount; i++)
        {
            var go = Instantiate(childPrefab);
            go.transform.position = startPos;
            go.transform.eulerAngles = startEulerAngles;
            creatures.Add(new creature());
            creatures[i].go = go;
            var brain = go.GetComponent<NeuralNet>();
            brain.setupNeuralNet();
            brain.DrawNeuralNet();
            brain.showNeuralNet = false;
            creatures[i].brain = brain;
            creatures[i].fD = go.GetComponent<FitnessData>();
        }
        //apply last brains
        Debug.Log("Applying last brains...");
        /*for(int i = 0; i < brains.Count; i++)
        {
            creatures[i].brain.layers = brains[i];
            creatures[i+generationCount/2].brain.layers = brains[i];
        }*/
        Debug.Log("Done.");
        //mutate rest based on last brains
        
        for (int c = 0; c < generationCount; c++)
        {
            for (int i = 0; i < creatures[c].brain.layers.Count; i++)
            {
                for (int j = 0; j < creatures[c].brain.layers[i].nodes.Count; j++)
                {
                    if (i > 0)
                    {
                        if (c >= generationCount / 2)
                        {
                            creatures[c].brain.layers[i].nodes[j].bias = brains[c - (generationCount / 2)][i].nodes[j].bias;
                            if (chance(chanceToMutate))
                            {
                                var r = Random.Range(-mutationRange, mutationRange);
                                var newVal = creatures[c].brain.layers[i].nodes[j].bias + (creatures[c].brain.layers[i - 1].nodes.Count * r);
                                newVal = Mathf.Clamp(newVal, -creatures[c].brain.layers[i-1].nodes.Count, creatures[c].brain.layers[i - 1].nodes.Count);
                                creatures[c].brain.layers[i].nodes[j].bias = newVal;
                            }
                        }
                        else
                        {
                            creatures[c].brain.layers[i].nodes[j].bias = brains[c][i].nodes[j].bias;
                        }
                    }
                    for (int k = 0; k < creatures[c].brain.layers[i].nodes[j].connections.Count; k++)
                    {
                        if (c >= generationCount / 2)
                        {
                            creatures[c].brain.layers[i].nodes[j].connections[k].value = brains[c-(generationCount/2)][i].nodes[j].connections[k].value;
                            if (chance(chanceToMutate))
                            {
                                var r = Random.Range(-mutationRange, mutationRange);
                                var newVal = creatures[c].brain.layers[i].nodes[j].connections[k].value + r;
                                newVal = Mathf.Clamp(newVal, -1f, 1f);
                                creatures[c].brain.layers[i].nodes[j].connections[k].value = Random.Range(-1f, 1f);
                            }
                        }
                        else
                        {
                            creatures[c].brain.layers[i].nodes[j].connections[k].value = brains[c][i].nodes[j].connections[k].value;
                        }
                    }
                }
            }
        }
        leaderMark = Instantiate(leaderMarkPrefab);
        leaderMark.localScale = leaderMarkSize;
        leaderMark.SetParent(creatures[0].go.transform);
        creatures[0].brain.showNeuralNet = true;
        manageMutationChances();
        generation++;
        genTxt.text = "Generation: " + generation +"\n"+"Chance to mutate = " + Mathf.Round(chanceToMutate*1000f)/1000f + "%"+"\n"+"MutationRange = "+Mathf.Round((mutationRange*100f)*1000f)/1000f+"%";
        if(OnNextGeneration!=null)
        OnNextGeneration();
        Debug.Log("Done.");
        t = generationTime;
        run = true;
    }

    void manageMutationChances()
    {
        chanceToMutate -= chanceToMutate * chanceToMutateOverGeneraionDiff;
        chanceToMutate = Mathf.Clamp(chanceToMutate, 0f, 100f);
        mutationRange -= mutationRange * mutatioRangeOverGenerationDIff;
        mutationRange = Mathf.Clamp(mutationRange, 0f, 1f);
    }

    void sortCreaturesByFitness()
    {
        Debug.Log("sorting...");
        List<creature> sortedCreatures = new List<creature>();
        //start with first value on the list
        //check every next value if its higher or equal to highestElement, replace highestElement with it
        //remove highestElement from creatures, add it to sortedCreatures
        //repeat untill creatures list is empty
        //creatures = sortedCreatures
        while(creatures.Count > 0)
        {
            creature highestElement = creatures[0];
            for (int i = 0; i < creatures.Count; i++)
            {
                if (creatures[i].fD.fitness >= highestElement.fD.fitness)
                {
                    highestElement = creatures[i];
                }
            }
            sortedCreatures.Add(highestElement);
            creatures.Remove(highestElement);
        }
        creatures = sortedCreatures;
        Debug.Log("sorted.");
    }
}
