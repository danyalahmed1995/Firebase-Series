using System.Text;
using AppleAuth;
using AppleAuth.Enums;
using AppleAuth.Extensions;
using AppleAuth.Interfaces;
using AppleAuth.Native;
using UnityEngine;
using System.Security.Cryptography;
using System.Collections.Generic;
using System;
using Firebase.Auth;
using System.Threading.Tasks;
using Firebase.Extensions;
using System.Collections;
using UnityEngine.SceneManagement;

public class _AppleSignIn : MonoBehaviour
{

    private const string AppleUserIdKey = "AppleUserId";

    private IAppleAuthManager _appleAuthManager;
    string a;
    private bool isLoggedIn;
    private FirebaseUser newUser;

    //comment line

    // Start is called before the first frame update
    void Start()
    {
        // If the current platform is supported
        if (AppleAuthManager.IsCurrentPlatformSupported)
        {
            // Creates a default JSON deserializer, to transform JSON Native responses to C# instances
            var deserializer = new PayloadDeserializer();
            // Creates an Apple Authentication manager with the deserializer
            this._appleAuthManager = new AppleAuthManager(deserializer);
        }

        else
        {
            print("Platform not supported");
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Updates the AppleAuthManager instance to execute
        // pending callbacks inside Unity's execution loop
        if (this._appleAuthManager != null)
        {
            this._appleAuthManager.Update();
        }
    }


    private void SignInWithApple()
    {
        var loginArgs = new AppleAuthLoginArgs(LoginOptions.IncludeEmail | LoginOptions.IncludeFullName);


        this._appleAuthManager.LoginWithAppleId(
            loginArgs,
            credential =>
            {
                var appleIdCredential = credential as IAppleIDCredential;

                var fullName = appleIdCredential.FullName;
                var identityToken = Encoding.UTF8.GetString(appleIdCredential.IdentityToken, 0, appleIdCredential.IdentityToken.Length);
                var authorizedCode = Encoding.UTF8.GetString(appleIdCredential.AuthorizationCode, 0, appleIdCredential.AuthorizationCode.Length);

                // If a sign in with apple succeeds, we should have obtained the credential with the user id, name, and email, save it
                PlayerPrefs.SetString(AppleUserIdKey, credential.User);

                print("Apple User Credentials::" + credential.User.ToString());

                //this.SetupGameMenu(credential.User, credential);

                print("Apple login identity token::" + identityToken);

                print("Apple login authorized code::" + authorizedCode);

                if (fullName != null)
                {
                    print("Apple user name short " + fullName.ToLocalizedString(PersonNameFormatterStyle.Short));
                    print("Apple user name medium " + fullName.ToLocalizedString(PersonNameFormatterStyle.Medium));
                    print("Apple user name long " + fullName.ToLocalizedString(PersonNameFormatterStyle.Long));
                    print("Apple user name default" + fullName.ToLocalizedString(PersonNameFormatterStyle.Default));


                }


                PerformLoginWithAppleIdAndFirebase(identityToken);


            },
            error =>
            {
                var authorizationErrorCode = error.GetAuthorizationErrorCode();
                Debug.LogWarning("Sign in with Apple failed " + authorizationErrorCode.ToString() + " " + error.ToString());
                //this.SetupLoginMenuForSignInWithApple();
            });
    }

    private void AttemptQuickLogin()
    {
        var quickLoginArgs = new AppleAuthQuickLoginArgs();

        // Quick login should succeed if the credential was authorized before and not revoked
        this._appleAuthManager.QuickLogin(
            quickLoginArgs,
            credential =>
            {
                // If it's an Apple credential, save the user ID, for later logins
                var appleIdCredential = credential as IAppleIDCredential;
                if (appleIdCredential != null)
                {
                    PlayerPrefs.SetString(AppleUserIdKey, credential.User);

                }

                //this.SetupGameMenu(credential.User, credential);
            },
            error =>
            {
                // If Quick Login fails, we should show the normal sign in with apple menu, to allow for a normal Sign In with apple
                var authorizationErrorCode = error.GetAuthorizationErrorCode();
                Debug.LogWarning("Quick Login Failed " + authorizationErrorCode.ToString() + " " + error.ToString());
                // this.SetupLoginMenuForSignInWithApple();
            });
    }

    private void CheckCredentialStatusForUserId(string appleUserId)
    {
        // If there is an apple ID available, we should check the credential state
        this._appleAuthManager.GetCredentialState(
            appleUserId,
            state =>
            {
                switch (state)
                {
                    // If it's authorized, login with that user id
                    case CredentialState.Authorized:
                        //this.SetupGameMenu(appleUserId, null);
                        return;

                    // If it was revoked, or not found, we need a new sign in with apple attempt
                    // Discard previous apple user id
                    case CredentialState.Revoked:
                    case CredentialState.NotFound:
                        //this.SetupLoginMenuForSignInWithApple();
                        PlayerPrefs.DeleteKey(AppleUserIdKey);
                        return;
                }
            },
            error =>
            {
                var authorizationErrorCode = error.GetAuthorizationErrorCode();
                Debug.LogWarning("Error while trying to get credential state " + authorizationErrorCode.ToString() + " " + error.ToString());
                //this.SetupLoginMenuForSignInWithApple();
            });
    }


    public void SignInWithAppleButtonPressed()
    {
        //this.SetupLoginMenuForAppleSignIn();
        this.SignInWithApple();
    }

    #region SHA256 RAW NONCE FOR APPLE AUTH WITH FIREBASE

    string GenerateRandomString(int length)
    {
        if (length <= 0)
        {
            throw new Exception("Expected nonce to have positive length");
        }

        const string charset = "0123456789ABCDEFGHIJKLMNOPQRSTUVXYZabcdefghijklmnopqrstuvwxyz-._";
        var cryptographicallySecureRandomNumberGenerator = new RNGCryptoServiceProvider();
        var result = string.Empty;
        var remainingLength = length;

        var randomNumberHolder = new byte[1];
        while (remainingLength > 0)
        {
            var randomNumbers = new List<int>(16);
            for (var randomNumberCount = 0; randomNumberCount < 16; randomNumberCount++)
            {
                cryptographicallySecureRandomNumberGenerator.GetBytes(randomNumberHolder);
                randomNumbers.Add(randomNumberHolder[0]);
            }

            for (var randomNumberIndex = 0; randomNumberIndex < randomNumbers.Count; randomNumberIndex++)
            {
                if (remainingLength == 0)
                {
                    break;
                }

                var randomNumber = randomNumbers[randomNumberIndex];
                if (randomNumber < charset.Length)
                {
                    result += charset[randomNumber];
                    remainingLength--;
                }
            }
        }

        return result;
    }

    string GenerateSHA256NonceFromRawNonce(string rawNonce)
    {
        var sha = new SHA256Managed();
        var utf8RawNonce = Encoding.UTF8.GetBytes(rawNonce);
        var hash = sha.ComputeHash(utf8RawNonce);

        var result = string.Empty;
        for (var i = 0; i < hash.Length; i++)
        {
            result += hash[i].ToString("x2");
        }

        return result;
    }


    public void PerformLoginWithAppleIdAndFirebase(string appleIdToken)
    {
        var rawNonce = GenerateRandomString(32);
        var nonce = GenerateSHA256NonceFromRawNonce(rawNonce);

        Firebase.Auth.FirebaseAuth auth = Firebase.Auth.FirebaseAuth.DefaultInstance;


        Firebase.Auth.Credential credential =
        Firebase.Auth.OAuthProvider.GetCredential("apple.com", appleIdToken, rawNonce, null);
        auth.SignInWithCredentialAsync(credential).ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("SignInWithCredentialAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("SignInWithCredentialAsync encountered an error: " + task.Exception);
                return;
            }

            newUser = task.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})",
                newUser.DisplayName, newUser.UserId);
            isLoggedIn = true;
        });

    }

    #endregion
}