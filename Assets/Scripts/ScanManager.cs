// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System;
using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;
using HoloToolkit.Unity.SpatialMapping;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;


public class ScanManager : MonoBehaviour, ISpeechHandler
{

    public TextMesh InstructionTextMesh;
    public GameObject progressBar;

    // Values to be filles from Unity Editor:
    public List<GameObject> Paintings;
    public List<TextMesh> paintingCaptions;

    public List<GameObject> Tags;
    public List<TextMesh> tagCaptions;

    [SerializeField]
    public List<int> Keys;
    [SerializeField]
    public Material Transparent;

    //    
    public List<GameObject> paintingChildren;
    private List<GameObject> allPaintings = new List<GameObject>();   

    private GameObject SpatialUnder;
    private SpatialUnderstandingCustomMesh customMeshScript;
    
    private GameObject bag;
    private bool hidden = true;


    // Use this for initialization
    void Start()
    {
        InputManager.Instance.PushFallbackInputHandler(this.gameObject);
        SpatialUnderstanding.Instance.RequestBeginScanning();
        SpatialUnderstanding.Instance.ScanStateChanged += ScanStateChanged;

        SpatialUnder = GameObject.FindWithTag("Understanding");
        customMeshScript = SpatialUnder.GetComponent<SpatialUnderstandingCustomMesh>();

        bag = GameObject.FindWithTag("Bag");

        progressBar.SetActive(false);
        gameObject.GetComponent<GamingManager>().enabled = false;

        for (int i = 0; i < 3; i++)
        {
            paintingChildren.Add(transform.GetChild(i).gameObject);
        }

    }

    // SPEECH // 

    public void OnSpeechKeywordRecognized(SpeechEventData eventData)
    {
        if (eventData.RecognizedText == "Stop")
        {
            StopScan(eventData.RecognizedText);
        }
        if (eventData.RecognizedText == "Show")
        {
            ShowBag(eventData.RecognizedText);
        }
        if (eventData.RecognizedText == "Hide")
        {
            HideBag(eventData.RecognizedText);
        }
    }

    public void StopScan(string command)
    {
        SpatialUnderstanding.Instance.RequestFinishScan();
    }

    public void ShowBag(string command)
    {
        if(hidden == true)
        {

            bag.GetComponent<Billboard>().enabled = false;
            bag.GetComponent<Tagalong>().enabled = false;
            bag.GetComponent<BoxCollider>().enabled = false;

            bag.GetComponent<TagsMenu>().ShowBag();      
            hidden = false;

        }

    }

    public void HideBag(string command)
    {
        if (hidden == false)
        {

            bag.GetComponent<Billboard>().enabled = true;
            bag.GetComponent<Tagalong>().enabled = true;
            bag.GetComponent<BoxCollider>().enabled = true;
            
            bag.GetComponent<TagsMenu>().HideBag();
            hidden = true;

        }
    }

    private void ScanStateChanged()
    {
        if (SpatialUnderstanding.Instance.ScanState == SpatialUnderstanding.ScanStates.Scanning)
        {
            LogSurfaceState();
        }
        else if (SpatialUnderstanding.Instance.ScanState == SpatialUnderstanding.ScanStates.Done)
        {
            customMeshScript.MeshMaterial = Transparent;
            InstantiatePaintingsOnWalls();
        }
    }

    //private void OnDestroy()
    //{
    //    SpatialUnderstanding.Instance.ScanStateChanged -= ScanStateChanged;
    //}

    // Update is called once per frame
    void Update()
    {
        switch (SpatialUnderstanding.Instance.ScanState)
        {
            case SpatialUnderstanding.ScanStates.None:
            case SpatialUnderstanding.ScanStates.ReadyToScan:
                break;
            case SpatialUnderstanding.ScanStates.Scanning:
                this.LogSurfaceState();
                break;
            case SpatialUnderstanding.ScanStates.Finishing:
                this.InstructionTextMesh.text = "State: Finishing Scan";
                break;
            case SpatialUnderstanding.ScanStates.Done:
                this.InstructionTextMesh.text = null;
                break;
            default:
                break;
        }
    }

    public void RestartGame(string command)
    {
        SceneManager.LoadScene("SpatialDemoScene");
    }

