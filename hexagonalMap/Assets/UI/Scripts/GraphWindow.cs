using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CodeMonkey.Utils;
using System;

public class GraphWindow : MonoBehaviour
{
    private static GraphWindow instance;
    [SerializeField] private Sprite dotSprite;
    private RectTransform graphContainer;
    private RectTransform labelTemplateX, labelTemplateY;
    private RectTransform dashTemplateX, dashTemplateY;
    private List<GameObject> gameObjectList;
    private GameObject tooltipGameObject;

    // Cached values
    private List<int> valueList;
    private IGraphVisual graphVisual;
    private int maxVisibleValueAmount;
    private Func<int, string> getAxisLabelX;
    private Func<float, string> getAxisLabelY;

    private void Awake()
    {
        instance = this;
        graphContainer = transform.Find("GraphContainer").GetComponent<RectTransform>();
        labelTemplateX = graphContainer.Find("LabelTemplateX").GetComponent<RectTransform>();
        labelTemplateY = graphContainer.Find("LabelTemplateY").GetComponent<RectTransform>();
        dashTemplateX = graphContainer.Find("DashTemplateX").GetComponent<RectTransform>();
        dashTemplateY = graphContainer.Find("DashTemplateY").GetComponent<RectTransform>();
        tooltipGameObject = graphContainer.Find("Tooltip").gameObject;
        gameObjectList = new List<GameObject>();

        List<int> valueList = new List<int>() { 50, 98, 32, 7, 12, 9, 33, 43, 64, 19, 99 };
        IGraphVisual lineGraphVisual = new LineGraphVisual(graphContainer, dotSprite, Color.green, new Color(1, 1, 1, 0.5f));
        IGraphVisual barChartVisual = new BarChartVisual(graphContainer, Color.white, 0.9f);

        //ShowGraph(valueList, (int _i) => "Year " + (_i + 1), (float _f) => "Population " + Mathf.RoundToInt(_f));

        ShowGraph(valueList, lineGraphVisual, -1);

        //FunctionPeriodic.Create(() =>
        //{
        //    valueList.Clear();
        //    for (int i = 0; i < 20; i++)
        //    {
        //        valueList.Add(UnityEngine.Random.Range(0, 500));
        //    }
        //    ShowGraph(valueList);
        //}, 1f);

        //bool useBarChart = true;
        //FunctionPeriodic.Create(() =>
        //{
        //    if (useBarChart)
        //    {
        //        ShowGraph(valueList, barChartVisual, -1);
        //    }
        //    else
        //    {
        //        ShowGraph(valueList, lineGraphVisual, -1);
        //    }
        //    useBarChart = !useBarChart;
        //}, 4f);

        transform.Find("BarChartButton").GetComponent<Button_UI>().ClickFunc = () =>
        {
            SetGraphVisual(barChartVisual);
        };
        transform.Find("LineGraphButton").GetComponent<Button_UI>().ClickFunc = () =>
        {
            SetGraphVisual(lineGraphVisual);
        };
        transform.Find("IncreaseButton").GetComponent<Button_UI>().ClickFunc = () =>
        {
            IncreaseVisibleAmount();
        };
        transform.Find("DecreaseButton").GetComponent<Button_UI>().ClickFunc = () =>
        {
            DecreaseVisibleAmount();
        };
    }

    public static void ShowTooltip_Static(string tooltipText, Vector2 anchoredPos)
    {
        instance.ShowTooltip(tooltipText, anchoredPos);
    }

    private void ShowTooltip(string tooltipText, Vector2 anchoredPos)
    {
        tooltipGameObject.SetActive(true);

        tooltipGameObject.GetComponent<RectTransform>().anchoredPosition = anchoredPos;
        Text tooltipUI = tooltipGameObject.transform.Find("Text").GetComponent<Text>();
        tooltipUI.text = tooltipText;
        float textPaddingSize = 4f;
        Vector2 backgroundSize = new Vector2(tooltipUI.preferredWidth + textPaddingSize * 2f, tooltipUI.preferredHeight + textPaddingSize * 2f);

        tooltipGameObject.transform.Find("Background").GetComponent<RectTransform>().sizeDelta = backgroundSize;

        tooltipGameObject.transform.SetAsLastSibling();
    }

    public static void HideTooltip_Static()
    {
        instance.HideTooltip();
    }

    private void HideTooltip()
    {
        tooltipGameObject.SetActive(false);
    }

