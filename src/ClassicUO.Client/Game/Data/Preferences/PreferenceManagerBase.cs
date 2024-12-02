using ClassicUO.Configuration;
using ClassicUO.Game.GameObjects;
using ClassicUO.Utility.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using static ClassicUO.Game.Data.StaticFilters;

namespace ClassicUO.Game.Data.Preferences
{
    internal abstract class PreferenceManagerBase
    {
        protected readonly string filePath;
        protected ushort defaultReplaceGraphic;
        protected List<ushort> defaultReplaceGraphicList;
        protected List<StaticCustomItens> staticCustomItens;

        public PreferenceManagerBase(string fileName)
        {
            filePath = Path.Combine(DirectoryPath, fileName);
            staticCustomItens = LoadFile();
        }

        public List<StaticCustomItens> LoadFile()
        {
            return ReadFile();
        }

        public List<StaticCustomItens> AddPreference(StaticCustomItens addItem)
        {
            try
            {
                var customItens = ReadFile();
                if (customItens != null)
                {
                    customItens.Add(addItem);
                    SaveFile(customItens);
                    return ReadFile();
                }
                return customItens;
            }
            catch (Exception ex)
            {
                Log.Warn($"File {filePath} is Wrong. {ex.Message}");
                return null;
            }
        }
        public List<StaticCustomItens> UpdatePreference(StaticCustomItens addItem, ushort graphic)
        {
            try
            {
                List<StaticCustomItens> customItens = ReadFile();

                var customItem = customItens.FirstOrDefault(x => x.Type.Equals(addItem.Type, StringComparison.OrdinalIgnoreCase) && x.Description.Equals(addItem.Description, StringComparison.OrdinalIgnoreCase));
                if (customItens != null)
                {
                    customItem?.ToReplaceGraphicArray.Add(graphic);
                    SaveFile(customItens);
                    return ReadFile();
                }
                return customItens;
            }
            catch (Exception ex)
            {
                Log.Warn($"File {filePath} is Wrong. {ex.Message}");
                return null;
            }
        }
        public List<StaticCustomItens> RemoveGraphic(ushort graphic)
        {
            try
            {
                var content = File.ReadAllText(filePath);
                var customItens = JsonSerializer.Deserialize<List<StaticCustomItens>>(content);

                foreach (var itemList in customItens)
                {
                    itemList.ToReplaceGraphicArray.Remove(graphic);
                }
                SaveFile(customItens);
                return ReadFile();
            }
            catch (Exception ex)
            {
                Log.Warn($"File {filePath} is Wrong. {ex.Message}");
                return null;
            }
        }
        private void CreateDefaultFile()
        {
            List<StaticCustomItens> replaceItem = [];

            replaceItem.Add(new StaticCustomItens()
            {
                Description = "Default",
                ReplaceToGraphic = defaultReplaceGraphic,
                ToReplaceGraphicArray = defaultReplaceGraphicList
            });

            var jsonString = JsonSerializer.Serialize(replaceItem);
            File.WriteAllText(filePath, jsonString);
        }
        private List<StaticCustomItens> ReadFile()
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    CreateDefaultFile();
                }
                var content = File.ReadAllText(filePath);
                var result = JsonSerializer.Deserialize<List<StaticCustomItens>>(content);
                return result;
            }
            catch (Exception ex)
            {
                Log.Warn($"File {filePath} is Wrong. {ex.Message}");
                return null;
            }
        }
        private void SaveFile(List<StaticCustomItens> customItens)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    CreateDefaultFile();
                }
                string content = JsonSerializer.Serialize(customItens);
                File.WriteAllText(filePath, content);
            }
            catch (Exception ex)
            {
                Log.Warn($"File {filePath} is Wrong. {ex.Message}");
            }
        }
    }
}
