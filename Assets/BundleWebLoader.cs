using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
using System.IO;
using System.Security.Cryptography;
 
public class BundleWebLoader : MonoBehaviour
{
    public string bundleUrl = "http://localhost/assetbundles/testbundle";
    public string assetName = "BundledObject";
    readonly object sync = new object();

    public Text DownloadedText;
     
    // Start is called before the first frame update
    IEnumerator Start()
    {
        using (WWW web = new WWW(bundleUrl))
        {
                yield return web;
                AssetBundle remoteAssetBundle = web.assetBundle;
                if (remoteAssetBundle == null) {
                    Debug.LogError("Failed to download AssetBundle!");
                yield break;
                }

                var http = new WebClient();
                http.DownloadProgressChanged += ProgressCallback;
                //http.DownloadFileCompleted += CompletedCallBack;
                
                Task.Run( () => http.DownloadFileTaskAsync(bundleUrl, assetName)).Wait();

                if (ComputeMD5Checksum(assetName) == FileSumm[0])
                {
                    Instantiate(remoteAssetBundle.LoadAsset(assetName));
                    remoteAssetBundle.Unload(false);
                }
                else
                {
                    Debug.LogError("File CheckSumm Error, Trying download new file");
                }
        }
    }

    private string progressbar;

    void Update()
    {
        DownloadedText.text = $"Downloaded: {progressbar}";
    }

    //private void CompletedCallBack(object sender, AsyncCompletedEventArgs e) => Debug.Log("Downloaded Complete!");

    private void ProgressCallback(object sender, DownloadProgressChangedEventArgs e)
    {
        var progress = string.Format("|{0,-30}| {1,3}% {2,9} / {3,-9}",
            new string((char)0x2592, e.ProgressPercentage * 30 / 100),
            e.ProgressPercentage,
            FormatSize(e.BytesReceived),
            FormatSize(e.TotalBytesToReceive));

        lock (sync)
        {
            Debug.Log($"Downloaded: {progress}");
            progressbar = progress;
        }
    }

    private string FormatSize(double size)
    {
        int grade = (int)Math.Log(size, 1024);
        grade = Math.Min(grade, SubSuffixes.Length);

        return string.Format("{0:N2}{1}", size / Math.Pow(1024, grade), SubSuffixes[grade]);
    }

    private string[] SubSuffixes =
    {
        "B",
        "KB",
        "MB",
        "GB",
        "TB"
    };


    private string[] FileSumm = 
    {
        "2108AF3FA2DC988D5641923F75306326"
    };

    private string ComputeMD5Checksum(string path)
    {
            using (FileStream fs = System.IO.File.OpenRead(path))
            {
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] fileData = new byte[fs.Length];
                fs.Read(fileData, 0, (int)fs.Length);
                byte[] checkSum = md5.ComputeHash(fileData);
                string result = BitConverter.ToString(checkSum).Replace("-", String.Empty);
                return result;
            }
    }
}