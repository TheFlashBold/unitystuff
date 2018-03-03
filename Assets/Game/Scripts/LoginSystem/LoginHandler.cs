﻿using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

#region Input Objects
[System.Serializable]
public struct LoginComponents
{
    public InputField UsernameInput;
    public InputField PasswordInput;
    public Text Text;
    public GameObject Window;
}

[System.Serializable]
public struct RegisterComponents
{
    public InputField UsernameInput;
    public InputField PasswordInput;
    public InputField PasswordRepeatInput;
    public Text Text;
    public GameObject Window;
}
#endregion

#region Definitions
/// <summary>
/// LoginResponse object
/// </summary>
[System.Serializable]
public class LoginResponse
{
    public string id;
    public string session;
    public bool success;
    public string error;
    public string data;
}

/// <summary>
/// RegisterResponse object
/// </summary>
[System.Serializable]
public class RegisterResponse
{
    public bool success;
    public string error;
}
#endregion

public class LoginHandler : MonoBehaviour {

    public string BackendUrl = "https://login.0zn.ch";

    public LoginComponents LoginComponents;
    public RegisterComponents RegisterComponents;

    public static User CurrentUser;

    #region Singleton
    public static LoginHandler instance;

    private void Awake()
    {
        instance = this;
    }
    #endregion

    #region Register
    #region magic
    public void UISwitchToRegister()
    {
        LoginComponents.Window.SetActive(false);
        RegisterComponents.Window.SetActive(true);
    }

    /// <summary>
    /// Register method invoked from UI
    /// </summary>
    public void UIRegister()
    {
        Register(RegisterComponents.UsernameInput.text, RegisterComponents.PasswordInput.text, RegisterComponents.PasswordRepeatInput.text);
    }

    /// <summary>
    /// Invoke the registering process.
    /// </summary>
    /// <param name="username"></param>
    /// <param name="password"></param>
    /// <param name="passwordrepeat"></param>
    public void Register(string username, string password, string passwordrepeat)
    {
        StartCoroutine(postRegisterData(username, password, passwordrepeat));
    }

