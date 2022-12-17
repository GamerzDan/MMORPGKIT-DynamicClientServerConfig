using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;


namespace MultiplayerARPG.MMO
{
    [DefaultExecutionOrder(-898)]
    public partial class MMOClientInstance : MonoBehaviour
    {
        public void SetNetworkSettings(MmoNetworkSetting[] NetworkSettings)
        {
            networkSettings = NetworkSettings;
        }
    }
}

/// <summary>
/// MMO.MMOClientInstance.Singleton.networkSettings needs to be changed from private to internal scope
/// </summary>
namespace MultiplayerARPG
{   
    public partial class GameInstance
    {
        /// <summary>
        /// Config file to read server connection data from, CSV file comma separated and each server separated by linebreak
        /// Should be CSV format of:
        /// Title, NetworkAddress, Port
        /// </summary>
        [Header("Dynamic Client Server Properties")]
        [SerializeField]
        protected string serverConfigFileName = "clientServerConfig.txt";

        [SerializeField]
        internal MMO.MmoNetworkSetting[] networkSettings;

        [SerializeField]
        MMO.MmoNetworkSetting _netSetting;

        //WEBGL doesn't support reading local files and Android needs to use WebRequest to read StreamingAssets
#if !UNITY_ANDROID && !UNITY_WEBGL && !UNITY_SERVER
        /// <summary>
        /// By this time MMOClientInstance's singleton is setup, so we try to find localServerConfig files
        /// and if found, replace networkSettings in MMOClientInstance with these
        /// </summary>
        [DevExtMethods("LoadedGameData")]
        public void DevExt_ReadClientServerConfig()
        {
            string full_path = string.Format("{0}/{1}", Application.streamingAssetsPath, serverConfigFileName);
            if (File.Exists(full_path))
            {
                try
                {
                    string csvText = File.ReadAllText(full_path);
                    if(csvText != "")
                    {
                        //Separate multiple servers
                        string[] _servers = csvText.Split(new char[] { '\n' });
                        networkSettings = new MMO.MmoNetworkSetting[_servers.Length];
                        int count = 0;

                        foreach(String _serverData in _servers)
                        {
                            //If read empty line as server, ignore it
                            if(_serverData.Trim() == "") { continue; }
                            //Separate Title, NetworkAddress, Port
                            string[] _config = _serverData.Split(new char[] { ',' });
                            _netSetting = new MMO.MmoNetworkSetting();
                            bool isValid = false;
                            if (_config.Length >= 3)
                            {
                                _netSetting.title = _config[0].Trim();
                                _netSetting.networkAddress = _config[1].Trim();
                                _netSetting.networkPort = Int32.Parse(_config[2].Trim());
                                isValid = true;
                            }
                            if (isValid)
                            {
                                networkSettings[count] = _netSetting;
                            }
                            count++;
                        }

                        //Assign to clientInstance
                        MMO.MMOClientInstance.Singleton.SetNetworkSettings(networkSettings);
                    }
                }
                catch (Exception e)
                {
                    Debug.Log("DevExt_ReadClientServerConfig: " + e.Message);
                }
            } else
            {
                Debug.Log("DevExt_ReadClientServerConfig FileNotFound: " + full_path);
            }
        }

#endif


#if (UNITY_ANDROID || UNITY_WEBGL || UNITY_EDITOR) && !UNITY_SERVER
        /// <summary>
        /// By this time MMOClientInstance's singleton is setup, so we try to find localServerConfig files
        /// and if found, replace networkSettings in MMOClientInstance with these
        /// </summary>
        [DevExtMethods("LoadedGameData")]
        public void DevExt_ReadClientServerConfigWebGL()
        {
            StartCoroutine(WebGLHandleClientServerConfig());
        }

        IEnumerator WebGLHandleClientServerConfig()
        {
            string path = "StreamingAssets/" + serverConfigFileName; //This works because index.html is in the same folder as StreamingAssets ?
            UnityWebRequest uwr = UnityWebRequest.Get(path);
            yield return uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("DevExt_ReadClientServerConfig CannotReadFile: " + uwr.error);
            }
            else
            {
                // Show results as text
                //Debug.Log(uwr.downloadHandler.text);
                try
                {
                    string csvText = uwr.downloadHandler.text;
                    if (csvText != "")
                    {
                        //Separate multiple servers
                        string[] _servers = csvText.Split(new char[] { '\n' });
                        networkSettings = new MMO.MmoNetworkSetting[_servers.Length];
                        int count = 0;

                        foreach (String _serverData in _servers)
                        {
                            //If read empty line as server, ignore it
                            if (_serverData.Trim() == "") { continue; }
                            //Separate Title, NetworkAddress, Port
                            string[] _config = _serverData.Split(new char[] { ',' });
                            _netSetting = new MMO.MmoNetworkSetting();
                            bool isValid = false;
                            if (_config.Length >= 3)
                            {
                                _netSetting.title = _config[0].Trim();
                                _netSetting.networkAddress = _config[1].Trim();
                                _netSetting.networkPort = Int32.Parse(_config[2].Trim());
                                isValid = true;
                            }
                            if (isValid)
                            {
                                networkSettings[count] = _netSetting;
                            }
                            count++;
                        }

                        //Assign to clientInstance
                        MMO.MMOClientInstance.Singleton.SetNetworkSettings(networkSettings);
                    }
                }
                catch (Exception e)
                {
                    Debug.Log("DevExt_ReadClientServerConfig: " + e.Message);
                }
            }
        }

#endif


#if UNITY_EDITOR
            //If our example config file exists in localStreamingAssets folder but not in root folder, copy it there
            private void OnValidate()
        {
            string full_path = string.Format("{0}/{1}", Application.streamingAssetsPath, serverConfigFileName);
            string local_path = RootPath;
            //Debug.Log(local_path);
            if (local_path != "")
            {
                local_path = local_path.Replace("DynamicClientServerConfig.cs", "StreamingAssets");
                //Debug.Log(local_path);
                if (File.Exists(Path.Combine(local_path, serverConfigFileName)) && !File.Exists(full_path))
                {
                    Debug.Log("DynamicClientServerConfig: StreamingAssets folder does not exist at root Assets level, creating it with example data");
                    CopyAll(new DirectoryInfo(local_path), new DirectoryInfo(Application.streamingAssetsPath));
                }
            }
        }
        public static string RootPath
        {
            get
            {
                var result = "";
                var projectPath = Directory.GetParent(Application.dataPath);
                //Debug.Log(projectPath + " " + Path.Combine(projectPath.FullName, "Assets"));
                var res = System.IO.Directory.GetFiles(Path.Combine(projectPath.FullName, "Assets"), "DynamicClientServerConfig.cs", SearchOption.AllDirectories);
                if(res.Length > 0)
                {
                    result = res[0];
                }
                return result;
            }
        }

        public static void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            if (!Directory.Exists(target.FullName))
            {
                Directory.CreateDirectory(target.FullName);
            }

            // Copy each file into the new directory.
            foreach (FileInfo fi in source.GetFiles())
            {
                Console.WriteLine(@"Copying {0}\{1}", target.FullName, fi.Name);
                fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir =
                    target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
        }
#endif

    }
}