    private void IncreaseVisibleAmount()
    {
        this.graphVisual.Reset();
        if (this.maxVisibleValueAmount + 1 <= this.valueList.Count) 
        {
            ShowGraph(this.valueList, this.graphVisual, this.maxVisibleValueAmount + 1, this.getAxisLabelX, this.getAxisLabelY);
        }
    }
    private void DecreaseVisibleAmount()
    {
        this.graphVisual.Reset();
        ShowGraph(this.valueList, this.graphVisual, this.maxVisibleValueAmount - 1, this.getAxisLabelX, this.getAxisLabelY);
    }
    private void SetGraphVisual(IGraphVisual graphVisual)
    {
        graphVisual.Reset();
        ShowGraph(this.valueList, graphVisual, this.maxVisibleValueAmount, this.getAxisLabelX, this.getAxisLabelY);
    }

    private void ShowGraph(List<int> valueList, IGraphVisual graphVisual, int maxVisibleValueAmount = -1, Func<int, string> getAxisLabelX = null, Func<float, string> getAxisLabelY = null)
    {
        this.valueList = valueList;
        this.graphVisual = graphVisual;
        this.getAxisLabelX = getAxisLabelX;
        this.getAxisLabelY = getAxisLabelY;
        if (getAxisLabelX == null) 
        {
            getAxisLabelX = delegate (int _i) { return _i.ToString(); };
        }
        if (getAxisLabelY == null)
        {
            getAxisLabelY = delegate (float _f) { return Mathf.RoundToInt(_f).ToString(); };
        }

        if (maxVisibleValueAmount <= 0)
        {
            maxVisibleValueAmount = valueList.Count;
        }
        this.maxVisibleValueAmount = maxVisibleValueAmount;

        foreach (GameObject gameObject in gameObjectList)
        {
            Destroy(gameObject);
        }
        gameObjectList.Clear();

        float graphHeight = graphContainer.sizeDelta.y;
        float graphWidth = graphContainer.sizeDelta.x;

        float yMax = 0f;

        for (int i = Mathf.Max(valueList.Count - maxVisibleValueAmount, 0); i < valueList.Count; i++) 
        {
            int value = valueList[i];
            if (value > yMax) 
            {
                yMax = value;
            }
        }

        yMax = yMax * 1.2f;

        float xSize = graphWidth / (maxVisibleValueAmount + 1);

        int xIndex = 0;

        for (int i = Mathf.Max(valueList.Count - maxVisibleValueAmount, 0); i < valueList.Count; i++)
        {
            float xPos = xSize + xIndex * xSize;
            float yPos = (valueList[i] / yMax) * graphHeight;

            string tooltipText = getAxisLabelY(valueList[i]);
            gameObjectList.AddRange(graphVisual.AddGraphVisual(new Vector2(xPos, yPos), xSize, tooltipText));

            RectTransform labelX = Instantiate(labelTemplateX);
            labelX.SetParent(graphContainer, false);
            labelX.gameObject.SetActive(true);
            labelX.anchoredPosition = new Vector2(xPos, -4f);
            labelX.GetComponent<Text>().text = getAxisLabelX(i + 1);
            gameObjectList.Add(labelX.gameObject);

            RectTransform dashX = Instantiate(dashTemplateX);
            dashX.SetParent(graphContainer, false);
            dashX.gameObject.SetActive(true);
            dashX.anchoredPosition = new Vector2(xPos, 0f);
            gameObjectList.Add(dashX.gameObject);

            xIndex++;
        }

        int seperatorCount = 10;
        for (int i = 0; i <= seperatorCount; i++)
        {
            RectTransform labelY = Instantiate(labelTemplateY);
            labelY.SetParent(graphContainer, false);
            labelY.gameObject.SetActive(true);
            float normalisedValue = i * 1f / seperatorCount;
            labelY.anchoredPosition = new Vector2(-14f, 9f + (normalisedValue * graphHeight));
            labelY.GetComponent<Text>().text = getAxisLabelY(normalisedValue * yMax);
            gameObjectList.Add(labelY.gameObject);

            if (i != 0 && i != seperatorCount) 
            {
                RectTransform dashY = Instantiate(dashTemplateY);
                dashY.SetParent(graphContainer, false);
                dashY.gameObject.SetActive(true);
                dashY.anchoredPosition = new Vector2(0f, normalisedValue * graphHeight);
                gameObjectList.Add(dashY.gameObject);
            }
        }
    }

    private interface IGraphVisual
    {
        List<GameObject> AddGraphVisual(Vector2 graphPos, float graphPosWidth, string tooltipText);
        void Reset();
    }

