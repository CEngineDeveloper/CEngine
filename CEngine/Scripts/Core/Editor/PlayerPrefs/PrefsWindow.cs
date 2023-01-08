using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEditor;
using UnityEngine;

namespace CYM
{
	public class PrefsWindow : EditorWindow
	{
		//[MenuItem("Tools/Windows/Prefs")]
		public static void ShowWindow()
		{
			EditorWindow.GetWindow(typeof(PrefsWindow)).titleContent = new GUIContent("PrefsWindow");
		}

		private const string UNIQUE_STRING = "0987654321qwertyuiopasdfghjklzxcvbnm[];,.";
		private const int UNIQUE_INT = int.MinValue;
		private const float UNIQUE_FLOAT = Mathf.NegativeInfinity;

		private static readonly string[] UNITY_HIDDEN_SETTINGS = new string[]
		{
			"UnityGraphicsQuality",
			"unity.cloud_userid",
			"unity.player_session_background_time",
			"unity.player_session_elapsed_time",
			"unity.player_sessionid"
		};

		private const float UpdateIntervalInSeconds = 1.0F;

		private bool waitTillPlistHasBeenWritten = false;
		private FileInfo tmpPlistFile;

		private List<PlayerPrefsEntry> ppeList = new List<PlayerPrefsEntry>();
		private Vector2 scrollPos;
		private string newKey = "";
		private string newValueString = "";
		private int newValueInt = 0;
		private float newValueFloat = 0;
		private float rotation = 0;
		private ValueType selectedType = ValueType.String;

		private bool showNewEntryBox = false;
		private bool isOneSelected = false;
		private bool sortAscending = true;   //Ascending is A-Z or 1-2. Descending is Z-A or 2-1.

		private float oldTime = 0;

		private string _searchString = string.Empty;
		private SearchFilterType _searchFilter = SearchFilterType.All;

		private List<PlayerPrefsEntry> filteredPpeList = new List<PlayerPrefsEntry>();

		private Texture2D _refreshIcon;
		private Texture2D _deleteIcon;
		private Texture2D _addIcon;
		private Texture2D _undoIcon;
		private Texture2D _saveIcon;

		private bool AutoRefresh
		{
			get { return EditorPrefs.GetBool("APPW-AutoRefresh", false); }
			set { EditorPrefs.SetBool("APPW-AutoRefresh", value); }
		}
		
		void OnEnable()
		{
			if (!IsUnityWritingToPlist())
				RefreshKeys();

			//Make sure we never subscribe twice as OnEnable will be called more often then you think :)
			EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
			EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
		}

		private void OnPlayModeStateChanged(PlayModeStateChange state)
		{
			waitTillPlistHasBeenWritten = IsUnityWritingToPlist();

			if (!waitTillPlistHasBeenWritten)
				RefreshKeys();
		}

		void Update()
		{
			//Auto refresh on Windows. On Mac this would be annoying because it takes longer so the user must manually refresh.
			if (AutoRefresh && Application.platform == RuntimePlatform.WindowsEditor
				&& EditorApplication.isPlaying)
			{
				float newtime = Mathf.Repeat(Time.timeSinceLevelLoad, UpdateIntervalInSeconds);
				if (newtime < oldTime)
					RefreshKeys();

				oldTime = newtime;
			}

			if (waitTillPlistHasBeenWritten)
			{
				if (tmpPlistFile != null && new FileInfo(tmpPlistFile.FullName).Exists)
				{
					//Keep on waiting
				}
				else
				{
					RefreshKeys();
					waitTillPlistHasBeenWritten = false;
				}

				rotation += 0.05F;
				Repaint();
			}

			//Only enable certain options when atleast one is selected
			isOneSelected = false;
			foreach (PlayerPrefsEntry item in filteredPpeList)
			{
				if (item.IsSelected)
				{
					isOneSelected = true;
					break;
				}
			}
		}

