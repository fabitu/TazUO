using ClassicUO.Configuration;
using ClassicUO.Game.GameObjects;
using ClassicUO.Game.UI.Gumps;
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
        JsonSerializerOptions serializerOptions = new JsonSerializerOptions() { IncludeFields = true };

        public PreferenceManagerBase(string fileName)
        {
            filePath = Path.Combine(DirectoryPath, fileName);
            staticCustomItens = LoadFile();
        }
        public List<StaticCustomItens> LoadFile()
        {
            return ReadFile();
        }
        public List<StaticCustomItens> SavePreferences(List<StaticCustomItens> preferences)
        {
            try
            {
                var customItens = ReadFile();
                customItens = preferences;
                SaveFile(customItens);
                return ReadFile();
            }
            catch (Exception ex)
            {
                Log.Warn($"File {filePath} is Wrong. {ex.Message}");
                return null;
            }
        }
        public List<StaticCustomItens> SavePreference(StaticCustomItens updatedItem)
        {
            try
            {
                var customItens = ReadFile();
                if (customItens != null)
                {
                    var customItem = customItens.FirstOrDefault(x => x.Description.Equals(updatedItem.Description, StringComparison.OrdinalIgnoreCase));
                    customItem = updatedItem;
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
        public List<StaticCustomItens> AddGraphic(StaticCustomItens addItem, ushort graphic)
        {
            try
            {
                StaticCustomItens customItem;
                List<StaticCustomItens> customItens = ReadFile();
                if (addItem == null)
                {
                    customItem = customItens.FirstOrDefault();
                }
                else
                {
                    customItem = customItens.FirstOrDefault(x => x.Description.Equals(addItem.Description, StringComparison.OrdinalIgnoreCase));
                }
                if (customItem != null)
                {
                    if (customItem != null && !customItem.ToReplaceGraphicArray.Contains(graphic))
                        customItem?.ToReplaceGraphicArray.Add(graphic);
                    SaveFile(customItens);
                    return ReadFile();
                }
                else
                {
                    addItem.ToReplaceGraphicArray.Add(graphic);
                    AddPreference(addItem);
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
                var customItens = JsonSerializer.Deserialize<List<StaticCustomItens>>(content, serializerOptions);

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
            try
            {
                List<StaticCustomItens> replaceItem = [];

                replaceItem.Add(new StaticCustomItens()
                {
                    Description = "Default",
                    ReplaceToGraphic = defaultReplaceGraphic,
                    ToReplaceGraphicArray = defaultReplaceGraphicList
                });
                SaveFile(replaceItem);
            }
            catch (Exception ex)
            {
                Log.Warn($"File {filePath} is Wrong. {ex.Message}");               
            }       
           
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
                var result = JsonSerializer.Deserialize<List<StaticCustomItens>>(content, serializerOptions);
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

                string content = JsonSerializer.Serialize<List<StaticCustomItens>>(customItens, serializerOptions);
                File.WriteAllText(filePath, content);
                ReloadPreferences();
            }
            catch (Exception ex)
            {
                Log.Warn($"File {filePath} is Wrong. {ex.Message}");
            }
        }
        public void ReloadPreferences()
        {
            try
            {
                Client.LoadTileData();
                OptionsGump optionsGump = new();
                optionsGump.Apply();
            }
            catch
            {
                
            }
            
        }
    }
}
