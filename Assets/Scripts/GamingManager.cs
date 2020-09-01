using System.Collections;
using System.Collections.Generic;
using HoloToolkit.Unity;
using HoloToolkit.Examples.InteractiveElements;
using UnityEngine;

public class GamingManager : MonoBehaviour
{
    public List<GameObject> paintings;

    public TextMesh ResultText;
    public GameObject progressBar;

    [SerializeField]
    public List<int> correctKeys;

    private List<List<int>> keysDictionary = new List<List<int>>();
    private bool flag = false;
    private int counter;

    private void Start()
    {

        for (int i = 0; i < 3; i++)
        {
            paintings.Add(this.gameObject.transform.GetChild(i).gameObject);
        }

        // Fill the dictionary with the correct keys per exhibit
        int z = 0;
        for (int i = 0; i < 3; i++)
        {
            List<int> subList = new List<int>();
            for (int j = 0; j < 2; j++)
            {
                subList.Add(correctKeys[z]);
                z++;
            }
            keysDictionary.Add(subList);
        }

        ResultText.text = null;
    }

    // Update is called once per frame

    private void Update()
    {         
        switch(flag)
        {         
            case false:
                CheckResult();
                break;
            case true:
                InstantiateWin();
                break;
            default:
                break;
        }      

    }

    private bool CheckResult()
    {
        counter = 0;       

        for (int i = 0; i < 3; i++)
        {
            TagInPainting tag1 = paintings[i].transform.GetChild(0).GetComponent<TagInPainting>();
            if(tag1.piece == null)
            {
                paintings[i].GetComponent<ShowTags>().RightInstantiatedCheckBar.GetComponent<Renderer>().material.SetColor("_Color", Color.gray);
            }
            else if (tag1.key == keysDictionary[i][0] || tag1.key == keysDictionary[i][1])
            {
                paintings[i].GetComponent<ShowTags>().RightInstantiatedCheckBar.GetComponent<Renderer>().material.SetColor("_Color", Color.green);
                counter++;
            }
            else 
            {
                paintings[i].GetComponent<ShowTags>().RightInstantiatedCheckBar.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
            }

            TagInPainting tag2 = paintings[i].transform.GetChild(1).GetComponent<TagInPainting>();
            if (tag2.piece == null)
            {
                paintings[i].GetComponent<ShowTags>().LeftInstantiatedCheckBar.GetComponent<Renderer>().material.SetColor("_Color", Color.gray);
            }
            else if (tag2.key == keysDictionary[i][0] || tag2.key == keysDictionary[i][1])
            {
                paintings[i].GetComponent<ShowTags>().LeftInstantiatedCheckBar.GetComponent<Renderer>().material.SetColor("_Color", Color.green);
                counter++;
            }
            else
            {
                paintings[i].GetComponent<ShowTags>().LeftInstantiatedCheckBar.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
            }
        }

        progressBar.GetComponent<SliderGestureControl>().SetSliderValue(16.67f * counter);

        if (counter == 6)
        {
            flag = true;
            return true;
        }

        return false;

    }

    private void InstantiateWin(){
        progressBar.SetActive(false);
        ResultText.text = "YOU WON !";        
    }

}
