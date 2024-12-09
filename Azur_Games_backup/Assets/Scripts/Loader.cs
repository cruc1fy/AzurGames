using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Loader : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        try
        {
            RemoteSettings.Completed += HandleRemoteSettings;
            RemoteSettings.ForceUpdate();
        }
        finally
        {
            StartCoroutine(FallBack());
        }
    }

    private void HandleRemoteSettings(bool wasUpdatedFromServer, bool settingsChanged, int serverResponse)
    {
        try
        {
            StopAllCoroutines();
            RemoteSettings.Completed -= HandleRemoteSettings;
        }
        finally
        {
            SceneManager.LoadScene(1);
        }
    }

    IEnumerator FallBack()
    {
        yield return new WaitForSeconds(3f);

        try
        {
            RemoteSettings.Completed -= HandleRemoteSettings;
        }
        finally
        {
            SceneManager.LoadScene(1);
        }
    }
}