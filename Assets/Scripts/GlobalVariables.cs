using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GlobalVariables {
	
    public static List<Vector3> objPositions = new List<Vector3>();
    public static List<Quaternion> objRotations = new List<Quaternion>();

    public static List<GameObject> instatiatedBagTags = new List<GameObject>();

    public static int presentKey;
    public static PieceOfArt pieceToTranspose;
    public static int paintingid;   
  
    
}
