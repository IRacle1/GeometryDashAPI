﻿using GeometryDashAPI.Data.Enums;
using GeometryDashAPI.Memory;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using GeometryDashAPI.Serialization;

namespace GeometryDashAPI.Data
{
    public class GameData
    {
        public Plist DataPlist { get; protected set; }

        private readonly string gameDataFile;

        public GameData(GameDataType type)
        {
            gameDataFile = ResolveFileName(type);
            Load().GetAwaiter().GetResult();
        }

        public GameData(string fileName)
        {
            gameDataFile = fileName;
            Load().GetAwaiter().GetResult();
        }

        protected GameData(string fileName, bool preventLoading)
        {
            gameDataFile = fileName;
            if (preventLoading)
                return;
            Load().GetAwaiter().GetResult();
        }

        public virtual async Task Load()
        {
            if (!File.Exists(gameDataFile))
                throw new FileNotFoundException($"file does not exists: '{gameDataFile}'");

            // File > Bytes > XOR > ToString > Replace > Base64 > Gzip
            var data = await File.ReadAllBytesAsync(gameDataFile);
            var dataZip = Encoding.ASCII.GetString(Crypt.XOR(data, 0xB)).Split('\0')[0];
            var resultPlist = Crypt.GZipDecompress(GameConvert.FromBase64(dataZip));

            DataPlist = new Plist(Encoding.ASCII.GetBytes(resultPlist));
        }

        public virtual bool TrySave(bool checkRunningGame, string fullName = null)
        {
            if (checkRunningGame && GameProcess.GameCount() > 0)
                return false;

            return TrySave(fullName);
        }

        public virtual bool TrySave(string fullName = null)
        {
            //Plist > ToString > GetBytes > Gzip > Base64 > Replace > GetBytes > XOR > File
            using var memory = new MemoryStream();
            DataPlist.SaveToStreamAsync(memory).GetAwaiter().GetResult();

            var base64 = GameConvert.ToBase64(Crypt.GZipCompress(memory.ToArray()));

            File.WriteAllBytesAsync(fullName ?? gameDataFile, Crypt.XOR(Encoding.ASCII.GetBytes(base64), 0xB)).GetAwaiter().GetResult();
            return true;
        }

        /// <summary>
        /// Saves class data to a file as a game save<br/><br/>
        /// Before saving, make sure that you have closed the game. Otherwise, after closing, the game will overwrite the file<br/>
        /// </summary>
        /// <param name="fullName">File to write the data.<br />
        /// use <b>null</b> value for default resolving
        /// </param>
        public void Save(string fullName = null)
        {
            using var memory = new MemoryStream();
            DataPlist.SaveToStream(memory);
            File.WriteAllBytes(fullName ?? gameDataFile, GetFileContent(memory));
        }

        /// <summary>
        /// Saves class data to a file as a game save<br/><br/>
        /// Before saving, make sure that you have closed the game. Otherwise, after closing, the game will overwrite the file<br/>
        /// </summary>
        /// <param name="fullName">File to write the data.<br />
        /// use <b>null</b> value for default resolving
        /// </param>
        public async Task SaveAsync(string fullName = null)
        {
            using var memory = new MemoryStream();
            await DataPlist.SaveToStreamAsync(memory);
            await File.WriteAllBytesAsync(fullName ?? gameDataFile, GetFileContent(memory));
        }

        public static string ResolveFileName(GameDataType type)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return $@"{Environment.GetEnvironmentVariable("LocalAppData")}\GeometryDash\CC{type}.dat";
            throw new InvalidOperationException($"can't resolve the directory with the saves on your operating system: '{RuntimeInformation.OSDescription}'. Use certain file name");
        }

        private static byte[] GetFileContent(MemoryStream memory)
        {
            var base64 = GameConvert.ToBase64(Crypt.GZipCompress(memory.ToArray()));
            return Crypt.XOR(Encoding.ASCII.GetBytes(base64), 0xB);
        }
    }
}
