using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Xml;
using UnityEngine;

namespace DarkMultiPlayer
{
    public class Settings
    {
        //Settings
        private static Settings singleton = new Settings();
        public string playerName;
        public string playerPublicKey;
        public string playerPrivateKey;
        public int cacheSize;
        public int disclaimerAccepted;
        public List<ServerEntry> servers;
        public Color playerColor;
        public KeyCode screenshotKey;
        public KeyCode chatKey;
        public string selectedFlag;
        public bool compressionEnabled;
        public bool revertEnabled;
        public DMPToolbarType toolbarType;
        private const string DEFAULT_PLAYER_NAME = "Player";
        private const string SETTINGS_FILE = "servers.xml";
        private const string CN_SETTINGS_FILE = "settings.cfg";
        private const string PUBLIC_KEY_FILE = "publickey.txt";
        private const string PRIVATE_KEY_FILE = "privatekey.txt";
        private const int DEFAULT_CACHE_SIZE = 100;
        private string dataLocation;
        private string settingsFile;
        private string cnSettingsFile;
        private string backupSettingsFile;
        private string backupCnSettingsFile;
        private string publicKeyFile;
        private string privateKeyFile;
        private string backupPublicKeyFile;
        private string backupPrivateKeyFile;

        public static Settings fetch
        {
            get
            {
                return singleton;
            }
        }

        public Settings()
        {
            string darkMultiPlayerDataDirectory = Path.Combine(Path.Combine(Path.Combine(Path.Combine(KSPUtil.ApplicationRootPath, "GameData"), "DarkMultiPlayer"), "Plugins"), "Data");
            if (!Directory.Exists(darkMultiPlayerDataDirectory))
            {
                Directory.CreateDirectory(darkMultiPlayerDataDirectory);
            }
            string darkMultiPlayerSavesDirectory = Path.Combine(Path.Combine(KSPUtil.ApplicationRootPath, "saves"), "DarkMultiPlayer");
            if (!Directory.Exists(darkMultiPlayerSavesDirectory))
            {
                Directory.CreateDirectory(darkMultiPlayerSavesDirectory);
            }
            dataLocation = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Data");
            settingsFile = Path.Combine(dataLocation, SETTINGS_FILE);
            cnSettingsFile = Path.Combine(dataLocation, CN_SETTINGS_FILE);
            backupSettingsFile = Path.Combine(darkMultiPlayerSavesDirectory, SETTINGS_FILE);
            backupCnSettingsFile = Path.Combine(darkMultiPlayerSavesDirectory, CN_SETTINGS_FILE);
            publicKeyFile = Path.Combine(dataLocation, PUBLIC_KEY_FILE);
            backupPublicKeyFile = Path.Combine(darkMultiPlayerSavesDirectory, PUBLIC_KEY_FILE);
            privateKeyFile = Path.Combine(dataLocation, PRIVATE_KEY_FILE);
            backupPrivateKeyFile = Path.Combine(darkMultiPlayerSavesDirectory, PRIVATE_KEY_FILE);
            LoadSettings();
        }

