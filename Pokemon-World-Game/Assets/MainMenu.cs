using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void join()
    {
        var usernamefield = GameObject.FindGameObjectWithTag("UserNameField");
        var serveripfield = GameObject.FindGameObjectWithTag("ServerIpField");

        string username = usernamefield.GetComponentInChildren<Text>().text;
        string serverip = serveripfield.GetComponentInChildren<Text>().text;

        Debug.Log(serverip);

        Global.Instance.Login(serverip, username);
        SceneManager.LoadScene("scene_map");
    }
}
