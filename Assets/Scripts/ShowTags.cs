using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;

public class ShowTags : MonoBehaviour, IInputClickHandler
{
    private List<Vector3> tagPositions = new List<Vector3>();
    private List<Quaternion> tagRotations = new List<Quaternion>();

    private GameObject Bag;
    private bool created = false;
    private bool visible = false;

    public List<GameObject> children;
    public int paintingID;
    public GameObject checkBar;
    public GameObject LeftInstantiatedCheckBar, RightInstantiatedCheckBar;
    public Dictionary<int, PieceOfArt> tagsInPainting = new Dictionary<int, PieceOfArt>(); // from ScanManager.cs

    // Use this for initialization
    void Start()
    {
        Bag = GameObject.FindWithTag("Bag");
        for (int i = 0; i < 2; i++)
        {
            children.Add(transform.GetChild(i).gameObject);
        }
    }

    public void OnInputClicked(InputClickedEventData eventData)
    {
        if (GlobalVariables.pieceToTranspose == null)
        {
            if (created == false)
            {
                created = true; // I instantiated the Paintings                
                visible = true; // I showed the paintings

                int i = 0;
                foreach (var item in tagsInPainting)
                {
                    children[i].transform.rotation = tagRotations[i];
                    children[i].transform.position = tagPositions[i];

                    Vector3 pos = children[i].transform.position;
                    pos = pos + children[i].transform.TransformDirection(new Vector3(0, 0, 0.1f));
                    children[i].transform.position = pos;

                    children[i].GetComponent<TagInPainting>().key = item.Key;
                    children[i].GetComponent<TagInPainting>().piece = item.Value;
                    children[i].GetComponent<TagInPainting>().ShowInPainting();

                    i++;
                }
                
            }
            else
            {
                if (visible == true)
                {
                    // If the paintings are visible, hide them
                    ToogleVisibilities(visible);
                    visible = false;
                }
                else
                {
                    // If the paintings are hidden, show them
                    ToogleVisibilities(visible);
                    visible = true;
                }
            }
        }
        else
        {
            TagsBackInPaintings();
        }

    }

    public void PositionTag(int index, Vector3 tagPosition, Quaternion tagRotation)
    {
        tagRotations.Add(tagRotation);
        tagPositions.Add(tagPosition);
    }

    public void InstantiateTag(int child, int id, PieceOfArt piece)
    {
        children[child].GetComponent<TagInPainting>().key = id;
        children[child].GetComponent<TagInPainting>().piece = piece;
        children[child].GetComponent<TagInPainting>().ShowInPainting();
    }

    private void ToogleVisibilities(bool v)
    {
        foreach(GameObject child in children){
            child.GetComponent<TagInPainting>().ToogleVisibility(!v);
        }
    }

    public void UpdateGlobals()
    {
        GlobalVariables.pieceToTranspose = null;
        GlobalVariables.presentKey = -1;       
    }

    public void TagsBackInPaintings()
    {
        if (children[0].GetComponent<TagInPainting>().instantiatedTag == null)
        {
            InstantiateTag(0, GlobalVariables.presentKey, GlobalVariables.pieceToTranspose);
            UpdateGlobals();
        }
        else
        {
            if (children[1].GetComponent<TagInPainting>().instantiatedTag == null)
            {
                InstantiateTag(1, GlobalVariables.presentKey, GlobalVariables.pieceToTranspose);
                UpdateGlobals();
            }
        }
    }

    public void InstantiateBars(){
        //
        // CheckBar Position and Size
        //
        float barXPos, barZPos;
        Vector3 rendererSize, leftPos, rightPos;        
        checkBar.transform.rotation = transform.rotation;
        checkBar.transform.Rotate(90, 0, 0);

        // CheckBar Position
        rendererSize = gameObject.GetComponent<BoxCollider>().size;
        barZPos = -rendererSize.y * 0.5f - 0.1f;
        barXPos = -rendererSize.x * 0.25f;
        checkBar.transform.position = transform.position + checkBar.transform.TransformDirection(new Vector3(barXPos, 0, barZPos));

        rightPos = checkBar.transform.position;
        leftPos = rightPos + checkBar.transform.TransformDirection(new Vector3(rendererSize.x * 0.5f, 0, 0));
        
        RightInstantiatedCheckBar = Instantiate(checkBar, rightPos, checkBar.transform.rotation);
        LeftInstantiatedCheckBar = Instantiate(checkBar, leftPos, checkBar.transform.rotation);
    }
}    
