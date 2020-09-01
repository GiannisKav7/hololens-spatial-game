using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;

public class TagInBag : MonoBehaviour, IInputClickHandler
{
    public int key;
    public PieceOfArt piece;
    public GameObject instantiatedBagTag;
    public BoxCollider box;
    
    public void OnInputClicked(InputClickedEventData eventData)
    {
        // Store Globals
        if(GlobalVariables.pieceToTranspose == null){
            GlobalVariables.pieceToTranspose = piece;
            GlobalVariables.presentKey = key;

            piece = null;
            key = -1;
            Destroy(instantiatedBagTag);
            Destroy(gameObject.GetComponent<BoxCollider>());
            gameObject.GetComponentInParent<TagsMenu>().RemoveTagFromBag();
        }
    }

    public void ShowInBag()
    {
        Quaternion rot;
        Vector3 rotEuler;

        rot = transform.rotation;
        rotEuler = transform.rotation.eulerAngles;
        rotEuler = new Vector3(rotEuler.x, rotEuler.y + 180, rotEuler.z);
        rot = Quaternion.Euler(rotEuler);

        // Instantiate
        instantiatedBagTag = Instantiate(piece.pieceOfArt, transform.position, rot);

        box = gameObject.AddComponent<BoxCollider>();
        Renderer rend = instantiatedBagTag.GetComponent<Renderer>();
        box.size = new Vector3(rend.bounds.size.x, rend.bounds.size.y, 0.001f);
        Debug.Log("INSTANTIATE IN BAG KEY: " + key);
    }
}

