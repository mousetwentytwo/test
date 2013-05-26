using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using Neurotoxin.Contour.Core.Attributes;
using Neurotoxin.Contour.Core.Constants;
using Neurotoxin.Contour.Core.Extensions;
using Neurotoxin.Contour.Core.Io.Gpd;
using Neurotoxin.Contour.Core.Io.Gpd.Entries;
using Neurotoxin.Contour.Core.Io.Stfs.Data;
using Neurotoxin.Contour.Core.Models;
using System.Linq;

namespace Neurotoxin.Contour.Core.Io.Stfs
{
    public abstract class StfsPackage : BinaryModelBase
    {
        #region MetaData

        [BinaryData]
        public virtual Magic Magic { get; set; }

        [BinaryData(0x228)]
        public virtual Certificate Certificate { get; set; }

        [BinaryData(16)]
        public virtual LicenseEntry[] LicenseData { get; set; }

        [BinaryData(20)]
        public virtual byte[] HeaderHash { get; set; }

        [BinaryData]
        public virtual int HeaderSize { get; set; }

        [BinaryData]
        public virtual ContentType ContentType { get; set; }

        [BinaryData]
        public virtual uint MetaDataVersion { get; set; }

        [BinaryData]
        public virtual ulong ContentSize { get; set; }

        [BinaryData]
        public virtual uint MediaId { get; set; }

        [BinaryData]
        public virtual uint Version { get; set; }

        [BinaryData]
        public virtual uint BaseVersion { get; set; }

        [BinaryData(4)]
        public virtual byte[] TitleId { get; set; }

        [BinaryData(1)]
        public virtual int Platform { get; set; }

        [BinaryData(1)]
        public virtual byte ExecutableType { get; set; }

        [BinaryData(1)]
        public virtual byte DiscNumber { get; set; }

        [BinaryData(1)]
        public virtual byte DiscInSet { get; set; }

        [BinaryData]
        public virtual uint SaveGameId { get; set; }

        [BinaryData(5)]
        public virtual byte[] ConsoleId { get; set; }

        [BinaryData(8)]
        public virtual byte[] ProfileId { get; set; }

        [BinaryData(0x24)]
        public virtual StfsVolumeDescriptor VolumeDescriptor { get; set; }

        [BinaryData]
        public virtual uint DataFileCount { get; set; }

        [BinaryData]
        public virtual ulong DataFileCombinedSize { get; set; }

        [BinaryData]
        public virtual VolumeDescriptorType DescriptorType { get; set; }

        [BinaryData]
        public virtual uint Reserved { get; set; }

        private IMediaInfo _mediaInfo;
        [BinaryData(0x4C)]
        public virtual IMediaInfo MediaInfo
        {
            get
            {
                if (_mediaInfo == null)
                {
                    switch (ContentType)
                    {
                        case ContentType.AvatarItem:
                            _mediaInfo = ModelFactory.GetModel<AvatarItemMediaInfo>(Binary, 0x3D9);
                            break;
                        case ContentType.Video:
                            _mediaInfo = ModelFactory.GetModel<VideoMediaInfo>(Binary, 0x3D9);
                            break;
                        default:
                            throw new NotSupportedException("STFS: Not supported content type: " + ContentType);
                    }
                }
                return _mediaInfo;
            }
        }

        [BinaryData(0x14)]
        public virtual byte[] DeviceId { get; set; }

        [BinaryData(0x900, StringReadOptions.AutoTrim)]
        public virtual string DisplayName { get; set; }

        [BinaryData(0x900, StringReadOptions.AutoTrim)]
        public virtual string DisplayDescription { get; set; }

        [BinaryData(0x80, StringReadOptions.AutoTrim)]
        public virtual string PublisherName { get; set; }

        [BinaryData(0x80, StringReadOptions.AutoTrim)]
        public virtual string TitleName { get; set; }

        [BinaryData(1)]
        public virtual TransferFlags TransferFlags { get; set; }

        [BinaryData]
        public virtual uint ThumbnailImageSize { get; set; }

        [BinaryData]
        public virtual uint TitleThumbnailImageSize { get; set; }

        [BinaryData(0x4000)]
        public virtual byte[] ThumbnailImage { get; set; }

        [BinaryData(0x4000)]
        public virtual byte[] TitleThumbnailImage { get; set; }

        [BinaryData]
        public virtual InstallerType InstallerType { get; set; }

        #endregion

        #region Other properties

        public FileEntry FileStructure { get; private set; }
        public Dictionary<FileEntry, GameFile> Games { get; protected set; }
        public Account Account { get; private set; }
        public FileEntry ProfileEntry { get; private set; }
        public DashboardFile ProfileInfo { get; private set; }
        public HashTable TopTable { get; private set; }

        protected List<int> UnallocatedHashEntries { get; set; }
        public Sex Sex { get; private set; }
        protected int TopLevel { get; private set; }
        protected int FirstHashTableAddress { get; private set; }

        protected bool IsModified { get; set; }

        #endregion

        #region Constructor

