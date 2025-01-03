using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EssentialObjectSpawn : MonoBehaviour
{
    [SerializeField] GameObject assentialObjectPrefab;

    private void Awake()
    {
        var exitstingObject = FindObjectsOfType<EssentialObject>();
        if(exitstingObject.Length == 0)
        {
            //if there is a grid 
            var spawnPos = new Vector3(0, 0, 0);

            var gird = FindObjectOfType<Grid>();
            if(gird != null)
            {
                spawnPos = gird.transform.position;
            }

            Instantiate(assentialObjectPrefab, spawnPos, Quaternion.identity);
        }
    }
}