		void OnGUI()
		{
			GUIStyle boldNumberFieldStyle = new GUIStyle(EditorStyles.numberField);
			boldNumberFieldStyle.font = EditorStyles.boldFont;

			GUIStyle boldToggleStyle = new GUIStyle(EditorStyles.toggle);
			boldToggleStyle.font = EditorStyles.boldFont;

			GUI.enabled = !waitTillPlistHasBeenWritten;

			//Toolbar
			EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
			{
				Rect optionsRect = GUILayoutUtility.GetRect(0, 20, GUILayout.ExpandWidth(false));

				if (GUILayout.Button(new GUIContent("Sort   " + (sortAscending ? "▼" : "▲"), "Change sorting to " + (sortAscending ? "descending" : "ascending")), EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
				{
					OnChangeSortModeClicked();
				}

				if (GUILayout.Button(new GUIContent("Options", "Contains additional functionality like \"Add new entry\" and \"Delete all entries\" "), EditorStyles.toolbarDropDown, GUILayout.ExpandWidth(false)))
				{
					GenericMenu options = new GenericMenu();
					options.AddItem(new GUIContent("New Entry..."), false, OnNewEntryClicked);
					options.AddSeparator("");
					options.AddItem(new GUIContent("Import..."), false, OnImport);
					options.AddItem(new GUIContent("Export Selected..."), false, OnExportSelected);
					options.AddItem(new GUIContent("Export All Entries"), false, OnExportAllClicked);
					options.AddSeparator("");
					options.AddItem(new GUIContent("Delete Selected Entries"), false, OnDeleteSelectedClicked);
					options.AddItem(new GUIContent("Delete All Entries"), false, OnDeleteAllClicked);
					options.DropDown(optionsRect);
				}

				GUILayout.Space(5);

				//Searchfield
				Rect position = GUILayoutUtility.GetRect(50, 250, 10, 50, EditorStyles.toolbarTextField);
				position.width -= 16;
				position.x += 16;
				SearchString = GUI.TextField(position, SearchString, EditorStyles.toolbarTextField);

				position.x = position.x - 18;
				position.width = 20;
				if (GUI.Button(position, "", ToolbarSeachTextFieldPopup))
				{
					GenericMenu options = new GenericMenu();
					options.AddItem(new GUIContent("All"), SearchFilter == SearchFilterType.All, OnSearchAllClicked);
					options.AddItem(new GUIContent("Key"), SearchFilter == SearchFilterType.Key, OnSearchKeyClicked);
					options.AddItem(new GUIContent("Value (Strings only)"), SearchFilter == SearchFilterType.Value, OnSearchValueClicked);
					options.DropDown(position);
				}

				position = GUILayoutUtility.GetRect(10, 10, ToolbarSeachCancelButton);
				position.x -= 5;
				if (GUI.Button(position, "", ToolbarSeachCancelButton))
				{
					SearchString = string.Empty;
				}

				GUILayout.FlexibleSpace();

				if (Application.platform == RuntimePlatform.WindowsEditor)
				{
					string refreshTooltip = "Should all entries be automaticly refreshed every " + UpdateIntervalInSeconds + " seconds?";
					AutoRefresh = GUILayout.Toggle(AutoRefresh, new GUIContent("Auto Refresh ", refreshTooltip), EditorStyles.toolbarButton, GUILayout.ExpandWidth(false), GUILayout.MinWidth(75));
				}

				if (GUILayout.Button(new GUIContent("Refresh", "Force a refresh, could take a few seconds."), EditorStyles.toolbarButton, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false)))
				{
					if (Application.platform == RuntimePlatform.OSXEditor)
						waitTillPlistHasBeenWritten = IsUnityWritingToPlist();

					RefreshKeys();
				}

				Rect r;
				if (Application.platform == RuntimePlatform.OSXEditor)
					r = GUILayoutUtility.GetRect(16, 16);
				else
					r = GUILayoutUtility.GetRect(9, 16);

				if (waitTillPlistHasBeenWritten)
				{
					GUI.Label(new Rect(r.x + 3, r.y, 16, 16), "Waitting...");
				}
			}
			EditorGUILayout.EndHorizontal();

			GUI.enabled = !waitTillPlistHasBeenWritten;

			//Show new entry box
			if (showNewEntryBox)
			{
				GUILayout.BeginHorizontal(GUI.skin.box);
				{
					GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
					{
						newKey = EditorGUILayout.TextField("Key", newKey);

						switch (selectedType)
						{
							default:
							case ValueType.String:
								newValueString = EditorGUILayout.TextField("Value", newValueString);
								break;
							case ValueType.Float:
								newValueFloat = EditorGUILayout.FloatField("Value", newValueFloat);
								break;
							case ValueType.Integer:
								newValueInt = EditorGUILayout.IntField("Value", newValueInt);
								break;
						}

						selectedType = (ValueType)EditorGUILayout.EnumPopup("Type", selectedType);
					}
					GUILayout.EndVertical();

					GUILayout.BeginVertical(GUILayout.Width(1));
					{
						GUILayout.BeginHorizontal();
						{
							GUILayout.FlexibleSpace();

							if (GUILayout.Button(new GUIContent("X", "Close"), EditorStyles.boldLabel, GUILayout.ExpandWidth(false)))
							{
								showNewEntryBox = false;
							}
						}
						GUILayout.EndHorizontal();

						if (GUILayout.Button(new GUIContent("Add", "Add a new key-value.")))
						{
							if (!string.IsNullOrEmpty(newKey))
							{
								switch (selectedType)
								{
									case ValueType.Integer:
										PlayerPrefs.SetInt(newKey, newValueInt);
										ppeList.Add(new PlayerPrefsEntry(newKey, newValueInt));
										break;
									case ValueType.Float:
										PlayerPrefs.SetFloat(newKey, newValueFloat);
										ppeList.Add(new PlayerPrefsEntry(newKey, newValueFloat));
										break;
									default:
									case ValueType.String:
										PlayerPrefs.SetString(newKey, newValueString);
										ppeList.Add(new PlayerPrefsEntry(newKey, newValueString));
										break;
								}
								PlayerPrefs.Save();
								Sort();
							}

							newKey = newValueString = "";
							newValueInt = 0;
							newValueFloat = 0;
							GUIUtility.keyboardControl = 0;	//move focus from textfield, else the text won't be cleared
							showNewEntryBox = false;
						}
					}
					GUILayout.EndVertical();
				}
				GUILayout.EndHorizontal();
			}

			GUILayout.Space(2);

			GUI.backgroundColor = Color.white;

			EditorGUI.indentLevel++;

			//Show all PlayerPrefs
			scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
			{
				EditorGUILayout.BeginVertical();
				{
					for (int i = 0; i < filteredPpeList.Count; i++)
					{
						if (filteredPpeList[i].Value != null)
						{
							EditorGUILayout.BeginHorizontal();
							{
								filteredPpeList[i].IsSelected = GUILayout.Toggle(filteredPpeList[i].IsSelected, new GUIContent("", "Toggle selection."), filteredPpeList[i].HasChanged ? boldToggleStyle : EditorStyles.toggle, GUILayout.MaxWidth(20), GUILayout.MinWidth(20), GUILayout.ExpandWidth(false));
								filteredPpeList[i].Key = EditorGUILayout.TextField(filteredPpeList[i].Key, filteredPpeList[i].HasChanged ? boldNumberFieldStyle : EditorStyles.numberField, GUILayout.MaxWidth(125), GUILayout.MinWidth(40), GUILayout.ExpandWidth(true));

								GUIStyle numberFieldStyle = filteredPpeList[i].HasChanged ? boldNumberFieldStyle : EditorStyles.numberField;

								switch (filteredPpeList[i].Type)
								{
									default:
									case ValueType.String:
										filteredPpeList[i].Value = EditorGUILayout.TextField("", (string)filteredPpeList[i].Value, numberFieldStyle, GUILayout.MinWidth(40));
										break;
									case ValueType.Float:
										filteredPpeList[i].Value = EditorGUILayout.FloatField("", (float)filteredPpeList[i].Value, numberFieldStyle, GUILayout.MinWidth(40));
										break;
									case ValueType.Integer:
										filteredPpeList[i].Value = EditorGUILayout.IntField("", (int)filteredPpeList[i].Value, numberFieldStyle, GUILayout.MinWidth(40));
										break;
								}

								GUILayout.Label(new GUIContent("?", filteredPpeList[i].Type.ToString()), GUILayout.ExpandWidth(false));

								GUI.enabled = filteredPpeList[i].HasChanged && !waitTillPlistHasBeenWritten;
								if (GUILayout.Button(new GUIContent("Save", "Save changes made to this value."), GUILayout.ExpandWidth(false)))
								{
									filteredPpeList[i].SaveChanges();
								}

								if (GUILayout.Button(new GUIContent("Undo", "Discard changes made to this value."), GUILayout.ExpandWidth(false)))
								{
									filteredPpeList[i].RevertChanges();
								}

								GUI.enabled = !waitTillPlistHasBeenWritten;

								if (GUILayout.Button(new GUIContent("Delete", "Delete this key-value."), GUILayout.ExpandWidth(false)))
								{
									PlayerPrefs.DeleteKey(filteredPpeList[i].Key);
									ppeList.Remove(filteredPpeList[i]);
									PlayerPrefs.Save();

									UpdateFilteredList();
									break;
								}
							}
							EditorGUILayout.EndHorizontal();
						}
					}
				}
				EditorGUILayout.EndVertical();
			}
			EditorGUILayout.EndScrollView();

			EditorGUI.indentLevel--;
		}

		#region Menu Actions

		private void OnChangeSortModeClicked()
		{
			sortAscending = !sortAscending;
			Sort();
		}

		private void OnNewEntryClicked()
		{
			showNewEntryBox = true;
		}

		private void OnImport()
		{
			string importPath = EditorUtility.OpenFilePanel("Import PlayerPrefs", "", "ppe");
			if (!string.IsNullOrEmpty(importPath))
			{
				FileInfo fi = new FileInfo(importPath);
				Dictionary<string, object> plist = (Dictionary<string, object>)Prefslist.readPlist(fi.FullName);

				foreach (KeyValuePair<string, object> kvp in plist)
				{
					PlayerPrefsEntry entry = null;

					if (kvp.Value is float)
						entry = new PlayerPrefsEntry(kvp.Key, (float)kvp.Value);
					else if (kvp.Value is int)
						entry = new PlayerPrefsEntry(kvp.Key, (int)kvp.Value);
					else if (kvp.Value is string)
						entry = new PlayerPrefsEntry(kvp.Key, (string)kvp.Value);

					if (entry != null)
					{
						PlayerPrefsEntry existingEntry = ppeList.Find(p => p.Key == entry.Key);
						if (existingEntry == null)
						{
							ppeList.Add(entry);
							entry.SaveChanges();
						}
						else if (!entry.Value.Equals(existingEntry.Value) && EditorUtility.DisplayDialog("Duplicate entry found", string.Format("Key \"{0}\" with value \"{1}\" already exists.{2}Do you want to overwrite it with value \"{3}\" ?", existingEntry.Key, existingEntry.Value, Environment.NewLine, entry.Value), "Yes", "No"))
						{
							ppeList.Remove(existingEntry);
							ppeList.Add(entry);
							entry.SaveChanges();
						}						
					}
				}

				Sort();
				Repaint();
			}
		}

		private void OnExportSelected()
		{
			Export(true);
		}

		private void OnExportAllClicked()
		{
			Export(false);
		}

		private void OnDeleteSelectedClicked()
		{
			if (isOneSelected)
			{
				if (!waitTillPlistHasBeenWritten)
				{
					if (EditorUtility.DisplayDialog("Are you sure?", "Are you sure you want to delete the selected keys? There is no undo!", "Delete", "Cancel"))
					{
						int count = filteredPpeList.Count - 1;
						for (int i = count; i >= 0; i--)
						{
							if (filteredPpeList[i].IsSelected)
							{
								PlayerPrefs.DeleteKey(filteredPpeList[i].Key);
								ppeList.Remove(filteredPpeList[i]);
							}
						}

						PlayerPrefs.Save();
						UpdateFilteredList();
					}
				}
				else
					Debug.LogError("Cannot delete PlayerPrefs entries because it is still loading.");
			}
			else
				Debug.LogError("Cannot delete PlayerPrefs entries because no entries has been selected.");
		}

		private void OnDeleteAllClicked()
		{
			for (int i = 0; i < ppeList.Count; i++)
			{
				ppeList[i].IsSelected = true;
			}
			isOneSelected = true;

			OnDeleteSelectedClicked();
		}

		private void Export(bool onlySelected)
		{
			Dictionary<string, object> entries = new Dictionary<string, object>();
			for (int i = 0; i < filteredPpeList.Count; i++)
			{
				PlayerPrefsEntry entry = filteredPpeList[i];
				if (!entries.ContainsKey(entry.Key))
				{
					if (onlySelected == false)
					{
						entries.Add(entry.Key, entry.Value);
					}
					else if (entry.IsSelected)
					{
						entries.Add(entry.Key, entry.Value);
					}
				}
			}

			if (onlySelected && entries.Count == 0)
			{
				Debug.LogError("Cannot export selected entries as no entries has been selected.");
			}
			else
			{
				string exportPath = EditorUtility.SaveFilePanel("Export all PlayPrefs entries", Application.dataPath, PlayerSettings.productName + "_PlayerPrefs", "ppe");

				if (!string.IsNullOrEmpty(exportPath))
				{
					string xml = Prefslist.writeXml(entries);
					File.WriteAllText(exportPath, xml);
					AssetDatabase.Refresh();
				}
			}
		}

		#endregion

		#region SearchMenu Actions

		private void OnSearchAllClicked()
		{
			SearchFilter = SearchFilterType.All;
		}

		private void OnSearchKeyClicked()
		{
			SearchFilter = SearchFilterType.Key;
		}

		private void OnSearchValueClicked()
		{
			SearchFilter = SearchFilterType.Value;
		}

		private void UpdateFilteredList()
		{
			filteredPpeList.Clear();

			if (!string.IsNullOrEmpty(SearchString))
			{
				for (int i = 0; i < ppeList.Count; i++)
				{
					if (SearchFilter == SearchFilterType.Key || SearchFilter == SearchFilterType.All)
					{
						if (ppeList[i].Key.ToLowerInvariant().Contains(SearchString.Trim().ToLowerInvariant()))
						{
							filteredPpeList.Add(ppeList[i]);
						}
					}

					//For the future: Figure out how to filter also on int/float and deal with dot '.' and comma ',' for different cultures
					if ((SearchFilter == SearchFilterType.Value || SearchFilter == SearchFilterType.All) && ppeList[i].Type == ValueType.String)
					{
						if (!filteredPpeList.Contains(ppeList[i]))  //Prevent duplicates when 'All' is selected
						{
							if (((string)ppeList[i].Value).ToLowerInvariant().Contains(SearchString.Trim().ToLowerInvariant()))
							{
								filteredPpeList.Add(ppeList[i]);
							}
						}
					}
				}
			}
			else
			{
				filteredPpeList.AddRange(ppeList);
			}
		}

		#endregion
		
		private void Sort()
		{
			if (sortAscending)
				ppeList.Sort(PlayerPrefsEntry.SortByNameAscending);
			else
				ppeList.Sort(PlayerPrefsEntry.SortByNameDescending);

			UpdateFilteredList();
		}

		private void RefreshKeys()
		{
			ppeList.Clear();
			string[] allKeys = GetAllKeys();

			for (int i = 0; i < allKeys.Length; i++)
			{
				ppeList.Add(new PlayerPrefsEntry(allKeys[i]));
			}

			//Updating of filtered list happens in Sort()
			Sort();
			Repaint();
		}

		private string[] GetAllKeys()
		{
			List<string> result = new List<string>();

			if (Application.platform == RuntimePlatform.WindowsEditor)
				result.AddRange(GetAllWindowsKeys());
			else if (Application.platform == RuntimePlatform.OSXEditor)
				result.AddRange(GetAllMacKeys());
#if UNITY_EDITOR_LINUX
			else if (Application.platform == RuntimePlatform.LinuxEditor) 
				result.AddRange(GetAllLinuxKeys());
#endif
			else
				Debug.LogError("Unsupported platform detected, please contact support@rejected-games.com and let us know.");

			//Remove something Unity sometimes saves in your PlayerPrefs
			for (int i = 0; i < UNITY_HIDDEN_SETTINGS.Length; i++) 
			{
				if (result.Contains (UNITY_HIDDEN_SETTINGS [i]))
					result.Remove (UNITY_HIDDEN_SETTINGS [i]);
			}

			return result.ToArray();
		}

		/// <summary>
		/// On Mac OS X PlayerPrefs are stored in ~/Library/Preferences folder, in a file named unity.[company name].[product name].plist, where company and product names are the names set up in Project Settings. The same .plist file is used for both Projects run in the Editor and standalone players. 
		/// </summary>
		private string[] GetAllMacKeys()
		{
			string plistPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "/Library/Preferences/unity." + PlayerSettings.companyName + "." + PlayerSettings.productName + ".plist";
			string[] keys = new string[0];

			if (File.Exists(plistPath))
			{
				FileInfo fi = new FileInfo(plistPath);
				Dictionary<string, object> plist = (Dictionary<string, object>)Prefslist.readPlist(fi.FullName);

				keys = new string[plist.Count];
				plist.Keys.CopyTo(keys, 0);
			}

			return keys;
		}

		/// <summary>
		/// On Windows, PlayerPrefs are stored in the registry under HKCU\Software\[company name]\[product name] key, where company and product names are the names set up in Project Settings.
		/// </summary>
		private string[] GetAllWindowsKeys()
		{
			RegistryKey cuKey = Registry.CurrentUser;
			RegistryKey unityKey;

			//The default location of PlayerPrefs pre Unity 5_5
#if UNITY_5_5_OR_NEWER
			unityKey = cuKey.CreateSubKey("Software\\Unity\\UnityEditor\\" + PlayerSettings.companyName + "\\" + PlayerSettings.productName);
#else
			unityKey = cuKey.CreateSubKey("Software\\" + PlayerSettings.companyName + "\\" + PlayerSettings.productName);

			if (unityKey.GetValueNames().Length == 0)
			{
				//On some machines (Windows 7 & 8 64bit using Unity5.4) PlayersPrefs are saved in HKEY_CURRENT_USER\SOFTWARE\AppDataLow\Software\CompanyName\ProjectName weird enough...
				unityKey = cuKey.CreateSubKey("Software\\AppDataLow\\Software\\" + PlayerSettings.companyName + "\\" + PlayerSettings.productName);
			}
#endif

			string[] values = unityKey.GetValueNames();
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = values[i].Substring(0, values[i].LastIndexOf("_"));
            }

            return values;
        }