        public void LoadSSettings()
        {

            //Read XML settings
            try
            {
                bool saveXMLAfterLoad = false;
                XmlDocument xmlDoc = new XmlDocument();
                if (File.Exists(backupSettingsFile) && !File.Exists(settingsFile))
                {
                    DarkLog.Debug("Restoring player settings file!");
                    File.Copy(backupSettingsFile, settingsFile);
                }
                if (!File.Exists(settingsFile))
                {
                    xmlDoc.LoadXml(newXMLString());
                    playerName = DEFAULT_PLAYER_NAME;
                    xmlDoc.Save(settingsFile);
                }
                if (!File.Exists(backupSettingsFile))
                {
                    DarkLog.Debug("Backing up player token and settings file!");
                    File.Copy(settingsFile, backupSettingsFile);
                }
                xmlDoc.Load(settingsFile);
                playerName = xmlDoc.SelectSingleNode("/settings/global/@username").Value;
                try
                {
                    cacheSize = Int32.Parse(xmlDoc.SelectSingleNode("/settings/global/@cache-size").Value);
                }
                catch
                {
                    DarkLog.Debug("Adding cache size to settings file");
                    saveXMLAfterLoad = true;
                    cacheSize = DEFAULT_CACHE_SIZE;
                }
                try
                {
                    disclaimerAccepted = Int32.Parse(xmlDoc.SelectSingleNode("/settings/global/@disclaimer").Value);
                }
                catch
                {
                    DarkLog.Debug("Adding disclaimer to settings file");
                    saveXMLAfterLoad = true;
                }
                try
                {
                    string floatArrayString = xmlDoc.SelectSingleNode("/settings/global/@player-color").Value;
                    string[] floatArrayStringSplit = floatArrayString.Split(',');
                    float redColor = float.Parse(floatArrayStringSplit[0].Trim());
                    float greenColor = float.Parse(floatArrayStringSplit[1].Trim());
                    float blueColor = float.Parse(floatArrayStringSplit[2].Trim());
                    //Bounds checking - Gotta check up on those players :)
                    if (redColor < 0f)
                    {
                        redColor = 0f;
                    }
                    if (redColor > 1f)
                    {
                        redColor = 1f;
                    }
                    if (greenColor < 0f)
                    {
                        greenColor = 0f;
                    }
                    if (greenColor > 1f)
                    {
                        greenColor = 1f;
                    }
                    if (blueColor < 0f)
                    {
                        blueColor = 0f;
                    }
                    if (blueColor > 1f)
                    {
                        blueColor = 1f;
                    }
                    playerColor = new Color(redColor, greenColor, blueColor, 1f);
                    OptionsWindow.fetch.loadEventHandled = false;
                }
                catch
                {
                    DarkLog.Debug("Adding player color to settings file");
                    saveXMLAfterLoad = true;
                    playerColor = PlayerColorWorker.GenerateRandomColor();
                    OptionsWindow.fetch.loadEventHandled = false;
                }
                try
                {
                    chatKey = (KeyCode)Int32.Parse(xmlDoc.SelectSingleNode("/settings/global/@chat-key").Value);
                }
                catch
                {
                    DarkLog.Debug("Adding chat key to settings file");
                    saveXMLAfterLoad = true;
                    chatKey = KeyCode.BackQuote;
                }
                try
                {
                    screenshotKey = (KeyCode)Int32.Parse(xmlDoc.SelectSingleNode("/settings/global/@screenshot-key").Value);
                }
                catch
                {
                    DarkLog.Debug("Adding screenshot key to settings file");
                    saveXMLAfterLoad = true;
                    chatKey = KeyCode.F8;
                }
                try
                {
                    selectedFlag = xmlDoc.SelectSingleNode("/settings/global/@selected-flag").Value;
                }
                catch
                {
                    DarkLog.Debug("Adding selected flag to settings file");
                    saveXMLAfterLoad = true;
                    selectedFlag = "Squad/Flags/default";
                }
                try
                {
                    compressionEnabled = Boolean.Parse(xmlDoc.SelectSingleNode("/settings/global/@compression").Value);
                }
                catch
                {
                    DarkLog.Debug("Adding compression flag to settings file");
                    compressionEnabled = true;
                }
                try
                {
                    revertEnabled = Boolean.Parse(xmlDoc.SelectSingleNode("/settings/global/@revert").Value);
                }
                catch
                {
                    DarkLog.Debug("Adding revert flag to settings file");
                    revertEnabled = true;
                }
                try
                {
                    toolbarType = (DMPToolbarType)Int32.Parse(xmlDoc.SelectSingleNode("/settings/global/@toolbar").Value);
                }
                catch
                {
                    DarkLog.Debug("Adding toolbar flag to settings file");
                    toolbarType = DMPToolbarType.BLIZZY_IF_INSTALLED;
                }
                XmlNodeList serverNodeList = xmlDoc.GetElementsByTagName("server");
                servers = new List<ServerEntry>();
                foreach (XmlNode xmlNode in serverNodeList)
                {
                    ServerEntry newServer = new ServerEntry();
                    newServer.name = xmlNode.Attributes["name"].Value;
                    newServer.address = xmlNode.Attributes["address"].Value;
                    Int32.TryParse(xmlNode.Attributes["port"].Value, out newServer.port);
                    servers.Add(newServer);
                }
                if (saveXMLAfterLoad)
                {
                    SaveSettings();
                }
            }
            catch (Exception e)
            {
                DarkLog.Debug("XML Exception: " + e);
            }

            //Read player token
            try
            {
                //Restore backup if needed
                if (File.Exists(backupPublicKeyFile) && File.Exists(backupPrivateKeyFile) && (!File.Exists(publicKeyFile) || !File.Exists(privateKeyFile)))
                {
                    DarkLog.Debug("Restoring backed up keypair!");
                    File.Copy(backupPublicKeyFile, publicKeyFile, true);
                    File.Copy(backupPrivateKeyFile, privateKeyFile, true);
                }
                //Load or create token file
                if (File.Exists(privateKeyFile) && File.Exists(publicKeyFile))
                {
                    playerPublicKey = File.ReadAllText(publicKeyFile);
                    playerPrivateKey = File.ReadAllText(privateKeyFile);
                }
                else
                {
                    DarkLog.Debug("Creating new keypair!");
                    GenerateNewKeypair();
                }
                //Save backup token file if needed
                if (!File.Exists(backupPublicKeyFile) || !File.Exists(backupPrivateKeyFile))
                {
                    DarkLog.Debug("Backing up keypair");
                    File.Copy(publicKeyFile, backupPublicKeyFile, true);
                    File.Copy(privateKeyFile, backupPrivateKeyFile, true);
                }
            }
            catch
            {
                DarkLog.Debug("Error processing keypair, creating new keypair");
                GenerateNewKeypair();
                DarkLog.Debug("Backing up keypair");
                File.Copy(publicKeyFile, backupPublicKeyFile, true);
                File.Copy(privateKeyFile, backupPrivateKeyFile, true);
            }
        }

