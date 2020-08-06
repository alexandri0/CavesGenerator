using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Controller : MonoBehaviour
{
    public int Loading { get; set; }
    public GameObject ballPrefab;

    GameObject ball;
    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.R))
        {
            if(Loading == 0)
            SceneManager.LoadScene(0);
        }

        if(Input.GetMouseButtonDown(0))
        {
            if(ball != null)
                Destroy(ball);
            Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            pos.z = 0;
            ball = Instantiate(ballPrefab, pos, Quaternion.identity);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
            GameClose();
    }

    public void GameClose()
    {
#if UNITY_EDITOR
        // Application.Quit() does not work in the editor so
        // UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
        UnityEditor.EditorApplication.isPlaying = false;
#else
         Application.Quit();
#endif
    }
}

