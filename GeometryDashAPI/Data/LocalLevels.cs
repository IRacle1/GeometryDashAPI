﻿using GeometryDashAPI.Data.Enums;
using GeometryDashAPI.Data.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace GeometryDashAPI.Data
{
    public class LocalLevels : GameData, IEnumerable<LevelCreatorModel>
    {
        private List<LevelCreatorModel> levels { get; set; }
        private Dictionary<string, Dictionary<int, int>> index;

        public int BinaryVersion
        {
            get => DataPlist["LLM_02"];
            set => DataPlist["LLM_02"] = value;
        }

        public int LevelCount => levels.Count;

        public LocalLevels() : base(GameDataType.LocalLevels)
        {
            this.LoadLevels();
        }

        public LocalLevels(string fullName) : base(fullName)
        {
            this.LoadLevels();
        }

        private LocalLevels(string fullName, bool preventLoading) : base(fullName ?? fullName)
        {
            
        }

        public override void Load()
        {
            base.Load();
            this.LoadLevels();
        }

        private void LoadLevels()
        {
            this.levels = new List<LevelCreatorModel>();
            foreach (var a in DataPlist["LLM_01"])
            {
                if (a.Key == "_isArr")
                    continue;
                LevelCreatorModel level = new LevelCreatorModel(a.Key, a.Value);
                levels.Add(level);
            }
            RecalculateIndex();
        }

        private void RecalculateIndex()
        {
            if (index == null)
                index = new Dictionary<string, Dictionary<int, int>>();
            else
                index.Clear();
            
            for (var i = 0; i < levels.Count; ++i)
            {
                var level = levels[i];
                var revisionIndexMap = index.GetOrCreate(level.Name, _ => new Dictionary<int, int>());
                if (!revisionIndexMap.ContainsKey(level.Revision))
                    revisionIndexMap.Add(level.Revision, i);
            }
        }

        [Obsolete("Use the instance.GetLevel(name, revision)")]
        public LevelCreatorModel GetLevelByName(string levelName)
        {
            return this.levels.Find(x => x.Name == levelName);
        }

        public LevelCreatorModel GetLevel(string levelName, int revision = 0)
        {
            return levels[index[levelName][revision]];
        }

        public LevelCreatorModel GetLevel(int index)
        {
            return levels[index];
        }

        public List<LevelCreatorModel> GetAllLevelsByName(string levelName)
        {
            return this.levels.FindAll(x => x.Name == levelName);
        }

        [Obsolete("Use the instance.LevelExists(name, revision)")]
        public bool LevelExist(string levelName)
        {
            return this.levels.Exists(x => x.Name == levelName);
        }

        public bool LevelExists(string levelName, int revision = 0)
        {
            return index.ContainsKey(levelName) && index[levelName].ContainsKey(revision);
        }

        public void Remove(params LevelCreatorModel[] levels)
        {
            foreach (var level in levels)
            {
                this.levels.Remove(level);
                DataPlist["LLM_01"].Remove(level.KeyInDict);
            }
            RecalculateIndex();
        }

        public IEnumerator<LevelCreatorModel> GetEnumerator()
        {
            foreach (var level in levels)
                yield return level;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public static Task<LocalLevels> LoadAsync()
        {
            throw new NotImplementedException();
        }
    }
}
