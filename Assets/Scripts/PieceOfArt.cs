using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceOfArt {

	public GameObject pieceOfArt;
	public TextMesh caption;

	public PieceOfArt(GameObject newPieceOfArt, TextMesh newCaption){
		pieceOfArt = newPieceOfArt;
		caption = newCaption;
	}
}