    /// <summary>
    /// Send data to the server
    /// </summary>
    /// <param name="username"></param>
    /// <param name="password"></param>
    /// <param name="passwordrepeat"></param>
    /// <returns></returns>
    IEnumerator postRegisterData(string username, string password, string passwordrepeat)
    {
        WWWForm form = new WWWForm();
        form.AddField("username", username);
        form.AddField("password", md5(password));
        form.AddField("passwordrepeat", md5(passwordrepeat));

        OnRegisterStart();

        using (UnityWebRequest www = UnityWebRequest.Post(BackendUrl + "/register", form))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                OnRegisterError(www);
                Debug.Log(www.error);
            }
            else
            {
                try
                {
                    RegisterResponse response = JsonUtility.FromJson<RegisterResponse>(www.downloadHandler.text);
                    if (response.success)
                    {
                        OnRegisterSuccess(response, www);
                    }
                    else
                    {
                        OnRegisterFailure(response.error, response, www);
                    }
                }
                catch (System.Exception e)
                {
                    OnRegisterError(www);
                }
            }
        }
    }
    #endregion
    /// <summary>
    /// Called when register starts
    /// </summary>
    private void OnRegisterStart()
    {
        RegisterComponents.Text.text = "Registering!";
    }

    /// <summary>
    /// Called when register succeeded
    /// </summary>
    /// <param name="www"></param>
    /// <param name="response"></param>
    private void OnRegisterSuccess(RegisterResponse response, UnityWebRequest www)
    {
        RegisterComponents.Text.text = "Register successful!";
        Debug.Log("success " + response.success);
    }

    /// <summary>
    /// Called when register fails (user already exists)
    /// </summary>
    /// <param name="www"></param>
    private void OnRegisterFailure(string error, RegisterResponse response, UnityWebRequest www)
    {
        RegisterComponents.Text.text = "User already exists!";
        Debug.Log("user already exists");
    }

    /// <summary>
    /// Called when register errors hard
    /// </summary>
    /// <param name="www"></param>
    private void OnRegisterError(UnityWebRequest www)
    {
        RegisterComponents.Text.text = "Something really bad happened!";
    }

    #endregion

    #region Login
    #region magic
    public void UISwitchToLogin()
    {
        LoginComponents.Window.SetActive(true);
        RegisterComponents.Window.SetActive(false);
    }

    /// <summary>
    /// Login method invoked from UI
    /// </summary>
    public void UILogin()
    {
        Login(LoginComponents.UsernameInput.text, LoginComponents.PasswordInput.text);
    }

    /// <summary>
    /// Invoke the login process.
    /// </summary>
    /// <param name="username">The username</param>
    /// <param name="password">The password</param>
    public void Login(string username, string password)
    {
        StartCoroutine(postLoginData(username, password));
    }

    /// <summary>
    /// Send the data to the server.
    /// </summary>
    /// <param name="username"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    IEnumerator postLoginData(string username, string password)
    {
        WWWForm form = new WWWForm();
        form.AddField("username", username);
        form.AddField("password", md5(password));

        OnLoginStart();

        using (UnityWebRequest www = UnityWebRequest.Post(BackendUrl + "/login", form))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                OnLoginError(www);
                Debug.Log(www.error);
            }
            else
            {
                try
                {
                    LoginResponse response = JsonUtility.FromJson<LoginResponse>(www.downloadHandler.text);
                    if (response.success)
                    {
                        CurrentUser = new User(LoginComponents.UsernameInput.text, response);
                        CurrentUser.UserData.Logins += 1;
                        CurrentUser.Save();
                        OnLoginSuccess(response, www);
                    }
                    else
                    {
                        OnLoginFailure(response.error, response, www);
                    }
                }
                catch (System.Exception e)
                {
                    OnLoginError(www);
                }                
            }
        }
    }
    #endregion
    /// <summary>
    /// Called when login starts
    /// </summary>
    private void OnLoginStart()
    {
        LoginComponents.Text.text = "Logging in!";
    }

    /// <summary>
    /// Called when login succeeded
    /// </summary>
    /// <param name="www"></param>
    /// <param name="response"></param>
    private void OnLoginSuccess(LoginResponse response, UnityWebRequest www)
    {
        LoginComponents.Text.text = "Login successful!";
        Debug.Log("success " + response.id + " " + response.success);
        // Change Scene?
    }

    /// <summary>
    /// Called when login fails (wrong password / user doesn't exist)
    /// </summary>
    /// <param name="www"></param>
    private void OnLoginFailure(string error, LoginResponse response, UnityWebRequest www)
    {
        LoginComponents.Text.text = "Wrong username / password";
        Debug.Log("wrong password");
    }

    /// <summary>
    /// Called when login errors hard
    /// </summary>
    /// <param name="www"></param>
    private void OnLoginError(UnityWebRequest www)
    {
        LoginComponents.Text.text = "Something really bad happened!";
    }
    #endregion
    
    #region Utils
    /// <summary>
    /// Generates MD5 from provided string
    /// </summary>
    /// <param name="strToEncrypt"></param>
    /// <returns></returns>
    public string md5(string strToEncrypt)
    {
        System.Text.UTF8Encoding ue = new System.Text.UTF8Encoding();
        byte[] bytes = ue.GetBytes(strToEncrypt);
        
        System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
        byte[] hashBytes = md5.ComputeHash(bytes);
        
        string hashString = "";

        for (int i = 0; i < hashBytes.Length; i++)
        {
            hashString += System.Convert.ToString(hashBytes[i], 16).PadLeft(2, '0');
        }

        return hashString.PadLeft(32, '0');
    }
    /// <summary>
    /// Crude invoke Coroutine form non Monobehaviour-Class hack :)
    /// </summary>
    /// <param name="c"></param>
    public void RunCoroutine(IEnumerator c)
    {
        StartCoroutine(c);
    }
    #endregion
}
