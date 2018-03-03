using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class SaveResponse
{
    public bool success;
    public string error;
}

[System.Serializable]
public struct UserData
{
    public int Logins;
    public string ProfilePicture;
}

public class User {
    public string username;
    public string id;
    private string session;
    public UserData UserData;

    public User(string username, LoginResponse response)
    {
        this.username = username;
        id = response.id;
        session = response.session;
        try
        {
            UserData = JsonUtility.FromJson<UserData>(response.data);
        }
        catch(System.Exception e)
        {
            Debug.Log("Loading user data failed, should not be happening.");
        }
    }

    public void Save()
    {
        LoginHandler.instance.RunCoroutine(SaveData());
    }

    IEnumerator SaveData()
    {
        Debug.Log("saving user " + username);
        WWWForm form = new WWWForm();
        form.AddField("id", id);
        form.AddField("session", session);
        form.AddField("data", JsonUtility.ToJson(UserData));

        using (UnityWebRequest www = UnityWebRequest.Post(LoginHandler.instance.BackendUrl + "/save", form))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                try
                {
                    RegisterResponse response = JsonUtility.FromJson<RegisterResponse>(www.downloadHandler.text);
                }
                catch (System.Exception e)
                {
                    Debug.Log(e);
                }
            }
        }
    }
}