    private class BarChartVisual : IGraphVisual
    {
        private RectTransform graphContainer;
        private Color barColour;
        private float barWidthMultiplier;

        public BarChartVisual(RectTransform graphContainer,Color barColour,float barWidthMultiplier)
        {
            this.graphContainer = graphContainer;
            this.barColour = barColour;
            this.barWidthMultiplier = barWidthMultiplier;
        }

        public List<GameObject> AddGraphVisual(Vector2 graphPos, float graphPosWidth, string tooltipText)
        {
            GameObject barGameObject = CreateBar(graphPos, graphPosWidth);
            Button_UI barButton_UI = barGameObject.AddComponent<Button_UI>();
            barButton_UI.MouseOverOnceFunc += () => {
                ShowTooltip_Static(tooltipText, graphPos);
            };
            barButton_UI.MouseOutOnceFunc += () => {
                HideTooltip_Static();
            };
            return new List<GameObject> { barGameObject };
        }

        public void Reset(){ }

        private GameObject CreateBar(Vector2 graphPos, float barWidth)
        {
            GameObject gameObject = new GameObject("bar", typeof(Image));
            gameObject.transform.SetParent(graphContainer, false);
            gameObject.GetComponent<Image>().color = barColour;
            RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(graphPos.x, 0f);
            rectTransform.sizeDelta = new Vector2(barWidth * barWidthMultiplier, graphPos.y);
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(0, 0);
            rectTransform.pivot = new Vector2(0.5f, 0f);
            return gameObject;
        }
    }

    private class LineGraphVisual : IGraphVisual
    {
        private RectTransform graphContainer;
        private Sprite dotSprite;
        private GameObject lastDotGameObject;
        private Color dotColour, dotConnectionColour;

        public LineGraphVisual(RectTransform graphContainer, Sprite dotSprite, Color dotColour, Color dotConnectionColour)
        {
            this.graphContainer = graphContainer;
            this.dotSprite = dotSprite;
            this.dotColour = dotColour;
            this.dotConnectionColour = dotConnectionColour;
            lastDotGameObject = null;          
        }

        public List<GameObject> AddGraphVisual(Vector2 graphPos, float graphPosWidth, string tooltipText)
        {
            List<GameObject> gameObjectList = new List<GameObject>();
            GameObject dotGameObject = CreateDot(graphPos);

            Button_UI dotButtonUI = dotGameObject.AddComponent<Button_UI>();
            dotButtonUI.MouseOverOnceFunc += () => {
                ShowTooltip_Static(tooltipText, graphPos);
            };
            dotButtonUI.MouseOutOnceFunc += () => {
                HideTooltip_Static();
            };

            gameObjectList.Add(dotGameObject);
            if (lastDotGameObject != null)
            {
                GameObject dotConnectionGameObject = CreateDotConnection(lastDotGameObject.GetComponent<RectTransform>().anchoredPosition, dotGameObject.GetComponent<RectTransform>().anchoredPosition);
                gameObjectList.Add(dotConnectionGameObject);
            }
            lastDotGameObject = dotGameObject;
            return gameObjectList;
        }

        public void Reset()
        {
            lastDotGameObject = null;
        }

        private GameObject CreateDot(Vector2 anchoredPos)
        {
            GameObject gameObject = new GameObject("dot", typeof(Image));
            gameObject.transform.SetParent(graphContainer, false);
            gameObject.GetComponent<Image>().sprite = dotSprite;
            gameObject.GetComponent<Image>().color = dotColour;
            RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = anchoredPos;
            rectTransform.sizeDelta = new Vector2(11, 11);
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(0, 0);
            return gameObject;
        }

        private GameObject CreateDotConnection(Vector2 dotPosA, Vector2 dotPosB)
        {
            GameObject gameObject = new GameObject("dotConnection", typeof(Image));
            gameObject.transform.SetParent(graphContainer, false);
            gameObject.GetComponent<Image>().color = dotConnectionColour;
            RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
            Vector2 dir = (dotPosB - dotPosA).normalized;
            float dist = Vector2.Distance(dotPosA, dotPosB);
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(0, 0);
            rectTransform.sizeDelta = new Vector2(dist, 3f);
            rectTransform.anchoredPosition = dotPosA + dir * dist * 0.5f;
            rectTransform.localEulerAngles = new Vector3(0, 0, UtilsClass.GetAngleFromVectorFloat(dir));
            return gameObject;
        }
    }
}
