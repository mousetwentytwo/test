using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using Microsoft.Practices.Composite.Events;
using Neurotoxin.Godspeed.Core.Caching;
using Neurotoxin.Godspeed.Core.Extensions;
using Neurotoxin.Godspeed.Presentation.Extensions;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.ContentProviders;
using Neurotoxin.Godspeed.Shell.Events;
using Neurotoxin.Godspeed.Shell.Models;

namespace Neurotoxin.Godspeed.Shell.Helpers
{
    public class SanityChecker
    {
        private readonly IEventAggregator _eventAggregator;

        public SanityChecker(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            eventAggregator.GetEvent<CachePopulatedEvent>().Subscribe(OnCachePopulated);
            eventAggregator.GetEvent<ShellInitializedEvent>().Subscribe(OnShellInitialized);
        }

        private void OnCachePopulated(CachePopulatedEventArgs e)
        {
            const int requiredCacheVersion = 2;
            var actualCacheVersion = e.CacheStore.TryGet("CacheVersion", 1);
            if (actualCacheVersion == requiredCacheVersion) return;

            Action setCacheVersion = () => e.CacheStore.Update("CacheVersion", requiredCacheVersion);
            if (e.InMemoryCacheItems.Count == 0)
            {
                setCacheVersion();
                return;
            }

            var payload = ApplicationExtensions.GetContentByteArray("/Resources/xbox_logo.png");
            var args = new CacheMigrationEventArgs(MigrateCacheItemVersion1ToVersion2, 
                                                   setCacheVersion, 
                                                   e.InMemoryCacheItems,
                                                   e.CacheStore, 
                                                   payload);
            _eventAggregator.GetEvent<CacheMigrationEvent>().Publish(args);
        }

        private void OnShellInitialized(ShellInitializedEventArgs e)
        {
            CheckFrameworkVersion();
            if (UserSettings.UseVersionChecker) CheckForNewerVersion();
        }

        private void CheckFrameworkVersion()
        {
            var applicationAssembly = Assembly.GetAssembly(typeof(Application));
            var fvi = FileVersionInfo.GetVersionInfo(applicationAssembly.Location);
            var actualVersion = new Version(fvi.ProductVersion);
            var requiredVersion = new Version(4, 0, 30319, 18408);

            var versionOk = actualVersion >= requiredVersion;
            GlobalVariables.DataGridSupportsRenaming = versionOk;
            if (versionOk) return;

            const string message = "<b>Warning!</b> Some of the features require .NET version 4.0.30319.18408 (October 2013) or newer. Please update .NET Framework and restart GODspeed to enable those features.";
            var args = new NotifyUserMessageEventArgs(message, MessageIcon.Info, MessageCommand.OpenUrl, "http://www.microsoft.com/en-us/download/details.aspx?id=40779");
            _eventAggregator.GetEvent<NotifyUserMessageEvent>().Publish(args);
        }

        private void CheckForNewerVersion()
        {
            var asm = Assembly.GetExecutingAssembly();
            var title = asm.GetAttribute<AssemblyTitleAttribute>().Title;
            const string url = "https://godspeed.codeplex.com/";
            WorkerThread.Run(() =>
            {
                try
                {
                    var request = WebRequest.Create(url);
                    var response = request.GetResponse();
                    var titlePattern = new Regex(@"\<span class=""rating_header""\>current.*?\<td\>(.*?)\</td\>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                    var datePattern = new Regex(@"\<span class=""rating_header""\>date.*?\<td\>.*?LocalTimeTicks=""(.*?)""", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                    string html;
                    using (var stream = response.GetResponseStream())
                    {
                        var sr = new StreamReader(stream, Encoding.UTF8);
                        html = sr.ReadToEnd();
                        sr.Close();
                    }
                    var latestTitle = titlePattern.Match(html).Groups[1].Value.Trim();
                    var latestDate = new DateTime(1970, 1, 1);
                    latestDate = latestDate.AddSeconds(long.Parse(datePattern.Match(html).Groups[1].Value)).ToLocalTime();
                    return new Tuple<string, DateTime>(latestTitle, latestDate);
                }
                catch
                {
                    return new Tuple<string, DateTime>(string.Empty, DateTime.MinValue);
                }
            },
            info =>
            {
                if (string.Compare(title, info.Item1, StringComparison.InvariantCultureIgnoreCase) != -1) return;
                var message = string.Format("<b>New version available!</b><br/>{0} ({1:yyyy.MM.dd HH:mm})", info.Item1, info.Item2);
                var args = new NotifyUserMessageEventArgs(message, MessageIcon.Info, MessageCommand.OpenUrl, "http://godspeed.codeplex.com", MessageFlags.None);
                _eventAggregator.GetEvent<NotifyUserMessageEvent>().Publish(args);
            });
        }

        private static void MigrateCacheItemVersion1ToVersion2(string key, CacheEntry<FileSystemItem> value, EsentPersistentDictionary cacheStore, object payload)
        {
            if (value.Content == null)
            {
                cacheStore.Remove(key);
                return;
            }
            if (value.Content.TitleType != TitleType.Game || value.Content.RecognitionState != RecognitionState.NotRecognized) return;

            value.Content.RecognitionState = value.Content.Thumbnail.EqualsWith((byte[]) payload)
                                                 ? RecognitionState.PartiallyRecognized
                                                 : RecognitionState.Recognized;
            cacheStore.Update(key, value);
        }
    }
}