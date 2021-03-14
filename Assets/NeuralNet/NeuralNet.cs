using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NeuralNet : MonoBehaviour
{
    [System.Serializable]
    public class Node
    {
        public string name = "";
        public float value;
        //[System.NonSerialized]
        public List<Node> connectedTo = new List<Node>();
        public List<Connection> connections = new List<Connection>();
        public Transform img;
        public float bias = 0;
    }

    [System.Serializable]
    public class Connection
    {
        public float value;
        public Transform img;
    }
    [System.Serializable]
    public class Layer
    {
        public List<Node> nodes = new List<Node>();
    }

    public string name = "NeuralNet";
    public bool showNeuralNet = true;

    public Transform nodePrefab, connectionPrefab, canvasPrefab;
    List<Transform> uiElements = new List<Transform>();
    public float nodeSize = 50f, connectionSize = 10f, ySize = 200f, xSpacing = 100f;

    [System.NonSerialized]
    public List<Node> inputLayer, outputLayer;
    public List<Layer> layers = new List<Layer>();

    public GameObject uiParent;

    private void Start()
    {
        inputLayer = layers[0].nodes;
        outputLayer = layers[layers.Count - 1].nodes;
/*
        setupNeuralNet();
        DrawNeuralNet();
        randomizeWeights();
        randomizeBias();*/

    }

    private void FixedUpdate()
    {
        updateNeuralNetValues();
        uiParent.SetActive(showNeuralNet);
        if (showNeuralNet)
        {
            updateNeuralNetUI();
        }
    }

    public void randomizeBias()
    {
        for (int i = 1; i < layers.Count; i++)
        {
            for (int j = 0; j < layers[i].nodes.Count; j++)
            {
                float range = layers[i - 1].nodes.Count;
                layers[i].nodes[j].bias = Random.Range(-range, range);
            }
        }


    }

    public void setupNeuralNet()
    {
        for (int i = 0; i < layers.Count - 1; i++)
        {
            for (int j = 0; j < layers[i].nodes.Count; j++)
            {
                for (int k = 0; k < layers[i + 1].nodes.Count; k++)
                {
                    layers[i].nodes[j].connectedTo.Add(layers[i + 1].nodes[k]);
                    layers[i].nodes[j].connections.Add(new Connection());
                }
            }
        }
    }

    public void DrawNeuralNet()
    {
        //draw nodes
        int row = 0;
        Transform UIobj;
        if (GameObject.Find("NeuralNet_UI"))
        {
            UIobj = GameObject.Find("NeuralNet_UI").transform;
        }
        else
        {
            UIobj = Instantiate(canvasPrefab);
            UIobj.name = "NeuralNet_UI";
        }

        for (int i = 0; i < layers.Count; i++)
        {
            for (int j = 0; j < layers[i].nodes.Count; j++)
            {
                var ndI = Instantiate(nodePrefab);
                uiElements.Add(ndI);
                layers[i].nodes[j].img = ndI;
                ndI.SetParent(UIobj);
                var nd = ndI.GetComponent<RectTransform>();
                nd.sizeDelta = new Vector2(nodeSize, nodeSize);
                nd.anchoredPosition = new Vector2(10f+row * xSpacing, -(j + 1) * (ySize / (layers[i].nodes.Count + 1)));
            }
            row++;
        }

        row = 0;
        //draw connections

        for (int i = 0; i < layers.Count; i++)
        {
            for (int j = 0; j < layers[i].nodes.Count; j++)
            {
                for (int k = 0; k < layers[i].nodes[j].connectedTo.Count; k++)
                {
                    var cnI = Instantiate(connectionPrefab);
                    cnI.SetParent(UIobj);
                    uiElements.Add(cnI);
                    layers[i].nodes[j].connections[k].img = cnI;
                    var cn = cnI.GetComponent<RectTransform>();
                    Vector2 pos = new Vector2(row * xSpacing + (nodeSize / 2f), -(j + 1) * (ySize / (layers[i].nodes.Count + 1)));
                    pos.y -= nodeSize / 2f;
                    var l = Vector2.Distance(layers[i].nodes[j].connectedTo[k].img.GetComponent<RectTransform>().anchoredPosition, layers[i].nodes[j].img.GetComponent<RectTransform>().anchoredPosition);
                    cn.anchoredPosition = pos;
                    cn.sizeDelta = new Vector2(l, connectionSize);
                    cn.transform.right = layers[i].nodes[j].connectedTo[k].img.GetComponent<RectTransform>().anchoredPosition - layers[i].nodes[j].img.GetComponent<RectTransform>().anchoredPosition;
                }
            }
            row++;
        }


        //set parent
        var go = Instantiate(uiParent);
        uiParent = go;
        uiParent.transform.name = name;
        uiParent.transform.SetParent(UIobj);
        uiElements.Reverse();
        for (int i = 0; i < uiElements.Count; i++)
        {
            uiElements[i].SetParent(uiParent.transform);
        }
    }

    private void OnDestroy()
    {
        Destroy(uiParent.gameObject);
    }

    public void setInputValue(float min, float max, float val, Node node)
    {
        //convert to -1,1
        var range = -min + max;
        var x = Mathf.Abs(min - val);
        node.value = (x / range);
    }

    public float getOutputValue(float min, float max, Node node)
    {
        //convert to -1,1
        var range = -min + max;
        var x = node.value * range;
        return x - Mathf.Abs(min);
    }

    float sigmoid(float x)
    {
        float e = 2.7182818284590451f;
        return 1f / (1f + Mathf.Pow(e, -x));
    }

    void updateNeuralNetValues()
    {
        for (int i = 1; i < layers.Count; i++)//layer
        {
            for (int j = 0; j < layers[i].nodes.Count; j++)//layer's node
            {
                for (int k = 0; k < layers[i - 1].nodes.Count; k++)//last layer's node
                {
                    layers[i].nodes[j].value = 0;
                    layers[i].nodes[j].value += layers[i - 1].nodes[k].value * layers[i - 1].nodes[k].connections[j].value;
                }
                layers[i].nodes[j].value = sigmoid(layers[i].nodes[j].value + layers[i].nodes[j].bias);
            }
        }
    }
    

    public void randomizeWeights()
    {
        for(int i = 0; i < layers.Count; i++)
        {
            for(int j = 0; j < layers[i].nodes.Count; j++)
            {
                for(int k = 0; k < layers[i].nodes[j].connections.Count; k++)
                {
                    layers[i].nodes[j].connections[k].value = Random.Range(-1f, 1f);
                }
            }
        }
    }

    void updateNeuralNetUI()
    {
        //nodes

        for(int i = 0; i < layers.Count; i++)
        {
            for(int j = 0; j < layers[i].nodes.Count; j++)
            {
                var val = Mathf.Round(layers[i].nodes[j].value * 100f) / 100f;
                layers[i].nodes[j].img.GetChild(0).GetComponent<Text>().text = val.ToString();
                var colVal = (layers[i].nodes[j].value);
                var col = new Color(colVal,colVal,colVal);
                layers[i].nodes[j].img.GetComponent<Image>().color = col;
                layers[i].nodes[j].img.GetChild(0).GetComponent<Text>().color = new Color(0.75f-colVal, 0.75f-colVal, 0.75f-colVal);
            }
        }


        //connections
        for (int i = 0; i < layers.Count; i++)
        {
            for (int j = 0; j < layers[i].nodes.Count; j++)
            {
                for (int k = 0; k < layers[i].nodes[j].connections.Count; k++)
                {
                    layers[i].nodes[j].connections[k].img.localScale = new Vector2(layers[i].nodes[j].connections[k].img.localScale.x, layers[i].nodes[j].connections[k].value);
                    if (layers[i].nodes[j].connections[k].value < 0)
                    {
                        layers[i].nodes[j].connections[k].img.GetComponent<Image>().color = Color.red;
                    }
                    else
                    {
                        layers[i].nodes[j].connections[k].img.GetComponent<Image>().color = Color.green;
                    }
                }
            }
        }
    }
}
