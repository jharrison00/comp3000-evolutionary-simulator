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
    private List<IGraphVisualObject> graphVisualObjectList;
    private GameObject tooltipGameObject;
    private List<RectTransform> yLabelList;

    // Cached values
    private List<int> valueList;
    private IGraphVisual graphVisual;
    private int maxVisibleValueAmount;
    private Func<int, string> getAxisLabelX;
    private Func<float, string> getAxisLabelY;
    private float xSize;

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
        graphVisualObjectList = new List<IGraphVisualObject>();
        yLabelList = new List<RectTransform>();

        List<int> valueList = new List<int>() { 50, 98, 32, 7, 12, 9, 33, 43, 64, 19, 99 };
        IGraphVisual lineGraphVisual = new LineGraphVisual(graphContainer, dotSprite, Color.green, new Color(1, 1, 1, 0.5f));
        IGraphVisual barChartVisual = new BarChartVisual(graphContainer, Color.white, 0.9f);

        //ShowGraph(valueList, (int _i) => "Year " + (_i + 1), (float _f) => "Population " + Mathf.RoundToInt(_f));

        ShowGraph(valueList, barChartVisual, -1);

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

        int value = 0;
        FunctionPeriodic.Create(() =>
        {
            UpdateValue(0, value);
            UpdateLastIndexValue(value + 5);
            value++;
        }, .1f);


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
        yLabelList.Clear();
        foreach (IGraphVisualObject graphVisualObject in graphVisualObjectList)
        {
            graphVisualObject.CleanUp();
        }
        graphVisualObjectList.Clear();

        float graphHeight = graphContainer.sizeDelta.y;
        float graphWidth = graphContainer.sizeDelta.x;

        float yMax = CalculateYScale();

        xSize = graphWidth / (maxVisibleValueAmount + 1);

        int xIndex = 0;

        for (int i = Mathf.Max(valueList.Count - maxVisibleValueAmount, 0); i < valueList.Count; i++)
        {
            float xPos = xSize + xIndex * xSize;
            float yPos = (valueList[i] / yMax) * graphHeight;

            string tooltipText = getAxisLabelY(valueList[i]);
            graphVisualObjectList.Add(graphVisual.CreateGraphVisualObject(new Vector2(xPos, yPos), xSize, tooltipText));

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
            yLabelList.Add(labelY);
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

    private void UpdateLastIndexValue(int value)
    {
        UpdateValue(valueList.Count - 1, value);
    }

    private void UpdateValue(int index,int value)
    {
        float yMaxBefore = CalculateYScale();

        valueList[index] = value;

        float graphWidth = graphContainer.sizeDelta.x;
        float graphHeight = graphContainer.sizeDelta.y;

        float yMax = CalculateYScale();
        bool yScaleChanged = yMaxBefore != yMax;

        if (!yScaleChanged)
        {
            float xPos = xSize + index * xSize;
            float yPos = (valueList[index] / yMax) * graphHeight;

            string tooltipText = valueList[index].ToString();
            graphVisualObjectList[index].SetGraphVisualObjectInfo(new Vector2(xPos, yPos), xSize, tooltipText);
        }
        else
        {
            int xIndex = 0;
            for (int i = Mathf.Max(valueList.Count - maxVisibleValueAmount, 0); i < valueList.Count; i++)
            {
                float xPos = xSize + xIndex * xSize;
                float yPos = (valueList[i] / yMax) * graphHeight;

                string tooltipText = valueList[i].ToString();
                graphVisualObjectList[xIndex].SetGraphVisualObjectInfo(new Vector2(xPos, yPos), xSize, tooltipText);

                xIndex++;
            }
            for (int i = 0; i < yLabelList.Count; i++)
            {
                float normalisedValue = i * 1f / yLabelList.Count;
                yLabelList[i].GetComponent<Text>().text = Mathf.RoundToInt(normalisedValue * yMax).ToString();
            }
        }
    }

    private float CalculateYScale()
    {
        float yMax = valueList[0];

        for (int i = Mathf.Max(valueList.Count - maxVisibleValueAmount, 0); i < valueList.Count; i++)
        {
            int value = valueList[i];
            if (value > yMax)
            {
                yMax = value;
            }
        }

        yMax = yMax * 1.2f;

        return yMax;
    }

    private interface IGraphVisual
    {
        IGraphVisualObject CreateGraphVisualObject(Vector2 graphPos, float graphPosWidth, string tooltipText);
        void Reset();
    }

    private interface IGraphVisualObject
    {
        void SetGraphVisualObjectInfo(Vector2 graphPos, float graphPosWidth, string tooltipText);
        void CleanUp();
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

        public IGraphVisualObject CreateGraphVisualObject(Vector2 graphPos, float graphPosWidth, string tooltipText)
        {
            GameObject barGameObject = CreateBar(graphPos, graphPosWidth);

            BarChartVisualObject barChartVisualObject = new BarChartVisualObject(barGameObject, barWidthMultiplier);
            barChartVisualObject.SetGraphVisualObjectInfo(graphPos, graphPosWidth, tooltipText);

            return barChartVisualObject;
        }

        public void Reset(){}

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

            Button_UI barButton_UI = gameObject.AddComponent<Button_UI>();

            return gameObject;
        }

        public class BarChartVisualObject : IGraphVisualObject
        {
            private GameObject barGameObject;
            private float barWidthMultiplier;

            public BarChartVisualObject(GameObject barGameObject, float barWidthMultiplier)
            {
                this.barGameObject = barGameObject;
                this.barWidthMultiplier = barWidthMultiplier;
            }

            public void SetGraphVisualObjectInfo(Vector2 graphPos, float graphPosWidth, string tooltipText)
            {
                RectTransform rectTransform = barGameObject.GetComponent<RectTransform>();
                rectTransform.anchoredPosition = new Vector2(graphPos.x, 0f);
                rectTransform.sizeDelta = new Vector2(graphPosWidth * barWidthMultiplier, graphPos.y);

                Button_UI barButtonUI = barGameObject.GetComponent<Button_UI>();
                barButtonUI.MouseOverOnceFunc = () => {
                    ShowTooltip_Static(tooltipText, graphPos);
                };
                barButtonUI.MouseOutOnceFunc = () => {
                    HideTooltip_Static();
                };
            }

            public void CleanUp()
            {
                Destroy(barGameObject);
            }
        }
    }

    private class LineGraphVisual : IGraphVisual
    {
        private RectTransform graphContainer;
        private Sprite dotSprite;
        private LineGraphVisualObject lastLineGraphVisualObject;
        private Color dotColour, dotConnectionColour;

        public LineGraphVisual(RectTransform graphContainer, Sprite dotSprite, Color dotColour, Color dotConnectionColour)
        {
            this.graphContainer = graphContainer;
            this.dotSprite = dotSprite;
            this.dotColour = dotColour;
            this.dotConnectionColour = dotConnectionColour;
            lastLineGraphVisualObject = null;          
        }

        public IGraphVisualObject CreateGraphVisualObject(Vector2 graphPos, float graphPosWidth, string tooltipText)
        {
            GameObject dotGameObject = CreateDot(graphPos);

            GameObject dotConnectionGameObject = null;
            if (lastLineGraphVisualObject != null)
            {
                dotConnectionGameObject = CreateDotConnection(lastLineGraphVisualObject.GetGraphPosition(), dotGameObject.GetComponent<RectTransform>().anchoredPosition);
            }

            LineGraphVisualObject lineGraphVisualObject = new LineGraphVisualObject(dotGameObject, dotConnectionGameObject, lastLineGraphVisualObject);
            lineGraphVisualObject.SetGraphVisualObjectInfo(graphPos, graphPosWidth, tooltipText);

            lastLineGraphVisualObject = lineGraphVisualObject;

            return lineGraphVisualObject;
        }

        public void Reset()
        {
            lastLineGraphVisualObject = null;
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

            Button_UI dotButtonUI = gameObject.AddComponent<Button_UI>();

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

        public class LineGraphVisualObject : IGraphVisualObject 
        {
            public event EventHandler OnChangedGraphVisualObjectInfo;
            private GameObject dotGameObject;
            private GameObject dotConnectionGameObject;
            private LineGraphVisualObject lastVisualObject;

            public LineGraphVisualObject(GameObject dotGameObject, GameObject dotConnectionGameObject, LineGraphVisualObject lastVisualObject)
            {
                this.dotGameObject = dotGameObject;
                this.dotConnectionGameObject = dotConnectionGameObject;
                this.lastVisualObject = lastVisualObject;

                if (lastVisualObject != null)
                {
                    lastVisualObject.OnChangedGraphVisualObjectInfo += LastVisualObject_OnChangedGraphVisualObjectInfo;
                }
            }

            private void LastVisualObject_OnChangedGraphVisualObjectInfo(object sender, EventArgs e)
            {
                UpdateDotConnection();
            }

            public void SetGraphVisualObjectInfo(Vector2 graphPos, float graphPosWidth, string tooltipText)
            {
                RectTransform rectTransform = dotGameObject.GetComponent<RectTransform>();
                rectTransform.anchoredPosition = GetGraphPosition();

                UpdateDotConnection();

                Button_UI dotButtonUI = dotGameObject.GetComponent<Button_UI>();
                dotButtonUI.MouseOverOnceFunc += () => {
                    ShowTooltip_Static(tooltipText, graphPos);
                };
                dotButtonUI.MouseOutOnceFunc += () => {
                    HideTooltip_Static();
                };

                if (OnChangedGraphVisualObjectInfo != null) OnChangedGraphVisualObjectInfo(this, EventArgs.Empty);
                
            }

            public void CleanUp()
            {
                Destroy(dotGameObject);
                Destroy(dotConnectionGameObject);
            }

            public Vector2 GetGraphPosition()
            {
                RectTransform rectTransform = dotGameObject.GetComponent<RectTransform>();
                return rectTransform.anchoredPosition;
            }

            private void UpdateDotConnection() 
            {
                if (dotConnectionGameObject != null)
                {
                    RectTransform dotConnectionRectTransform = dotConnectionGameObject.GetComponent<RectTransform>();
                    Vector2 dir = (lastVisualObject.GetGraphPosition() - GetGraphPosition()).normalized;
                    float dist = Vector2.Distance(GetGraphPosition(), lastVisualObject.GetGraphPosition());
                    dotConnectionRectTransform.sizeDelta = new Vector2(dist, 3f);
                    dotConnectionRectTransform.anchoredPosition = GetGraphPosition() + dir * dist * 0.5f;
                    dotConnectionRectTransform.localEulerAngles = new Vector3(0, 0, UtilsClass.GetAngleFromVectorFloat(dir));
                }
            }
        }
    }
}
