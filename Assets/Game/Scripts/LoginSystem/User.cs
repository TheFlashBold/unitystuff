using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace TheFlashBold.LoginSystem
{

    #region Definitions
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

    public enum UserState : int
    {
        Online = 0,
        InMatch = 1,
        Searching = 2,
        Away = 3,
        Offline = 4
    };

    [System.Serializable]
    public struct Friend
    {
        public string username;
        public string id;
        public UserState UserState;
        public FriendData FriendData;
    }

    [System.Serializable]
    public struct FriendData
    {
        public string ProfilePicture;
    }
    #endregion

    public class User
    {
        public string username;
        public string id;
        private string session;
        public UserData UserData;

        public User(string username, LoginResponse response)
        {
            this.username = username;
            id = response.id;
            session = response.session;
            UserData = response.data;
        }

        #region Save Data

        public void Save()
        {
            LoginHandler.RunCoroutine(SaveData());
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
        #endregion
    }
}