        public StfsPackage(OffsetTable offsetTable, BinaryContainer binary, int startOffset) : base(offsetTable, binary, startOffset)
        {
            LogHelper.LogDuration("STFS package parse", Parse);
        }

        #endregion

        #region Read

        private void Parse()
        {
            FirstHashTableAddress = ((HeaderSize + 0x0FFF) & 0x7FFFF000);
            Sex = (VolumeDescriptor.BlockSeperation & 1) == 1 ? Sex.Female : Sex.Male;
            if (VolumeDescriptor.AllocatedBlockCount >= 0x70E4) throw new NotSupportedException("STFS package too big to handle!");
            TopLevel = VolumeDescriptor.AllocatedBlockCount >= 0xAA ? 1 : 0;

            LogHelper.LogDuration("Collect hash entries", () =>
            {
                UnallocatedHashEntries = new List<int>();
                TopTable = GetLevelNHashTable(0, TopLevel);
                if (TopLevel == 1)
                {
                    TopTable.Tables = new List<HashTable>();
                    for (var i = 0; i < TopTable.EntryCount; i++)
                    {
                        var table = GetLevelNHashTable(i, 0);
                        TopTable.Entries[i].RealBlock = table.Block;
                        TopTable.Tables.Add(table);
                    }
                }
            });
            LogHelper.NotifyStatusBarText("Hash entries collected");

            LogHelper.LogDuration("Collect file entries", () => FileStructure = ReadFileListing());
            LogHelper.NotifyStatusBarText("File entries collected");
        }

        public void SwitchTables()
        {
            var oldTopTable = TopTable;
            IsModified = true;
            UnallocatedHashEntries = new List<int>();
            
            var topTableOffset = TopTable.StartOffset;
            var topTableBuffer = Binary.ReadBytes(topTableOffset, 0x1000);
            if (VolumeDescriptor.BlockSeperation == 2)
            {
                topTableOffset -= 0x1000;
                VolumeDescriptor.BlockSeperation = 0;
            }
            else
            {
                topTableOffset += 0x1000;
                VolumeDescriptor.BlockSeperation = 2;
            }
            Binary.WriteBytes(topTableOffset, topTableBuffer, 0, 0x1000);
            TopTable = GetLevelNHashTable(0, TopLevel);

            if (TopLevel == 0) return;

            TopTable.Tables = new List<HashTable>();
            for(var i = 0; i < TopTable.EntryCount; i++)
            {
                var tableEntry = TopTable.Entries[i];
                var offset = oldTopTable.Tables[i].StartOffset;
                var buffer = Binary.ReadBytes(offset, 0x1000);
                if (tableEntry.Status == BlockStatus.NewlyAllocated || tableEntry.Status == BlockStatus.PreviouslyAllocated)
                {
                    offset -= 0x1000;
                    tableEntry.Status = BlockStatus.Allocated;
                }
                else
                {
                    offset += 0x1000;
                    tableEntry.Status = BlockStatus.NewlyAllocated;
                }
                Binary.WriteBytes(offset, buffer, 0, 0x1000);
                var table = GetLevelNHashTable(i, 0);
                tableEntry.RealBlock = table.Block;
                TopTable.Tables.Add(table);

                for (var j = 0; j < table.EntryCount; j++)
                {
                    var entry = table.Entries[j];
                    if (entry.Status == BlockStatus.NewlyAllocated) entry.Status = BlockStatus.Allocated;
                }
            }
        }

        public HashTable GetLevelNHashTable(int index, int level)
        {
            if (level < 0 || level > TopLevel)
                throw new ArgumentException("Invalid level: " + level);                

            var x = TopLevel != level ? 0xAA : 1;
            var current = ComputeLevelNBackingHashBlockNumber(index * x, level);
            int entryCount;

            if (level == TopLevel)
            {
                if (VolumeDescriptor.BlockSeperation == 2) current++;

                entryCount = VolumeDescriptor.AllocatedBlockCount;
                if (entryCount >= 0xAA) entryCount = (entryCount + 0xA9)/0xAA;
            }
            else if (level + 1 == TopLevel)
            {
                var entry = TopTable.Entries[index];
                if (entry.Status == BlockStatus.NewlyAllocated || entry.Status == BlockStatus.PreviouslyAllocated)
                    current++;

                // calculate the number of entries in the requested table
                entryCount = index + 1 == TopTable.EntryCount 
                    ? VolumeDescriptor.AllocatedBlockCount%0xAA 
                    : index == TopTable.EntryCount ? 0 : 0xAA;
            }
            else
            {
                throw new NotSupportedException();
            }

            var currentHashAddress = (current << 0xC) + FirstHashTableAddress;
            Binary.EnsureBinarySize(currentHashAddress + 0x1000);    
            var table = ModelFactory.GetModel<HashTable>(Binary, currentHashAddress);
            table.Block = current;
            table.EntryCount = entryCount;

            for (var j = 0; j < 0xAA; j++)
            {
                var entry = table.Entries[j];
                entry.Block = index * 0xAA + j;
                entry.RealBlock = GetRealBlockNum(entry.Block.Value);
            }
            if (level == 0)
            {
                var unallocatedEntries = table.Entries.Where(e => e.Status == BlockStatus.Unallocated || e.Status == BlockStatus.PreviouslyAllocated);
                UnallocatedHashEntries.AddRange(unallocatedEntries.Select(e => e.Block.Value));
            }

            return table;
        }

