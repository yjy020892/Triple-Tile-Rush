using UnityEngine;

public class SortingRuntimeBootstrap : MonoBehaviour
{
    private void Awake()
    {
        if (FindObjectOfType<SortingGameController>() != null && FindObjectsOfType<SortingRuntimeBootstrap>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        if (FindObjectOfType<SortingGameController>() == null)
        {
            GameObject controllerObject = new GameObject("SortingGameController");
            controllerObject.transform.SetParent(transform, false);
            controllerObject.AddComponent<SortingGameController>();
        }
    }
}
