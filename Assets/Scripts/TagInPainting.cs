using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;

public class TagInPainting : MonoBehaviour, IInputClickHandler
{
    public int key; // from ShowTags.cs
    public PieceOfArt piece; // from ShowTags.cs
    public Vector3 position;
    public GameObject instantiatedTag;
    private TextMesh instantiatedTagCaption; 
    private GameObject bag;
    private GameObject mapping;

    private void Start() {
        bag = GameObject.FindWithTag("Bag");
        mapping = GameObject.FindWithTag("Mapping");
    }

    public void OnInputClicked(InputClickedEventData eventData)
    {   
        // If tag isn't in the Bag 
        if (bag.GetComponent<TagsMenu>().IsTagInBag(key) == false)
        {        
            bag.GetComponent<TagsMenu>().AddTags(key, piece);       
            RemoveTagFromPainting();
        }
    }

    public void ShowInPainting()
    {
        Vector3 tagRendererSize;
        float captionYPos, captionXPos;
        Quaternion rot = transform.rotation;

        // Add and resize BoxCollider 
        gameObject.AddComponent<BoxCollider>();
        BoxCollider box = gameObject.GetComponent<BoxCollider>();
        SpriteRenderer sprite = piece.pieceOfArt.GetComponent<SpriteRenderer>();
        box.size = new Vector3(sprite.bounds.size.x, sprite.bounds.size.y, 0.01f);

        // Instantiate Paintings
        instantiatedTag = Instantiate(piece.pieceOfArt, transform.position, rot);

        // Caption Position                      
        tagRendererSize = instantiatedTag.GetComponent<Renderer>().bounds.size; // Size of Instantiated Object using Renderer Component
        captionYPos = -tagRendererSize.y * 0.5f - 0.05f; // On y axis  get relative position to the box collider size 
        captionXPos = tagRendererSize.x * 0.5f;
        piece.caption.transform.position = transform.position + transform.TransformDirection(new Vector3(captionXPos, captionYPos, 0));

        // Caption Rotation
        piece.caption.transform.rotation = rot;
        piece.caption.transform.Rotate(0, 180, 0);

        // Instantiate Caption
        instantiatedTagCaption = Instantiate(piece.caption, piece.caption.transform.position, piece.caption.transform.rotation);  
    }

    public void ToogleVisibility(bool visibility)
    {
        if(instantiatedTag != null){
            gameObject.SetActive(visibility);
            instantiatedTag.GetComponent<Renderer>().enabled = visibility;
            instantiatedTagCaption.GetComponent<Renderer>().enabled = visibility;
        }
    }

    private void RemoveTagFromPainting(){
        gameObject.GetComponentInParent<ShowTags>().tagsInPainting.Remove(key);       
        piece = null;
        key = -1;
        Destroy(instantiatedTag);
        Destroy(GetComponent<BoxCollider>());
        Destroy(instantiatedTagCaption);
    }   
}