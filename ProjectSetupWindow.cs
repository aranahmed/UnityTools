using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq; // allow us to create new directories inside of Unity
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;


namespace MyTools
{
    public class ProjectSetupWindow : EditorWindow
    {
        #region Variables
        static ProjectSetupWindow win;

        private string gameName;

        private string newFolderName;

        private bool artFolderEnabled;

        bool toggle;
         
        
        private List<string> folderNames = new List<string>
        {
            "Animation",
            "Models",
            "Materials",
            "Prefabs"
        };

        private List<string> directoryNames = new List<string>
        {
            "Art",
            "Code",
            "Resources",
            "Prefabs"
        };
        
        // private string newFolderName {
        //     get;
        //     set;
        // }

        #endregion

        #region MainMethods

        public static void InitWindow()
        {
            win = GetWindow<ProjectSetupWindow>(
                "Project Setup"
            
            );
            win.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            gameName = EditorGUILayout.TextField("Game Name: ", gameName);
            EditorGUILayout.EndHorizontal();


            for (int i = 0; i < folderNames.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();

                
                toggle = GUILayout.Toggle(toggle, folderNames[i]);
                
                EditorGUILayout.EndHorizontal();
            }
                //string newFolderName = EditorGUILayout.TextField(""); 
         


            newFolderName = EditorGUILayout.TextField("", newFolderName);
            
           
            if (GUILayout.Button("Add Folder", GUILayout.Height(15), GUILayout.ExpandWidth(false)))
            {
                EditorGUILayout.BeginHorizontal();

                Debug.Log(newFolderName);
                folderNames.Add(newFolderName);
                EditorGUILayout.EndHorizontal();
            }
           

            if (GUILayout.Button("Create Project Structure", GUILayout.Height(35), GUILayout.ExpandWidth(true)))
            {
                CreateProjectFolder();
            }
            
            
            if (win != null)
            {
                win.Repaint();
            }
        }

        private void OnEnable()
        {
            
        }

        private void OnDisable()
        {
            folderNames.Clear();
        }

        #endregion

        #region Custom Methods

        void CreateProjectFolder()
        {
            if (string.IsNullOrEmpty(gameName))
            {
                 EditorUtility.DisplayDialog("Warning","You need to name your project", "ok");
                 return;
            }


            if (gameName == "Game")
            {
                if (!EditorUtility.DisplayDialog("Project Setup Warning",
                        "Do you really want to call your project Game?",
                        "Yes", "No"))
                {
                    CloseWindow();
                    return;
                }
            }
               
            
            string assetPath = Application.dataPath;
            string rootPath = assetPath + "/" + gameName;

            
            DirectoryInfo rootInfo = Directory.CreateDirectory(rootPath);

            if (!rootInfo.Exists)
            {
                return;
            }

            CreateSubFolders(rootPath);
            
            

            CloseWindow();
        }

        void CreateSubFolders(string rootPath)
        {
            DirectoryInfo rootInfo = null;

            //List<string> folderNames = new List<string>();

            rootInfo = CreateRootDirectory(rootPath + "/" + directoryNames[0]);
          
            if (rootInfo.Exists)
            {
                //folderNames.Clear()
                for (int i = 0; i < folderNames.Count; i++)
                {
                   // folderNames.Clear();
                    CreateFolders(rootPath + "/" + directoryNames[0], folderNames);
                }
            }
            
            //New Directory : Code
            rootInfo = CreateRootDirectory(rootPath + "/" + directoryNames[1]);
            
            if (rootInfo.Exists)
            {
                
                folderNames.Clear();
                
                folderNames.Add("Editor");
                folderNames.Add("Scripts");
                folderNames.Add("Shaders");
                CreateFolders(rootPath + "/" + directoryNames[1], folderNames);
            }
            
            //New Directory : Resources
            rootInfo = CreateRootDirectory(rootPath + "/" + directoryNames[2]);
            
            if (rootInfo.Exists)
            {
                
                folderNames.Clear();
                
                folderNames.Add("Characters");
                folderNames.Add("Managers");
                folderNames.Add("Props");
                folderNames.Add("UI");
                CreateFolders(rootPath + "/" + directoryNames[2], folderNames);
            }
            
            //New Directory : Prefabs
            rootInfo = CreateRootDirectory(rootPath + "/" + directoryNames[3]);
            
            if (rootInfo.Exists)
            {
                
                folderNames.Clear();
                
                folderNames.Add("Characters");
                folderNames.Add("Props");
                folderNames.Add("UI");
                CreateFolders(rootPath + "/" + directoryNames[3], folderNames);
            }
            
            // Create a scene
            DirectoryInfo sceneInfo = CreateRootDirectory(rootPath + "/" + "Scenes");

            if (sceneInfo.Exists)
            {
                CreateScene(rootPath + "/" + "Scenes", gameName + "_Main" );
                CreateScene(rootPath + "/" + "Scenes", gameName + "_Frontend" );
                CreateScene(rootPath + "/" + "Scenes", gameName + "_Startup" );
               
            }
            
        }

        DirectoryInfo CreateRootDirectory(string directoryPath)
        {
            return Directory.CreateDirectory(directoryPath);
        }
        
        void CreateFolders(string aPath, List<string> folders)
        {
            foreach (string folder in folders)
            {
                Directory.CreateDirectory(aPath + "/" + folder);
            }
        }

        void CreateScene(string aPath, string aName)
        {
            Scene currentScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            EditorSceneManager.SaveScene(currentScene, aPath + "/" + aName + ".unity", true);
        }
        
        void CloseWindow()
        {
            if (win)
            {
                win.Close();
            }
        }
        #endregion
    }
}