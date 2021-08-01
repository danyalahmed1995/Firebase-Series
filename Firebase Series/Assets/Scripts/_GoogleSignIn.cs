    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Google;
    using UnityEngine;
    using UnityEngine.Networking;
    using UnityEngine.UI;
    using Firebase.Auth;
using Firebase;

public class _GoogleSignIn : MonoBehaviour
    {

        public string webClientId = "<your client id here>";
        public string webClientSecret = "<your client secret here>";

        private GoogleSignInConfiguration configuration;
    FirebaseAuth auth;

        // Defer the configuration creation until Awake so the web Client ID
        // Can be set via the property inspector in the Editor.
        void Awake()
        {
            configuration = new GoogleSignInConfiguration
            {
                WebClientId = webClientId,
                RequestIdToken = true
            };
        CheckFirebaseDependencies();

        }

    private void CheckFirebaseDependencies()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                if (task.Result == DependencyStatus.Available)
                    auth = FirebaseAuth.DefaultInstance;
                else
                    print("Could not resolve all Firebase dependencies: " + task.Result.ToString());
            }
            else
            {
                print("Dependency check was not completed. Error : " + task.Exception.Message);
            }
        });
    }

    private void SignInWithGoogleOnFirebase(string idToken)
    {
        Credential credential = GoogleAuthProvider.GetCredential(idToken, null);

        auth.SignInWithCredentialAsync(credential).ContinueWith(task =>
        {
            AggregateException ex = task.Exception;
            if (ex != null)
            {
                if (ex.InnerExceptions[0] is FirebaseException inner && (inner.ErrorCode != 0))
                   print("\nError code = " + inner.ErrorCode + " Message = " + inner.Message);
            }
            else
            {
              
                print("Sign In Successful.");
                print("FirebaseUserId=" + task.Result.UserId);
                print("PhotoUrl = " + task.Result.PhotoUrl);
 
            }
        });
    }

    public void OnSignIn()
        {
            GoogleSignIn.Configuration = configuration;
            GoogleSignIn.Configuration.UseGameSignIn = false;
            GoogleSignIn.Configuration.RequestIdToken = true;
            GoogleSignIn.Configuration.RequestAuthCode = true;
            GoogleSignIn.Configuration.RequestEmail = true;
            print("Calling SignIn");

            GoogleSignIn.DefaultInstance.SignIn().ContinueWith(
              OnAuthenticationFinished);
        }

        public void OnSignOut()
        {
            print("Calling SignOut");
            GoogleSignIn.DefaultInstance.SignOut();
        }

        public void OnDisconnect()
        {
            print("Calling Disconnect");
            GoogleSignIn.DefaultInstance.Disconnect();
        }

        internal void OnAuthenticationFinished(Task<GoogleSignInUser> task)
        {
            if (task.IsFaulted)
            {
                using (IEnumerator<System.Exception> enumerator =
                        task.Exception.InnerExceptions.GetEnumerator())
                {
                    if (enumerator.MoveNext())
                    {
                        GoogleSignIn.SignInException error =
                                (GoogleSignIn.SignInException)enumerator.Current;
                        print("Got Error: " + error.Status + " " + error.Message);
                    }
                    else
                    {
                        print("Got Unexpected Exception?!?" + task.Exception);
                    }
                }
            }
            else if (task.IsCanceled)
            {
                print("Canceled");
            }
            else
            {
                print("Welcome: " + task.Result.DisplayName + "!");
                print("Welcome: " + task.Result.Email + "!");
                print("AuthCode: " + task.Result.AuthCode + "!");
                // AddStatusText("IdToken: " + task.Result.IdToken+ "!");
                print("UserId: " + task.Result.UserId + "!");
                Debug.Log("AuthCode: " + task.Result.AuthCode);
                Debug.Log("IdToken: " + task.Result.IdToken);
                Debug.Log("UserId: " + task.Result.UserId);
                SignInWithGoogleOnFirebase(task.Result.IdToken);
            //StartCoroutine(GetAccessToken(task.Result.AuthCode));


        }
        }

        public void OnSignInSilently()
        {
            GoogleSignIn.Configuration = configuration;
            GoogleSignIn.Configuration.UseGameSignIn = false;
            GoogleSignIn.Configuration.RequestIdToken = true;
            print("Calling SignIn Silently");

            GoogleSignIn.DefaultInstance.SignInSilently()
                  .ContinueWith(OnAuthenticationFinished);
        }


        public void OnGamesSignIn()
        {
            GoogleSignIn.Configuration = configuration;
            GoogleSignIn.Configuration.UseGameSignIn = true;
            GoogleSignIn.Configuration.RequestIdToken = false;

            print("Calling Games SignIn");

            GoogleSignIn.DefaultInstance.SignIn().ContinueWith(
              OnAuthenticationFinished);
        }

        IEnumerator GetAccessToken(string yourAuthCode)
        {
            WWWForm form = new WWWForm();
            form.AddField("client_id", webClientId);
            form.AddField("client_secret", webClientSecret);
            form.AddField("grant_type", "authorization_code");
            form.AddField("code", yourAuthCode);

            UnityWebRequest www = UnityWebRequest.Post("https://www.googleapis.com/oauth2/v4/token", form);
            yield return www.SendWebRequest();

            //GoogleData googleData = JsonUtility.FromJson<GoogleData>(www.downloadHandler.text);
            Debug.Log("Request Data:" + www.downloadHandler.text);

            if (www.isNetworkError || www.isHttpError)
            {
            }
            else
            {
                //Access token response
                // GoogleData googleData = JsonUtility.FromJson<GoogleData>(www.downloadHandler.text);
                // Debug.Log("JSON Data:"+googleData);

            }
        }

        
    }

public class GoogleData
{
    public string access_token;
    public int expires_in;
    public string refresh_token;
    public string scope;
    public string token_type;
    public string id_token;
}