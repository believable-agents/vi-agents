#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using ViAgents;
using ViAgents.Schedules;
using ViAgents.Actions;

public static class ScriptableObjectUtility
{
	/// <summary>
	//	This makes it easy to create, name and place unique new ScriptableObject asset files.
	/// </summary>
	public static void CreateAsset<T> () where T : ScriptableObject
	{
		T asset = ScriptableObject.CreateInstance<T> ();
		
		string path = AssetDatabase.GetAssetPath (Selection.activeObject);
		if (path == "") 
		{
			path = "Assets";
		} 
		else if (Path.GetExtension (path) != "") 
		{
			path = path.Replace (Path.GetFileName (AssetDatabase.GetAssetPath (Selection.activeObject)), "");
		}
		
		string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath (path + "/New " + typeof(T).ToString() + ".asset");
		
		AssetDatabase.CreateAsset (asset, assetPathAndName);
		
		AssetDatabase.SaveAssets ();
		EditorUtility.FocusProjectWindow ();
		Selection.activeObject = asset;
	}

	//public class SkinTonesAsset
	//{
	//	[MenuItem("Assets/Create/ScriptableObjects/Color Tones")]
	//	public static void CreateAsset ()
	//	{
	//		ScriptableObjectUtility.CreateAsset<RaceColorTones> ();
	//	}
	//}

	//public class BasePopulationAsset
	//{
	//	[MenuItem("Assets/Create/ScriptableObjects/Base Population")]
	//	public static void CreateAsset ()
	//	{
	//		ScriptableObjectUtility.CreateAsset<BasePopulation> ();
	//	}
	//}

    public class ScheduleAsset
    {
        [MenuItem("Assets/Create/ScriptableObjects/Schedule")]
        public static void CreateAsset ()
        {
            ScriptableObjectUtility.CreateAsset<Schedule> ();
        }
    }

    public class ScheduleActionset
    {
        [MenuItem("Assets/Create/ScriptableObjects/ActionSet")]
        public static void CreateAsset()
        {
            ScriptableObjectUtility.CreateAsset<ActionSet>();
        }
    }
}
#endif