        private void GenerateNewKeypair()
        {
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(1024))
            {
                try
                {
                    playerPublicKey = rsa.ToXmlString(false);
                    playerPrivateKey = rsa.ToXmlString(true);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error: " + e);
                }
                finally
                {
                    //Don't save the key in the machine store.
                    rsa.PersistKeyInCsp = false;
                }
            }
            File.WriteAllText(publicKeyFile, playerPublicKey);
            File.WriteAllText(privateKeyFile, playerPrivateKey);
        }

        public void SaveSSettings()
        {
            XmlDocument xmlDoc = new XmlDocument();
            if (File.Exists(settingsFile))
            {
                xmlDoc.Load(settingsFile);
            }
            else
            {
                xmlDoc.LoadXml(newXMLString());
            }
            xmlDoc.SelectSingleNode("/settings/global/@username").Value = playerName;
            try
            {
                xmlDoc.SelectSingleNode("/settings/global/@cache-size").Value = cacheSize.ToString();
            }
            catch
            {
                XmlAttribute cacheAttribute = xmlDoc.CreateAttribute("cache-size");
                cacheAttribute.Value = DEFAULT_CACHE_SIZE.ToString();
                xmlDoc.SelectSingleNode("/settings/global").Attributes.Append(cacheAttribute);
            }
            try
            {
                xmlDoc.SelectSingleNode("/settings/global/@disclaimer").Value = disclaimerAccepted.ToString();
            }
            catch
            {
                XmlAttribute disclaimerAttribute = xmlDoc.CreateAttribute("disclaimer");
                disclaimerAttribute.Value = "0";
                xmlDoc.SelectSingleNode("/settings/global").Attributes.Append(disclaimerAttribute);
            }
            try
            {
                xmlDoc.SelectSingleNode("/settings/global/@player-color").Value = playerColor.r.ToString() + ", " + playerColor.g.ToString() + ", " + playerColor.b.ToString();
            }
            catch
            {
                XmlAttribute colorAttribute = xmlDoc.CreateAttribute("player-color");
                colorAttribute.Value = playerColor.r.ToString() + ", " + playerColor.g.ToString() + ", " + playerColor.b.ToString();
                xmlDoc.SelectSingleNode("/settings/global").Attributes.Append(colorAttribute);
            }
            try
            {
                xmlDoc.SelectSingleNode("/settings/global/@chat-key").Value = ((int)chatKey).ToString();
            }
            catch
            {
                XmlAttribute chatKeyAttribute = xmlDoc.CreateAttribute("chat-key");
                chatKeyAttribute.Value = ((int)chatKey).ToString();
                xmlDoc.SelectSingleNode("/settings/global").Attributes.Append(chatKeyAttribute);
            }
            try
            {
                xmlDoc.SelectSingleNode("/settings/global/@screenshot-key").Value = ((int)screenshotKey).ToString();
            }
            catch
            {
                XmlAttribute screenshotKeyAttribute = xmlDoc.CreateAttribute("screenshot-key");
                screenshotKeyAttribute.Value = ((int)screenshotKey).ToString();
                xmlDoc.SelectSingleNode("/settings/global").Attributes.Append(screenshotKeyAttribute);
            }
            try
            {
                xmlDoc.SelectSingleNode("/settings/global/@selected-flag").Value = selectedFlag;
            }
            catch
            {
                XmlAttribute selectedFlagAttribute = xmlDoc.CreateAttribute("selected-flag");
                selectedFlagAttribute.Value = selectedFlag;
                xmlDoc.SelectSingleNode("/settings/global").Attributes.Append(selectedFlagAttribute);
            }
            try
            {
                xmlDoc.SelectSingleNode("/settings/global/@compression").Value = compressionEnabled.ToString();
            }
            catch
            {
                XmlAttribute compressionAttribute = xmlDoc.CreateAttribute("compression");
                compressionAttribute.Value = compressionEnabled.ToString();
                xmlDoc.SelectSingleNode("/settings/global").Attributes.Append(compressionAttribute);
            }
            try
            {
                xmlDoc.SelectSingleNode("/settings/global/@revert").Value = revertEnabled.ToString();
            }
            catch
            {
                XmlAttribute revertAttribute = xmlDoc.CreateAttribute("revert");
                revertAttribute.Value = revertEnabled.ToString();
                xmlDoc.SelectSingleNode("/settings/global").Attributes.Append(revertAttribute);
            }
            try
            {
                xmlDoc.SelectSingleNode("/settings/global/@toolbar").Value = ((int)toolbarType).ToString();
            }
            catch
            {
                XmlAttribute toolbarAttribute = xmlDoc.CreateAttribute("toolbar");
                toolbarAttribute.Value = revertEnabled.ToString();
                xmlDoc.SelectSingleNode("/settings/global").Attributes.Append(toolbarAttribute);
            }
            XmlNode serverNodeList = xmlDoc.SelectSingleNode("/settings/servers");
            serverNodeList.RemoveAll();
            foreach (ServerEntry server in servers)
            {
                XmlElement serverElement = xmlDoc.CreateElement("server");
                serverElement.SetAttribute("name", server.name);
                serverElement.SetAttribute("address", server.address);
                serverElement.SetAttribute("port", server.port.ToString());
                serverNodeList.AppendChild(serverElement);
            }
            xmlDoc.Save(settingsFile);
            File.Copy(settingsFile, backupSettingsFile, true);
        }