        private int ComputeLevelNBackingHashBlockNumber(int blockNum, int level)
        {
            var blockStep = 0xAB;
            if (Sex == Sex.Male) blockStep++;

            switch (level)
            {
                case 0:
                    if (blockNum < 0xAA) return 0;
                    var num = (blockNum / 0xAA) * blockStep + 1;
                    if (Sex == Sex.Male) num++;
                    return num;
                case 1:
                    return blockStep;
                default:
                    throw new NotSupportedException("Invalid level: " + level);
            }
        }

        private FileEntry ReadFileListing()
        {
            var root = ModelFactory.GetModel<FileEntry>();
            root.PathIndicator = 0xFFFF;
            root.Name = "Root";
            root.EntryIndex = 0xFFFF;

            var block = VolumeDescriptor.FileTableBlockNum;

            var fl = new List<FileEntry>();
            for (var x = 0; x < VolumeDescriptor.FileTableBlockCount; x++)
            {
                var currentAddr = GetRealAddressOfBlock(block);
                for (var i = 0; i < 64; i++)
                {
                    var addr = currentAddr + i * 0x40;
                    var fe = ModelFactory.GetModel<FileEntry>(Binary, addr);
                    if (block == VolumeDescriptor.FileTableBlockNum && i == 0)
                    fe.FileEntryAddress = addr;
                    fe.EntryIndex = (x * 0x40) + i;

                    if (fe.Name != String.Empty)
                        fl.Add(fe);
                }
                var he = GetHashEntry(block);
                block = he.NextBlock;
            }

            BuildFileHierarchy(fl, root);
            return root;
        }

        private int GetRealBlockNum(int blockNum)
        {
            // check for invalid block number
            if (blockNum >= 0x70E4)
                throw new InvalidOperationException("STFS: Block number must be less than 0xFFFFFF.\n");
            var byteGender = Convert.ToByte(Sex);
            var backingDataBlockNumber = (((blockNum + 0xAA) / 0xAA) << byteGender) + blockNum;
            if (blockNum >= 0xAA) backingDataBlockNumber += ((blockNum + 0x70E4) / 0x70E4) << byteGender;
            return backingDataBlockNumber;
        }

        public int GetRealAddressOfBlock(int blockNum)
        {
            return (GetRealBlockNum(blockNum) << 0x0C) + FirstHashTableAddress;
        }

        public HashEntry GetHashEntry(int blockNum)
        {
            switch (TopLevel)
            {
                case 0:
                    return TopTable.Entries[blockNum];
                case 1:
                    return TopTable.Tables[blockNum / 0xAA].Entries[blockNum % 0xAA];
                default:
                    throw new NotSupportedException("Not supported table level");
            }
        }

        private static void BuildFileHierarchy(List<FileEntry> entries, FileEntry parentFolder)
        {
            foreach (var entry in entries.Where(entry => entry.PathIndicator == parentFolder.EntryIndex))
            {
                // add it if it's a file
                if (!entry.IsDirectory) parentFolder.Files.Add(entry);
                // if it's a directory and not the current directory, then add it
                else if (entry.EntryIndex != parentFolder.EntryIndex) parentFolder.Folders.Add(entry);
            }

            // for every folder added, add the files to them
            foreach (var entry in parentFolder.Folders)
                BuildFileHierarchy(entries, entry);
        }

