using UnityEngine;

public class SortingRuntimeBootstrap : MonoBehaviour
{
    private void Awake()
    {
        if (FindObjectsOfType<SortingRuntimeBootstrap>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        if (FindObjectOfType<SortingGameController>() == null)
        {
            Debug.LogError("[SortingRuntimeBootstrap] SortingGameController is missing from the scene.");
        }
    }
}