        public void LoadSettings()
        {
            bool saveAfterLoad = false;
            ConfigNode mainNode = new ConfigNode();

            if (File.Exists(backupCnSettingsFile) && !File.Exists(cnSettingsFile))
            {
                DarkLog.Debug("restoring settings");
                File.Copy(backupCnSettingsFile, cnSettingsFile);
            }

            if (!File.Exists(cnSettingsFile))
            {
                mainNode = GetDefaultSettings();
                playerName = DEFAULT_PLAYER_NAME;
                mainNode.Save(cnSettingsFile);
            }

            if (!File.Exists(backupCnSettingsFile))
            {
                DarkLog.Debug("Backing up settings");
                File.Copy(cnSettingsFile, backupCnSettingsFile);
            }

            mainNode = ConfigNode.Load(cnSettingsFile);

            ConfigNode settingsNode = mainNode.GetNode("SETTINGS");
            ConfigNode playerNode = settingsNode.GetNode("PLAYER");
            ConfigNode bindingsNode = settingsNode.GetNode("KEYBINDINGS");

            playerName = playerNode.GetValue("name");

            if (!int.TryParse(settingsNode.GetValue("cacheSize"), out cacheSize))
            {
                DarkLog.Debug("Adding cache size to settings");
                cacheSize = DEFAULT_CACHE_SIZE;
                saveAfterLoad = true;
            }

            if (!int.TryParse(settingsNode.GetValue("disclaimer"), out disclaimerAccepted))
            {
                DarkLog.Debug("Adding disclaimer to settings");
                disclaimerAccepted = 0;
                saveAfterLoad = true;
            }

            if (!playerNode.TryGetValue("color", ref playerColor))
            {
                DarkLog.Debug("Adding color to settings");
                playerColor = PlayerColorWorker.GenerateRandomColor();
                OptionsWindow.fetch.loadEventHandled = false;
                saveAfterLoad = true;
            }

            int chatKey = (int)KeyCode.BackQuote, screenshotKey = (int)KeyCode.F8;
            if (!int.TryParse(bindingsNode.GetValue("chat"), out chatKey))
            {
                DarkLog.Debug("Adding chat key to settings");
                this.chatKey = KeyCode.BackQuote;
                saveAfterLoad = true;
            }
            else
            {
                this.chatKey = (KeyCode)chatKey;
            }

            if (!int.TryParse(bindingsNode.GetValue("screenshot"), out screenshotKey))
            {
                DarkLog.Debug("Adding screenshot key to settings");
                this.screenshotKey = KeyCode.F8;
                saveAfterLoad = true;
            }
            else
            {
                this.screenshotKey = (KeyCode)screenshotKey;
            }

            if (!playerNode.TryGetValue("flag", ref selectedFlag))
            {
                DarkLog.Debug("Adding selected flag to settings file");
                selectedFlag = "Squad/Flags/default";
                saveAfterLoad = true;
            }

            if (!settingsNode.TryGetValue("compression", ref compressionEnabled))
            {
                DarkLog.Debug("Adding compression flag to settings file");
                compressionEnabled = true;
                saveAfterLoad = true;
            }

            if (!settingsNode.TryGetValue("revert", ref revertEnabled))
            {
                DarkLog.Debug("Adding revert flag to settings file");
                revertEnabled = true;
                saveAfterLoad = true;
            }

            int toolbarType;
            if (!int.TryParse(settingsNode.GetValue("toolbar"), out toolbarType))
            {
                DarkLog.Debug("Adding toolbar flag to settings file");
                this.toolbarType = DMPToolbarType.BLIZZY_IF_INSTALLED;
                saveAfterLoad = true;
            }
            else
            {
                this.toolbarType = (DMPToolbarType)toolbarType;
            }

            ConfigNode serversNode = settingsNode.GetNode("SERVERS");
            servers = new List<ServerEntry>();
            if (serversNode.HasNode("SERVER"))
            {
                foreach (ConfigNode serverNode in serversNode.GetNodes("SERVER"))
                {
                    ServerEntry newServer = new ServerEntry();
                    newServer.name = serverNode.GetValue("name");
                    newServer.address = serverNode.GetValue("address");
                    serverNode.TryGetValue("port", ref newServer.port);
                    servers.Add(newServer);
                }
            }

            if (saveAfterLoad) SaveSettings();
        }

