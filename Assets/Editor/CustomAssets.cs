using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.IO;


public static class CustomAssets {
	#region Menu items
	[MenuItem ( "Assets/Create/ForceMaterial" )]
	public static void CreateForceMaterial () {
		Create <ForceMaterial> ();
	}

	[MenuItem ( "Assets/Create/DamageMaterial" )]
	public static void CreateDamageMaterial () {
		Create <DamageMaterial> ();
	}

	[MenuItem ( "Assets/Create/Inventory" )]
	public static void CreateInventory () {
		Create <Inventory> ();
	}
	#endregion Menu items

	public static string RootPath {
		get {
			var parts = Application.dataPath.Split ( new [] { '/' }, System.StringSplitOptions.RemoveEmptyEntries );
			string path = string.Join ( "/", parts, 0, parts.Length - 1 );
			
			return	path;
		}
	}

	public static TAsset Create <TAsset> ()
		where TAsset : ScriptableObject
	{
		string dir = AssetDatabase.GetAssetPath ( Selection.activeObject );
		var asset = ScriptableObject.CreateInstance <TAsset> ();
		Save ( asset, dir );
		
		return	asset;
	}

	public static void Save ( Object asset, string dir, string fileName = null, bool focus = true ) {
		if ( string.IsNullOrEmpty ( dir ) )
			dir = "Assets";
		else {
			string rootPath = RootPath;
			string absoluteDir = Path.Combine ( rootPath, dir );

			if ( File.Exists ( absoluteDir ) ) {
				dir = Path.GetDirectoryName ( absoluteDir );
				dir = dir.Substring ( rootPath.Length + 1 );	// +1 is for last slash.
			}
		}

		if ( !Directory.Exists ( dir ) )
			Directory.CreateDirectory ( dir );

		if ( fileName == null )
			fileName = asset.GetType ().Name + ".asset";

		string path = Path.Combine ( dir, fileName );
		path = AssetDatabase.GenerateUniqueAssetPath ( path );
		AssetDatabase.CreateAsset ( asset, path );

		if ( focus )
			ProjectWindowUtil.ShowCreatedAsset ( asset );
	}
}
