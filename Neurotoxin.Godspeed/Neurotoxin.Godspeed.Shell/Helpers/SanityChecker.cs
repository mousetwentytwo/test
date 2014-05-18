using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using Microsoft.Practices.Composite.Events;
using Neurotoxin.Godspeed.Core.Caching;
using Neurotoxin.Godspeed.Core.Extensions;
using Neurotoxin.Godspeed.Presentation.Extensions;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.ContentProviders;
using Neurotoxin.Godspeed.Shell.Events;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Neurotoxin.Godspeed.Shell.Models;
using Neurotoxin.Godspeed.Shell.Reporting;
using Neurotoxin.Godspeed.Shell.ViewModels;
using Neurotoxin.Godspeed.Shell.Views.Dialogs;
using System.Linq;
using Resx = Neurotoxin.Godspeed.Shell.Properties.Resources;

namespace Neurotoxin.Godspeed.Shell.Helpers
{
    public class SanityChecker
    {
        private const string PARTICIPATIONMESSAGEKEY = "ParticipationMessage";
        private const string FACEBOOKMESSAGEKEY = "FacebookMessage";
        private const string CODEPLEXMESSAGEKEY = "CodeplexMessage";
        private const string NEWVERSIONAVAILABLEMESSAGEKEY = "NewVersionAvailableMessage";
        private const string NEWERDOTNETVERSIONREQUIREDMESSAGEKEY = "NewerDotNetVersionRequiredMessage";

        private readonly Timer _participationTimer;
        private readonly Timer _facebookTimer;
        private readonly Timer _codeplexTimer;

        private readonly IUserSettings _userSettings;
        private readonly IEventAggregator _eventAggregator;

        public SanityChecker(IStatisticsViewModel statistics, IUserSettings userSettings, IEventAggregator eventAggregator)
        {
            _userSettings = userSettings;
            _eventAggregator = eventAggregator;
            eventAggregator.GetEvent<CachePopulatedEvent>().Subscribe(OnCachePopulated);
            eventAggregator.GetEvent<ShellInitializedEvent>().Subscribe(OnShellInitialized);

            if (!_userSettings.DisableUserStatisticsParticipation.HasValue)
            {
                _participationTimer = new Timer(ParticipationMessage, null, 60000, -1);
            }
            if (!_userSettings.IsMessageIgnored(FACEBOOKMESSAGEKEY))
            {
                _facebookTimer = new Timer(FacebookMessage, null, 600000, -1);
            }
            if (!_userSettings.IsMessageIgnored(CODEPLEXMESSAGEKEY) && statistics.ApplicationStarted > 9 && statistics.TotalUsageTime > new TimeSpan(0, 2, 0, 0))
            {
                _codeplexTimer = new Timer(CodeplexMessage, null, 60000, -1);
            }
        }

        private void OnCachePopulated(CachePopulatedEventArgs e)
        {
            const int requiredCacheVersion = 3;
            var actualCacheVersion = e.CacheStore.TryGet("CacheVersion", 1);
            if (actualCacheVersion == requiredCacheVersion) return;

            Action setCacheVersion = () => e.CacheStore.Update("CacheVersion", requiredCacheVersion);

            var cacheKeys = e.CacheStore.Keys.Where(k => k.StartsWith(Strings.UserMessageCacheItemPrefix)).ToList();
            if (cacheKeys.Any())
            {
                var resx = Resx.ResourceManager.GetResourceSet(CultureInfo.InvariantCulture, true, false);
                foreach (DictionaryEntry entry in resx)
                {
                    var resxKey = (string)entry.Key;
                    if (!resxKey.EndsWith("Message")) continue;
                    var resxValue = (string)entry.Value;
                    var cacheKey = cacheKeys.FirstOrDefault(k => k == Strings.UserMessageCacheItemPrefix + resxValue.Hash());
                    if (cacheKey == null) continue;
                    e.CacheStore.Remove(cacheKey);
                    e.CacheStore.Set(Strings.UserMessageCacheItemPrefix + resxKey.Hash(), true);
                }
            }

            if (actualCacheVersion == 1 && e.InMemoryCacheItems.Count != 0)
            {
                var payload = ApplicationExtensions.GetContentByteArray("/Resources/xbox_logo.png");
                var args = new CacheMigrationEventArgs(MigrateCacheItemVersion1ToVersion2,
                    setCacheVersion,
                    e.InMemoryCacheItems,
                    e.CacheStore,
                    payload);
                _eventAggregator.GetEvent<CacheMigrationEvent>().Publish(args);
            }
            else
            {
                setCacheVersion();
            }
        }

        private void OnShellInitialized(ShellInitializedEventArgs e)
        {
            CheckFrameworkVersion();
            RetryFailedUserReports();
            if (_userSettings.UseVersionChecker) CheckForNewerVersion();
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

            var args = new NotifyUserMessageEventArgs(NEWERDOTNETVERSIONREQUIREDMESSAGEKEY, MessageIcon.Warning, MessageCommand.OpenUrl, "http://www.microsoft.com/en-us/download/details.aspx?id=40779");
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
                var args = new NotifyUserMessageEventArgs(NEWVERSIONAVAILABLEMESSAGEKEY, MessageIcon.Info, MessageCommand.OpenUrl, "http://godspeed.codeplex.com", MessageFlags.None, info.Item1, info.Item2);
                _eventAggregator.GetEvent<NotifyUserMessageEvent>().Publish(args);
            });
        }

        private static void RetryFailedUserReports()
        {
            var queue = Directory.GetDirectories(Path.Combine(App.DataDirectory, "post"));
            foreach (var item in queue.OrderBy(f => f))
            {
                var file = Directory.GetFiles(item).First();
                if (!HttpForm.Repost(file)) continue;
                Directory.Delete(item, true);
            }
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

        private void ParticipationMessage(object state)
        {
            _participationTimer.Dispose();
            var args = new NotifyUserMessageEventArgs(PARTICIPATIONMESSAGEKEY, MessageIcon.Info, MessageCommand.OpenDialog, typeof(UserStatisticsParticipationDialog));
            _eventAggregator.GetEvent<NotifyUserMessageEvent>().Publish(args);
        }

        private void FacebookMessage(object state)
        {
            _facebookTimer.Dispose();
            var args = new NotifyUserMessageEventArgs(FACEBOOKMESSAGEKEY, MessageIcon.Info, MessageCommand.OpenUrl, "http://www.facebook.com/godspeedftp", MessageFlags.Ignorable | MessageFlags.IgnoreAfterOpen);
            _eventAggregator.GetEvent<NotifyUserMessageEvent>().Publish(args);
        }

        private void CodeplexMessage(object state)
        {
            _codeplexTimer.Dispose();
            var args = new NotifyUserMessageEventArgs(CODEPLEXMESSAGEKEY, MessageIcon.Info, MessageCommand.OpenUrl, "http://godspeed.codeplex.com", MessageFlags.Ignorable | MessageFlags.IgnoreAfterOpen);
            _eventAggregator.GetEvent<NotifyUserMessageEvent>().Publish(args);
        }

    }
}