		/// <summary>
		/// On Linux, PlayersPrefs are stored in /home/lukas/.config/unity3d/[companyName]/[productName]/prefs, where company and product names are the names set up in Project Settings.
		/// </summary>
		private string[] GetAllLinuxKeys()
		{
			string path = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "/.config/unity3d/" + PlayerSettings.companyName + "/" + PlayerSettings.productName + "/prefs";
			List<string> keys = new List<string>();

			XmlDocument xmlDoc = new XmlDocument();
			if (System.IO.File.Exists(path))
			{
				xmlDoc.LoadXml(System.IO.File.ReadAllText(path));
			}

			foreach (XmlElement node in xmlDoc.SelectNodes("unity_prefs/pref"))
			{
				keys.Add(node.GetAttribute("name"));
			}

			return keys.ToArray();
		}

		private bool IsUnityWritingToPlist()
		{
			bool result = false;

			//Mac specific, unfortunately no editor_mac preprocessor is available
			if (Application.platform == RuntimePlatform.OSXEditor)
			{
				//Find the tempPlistFile, while it exists we know Unity is still busy writen the last version of PlayerPrefs
				string preferencesPath = string.Format("{0}/Library/Preferences/", Environment.GetFolderPath(Environment.SpecialFolder.Personal));
				FileInfo plistFile = new FileInfo(string.Format("{0}unity.{1}.{2}.plist", preferencesPath, PlayerSettings.companyName, PlayerSettings.productName));
				DirectoryInfo di = new DirectoryInfo(preferencesPath);
				FileInfo[] allPreferenceFiles = di.GetFiles();

				foreach (FileInfo fi in allPreferenceFiles)
				{
					if (fi.FullName.Contains(plistFile.FullName))
					{
						if (!fi.FullName.EndsWith(".plist"))
						{
							tmpPlistFile = fi;
							result = true;
						}
					}
				}
			}
			else if (Application.platform == RuntimePlatform.WindowsEditor)
				result = false;

			return result;
		}

