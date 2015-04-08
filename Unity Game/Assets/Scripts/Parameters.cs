using UnityEngine;
using UnityEngine.UI;

using System;
using System.Collections;

public class Parameters : MonoBehaviour {

    public InputField nodesField;
    public Slider minDegreeSlider;
    public Slider maxDegreeSlider;
    public Toggle maxDegreeInf;
    public Slider crossingsSlider;

    public Text degreeEcho;
    public Text crossingsEcho;

    // Parameters
    private float bound;
    private float aspectRatio = 2f;
    private float spread = 0;

    // Singleton object
    private static Parameters instance;

    /*
     * IMPORTANT METHODS
     */

    // calls defaults
    void Start()
    {
        SetNumNodes(100);
        SetMinDegree(3);
        SetMaxDegree(5);
        SetMaxCrossings(0);

        // Always always always do the following
        Parameters.instance = this;
        CalculateBounds();
    }


    private void CalculateBounds()
    {
        bound = (2*GetNodeMoat() + 1) * Mathf.Sqrt((2f + spread) * GetNumNodes());
    }


    /*
     * SETTERS
     */

    public void SetNumNodes(int n)
    {
        // correct too low value
        if (n < 3)
        {
            n = 3;
            nodesField.text = n.ToString();
        }

        // adjust sliders maximum
        minDegreeSlider.maxValue = n - 1;
        maxDegreeSlider.maxValue = n - 1;

        // disable unnecesary crossings
        if (n < 4)
        {
            crossingsSlider.gameObject.SetActive(false);
        }
        else
        {
            crossingsSlider.gameObject.SetActive(true);
            crossingsSlider.maxValue = (n - 2) / 2;
        }

        // update text field
        nodesField.text = Convert.ToString(n);

        // Update bounds
        CalculateBounds();
    }

    public void SetMinDegree(int x)
    {
        // change label
        String maxEcho = degreeEcho.text.Substring(degreeEcho.text.IndexOf("-"));
        degreeEcho.text = Convert.ToString(x) + maxEcho;

        // update sliders
        minDegreeSlider.value = x;
        maxDegreeSlider.minValue = x;
    }

    public void SetMaxDegree(int x)
    {
        // change label
        String minEcho = degreeEcho.text.Substring(0, degreeEcho.text.IndexOf("-") + 1);

        // if -1, infinite
        if (x == -1)
        {
            degreeEcho.text = minEcho + "inf";
            maxDegreeSlider.enabled = false;
            maxDegreeInf.isOn = true;
        }
        else
        {
            x = Mathf.Max(x, Convert.ToInt32(minDegreeSlider.value));
            degreeEcho.text = minEcho + Convert.ToString(x);
            maxDegreeSlider.value = x;
            maxDegreeSlider.enabled = true;
            maxDegreeInf.isOn = false;
        }
    }

    public void SetMaxCrossings(int x)
    {
        crossingsEcho.text = Convert.ToString(x);
        crossingsSlider.value = x;
    }

    /*
     * GETTERS
     */

    public int GetNumNodes()
    {
        return Convert.ToInt32(nodesField.text);
    }

    public int GetMinDegree()
    {
        return Convert.ToInt32(minDegreeSlider.value);
    }


    public int GetMaxDegree()
    {
        return maxDegreeInf.isOn ? GetNumNodes() - 1 : Convert.ToInt32(maxDegreeSlider.value);
    }

    public int GetMaxCrosses()
    {
        return Convert.ToInt32(crossingsSlider.value);
    }

    public float GetNodeRadius()
    {
        return Mathf.Min(GetxBound(), GetyBound()) * .3f;
    }

    public int GetNodeMoat()
    {
        return 2;
    }

    public int GetxBound()
    {
        return Mathf.CeilToInt(bound * Mathf.Sqrt(aspectRatio));
    }

    public int GetyBound()
    {
        return Mathf.CeilToInt(bound / Mathf.Sqrt(aspectRatio));
    }

    /*
     * CALLED BY BUTTONS AND SLIDERS
     */

    public void ChangeNodes()
    {
        SetNumNodes(Convert.ToInt32(nodesField.text));
    }

    public void ChangeMinDegree()
    {
        SetMinDegree(Convert.ToInt32(minDegreeSlider.value));
    }

    public void ChangeMaxDegree()
    {
        SetMaxDegree(Convert.ToInt32(maxDegreeSlider.value));
    }

    public void ChangeMaxInf()
    {
        SetMaxDegree(maxDegreeInf.isOn ? -1 : Convert.ToInt32(maxDegreeSlider.value));
    }

    public void ChangeCrossings()
    {
        SetMaxCrossings(Convert.ToInt32(crossingsSlider.value));
    }

    public void Open()
    {
        gameObject.SetActive(true);
    }


    public void Close()
    {
        gameObject.SetActive(false);
    }

    // Singleton method
    public static Parameters Get()
    {
        return instance;
    }
}
