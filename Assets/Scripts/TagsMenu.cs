using System;
using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;
using UnityEngine;
using System.Collections.Generic;

public class TagsMenu : MonoBehaviour
{
    public Dictionary<int, PieceOfArt> tagsInBagList = new Dictionary<int, PieceOfArt>();
    public List<GameObject> children;
    
    private void Start()
    {
        for (int i = 0; i < 6; i++)
        {
            children.Add(transform.GetChild(i).gameObject);
        }   
    }

    // Tags
    public void AddTags(int key, PieceOfArt piece)
    {
        int i = 0;
        foreach(GameObject item in children)
        {
            i++;
            if(item.GetComponent<TagInBag>().piece == null)
            {
                item.GetComponent<TagInBag>().key = key;
                item.GetComponent<TagInBag>().piece = piece;
                tagsInBagList.Add(key, piece);
                break;
            }
        }       
    }
    
    public void PositionBag()
    {
        foreach(GameObject child in children){
            child.transform.rotation = gameObject.transform.rotation;
        }
        children[0].transform.position = transform.position + transform.TransformDirection(new Vector3(-0.3f, 0, 0.1f));
        children[1].transform.position = transform.position + transform.TransformDirection(new Vector3(-0.3f, 0.4f, 0.1f));
        children[2].transform.position = transform.position + transform.TransformDirection(new Vector3(0.3f, 0.4f, 0.1f));
        children[3].transform.position = transform.position + transform.TransformDirection(new Vector3(0.3f, 0, 0.1f));
        children[4].transform.position = transform.position + transform.TransformDirection(new Vector3(0.3f, -0.4f, 0.1f));
        children[5].transform.position = transform.position + transform.TransformDirection(new Vector3(-0.3f, -0.4f, 0.1f));
    }
    
    public bool IsTagInBag(int id)
    {
        return tagsInBagList.ContainsKey(id);
    }

    public void ShowBag()
    {
        foreach (GameObject item in children)
        {
            if(item.GetComponent<TagInBag>().piece != null)
            {
                item.GetComponent<TagInBag>().ShowInBag();
            }
        }
    }

    public void HideBag()
    {
        foreach (GameObject item in children)
        {
            TagInBag tagInBag = item.GetComponent<TagInBag>();
            Destroy(tagInBag.instantiatedBagTag);
            Destroy(tagInBag.GetComponent<BoxCollider>());
        }
    }

    public void RemoveTagFromBag()
    {
        tagsInBagList.Remove(GlobalVariables.presentKey);
        Debug.Log("REMOVE tag with key: " + GlobalVariables.presentKey + " and List SIZE: " + tagsInBagList.Count);
    }
}
    