		private string SearchString
		{
			get { return _searchString; }
			set
			{
				if (_searchString != value)
				{
					_searchString = value;

					UpdateFilteredList();
				}
			}
		}

		private SearchFilterType SearchFilter
		{
			get { return _searchFilter; }
			set
			{
				if (_searchFilter != value)
				{
					_searchFilter = value;
					UpdateFilteredList();
				}
			}
		}

		private class PlayerPrefsEntry
		{
			private string key;
			private object value;

			public ValueType Type;
			public bool IsSelected = false;
			public bool HasChanged = false;

			private string oldKey;

			public PlayerPrefsEntry(string key)
			{
				this.key = key;
				oldKey = key;

				RetrieveValue();
			}

			public PlayerPrefsEntry(string key, string value)
			{
				this.key = key;
				this.value = value;
				this.Type = ValueType.String;
			}

			public PlayerPrefsEntry(string key, float value)
			{
				this.key = key;
				this.value = value;
				this.Type = ValueType.Float;
			}

			public PlayerPrefsEntry(string key, int value)
			{
				this.key = key;
				this.value = value;
				this.Type = ValueType.Integer;
			}

			public void SaveChanges()
			{
				switch (Type)
				{
					default:
					case ValueType.String:
						PlayerPrefs.SetString(Key, (string)Value);
						break;
					case ValueType.Float:
						PlayerPrefs.SetFloat(Key, (float)Value);
						break;
					case ValueType.Integer:
						PlayerPrefs.SetInt(Key, (int)Value);
						break;
				}

				if (oldKey != Key)
				{
					PlayerPrefs.DeleteKey(oldKey);
					oldKey = Key;
				}

				HasChanged = false;

				//Incase the user exits without saving project
				PlayerPrefs.Save();
			}