        public FileEntry GetFileEntry(string path, bool allowNull = false)
        {
            var folder = FileStructure;
            var parts = path.Split(new[] { @"\" }, StringSplitOptions.RemoveEmptyEntries);
            var i = parts[0] == "Root" ? 1 : 0;
            while (i < parts.Length - 1)
            {
                folder = folder.Folders.FirstOrDefault(f => f.Name == parts[i]);
                if (folder == null)
                    throw new DirectoryNotFoundException("Folder does not exists in the package: " + parts[i]);
                i++;
            }
            var file = folder.Files.FirstOrDefault(f => f.Name == parts[i]);
            if (allowNull) return file;
            if (file == null)
                throw new FileNotFoundException("File does not exists in the package: " + parts[i]);
            return file;
        }

        #endregion

        #region Extract

        private void ExtractProfile()
        {
            LogHelper.LogDuration("Extract Profile", () =>
            {
                ProfileEntry = GetFileEntry(TitleId.ToHex() + ".gpd");
                ProfileInfo = ModelFactory.GetModel<DashboardFile>(ExtractFile(ProfileEntry));
                ProfileInfo.Parse();
            });

            LogHelper.NotifyStatusBarText("Profile extracted");
        }

        private void ExtractAccount()
        {
            LogHelper.LogDuration("Extract Account", () =>
            {
                var account = ExtractFile("Account");
                Account = Account.Decrypt(new MemoryStream(account), ConsoleType.Retail);
            });
            LogHelper.NotifyStatusBarText("Account extracted");
        }

        protected virtual void ExtractGames()
        {
            LogHelper.LogDuration("Extract Game files", () =>
            {
                var games = FileStructure.Files.Where(f => Path.GetExtension(f.Name) == ".gpd"
                                                        && ProfileInfo.TitlesPlayed.Any(t => t.TitleCode == Path.GetFileNameWithoutExtension(f.Name)));
                var count = games.Count();
                LogHelper.NotifyStatusBarMax(count);
                LogHelper.NotifyStatusBarText(count + " title found");
                var i = 0;
                Games = new Dictionary<FileEntry, GameFile>();
                foreach (var gpd in games)
                {
                    GetGameFile(gpd);
                    LogHelper.NotifyStatusBarChange(++i);
                    LogHelper.NotifyStatusBarText(string.Format("{0}/{1} {2} extracted", i, count, gpd.Name));
                }
            });
        }

        private ProfileEmbeddedContent ExtractPec()
        {
            var pec = ExtractFile("PEC");
            var model = ModelFactory.GetModel<ProfileEmbeddedContent>(pec);
            model.ExtractGames();
            return model;
        }

        public void ExtractContent()
        {
            ExtractProfile();
            ExtractGames();
            ExtractAccount();
        }

        public GameFile GetGameFile(string fileName, bool parse = false)
        {
            if (fileName.StartsWith("F") || !fileName.EndsWith(".gpd"))
                throw new NotSupportedException("Invalid file: " + fileName);

            var entry = GetFileEntry(fileName, true);
            return entry == null ? null : GetGameFile(entry, parse);
        }

        protected GameFile GetGameFile(FileEntry entry, bool parse = false)
        {
            if (!Games.ContainsKey(entry))
            {
                var file = ExtractFile(entry);
                return CreateGameFileModel(entry, file, parse);
            }
            var game = Games[entry];
            if (parse && !game.IsParsed) game.Parse();
            return game;
        }

        private GameFile CreateGameFileModel(FileEntry entry, byte[] binary, bool parse)
        {
            var game = ModelFactory.GetModel<GameFile>(binary);
            if (parse) game.Parse();
            game.TitleId = Path.GetFileNameWithoutExtension(entry.Name);
            Games.Add(entry, game);
            return game;
        }

        public void ExtractAll(string outPath)
        {
            ExtractDirectory(FileStructure, outPath);
        }

        public void ExtractDirectory(FileEntry folder, string outPath)
        {
            var dir = Path.Combine(outPath, folder.Name);
            Directory.CreateDirectory(dir);
            foreach (var subfolder in folder.Folders)
            {
                ExtractDirectory(subfolder, dir);
            }
            foreach (var file in folder.Files)
            {
                ExtractFile(file, dir);
            }
        }

        public byte[] ExtractFile(string pathInPackage)
        {
            var entry = GetFileEntry(pathInPackage);
            return ExtractFile(entry);
        }

        public void ExtractFile(FileEntry entry, string dir)
        {
            File.WriteAllBytes(Path.Combine(dir, entry.Name), ExtractFile(entry));
        }

        private byte[] ExtractFile(FileEntry entry)
        {
            var fileSize = entry.FileSize;
            var output = new byte[fileSize];
            var outpos = 0;
            var block = entry.StartingBlockNum;
            var remaining = fileSize;
            var i = 0;
            entry.BlockList = new List<int>();
            do
            {
                if (block >= VolumeDescriptor.AllocatedBlockCount)
                    throw new InvalidOperationException("STFS: Reference to illegal block number.\n");

                entry.BlockList.Add(block);
                var readBlock = remaining > 0x1000 ? 0x1000 : remaining;
                remaining -= readBlock;
                var pos = GetRealAddressOfBlock(block);
                Binary.ReadBytes(pos, output, outpos, readBlock);
                //BinMap.Add(pos, 0x1000, entry.Name, "File part #" + i, block);
                outpos += readBlock;

                var he = GetHashEntry(block);
                block = he.NextBlock;
                i++;
            } while (remaining != 0);

            return output;
        }

        #endregion

        #region Write

        public void Save(string path)
        {
            //HACK Horizon
            VolumeDescriptor.AllocatedBlockCount += 4;
            TopTable.Tables.Last().EntryCount += 4;
            Binary.EnsureBinarySize(GetRealAddressOfBlock(VolumeDescriptor.AllocatedBlockCount));

            Rehash();
            Resign();

            Binary.Save(path);
        }

        private int[] AllocateBlocks(int count)
        {
            if (count <= 0) return new int[0];

            var freeCount = UnallocatedHashEntries.Count > count ? count : UnallocatedHashEntries.Count;
            var res = new List<int>();
            if (freeCount > 0)
            {
                res.AddRange(UnallocatedHashEntries.Take(freeCount));
                UnallocatedHashEntries.RemoveRange(0, freeCount);

                var lastTable = TopLevel == 0 ? TopTable : TopTable.Tables[TopTable.EntryCount - 1];
                var lastEntry = lastTable.Entries.FirstOrDefault(e => e.Block == res.Last());
                if (lastEntry != null)
                {
                    var index = lastTable.Entries.ToList().IndexOf(lastEntry);
                    lastTable.EntryCount = index + 1;
                    if (res.Last() + 1 > VolumeDescriptor.AllocatedBlockCount)
                    {
                        VolumeDescriptor.AllocatedBlockCount = res.Last() + 1;
                        Binary.EnsureBinarySize(GetRealAddressOfBlock(VolumeDescriptor.AllocatedBlockCount));
                    }
                }
            }
            var toAlloc = count - freeCount;
            if (toAlloc > 0)
            {
                switch (TopLevel)
                {
                    case 0:
                        throw new NotImplementedException("Table upgrade from lvl 0 to 1 needed!");
                    case 1:
                        if (TopTable.EntryCount == 0xAA)
                            throw new NotImplementedException("Table upgrade from lvl 1 to 2 needed!");

                        //allocate a new table
                        var table = GetLevelNHashTable(TopTable.EntryCount, 0);
                        var tableEntry = TopTable.Entries[TopTable.EntryCount++];
                        tableEntry.RealBlock = table.Block;
                        tableEntry.Status = BlockStatus.Allocated;
                        TopTable.Tables.Add(table);

                        res.AddRange(AllocateBlocks(toAlloc));
                        break;
                    default:
                        throw new NotSupportedException("Unsupported level" + TopLevel);
                }
            }
            return res.ToArray();
        }

        public void UnlockAchievement(string titleId, int achievementId, byte[] image)
        {
            var fileEntry = GetFileEntry(titleId + ".gpd");
            var game = Games[fileEntry];
            game.UnlockAchievement(achievementId, image);
            RebuildGame(game, fileEntry);
        }

        private void RebuildGame(GameFile game, FileEntry fileEntry = null, TitleEntry titleEntry = null)
        {
            Debug.WriteLine("{0} {1}", game.TitleId, game.Title);
            var name = game.TitleId + ".gpd";
            fileEntry = fileEntry ?? GetFileEntry(name, true);
            titleEntry = titleEntry ?? ProfileInfo.TitlesPlayed.FirstOrDefault(t => t.TitleCode == game.TitleId);
            if (titleEntry == null)
                throw new ArgumentException("Invalid title: " + game.TitleId);
            game.Rebuild();

            ////HACK: Horizon emu
            //RemoveFile(fileEntry);

            ReplaceFile(fileEntry, game);
            titleEntry.AchievementsUnlocked = game.UnlockedAchievementCount;
            titleEntry.GamerscoreUnlocked = game.Gamerscore;
        }

        public void MergeWith(StfsPackage otherProfile)
        {
            if (!IsModified) SwitchTables();

            var count = otherProfile.ProfileInfo.TitlesPlayed.Count;
            LogHelper.NotifyStatusBarMax(count);
            LogHelper.NotifyStatusBarText(count + " title needs to be merged");

            var otherPec = otherProfile.ExtractPec();
            var pecFileEntry = GetFileEntry("PEC");
            var pec = ExtractPec();

            var i = 0;

            //HACK horizon
            int? newBlock = AllocateBlocks(1)[0];

            foreach (var title in otherProfile.ProfileInfo.TitlesPlayed)
            {
                var watch = new Stopwatch();
                watch.Start();
                var name = title.TitleCode + ".gpd";
                var otherGame = otherProfile.GetGameFile(name, true);
                var otherAvatarAwards = otherPec.GetGameFile(name, true);

                var fileEntry = GetFileEntry(name, true);
                if (fileEntry != null)
                {
                    var titleEntry = ProfileInfo.TitlesPlayed.FirstOrDefault(t => t.TitleCode == title.TitleCode);
                    if (titleEntry != null)
                    {
                        //Title already exists in target, merge is necessary
                        var game = GetGameFile(fileEntry, true);
                        if (game.MergeWith(otherGame))
                        {
                            RebuildGame(game);
                        }

                        if (otherAvatarAwards != null)
                        {
                            var avatarAwards = pec.GetGameFile(name, true);
                            if (avatarAwards != null)
                            {
                                if (avatarAwards.MergeWith(otherAvatarAwards))
                                {
                                    avatarAwards.Rebuild();
                                    var pecGpdEntry = pec.GetFileEntry(name);
                                    pec.ReplaceFile(pecGpdEntry, avatarAwards);
                                }
                            }
                            else
                            {
                                pec.AddFile(name, otherAvatarAwards.Binary.ReadAll());
                            }
                        }
                    }
                    else
                    {
                        throw new Exception("Title doesn't exists, but the gpd file does");
                        //Title doesn't exists, but the gpd file does, we just replace that

                        //var otherBinary = otherGame.Binary.ReadAll();
                        //ReplaceFile(fileEntry, otherBinary);
                        //var tid = title.TitleId.ToArray();
                        //Array.Reverse(tid);
                        //var id = BitConverter.ToUInt32(tid, 0);
                        //ProfileInfo.AddNewEntry<TitleEntry>(EntryType.Title, title.AllBytes, id);
                        //info = "added";
                    }
                }
                else
                {
                    if (otherAvatarAwards != null) pec.AddFile(name, otherAvatarAwards.Binary.ReadAll());

                    //Add gpd and title
                    var otherBinary = otherGame.Binary.ReadAll();

                    //File.WriteAllBytes(@"d:\NTX-CNT\Contour\Resources\mergeable\aktualis\q\" + i + "_" + name, otherBinary);

                    fileEntry = AddFile(name, otherBinary);
                    CreateGameFileModel(fileEntry, otherBinary, true);

                    var tid = title.TitleId.ToArray();
                    Array.Reverse(tid);
                    var id = BitConverter.ToUInt32(tid, 0);
                    ProfileInfo.AddNewEntry<TitleEntry>(EntryType.Title, title.AllBytes, id);
                }
                watch.Stop();
                LogHelper.NotifyStatusBarChange(++i);
                LogHelper.NotifyStatusBarText(string.Format("{0}/{1} {2} merged", i, count, title.TitleName));
            }

            LogHelper.NotifyStatusBarText("Rebuilding profile...");
            foreach (var title in ProfileInfo.TitlesPlayed)
            {
                var name = title.TitleCode + ".gpd";
                var game = GetGameFile(name, true);
                title.LastAchievementEarnedOn = game.AchievementCount > 0 ? game.Achievements.Max(a => a.UnlockTime) : DateTime.MinValue;
            }

            //HACK: Horizon
            var previous = 0;
            var block = VolumeDescriptor.FileTableBlockNum;
            for (var k = 0; k < VolumeDescriptor.FileTableBlockCount - 1; k++)
            {
                var hek = GetHashEntry(block);
                previous = block;
                block = hek.NextBlock;
            }
            Binary.WriteBytes(GetRealAddressOfBlock(newBlock.Value), Binary.ReadBytes(GetRealAddressOfBlock(block), 0x1000), 0, 0x1000);
            var he = GetHashEntry(block);
            he.Status = BlockStatus.PreviouslyAllocated;
            he = GetHashEntry(newBlock.Value);
            he.Status = BlockStatus.NewlyAllocated;
            UnallocatedHashEntries.Add(block);
            GetHashEntry(previous).NextBlock = newBlock.Value;

            ProfileInfo.Recalculate();
            ProfileInfo.Rebuild();
            ReplaceFile(ProfileEntry, ProfileInfo);

            pec.Rehash();
            pec.Resign();
            ReplaceFile(pecFileEntry, pec.Binary.ReadAll());
        }

        public void ReplaceFile(FileEntry fileEntry, GpdFile gpd)
        {
            ReplaceFile(fileEntry, gpd.Binary.ReadAll());
        }

        public void ReplaceFile(FileEntry fileEntry, byte[] content)
        {
            if (fileEntry.IsDirectory)
                throw new NotSupportedException("Directories doesn't have content!");

            var remaining = content.Length;

            var allocatedBlockCount = fileEntry.BlocksForFile;
            var newBlockCount = (content.Length + 0xFFF)/0x1000;
            var blocks = AllocateBlocks(newBlockCount - allocatedBlockCount);
            var block = fileEntry.StartingBlockNum;
            if (allocatedBlockCount == 0)
            {
                block = blocks[0];
                fileEntry.StartingBlockNum = blocks[0];
            }
            var consecutive = true;

            for (var i = 0; i < newBlockCount; i++)
            {
                var pos = GetRealAddressOfBlock(block);
                var size = remaining > 0x1000 ? 0x1000 : remaining;
                remaining -= size;
                var buffer = new byte[0x1000];
                Buffer.BlockCopy(content, i * 0x1000, buffer, 0, size);
                Binary.WriteBytes(pos, buffer, 0, 0x1000);

                var he = GetHashEntry(block);
                if (i < allocatedBlockCount - 1)
                {
                    he.Status = BlockStatus.Allocated;
                    if (he.NextBlock != block + 1) consecutive = false;
                    block = he.NextBlock;
                } 
                else if (i < newBlockCount - 1)
                {
                    he.Status = BlockStatus.NewlyAllocated;
                    block = blocks[i - allocatedBlockCount+1];
                    he.NextBlock = block;
                    if (he.NextBlock != he.Block + 1) consecutive = false;
                }
                else
                {
                    he.Status = BlockStatus.NewlyAllocated;
                    he.NextBlock = 0xFFFFFF;
                }
            }

            for (var i = newBlockCount; i < allocatedBlockCount; i++)
            {
                var he = GetHashEntry(block);
                if (UnallocatedHashEntries.Contains(block))
                    throw new Exception("qwe");
                UnallocatedHashEntries.Add(block);
                he.Status = BlockStatus.PreviouslyAllocated;
                block = he.NextBlock;
                he.NextBlock = 0xFFFFFF;
            }

            fileEntry.FileSize = content.Length;
            fileEntry.BlocksForFile = newBlockCount;
            fileEntry.BlocksForFileCopy = newBlockCount;
            fileEntry.BlocksAreConsecutive = consecutive;
        }

        public FileEntry AddFile(string name, byte[] content)
        {
            return AddFile(FileStructure, name, content);
        }

        public FileEntry AddFile(FileEntry parent, string name, byte[] content)
        {
            int? previous;
            int block;

            var newEntry = AllocateNewFileEntry(out block, out previous);
            newEntry.CacheEnabled = false;

            newEntry.Name = name;
            newEntry.Flags = (FileEntryFlags) name.Length;
            newEntry.PathIndicator = (ushort)parent.EntryIndex;
            newEntry.BlocksForFile = 0;
            ReplaceFile(newEntry, content);
            parent.Files.Add(newEntry);

            return newEntry;
        }

        private FileEntry AllocateNewFileEntry(out int block, out int? previous)
        {
            previous = null;
            block = VolumeDescriptor.FileTableBlockNum;
            for (var x = 0; x < VolumeDescriptor.FileTableBlockCount; x++)
            {
                var currentAddr = GetRealAddressOfBlock(block);
                for (var i = 0; i < 64; i++)
                {
                    var addr = currentAddr + i * 0x40;
                    var fe = ModelFactory.GetModel<FileEntry>(Binary, addr);

                    if (fe.Name == String.Empty)
                    {
                        fe.FileEntryAddress = addr;
                        fe.EntryIndex = (x * 0x40) + i;
                        fe.CreatedTimeStamp = DateTime.Now.ToFatFileTime();
                        fe.AccessTimeStamp = DateTime.Now.ToFatFileTime();
                        return fe;
                    }
                }
                var he = GetHashEntry(block);
                previous = block;
                block = he.NextBlock;
            }
            throw new Exception("Dafuq! No space left for a new file!");
        }

        public void RemoveFile(FileEntry fileEntry)
        {
            var block = fileEntry.StartingBlockNum;
            for (var i = 0; i < fileEntry.BlocksForFile; i++)
            {
                var he = GetHashEntry(block);
                he.Status = BlockStatus.PreviouslyAllocated;
                if (UnallocatedHashEntries.Contains(block))
                    throw new Exception("qwe");
                UnallocatedHashEntries.Add(block);
                block = he.NextBlock;
            }
            fileEntry.BlocksForFile = 0;
        }

        #endregion

        #region Security

        private static readonly byte[] DefaultD = new byte[]
                                                      {
                                                          0x6D, 0x4C, 0xCF, 0x3D, 0xE8, 0x65, 0x51, 0xFF, 0x2D, 0xAC,
                                                          0xC1, 0x90, 0xE7, 0x47, 0xEB, 0xC6, 0x74, 0x58, 0xD0, 0x2D,
                                                          0x19, 0x08, 0xAC, 0x79, 0xCE, 0xD0, 0x1D, 0xA3, 0x1C, 0xC3,
                                                          0x2E, 0x39, 0x8E, 0xC7, 0xEF, 0x66, 0xFA, 0xE4, 0x2F, 0x10,
                                                          0x42, 0xA8, 0x4E, 0xE7, 0xA1, 0xFD, 0xF4, 0xF0, 0xCB, 0x64,
                                                          0x67, 0xA6, 0x10, 0x4D, 0x6D, 0x3A, 0x56, 0x9D, 0x1F, 0xEC,
                                                          0x51, 0xFC, 0xC2, 0x26, 0x45, 0xC2, 0xDE, 0xF9, 0x9B, 0x4C,
                                                          0x4C, 0x93, 0x4D, 0xA8, 0x2B, 0x48, 0xAC, 0xED, 0xD7, 0xFC,
                                                          0xEA, 0xE9, 0x72, 0xFB, 0xB2, 0x39, 0x88, 0xC1, 0x07, 0x34,
                                                          0x6F, 0x2A, 0x07, 0x7E, 0x97, 0x81, 0xF5, 0x02, 0x21, 0xFA,
                                                          0xCD, 0xDD, 0x30, 0xDD, 0xE5, 0x41, 0xB3, 0x4A, 0x22, 0x73,
                                                          0x80, 0x89, 0x2B, 0x9E, 0x90, 0xAF, 0xC4, 0x0A, 0x8A, 0x50,
                                                          0x15, 0x0F, 0xBD, 0x6E, 0xD4, 0x95, 0x37, 0x79
                                                      };

        public virtual void Resign(string kvPath = null)
        {
            ResignPackage(kvPath ?? "KV_dec.bin", 0x344, 0x118, 0x22C);
        }

        protected void ResignPackage(string kvPath, int headerStart, int size, int toSignLoc)
        {
            using (var kv = new FileStream(@"Resources\" + kvPath, FileMode.Open))
            {
                var rsaParameters = GetRSAParameters(kv);

                // read the certificate
                kv.Position = 0x9B8 + (kv.Length == 0x4000 ? 0x10 : 0);
                Certificate.PublicKeyCertificateSize = kv.ReadShort();
                kv.Read(Certificate.OwnerConsoleId, 0, 5);
                Certificate.OwnerConsolePartNumber = kv.ReadWString(0x11);
                Certificate.OwnerConsoleType = (ConsoleType)(kv.ReadUInt() & 3);
                Certificate.DateGeneration = kv.ReadWString(8);
                Certificate.PublicExponent = kv.ReadUInt();
                kv.Read(Certificate.PublicModulus, 0, 128);
                kv.Read(Certificate.CertificateSignature, 0, 256);

                ConsoleId = Certificate.OwnerConsoleId;

                HeaderHash = HashBlock(headerStart, ((HeaderSize + 0xFFF) & 0xF000) - headerStart);

                var rsaEncryptor = new RSACryptoServiceProvider();
                var rsaSigFormat = new RSAPKCS1SignatureFormatter(rsaEncryptor);
                rsaEncryptor.ImportParameters(rsaParameters);
                rsaSigFormat.SetHashAlgorithm("SHA1");
                var signature = rsaSigFormat.CreateSignature(HashBlock(toSignLoc, size));
                Array.Reverse(signature);

                Certificate.Signature = signature;
            }
        }

        public void Rehash()
        {
            int unallocCount = 0;
            switch (TopLevel)
            {
                case 0:
                    for (var i = 0; i < TopTable.EntryCount; i++)
                    {
                        var pos = GetRealAddressOfBlock(i);
                        var entry = TopTable.Entries[i];
                        if (entry.Status == BlockStatus.Unallocated || entry.Status == BlockStatus.PreviouslyAllocated)
                            unallocCount++;
                        else
                            entry.BlockHash = HashBlock(pos);
                    }
                    break;
                case 1:
                    for (var i = 0; i < TopTable.EntryCount; i++)
                    {
                        for (var j = 0; j < TopTable.Tables[i].EntryCount; j++)
                        {
                            var lowEntry = TopTable.Tables[i].Entries[j];
                            var pos = GetRealAddressOfBlock(lowEntry.Block.Value);
                            if (lowEntry.Status == BlockStatus.Unallocated || lowEntry.Status == BlockStatus.PreviouslyAllocated)
                                unallocCount++;
                            else
                                lowEntry.BlockHash = HashBlock(pos);
                        }
                        var tableEntries = TopTable.Tables[i].Entries.Take(TopTable.Tables[i].EntryCount).ToArray();
                        var u = tableEntries.Count(e => e.Status == BlockStatus.Unallocated);
                        var p = tableEntries.Count(e => e.Status == BlockStatus.PreviouslyAllocated);
                        var highEntry = TopTable.Entries[i];
                        highEntry.BlockHash = HashBlock(TopTable.Tables[i].StartOffset);
                        highEntry.NextBlock = (p << 15) + u;
                    }
                    break;
                default:
                    throw new NotSupportedException("Not supported level: " + TopLevel);
            }

            TopTable.AllocatedBlockCount = VolumeDescriptor.AllocatedBlockCount;
            VolumeDescriptor.UnallocatedBlockCount = unallocCount;
            VolumeDescriptor.TopHashTableHash = HashBlock(TopTable.StartOffset);

            const int headerStart = 0x344;

            // calculate header size / first hash table address
            var calculated = ((HeaderSize + 0xFFF) & 0xF000);
            var headerSize = calculated - headerStart;

            HeaderHash = HashBlock(headerStart, headerSize);
        }

        public byte[] HashBlock(int pos, int length = 0x1000)
        {
            var sha1 = new SHA1CryptoServiceProvider();
            return sha1.ComputeHash(Binary.ReadBytes(pos, length));
        }

        private static RSAParameters GetRSAParameters(Stream kv)
        {
            var p = new RSAParameters
                        {
                            Exponent = new byte[4],
                            Modulus = new byte[0x80],
                            P = new byte[0x40],
                            Q = new byte[0x40],
                            DP = new byte[0x40],
                            DQ = new byte[0x40],
                            InverseQ = new byte[0x40]
                        };

            kv.Position = 0x28C + (kv.Length == 0x4000 ? 0x10 : 0);
            p.Exponent = kv.ReadBytes(0x4);
            kv.Position += 8;
            p.Modulus = kv.ReadBytes(0x80);
            p.P = kv.ReadBytes(0x40);
            p.Q = kv.ReadBytes(0x40);
            p.DP = kv.ReadBytes(0x40);
            p.DQ = kv.ReadBytes(0x40);
            p.InverseQ = kv.ReadBytes(0x40);

            p.Modulus.SwapBytes(8);
            p.P.SwapBytes(8);
            p.Q.SwapBytes(8);
            p.DP.SwapBytes(8);
            p.DQ.SwapBytes(8);
            p.InverseQ.SwapBytes(8);
            p.D = DefaultD;

            return p;
        }

        #endregion

    }
}