        public void SaveSettings()
        {
            ConfigNode mainNode = new ConfigNode();
            ConfigNode settingsNode = mainNode.AddNode("SETTINGS");
            ConfigNode playerNode = settingsNode.AddNode("PLAYER");

            playerNode.SetValue("name", playerName, true);
            playerNode.SetValue("color", playerColor, true);
            playerNode.SetValue("flag", selectedFlag, true);
            DarkLog.Debug(playerNode.ToString());

            ConfigNode bindingsNode = settingsNode.AddNode("KEYBINDINGS");
            bindingsNode.SetValue("chat", (int)chatKey, true);
            bindingsNode.SetValue("screenshot", (int)screenshotKey, true);

            settingsNode.SetValue("cacheSize", cacheSize, true);
            settingsNode.SetValue("disclaimer", disclaimerAccepted, true);
            settingsNode.SetValue("compression", compressionEnabled, true);
            settingsNode.SetValue("revert", revertEnabled, true);
            settingsNode.SetValue("toolbar", (int)toolbarType, true);

            ConfigNode serversNode = settingsNode.AddNode("SERVERS");
            serversNode.ClearNodes();
            foreach (ServerEntry server in servers)
            {
                ConfigNode serverNode = serversNode.AddNode("SERVER");
                serverNode.AddValue("name", server.name);
                serverNode.AddValue("address", server.address);
                serverNode.AddValue("port", server.port);
            }
            mainNode.Save(cnSettingsFile);

            File.Copy(cnSettingsFile, backupCnSettingsFile, true);
        }

        public ConfigNode GetDefaultSettings()
        {
            ConfigNode mainNode = new ConfigNode();
            ConfigNode settingsNode = new ConfigNode("SETTINGS");
            settingsNode.AddValue("cacheSize", DEFAULT_CACHE_SIZE);

            ConfigNode playerNode = new ConfigNode("PLAYER");
            playerNode.AddValue("name", DEFAULT_PLAYER_NAME);

            ConfigNode bindingsNode = new ConfigNode("KEYBINDINGS");

            ConfigNode serversNode = new ConfigNode("SERVERS");

            settingsNode.AddNode(playerNode);
            settingsNode.AddNode(bindingsNode);
            settingsNode.AddNode(serversNode);

            mainNode.AddNode(settingsNode);
            return mainNode;
        }

        private string newXMLString()
        {
            return String.Format("<?xml version=\"1.0\"?><settings><global username=\"{0}\" cache-size=\"{1}\"/><servers></servers></settings>", DEFAULT_PLAYER_NAME, DEFAULT_CACHE_SIZE);
        }
    }

    public class ServerEntry
    {
        public string name;
        public string address;
        public int port;
    }
}