			public void RevertChanges()
			{
				RetrieveValue();
			}

			public void RetrieveValue()
			{
				Key = oldKey;

				if (PlayerPrefs.GetString(Key, UNIQUE_STRING) != UNIQUE_STRING)
				{
					Type = ValueType.String;
					value = PlayerPrefs.GetString(Key);
				}
				else if (PlayerPrefs.GetInt(Key, UNIQUE_INT) != UNIQUE_INT)
				{
					Type = ValueType.Integer;
					value = PlayerPrefs.GetInt(Key);
				}
				else if (PlayerPrefs.GetFloat(Key, UNIQUE_FLOAT) != UNIQUE_FLOAT)
				{
					Type = ValueType.Float;
					value = PlayerPrefs.GetFloat(Key);
				}

				oldKey = Key;

				//Don't mark the first set Value as changed
				HasChanged = false;
			}

			public string Key
			{
				get
				{
					return key;
				}

				set
				{
					if (value != key)
					{
						HasChanged = true;
						key = value;
					}
				}
			}

			public object Value
			{
				get
				{
					return this.value;
				}

				set
				{
					if (!value.Equals(this.value))
					{
						this.value = value;

						HasChanged = true;
					}
				}
			}

			public static int SortByNameAscending(PlayerPrefsEntry a, PlayerPrefsEntry b)
			{
				return string.Compare(a.Key, b.Key);
			}

			public static int SortByNameDescending(PlayerPrefsEntry a, PlayerPrefsEntry b)
			{
				return string.Compare(b.Key, a.Key);
			}
		}

		private enum ValueType
		{
			String,
			Float,
			Integer
		}

		private enum SearchFilterType
		{
			All,
			Key,
			Value
		}


		//Unity Build-in styles
		private GUIStyle ToolbarSeachTextFieldPopup { get { return GetStyle("ToolbarSeachTextFieldPopup"); } }
		private GUIStyle ToolbarSeachCancelButton { get { return GetStyle("ToolbarSeachCancelButton"); } }

		private GUIStyle GetStyle(string styleName)
		{
			GUIStyle guiStyle = GUI.skin.FindStyle(styleName) ?? EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).FindStyle(styleName);
			if (guiStyle == null)
			{
				Debug.LogError((object)("Missing built-in guistyle " + styleName));
				guiStyle = GUI.skin.button;
			}
			return guiStyle;
		}
	}
}