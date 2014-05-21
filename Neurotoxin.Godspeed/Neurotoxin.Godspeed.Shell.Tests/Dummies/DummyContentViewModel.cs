using System;
using System.IO;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Neurotoxin.Godspeed.Shell.Models;
using Neurotoxin.Godspeed.Shell.Tests.Helpers;
using Neurotoxin.Godspeed.Shell.ViewModels;

namespace Neurotoxin.Godspeed.Shell.Tests.Dummies
{
    public class DummyContentViewModel : FileListPaneViewModelBase<DummyContent>
    {
        public DummyContentViewModel(FakingRules rules)
        {
            FileManager.FakingRules = rules;
            Initialize();
        }

        protected override string ExportActionDescription
        {
            get { return C.Random<string>(); }
        }

        protected override string ImportActionDescription
        {
            get { return C.Random<string>(); }
        }

        public override bool IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        public override bool IsVerificationEnabled
        {
            get { throw new NotImplementedException(); }
        }

        public override string GetTargetPath(string path)
        {
            return path;
        }

        protected override bool SaveToFileStream(FileSystemItem item, FileStream fs, long remoteStartPosition)
        {
            //TODO: implement mimic logic
            return true;
        }

        protected override bool CreateFile(string targetPath, FileSystemItem source)
        {
            var target = source.Clone();
            target.Path = targetPath;
            FileManager.AddFile(target);
            return true;
        }

        protected override bool OverwriteFile(string targetPath, FileSystemItem source)
        {
            //TODO: implement mimic logic
            return true;
        }

        protected override bool ResumeFile(string targetPath, FileSystemItem source)
        {
            //TODO: implement mimic logic
            return true;
        }

        public override void Abort()
        {
            //TODO: implement mimic logic
        }
    }
}