    private void LogSurfaceState()
    {
        IntPtr statsPtr = SpatialUnderstanding.Instance.UnderstandingDLL.GetStaticPlayspaceStatsPtr();
        if (SpatialUnderstandingDll.Imports.QueryPlayspaceStats(statsPtr) != 0)
        {
            var stats = SpatialUnderstanding.Instance.UnderstandingDLL.GetStaticPlayspaceStats();
            this.InstructionTextMesh.text = string.Format("TotalSurfaceArea: {0:0.##}\nWallSurfaceArea: {1:0.##}\nHorizSurfaceArea: {2:0.##}", stats.TotalSurfaceArea, stats.WallSurfaceArea, stats.HorizSurfaceArea);
        }
    }


    private void InstantiatePaintingsOnWalls()
    {
        // Shuffle the Tags List

        // Assign Tags to Paintings
        AssignTagsToPaintings(Keys, Tags, tagCaptions);

        //
        //  ALGORITHM FOR POSITIONING 
        //
        // Add every gameobject that I want to show in a list
        GeneralLists();

        const int QueryResultMaxCount = 128;
        SpatialUnderstandingDllTopology.TopologyResult[] _resultsTopology = new SpatialUnderstandingDllTopology.TopologyResult[QueryResultMaxCount];
        var minHeightOfWallSpace = 0.5f;
        var minWidthOfWallSpace = 0.5f;
        var minHeightAboveFloor = 1.0f;
        var minFacingClearance = 0.2f;

        var resultsTopologyPtr = SpatialUnderstanding.Instance.UnderstandingDLL.PinObject(_resultsTopology);
        var locationCount = SpatialUnderstandingDllTopology.QueryTopology_FindPositionsOnWalls(minHeightOfWallSpace, minWidthOfWallSpace, minHeightAboveFloor, minFacingClearance, _resultsTopology.Length, resultsTopologyPtr);

        int lastIndex = 0;

        if (locationCount > 0)
        {

            //
            // Finding Position and rotation of Paintings
            //

            GlobalVariables.objPositions.Add(_resultsTopology[0].position);// first painting
            GlobalVariables.objRotations.Add(Quaternion.LookRotation(_resultsTopology[0].normal, Vector3.up));

            // last topology index is zero
            int paintCounter = 0;
            // for the rest paintings
            for(int i = 1; i < allPaintings.Count; i++)
            {
                // Checking every topology location
                for(int j = lastIndex + 1; j < locationCount; j++)
                {
                    // Check if this GameObject is too close with with the previous one 
                    if(IsDistanceBetween(i, _resultsTopology[j].position))
                    {
                        // Assign positions
                        Vector3 pos = new Vector3(_resultsTopology[j].position.x, _resultsTopology[j].position.y, _resultsTopology[j].position.z);
                        Quaternion rot = Quaternion.LookRotation(_resultsTopology[j].normal, Vector3.up);
                        
                        GlobalVariables.objPositions.Add(pos);
                        GlobalVariables.objRotations.Add(rot);// first painting

                        lastIndex = j; 
                        break;
                    }                    
                }
               
            }

            for(int z = 0; z < allPaintings.Count; z++)
            {
                // Instantiate PAINTINGS
                if ((z - 1) % 3 == 0)
                {

                    paintingChildren[paintCounter].transform.position = GlobalVariables.objPositions[z];
                    paintingChildren[paintCounter].transform.rotation = GlobalVariables.objRotations[z];
                    InstantiatePaintingWithCaption(paintingChildren[paintCounter], paintingCaptions[paintCounter], Paintings[paintCounter]);

                    paintCounter++;
                }
                else
                {
                    // Position TAGS
                    if (z % 3 == 0)
                    {
                        paintingChildren[z / 3].GetComponent<ShowTags>().PositionTag(0, GlobalVariables.objPositions[z], GlobalVariables.objRotations[z]);        
                    }
                    else
                    {
                        paintingChildren[z / 3].GetComponent<ShowTags>().PositionTag(1, GlobalVariables.objPositions[z], GlobalVariables.objRotations[z]);
                    }

                }
            }
            
            // Position the bag and its children
            bag.transform.position = new Vector3(0, 0, 1.0f);
            bag.transform.rotation = Quaternion.LookRotation(Vector3.forward);

            bag.GetComponent<TagsMenu>().PositionBag();

            // Instantiate Bars now that I have the Sizes of the painting objects (or box colliders)
            foreach(GameObject child in paintingChildren)
            {
                child.GetComponent<ShowTags>().InstantiateBars();
            }

            progressBar.SetActive(true);
            gameObject.GetComponent<GamingManager>().enabled = true;

        }
        else
        {
            this.InstructionTextMesh.text = "I can't find enough space to place the painting.";
        }

    }

