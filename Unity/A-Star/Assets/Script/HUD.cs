using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A HUD to show info on the tile under the mouse
/// </summary>
public class HUD : MonoBehaviour
{
    Text xText;
    Text yText;
    Text tText;
    Text gText;
    Text fText;

    // Start is called before the first frame update
    void Start()
    {
        xText = transform.Find("Panel/xText").GetComponent<Text>();
        yText = transform.Find("Panel/yText").GetComponent<Text>();
        tText = transform.Find("Panel/tText").GetComponent<Text>();
        gText = transform.Find("Panel/gText").GetComponent<Text>();
        fText = transform.Find("Panel/fText").GetComponent<Text>();
    }

    public void ShowNodeInfo(Node node)
    {
        if (node){
            xText.text = "x: " + node.X;
            yText.text = "y: " + node.Z;
            tText.text = "terrain: " + node.Terrain;
            gText.text = "g: " + node.G;
            fText.text = "f: " + node.F;
        }
        else
        {
            xText.text = "x: -";
            yText.text = "y: -";
            tText.text = "terrain: -";
            gText.text = "g: -";
            fText.text = "f: -";
        }
    }
}
