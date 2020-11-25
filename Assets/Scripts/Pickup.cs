using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Database;
using Firebase.Unity;
using Firebase;
using Firebase.Unity.Editor;
using System;
using System.Threading.Tasks;

public class Pickup : MonoBehaviour{
    Player player;
    public int points;
    protected Dictionary<string, Firebase.Auth.FirebaseUser> userByAuth =
    new Dictionary<string, Firebase.Auth.FirebaseUser>();
    DependencyStatus dependencyStatus = DependencyStatus.UnavailableOther;
    Firebase.Auth.FirebaseAuth auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
    protected string displayName = "";

    // Start is called before the first frame update
    void Start(){
        player = FindObjectOfType<Player>();

        // Set these values before calling into the realtime database.
        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://sweetseeds-edcf2.firebaseio.com/");
        FirebaseApp.DefaultInstance.SetEditorP12FileName("sweetseeds-abf2e299118d.p12");
        FirebaseApp.DefaultInstance.SetEditorServiceAccountEmail("serviceaccount@sweetseeds.iam.gserviceaccount.com");
        FirebaseApp.DefaultInstance.SetEditorP12Password("notasecret");

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

        // Get the root reference location of the database.
        DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference;
        PostToDatabase(reference);
    }

    // Initialize the Firebase database:
    protected virtual void InitializeFirebase()
    {
        FirebaseApp app = FirebaseApp.DefaultInstance;

        auth.SignInWithEmailAndPasswordAsync("mail@mail.com", "strongpassword").ContinueWith(task =>
        {
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
            Firebase.Auth.FirebaseUser newUser = task.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})",
                newUser.DisplayName, newUser.UserId);
            //Getting client id for FB using device id
            FirebaseDatabase.DefaultInstance.GetReference("node1").Child("node2").Child("node3")
                            .ValueChanged += AuthStateChanged;
        });

    }

    // Track state changes of the auth object.
    void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        Firebase.Auth.FirebaseAuth senderAuth = sender as Firebase.Auth.FirebaseAuth;
        Firebase.Auth.FirebaseUser user = null;
        if (senderAuth != null) userByAuth.TryGetValue(senderAuth.App.Name, out user);
        if (senderAuth == auth && senderAuth.CurrentUser != user)
        {
            bool signedIn = user != senderAuth.CurrentUser && senderAuth.CurrentUser != null;
            if (!signedIn && user != null)
            {
                Debug.Log("Signed out " + user.UserId);
                //user is logged out, load login screen 
                //SceneManager.LoadSceneAsync("scene_01");
            }
            user = senderAuth.CurrentUser;
            userByAuth[senderAuth.App.Name] = user;
            if (signedIn)
            {
                Debug.Log("Signed in " + user.UserId);
                displayName = user.DisplayName ?? "";
            }
        }
    }

    private string PostToDatabase(DatabaseReference reference)
    {
        GamePoints gamePoints = new GamePoints(points);
        string json = JsonUtility.ToJson(gamePoints);
        reference.Child("game_points").Child("points").SetRawJsonValueAsync(json);
        return "Posted to Database";
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            player.points += points;
            Destroy(gameObject);
        }
    }

    //private void OnDestroy()
    //{
    //    auth.SignOut();
    //}

}