    private void GeneralLists()
    {

        int sizeOfList = Paintings.Count + Tags.Count;
        int i = 0, j = 0;
        foreach (GameObject obj in paintingChildren)
        {
            allPaintings.Add(Tags[i]);
            allPaintings.Add(Paintings[j]);
            allPaintings.Add(Tags[i + 1]);

            i += 2;
            j++;
        }
    }

    private void AssignTagsToPaintings(List<int> keys, List<GameObject> objs, List<TextMesh> caps)
    {
        int j = 0;
        foreach (GameObject item in paintingChildren)
        {
            PieceOfArt piece;
            int k = 0;
            for (int i = 2 * j; i < 2 * j + 2; i++)
            {
                piece = new PieceOfArt(objs[i], caps[i]);
                //
                //
                item.GetComponent<ShowTags>().tagsInPainting.Add(keys[i], piece);
                item.transform.GetChild(k).GetComponent<TagInPainting>().key = keys[i];
                item.transform.GetChild(k).GetComponent<TagInPainting>().piece = piece;
                k++;
                //
                //
            }

            j++;
        }
    }

    private bool IsDistanceBetween(int index, Vector3 posCurrent)
    {

        Vector3 posPrevious = GlobalVariables.objPositions[index - 1];

        float lengthPrevious = allPaintings[index - 1].GetComponent<SpriteRenderer>().bounds.size.x;
        float lengthCurrent = allPaintings[index].GetComponent<SpriteRenderer>().bounds.size.x;

        float desiredDistance = lengthCurrent*0.5f + lengthPrevious*0.5f;

        float dis = Vector3.Distance(posCurrent, posPrevious);

        if (dis > desiredDistance && Vector3.Distance(posCurrent, GlobalVariables.objPositions[0]) > desiredDistance)
        {
            return true;
        }

        return false;
    }

    private void InstantiatePaintingWithCaption(GameObject paint, TextMesh cap, GameObject art)
    {
        GameObject instantiatedPainting;
        Vector3 rendererSize;
        float captionYPos, captionXPos;

        // Painting attributes
   
        paint.transform.position = paint.transform.position + paint.transform.TransformDirection(new Vector3(0, 0, 0.05f));

        // Add and resize BoxCollider 
        paint.AddComponent<BoxCollider>();
        BoxCollider box = paint.GetComponent<BoxCollider>();
        SpriteRenderer sprite = art.GetComponent<SpriteRenderer>();
        box.size = new Vector3(sprite.bounds.size.x, sprite.bounds.size.y, 0.01f);

        // Instantiate Paintings
        instantiatedPainting = Instantiate(art, paint.transform.position, paint.transform.rotation);
        
        rendererSize = instantiatedPainting.GetComponent<Renderer>().bounds.size; // Size of Instantiated Object using Renderer Component

        // Caption Position                      
        captionYPos = -rendererSize.y * 0.5f - 0.05f; // On y axis  get relative position to the box collider size 
        captionXPos = rendererSize.x * 0.5f;
        cap.transform.position = paint.transform.position + paint.transform.TransformDirection(new Vector3(captionXPos, captionYPos, 0));

        // Caption Rotation
        Vector3 rotEuler;
        cap.transform.rotation = paint.transform.rotation;
        rotEuler = cap.transform.rotation.eulerAngles;
        rotEuler = new Vector3(rotEuler.x, rotEuler.y + 180, rotEuler.z);
        cap.transform.rotation = Quaternion.Euler(rotEuler);

        // Instantiate Captions
        Instantiate(cap, cap.transform.position, cap.transform.rotation);


    }

    
}


