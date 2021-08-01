using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FirebaseAuthentication : MonoBehaviour
{
    [SerializeField] InputField email_Field;
    [SerializeField] InputField password_Field;


    public void LOGIN_ANONYMOUSLY()
    {
        Firebase.Auth.FirebaseAuth.DefaultInstance.SignInAnonymouslyAsync().ContinueWith((task => {

            if (task.IsCanceled)
            {
                print("Task Cancelled");
                return;
            }

            if (task.IsFaulted)
            {
                print("Task Faulted");
            }
            if (task.IsCompleted)
            {
                print("Task Completed::" + task.Result.UserId);
            }

        }));
    }

    public void REGISTER()
    {
        if (email_Field.text.Equals("") || password_Field.text.Equals(""))
        {
            print("Email or password cant be null");
            return;
        }

        Firebase.Auth.FirebaseAuth.DefaultInstance.CreateUserWithEmailAndPasswordAsync(email_Field.text, password_Field.text).ContinueWith((task => {

            if (task.IsCanceled)
            {
                print("Task Cancelled");
                return;
            }


            if (task.IsFaulted)
            {
                print("Task Faulted");
                return;
            }


            if (task.IsCompleted)
            {
                print("Registration complete::");
                print(task.Result.UserId);
                print(task.Result.DisplayName);
                print(task.Result.Email);
            }

        }));
    }

    public void LOGIN()
    {
        if (email_Field.text.Equals("") || password_Field.text.Equals(""))
        {
            print("Email or password cant be null");
            return;
        }


        Firebase.Auth.FirebaseAuth.DefaultInstance.SignInWithEmailAndPasswordAsync(email_Field.text, password_Field.text).ContinueWith((task =>
        {

            if (task.IsCanceled)
            {
                print("Task Cancelled");
                return;
            }


            if (task.IsFaulted)
            {
                print("Task Faulted");
                return;
            }


            if (task.IsCompleted)
            {
                print("Sign in complete::");
                print(task.Result.UserId);
                print(task.Result.DisplayName);
                print(task.Result.Email);
            }
        }));
    }


}
