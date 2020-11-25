using System;
using System.Collections;
using System.Collections.Generic;
using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;
using UnityEngine;
using UnityEngine.UI;

public class Authentication : MonoBehaviour
{
    private Firebase.Auth.FirebaseAuth auth;
    private Firebase.Auth.FirebaseUser newUser;
    private Firebase.DependencyStatus dependencyStatus = Firebase.DependencyStatus.UnavailableOther;
    protected Dictionary<string, Firebase.Auth.FirebaseUser> userByAuth =
    new Dictionary<string, Firebase.Auth.FirebaseUser>();
    protected string email = "";
    protected string password = "";
    protected string displayName = "";
    public Text emailText;
    public Text passwordText;

    void Awake()
    {
        //Using Firebase SDK
        //This is needed only for the unity editor
        FirebaseApp.DefaultInstance.SetEditorP12FileName("sweetseeds-abf2e299118d.p12");
        FirebaseApp.DefaultInstance.SetEditorServiceAccountEmail("firebase-adminsdk-h6ayv@sweetseeds-edcf2.iam.gserviceaccount.com");
        FirebaseApp.DefaultInstance.SetEditorP12Password("notasecret");
        //--------------------------------------

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
            dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                InitializeFirebase();
            }
            else
            {
                Debug.LogError(
                  "Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        });
    }

    // Initialize the Firebase database:
    protected virtual void InitializeFirebase()
    {
        FirebaseApp app = FirebaseApp.DefaultInstance;
        // NOTE: You'll need to replace this url with your Firebase App's database
        // path in order for the database connection to work correctly in editor

        //This is needed only for the unity editor
        app.SetEditorDatabaseUrl("https://sweetseeds-edcf2.firebaseio.com/");
        if (app.Options.DatabaseUrl != null) app.SetEditorDatabaseUrl(app.Options.DatabaseUrl);
        //--------------------------------------
        auth = Firebase.Auth.FirebaseAuth.DefaultInstance;

        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWith(task => {
            if (task.IsCanceled)
            {
                Debug.LogError("CreateUserWithEmailAndPasswordAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("CreateUserWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                return;
            }

            // Firebase user has been created.
            newUser = task.Result;
            Debug.LogFormat("Firebase user created successfully: {0} ({1})",
                newUser.DisplayName, newUser.UserId);
        });

        auth.SignInWithEmailAndPasswordAsync("mail@mail.com", "strongpassword").ContinueWith(task => {
            if (task.IsCanceled)
            {
                Debug.LogError("SignInWithEmailAndPasswordAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("SignInWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                return;
            }
            newUser = task.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})",
                newUser.DisplayName, newUser.UserId);
            //Getting client id for FB using device id
            FirebaseDatabase.DefaultInstance.GetReference("node1").Child("node2").Child("node3")
            .ValueChanged += AuthStateChanged;
        });
    }

    //Tracks the state changes of the auth object
    private void AuthStateChanged(object sender, ValueChangedEventArgs e){
        if(auth.CurrentUser != newUser){
            bool signedIn = newUser != auth.CurrentUser && auth.CurrentUser != null;
            if(!signedIn && newUser != null){
                Debug.Log("Signed out " + newUser.UserId);
            }
            newUser = auth.CurrentUser;
            if(signedIn){
                Debug.Log("Signed in " + newUser.UserId);
                displayName = newUser.DisplayName ?? "";
                email = newUser.Email ?? "";                  
            }
        }
    }
}
