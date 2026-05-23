using UnityEditor;
using UnityEngine;

public static class ClearUserNamePref
{
    [MenuItem("Dev/Clear UserName Pref")]
    public static void ClearUserName()
    {
        PlayerPrefs.DeleteKey("UserName");
        PlayerPrefs.Save();
        Debug.Log("PlayerPrefs: Cleared UserName");
    }
}
