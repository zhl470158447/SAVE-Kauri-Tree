using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/**
 * This class can draw the different elements of the tree to have a visual feedback of its growth
 **/
public class TreeDrawer: MonoBehaviour
{
	public GameObject _treeObject;
	Generate _treeGenerator;

	public GameObject _leafObject;
	public GameObject _rangeObject;
	public GameObject _killRangeObject;

	public Material _leaf;
	public Material _leafInactive;
	public Material _attractionRange;

	// the attractors
	GameObject[] attractors;

	// Start is called before the first frame update
	void Start () {
		_treeGenerator = _treeObject.GetComponent<Generate>();
		attractors = new GameObject[_treeGenerator.numAttracionPointsB];

		for (int i = 0; i < _treeGenerator.numAttracionPointsB; i++) {
			attractors[i] = Instantiate(_leafObject);
			attractors[i].transform.parent = transform;
		}
	}

	// Update is called once per frame
	void Update () {
		// we update the attractors
		if (_treeGenerator.numAttracionPointsB != attractors.Length) {
			GameObject[] attr = new GameObject[_treeGenerator.numAttracionPointsB];
			for (int i = 0; i < _treeGenerator.numAttracionPointsB; i++) {
				attr[i] = attractors[i];
			}
			for (int i = _treeGenerator.numAttracionPointsB; i < attractors.Length; i++) {
				Destroy(attractors[i]);
			}
			attractors = attr;
		}



		/*
		_rangeObject.transform.localScale = new Vector3(_treeGenerator._attractionRange, _treeGenerator._attractionRange, _treeGenerator._attractionRange) * 2;
		_rangeObject.transform.position = _treeGenerator._extremities[0]._end;

		_killRangeObject.transform.localScale = new Vector3(_treeGenerator._killRange, _treeGenerator._killRange, _treeGenerator._killRange) * 2;
		_killRangeObject.transform.position = _treeGenerator._extremities[0]._end;
		*/
	}
}
