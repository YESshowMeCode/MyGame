// ========================================================
// Author：ChenGaoshuang 
// CreateTime：2020/04/20 17:28:16
// FileName：Assets/Scripts/Tools/FileWatcher.cs
// ========================================================



using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
/// <summary>
/// 监听文件改变
/// </summary>
public sealed class FileWatcher : MonoBehaviour
{
#if UNITY_EDITOR_WIN 
    class WatchInfo
    {
        FileSystemWatcher watcher;
        string fileter;
        List<string> changeFiles;
        System.Action<string> changeAction;
        string dirPath;

        public WatchInfo(string _dirPath, string _filter, System.Action<string> _changeAction)
        {
            watcher = new FileSystemWatcher();
            changeFiles = new List<string>();
            fileter = _filter;
            dirPath = _dirPath;
            changeAction = _changeAction;
            CreateWatcher(watcher, dirPath, changeFiles, fileter);
        }

        public void UpdateLs()
        {
            if (changeFiles.Count > 0 && null != changeAction)
            {
                foreach (var changeFile in changeFiles)
                {
                    changeAction(changeFile);
                }
                changeFiles.Clear();
            }
        }
        private void AddToLs(List<string> ls, string elem)
        {
            if (!ls.Contains(elem))
            {
                ls.Add(elem);
            }
        }

        private void CreateWatcher(FileSystemWatcher watcher, string path, List<string> changeLs, string fileFilter)
        {
            CreateWatcher(watcher, path, fileFilter, (object source, FileSystemEventArgs e) =>
            {
                AddToLs(changeLs, e.FullPath);
            }, (object source, RenamedEventArgs e) => {
                AddToLs(changeLs, e.FullPath);
            });
        }

        private void CreateWatcher(FileSystemWatcher watcher, string path, string fileFilter, FileSystemEventHandler onChanged, RenamedEventHandler onRenamed)
        {
            watcher.Path = Path.GetFullPath(path);
            // Watch for changes in LastAccess and LastWrite times, and
            // the renaming of files or directories.
            watcher.NotifyFilter = NotifyFilters.LastWrite;
            // Only watch text files.
            watcher.Filter = fileFilter;
            watcher.IncludeSubdirectories = true;
            // Add event handlers.
            watcher.Changed += onChanged;
            watcher.Created += onChanged;
            watcher.Deleted += onChanged;
            watcher.Renamed += onRenamed;
            // Begin watching.
            watcher.EnableRaisingEvents = true;
        }
    }
    private static FileWatcher _instance;
    private List<WatchInfo> watchInfos = new List<WatchInfo>();
    public static void Create(GameObject go)
    {
        _instance = go.AddComponent<FileWatcher>();
    }

    IEnumerator Start()
    {
        //watchInfos.Add(new WatchInfo(GameSetting.codePath, "*.lua", (changeCode) => {
        //    string s = changeCode.Replace(Path.GetFullPath(GameSetting.codePath + "src/"), "").Replace("\\", ".").Replace(".lua", "");
        //    Game.Client.Instance.OnMessage("codechange:" + s);
        //}));
        yield return null;
    }
    void LateUpdate()
    {
        foreach (var watcher in watchInfos)
        {
            watcher.UpdateLs();
        }
    }
# endif
}
