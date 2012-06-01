﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows;
using WPFAutoCompleteTextbox;
using System.Globalization;

namespace CampahApp
{
    class FileIO
    {
        AutoCompleteTextBox tbBuyItemSelect;
        public FileIO(AutoCompleteTextBox tbBuyItemSelect)
        {
            ThreadManager.threadRunner(loadAHResourcesXML);
            this.tbBuyItemSelect = tbBuyItemSelect;
        }

        public bool loadBidList(string file)
        {
            try
            {
                if (!File.Exists(file))
                {
                    return false;
                }
                XmlDocument xDoc = new XmlDocument();
                xDoc.Load(file);
                XmlNodeList items;
                items = xDoc.GetElementsByTagName("item");
                RunningData.Instance.BidList.Clear();
                foreach (XmlNode item in items)
                {
                    int minimum;
                    int maximum;
                    int increment;
                    int quantity;
                    bool stack;
                    string name = item.Attributes["name"].Value;
                    if (!int.TryParse(item.Attributes["minimum"].Value, out minimum))
                        return false;
                    if (!int.TryParse(item.Attributes["maximum"].Value, out maximum))
                        return false;
                    if (!int.TryParse(item.Attributes["increment"].Value, out increment))
                        return false;
                    if (!int.TryParse(item.Attributes["quantity"].Value, out quantity))
                        return false;
                    if (!bool.TryParse(item.Attributes["stack"].Value, out stack))
                        return false;
                    AHItem reqitem = AuctionHouse.GetItem(name);//items[name.ToLower()];
                    if (reqitem == null)
                        return false;
                    RunningData.Instance.BidList.Add(new ItemRequest(minimum, maximum, increment, quantity, stack, reqitem));
                }
                CampahStatus.Instance.CurrentPath = file;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool saveBidList(string file)
        {
            try
            {
                XmlTextWriter tw = new XmlTextWriter(file, null);
                tw.Formatting = Formatting.Indented;
                tw.WriteStartDocument();
                tw.WriteStartElement("BidList");
                tw.WriteComment("This XML was automatically generated by Campah.exe");
                tw.WriteComment("Editing this XML may cause Campah to no longer function properly");
                foreach (ItemRequest item in RunningData.Instance.BidList)
                {
                    tw.WriteStartElement("item");
                    tw.WriteAttributeString("name", item.ItemData.Name);
                    tw.WriteAttributeString("stack", item.Stack.ToString());
                    tw.WriteAttributeString("minimum", item.Minimum.ToString());
                    tw.WriteAttributeString("maximum", item.Maximum.ToString());
                    tw.WriteAttributeString("increment", item.Increment.ToString());
                    tw.WriteAttributeString("quantity", item.Quantity.ToString());
                    tw.WriteEndElement();
                }
                tw.WriteEndElement();
                tw.WriteEndDocument();
                tw.Flush();
                tw.Close();
                CampahStatus.Instance.CurrentPath = file;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void loadAHResourcesXML()
        {
            if (!File.Exists("ahresources.xml"))
            {
                MessageBox.Show("Error! The file ahresources.xml was not found.\r\nCreate a new ahresources by moving your character near an\r\nauction house and pressing the \"Create New AH Resources\" button in Campah Settings.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load("ahresources.xml");
            XmlNodeList items;
            items = xDoc.GetElementsByTagName("item");
            AuctionHouse.items.Clear();
            foreach (XmlNode item in items)
            {
                AuctionHouse.Add(new AHItem(int.Parse(item.Attributes["id"].Value, NumberStyles.HexNumber),
                    item.Attributes["name"].Value,
                    bool.Parse(item.Attributes["stackable"].Value),
                    item.Attributes["address"].Value));
            }
            populateAutoCompleteTB(AuctionHouse.items.Keys.ToArray<String>());
        }

        private void populateAutoCompleteTB(String[] items)
        {
            tbBuyItemSelect.ClearList();
            foreach (string item in items)
                tbBuyItemSelect.AddItem(new AutoCompleteEntry(item, null));
        }

        public void defaultcampahsettings()
        {
            CampahStatus.Instance.BuyCycleWait = 1; CampahStatus.Instance.GlobalDelay = 400;
            RunningData.Instance.AHTargetList.Clear();
            RunningData.Instance.AHTargetList.Add(new AHTarget("Auction Counter"));
        }

        public void loadSettingsXML()
        {
            if (!File.Exists("campah_settings.xml"))
            {
                defaultcampahsettings();
                SaveCampahSettings();
            }
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load("campah_settings.xml");
            XmlNodeList items;
            items = xDoc.GetElementsByTagName("Target");
            RunningData.Instance.AHTargetList.Clear();
            foreach (XmlNode item in items)
            {
                RunningData.Instance.AHTargetList.Add(new AHTarget(item.Attributes["Name"].Value));
            }

            double delay;
            double cyclewait;
            bool blockcommands;
            bool automaticupdates = true;
            bool alwaysontop = false;
            bool openlast = false;
            bool cheapo = false;
            bool allowcyclerandom = true;
            string lowballamount = "";
            string chatfilters = "";
            int oneclickmin = 80;
            int oneclickinc = 10;
            int webtimeout = 5000;

            string currentpath = Environment.CurrentDirectory;
            try
            {
                double.TryParse(xDoc["Settings"]["GlobalSettings"].Attributes["Delay"].Value, out delay);
                double.TryParse(xDoc["Settings"]["BuySettings"].Attributes["CycleWait"].Value, out cyclewait);
                bool.TryParse(xDoc["Settings"]["BuySettings"].Attributes["AllowCycleRandom"].Value, out allowcyclerandom);
                bool.TryParse(xDoc["Settings"]["BuySettings"].Attributes["CheapMode"].Value, out cheapo);
                lowballamount = xDoc["Settings"]["BuySettings"].Attributes["LowballAmount"].Value;
                bool.TryParse(xDoc["Settings"]["GlobalSettings"].Attributes["BlockCommands"].Value, out blockcommands);
                bool.TryParse(xDoc["Settings"]["GlobalSettings"].Attributes["AutomaticUpdates"].Value, out automaticupdates);
                bool.TryParse(xDoc["Settings"]["GlobalSettings"].Attributes["AlwaysOnTop"].Value, out alwaysontop);
                bool.TryParse(xDoc["Settings"]["GlobalSettings"].Attributes["OpenLastOnLoad"].Value, out openlast);
                chatfilters = xDoc["Settings"]["GlobalSettings"].Attributes["ChatFilters"].Value;
                int.TryParse(xDoc["Settings"]["BuySettings"].Attributes["OneClickMinimumPercent"].Value, out oneclickmin);
                int.TryParse(xDoc["Settings"]["BuySettings"].Attributes["OneClickIncrementCount"].Value, out oneclickinc);
                int.TryParse(xDoc["Settings"]["BuySettings"].Attributes["WebTimeout"].Value, out webtimeout);
            }
            catch
            {
                delay = 500;
                cyclewait = 1;
                blockcommands = false;
            }

            CampahStatus.Instance.ChatFilter = chatfilters;
            CampahStatus.Instance.GlobalDelay = delay;
            CampahStatus.Instance.BuyCycleWait = cyclewait;
            CampahStatus.Instance.AllowCycleRandom = allowcyclerandom;
            CampahStatus.Instance.BlockCommands = blockcommands;
            CampahStatus.Instance.AutomaticUpdates = automaticupdates;
            CampahStatus.Instance.AlwaysOnTop = alwaysontop;
            CampahStatus.Instance.CurrentPath = currentpath;
            CampahStatus.Instance.OpenLast = openlast;
            CampahStatus.Instance.CheapO = cheapo;
            CampahStatus.Instance.LowballBid = lowballamount;
            CampahStatus.Instance.OneClickMin = oneclickmin;
            CampahStatus.Instance.OneClickInc = oneclickinc;
            CampahStatus.Instance.WebTimeout = webtimeout;
            SaveCampahSettings();
        }

        public void SaveCampahSettings()
        {
            XmlTextWriter tw = new XmlTextWriter("campah_settings.xml", null);
            tw.Formatting = Formatting.Indented;
            tw.WriteStartDocument();
            tw.WriteStartElement("Settings");
            tw.WriteComment("This XML was automatically generated by Campah.exe");
            tw.WriteComment("Editing this XML may cause Campah to no longer function properly");
            tw.WriteStartElement("AHTarget");
            foreach (AHTarget target in RunningData.Instance.AHTargetList)
            {
                tw.WriteStartElement("Target");
                tw.WriteAttributeString("Name", target.TargetName);
                tw.WriteEndElement();
            }
            tw.WriteEndElement();

            tw.WriteStartElement("GlobalSettings");
            tw.WriteAttributeString("Delay", CampahStatus.Instance.GlobalDelay.ToString("#"));
            tw.WriteAttributeString("BlockCommands", CampahStatus.Instance.BlockCommands.ToString());
            tw.WriteAttributeString("AutomaticUpdates", CampahStatus.Instance.AutomaticUpdates.ToString());
            tw.WriteAttributeString("AlwaysOnTop", CampahStatus.Instance.AlwaysOnTop.ToString());
            tw.WriteAttributeString("OpenLastOnLoad", CampahStatus.Instance.OpenLast.ToString());
            tw.WriteAttributeString("ChatFilters", CampahStatus.Instance.ChatFilter);
            tw.WriteEndElement();
            tw.WriteStartElement("BuySettings");
            tw.WriteAttributeString("CycleWait", CampahStatus.Instance.BuyCycleWait.ToString("#.0"));
            tw.WriteAttributeString("AllowCycleRandom", CampahStatus.Instance.AllowCycleRandom.ToString());
            tw.WriteAttributeString("CheapMode", CampahStatus.Instance.CheapO.ToString());
            tw.WriteAttributeString("LowballAmount", CampahStatus.Instance.LowballBid);
            tw.WriteAttributeString("OneClickMinimumPercent", CampahStatus.Instance.OneClickMin.ToString());
            tw.WriteAttributeString("OneClickIncrementCount", CampahStatus.Instance.OneClickInc.ToString());
            tw.WriteAttributeString("WebTimeout", CampahStatus.Instance.WebTimeout.ToString());
            tw.WriteEndElement();

            tw.WriteEndElement();
            tw.WriteEndDocument();
            tw.Flush();
            tw.Close();
        }
    }
}