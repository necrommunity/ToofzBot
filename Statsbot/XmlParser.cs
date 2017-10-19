﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data;
using System.Xml;
using System.Net;
using Newtonsoft.Json;

namespace Statsbot
{

    public static class XmlParser
    {

        //currently not in use except ReadLeaderboards()

        public static Dictionary<Category, Leaderboard> lbInfo;

        public static void RegisterLeaderboards(Dictionary<Category, Leaderboard> lbInfo)
        {
            var list = new List<Leaderboard>();
            foreach (Leaderboard lb in lbInfo.Values)
            {
                list.Add(lb);
            }
            File.WriteAllText(@"Leaderboards.json", JsonConvert.SerializeObject(list, Newtonsoft.Json.Formatting.Indented));
        }

        public static void ReadLeaderboards()
        {
            var list = JsonConvert.DeserializeObject<List<Leaderboard>>(File.ReadAllText(@"Leaderboards.json"));
            lbInfo = new Dictionary<Category, Leaderboard>();
            foreach (Leaderboard lb in list)
            {
                lbInfo.Add(lb.Category, lb);
            }
        }

        public static Dictionary<Category, Leaderboard> ParseIndex()
        {
            string xml = ApiSender.GetLeaderboard("", 0);
            Dictionary<Category, Leaderboard> list = new Dictionary<Category, Leaderboard>();
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);

            foreach (XmlNode n in doc.DocumentElement.ChildNodes)
            {
                if (n.Name == "leaderboard")
                {
                    Leaderboard lb = new Leaderboard();

                    foreach (XmlNode e in n)
                    {
                        switch (e.Name)
                        {
                            case ("lbid"):
                                lb.ID = e.InnerText;
                                break;
                            case ("entries"):
                                lb.EntryCount = int.Parse(e.InnerText);
                                break;
                            case ("display_name"):
                                string s = e.InnerText;
                                lb.DisplayName = s;
                                if (!s.Contains("Amplified"))
                                    lb.Category.Product = Product.Classic;
                                if (s.Contains("Seeded"))
                                    lb.Category.Seeded = true;
                                if (s.Contains("Hard"))
                                    lb.Category.Mode = Mode.Hardmode;
                                if (s.Contains("Return"))
                                    lb.Category.Mode = Mode.NoReturn;
                                if (s.Contains("Random"))
                                    lb.Category.Mode = Mode.Randomizer;
                                if (s.Contains("Phasing"))
                                    lb.Category.Mode = Mode.Phasing;
                                if (s.Contains("Mystery"))
                                    lb.Category.Mode = Mode.Mystery;
                                for (int i = 0; i < 3; i++)
                                {
                                    if (s.Contains(Enum.GetNames(typeof(RunType))[i]))
                                    {
                                        lb.Category.Type = (RunType)i;
                                        break;
                                    }
                                }
                                for (int i = 0; i < 16; i++)
                                {
                                    if (s.Contains(Enum.GetNames(typeof(Character))[i]))
                                    {
                                        lb.Category.Char = (Character)i;
                                        break;
                                    }
                                }
                                break;
                        }
                    }

                    list.Add(lb.Category, lb);
                }
            }
            RegisterLeaderboards(list);
            return list;
        }

        public static List<SteamEntry> ParseLeaderboard(Leaderboard lb, int offset)
        {
            string xml = ApiSender.GetLeaderboard(lb.ID, offset);
            List<SteamEntry> list = new List<SteamEntry>();
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);

            lbInfo[lb.Category].EntryCount = Convert.ToInt32(doc.DocumentElement.ChildNodes[3].InnerText);

            int i = 7;
            if (doc.DocumentElement.ChildNodes[6].Name == "nextRequestURL")
                i = 8;

            foreach (XmlNode n in doc.DocumentElement.ChildNodes[i])
            {
                SteamEntry en = new SteamEntry();

                foreach (XmlNode e in n)
                {
                    switch (e.Name)
                    {
                        case "steamid":
                            en.Steamid = e.InnerText;
                            break;
                        case "score":
                            en.Score = int.Parse(e.InnerText);
                            break;
                        case "rank":
                            en.Rank = int.Parse(e.InnerText);
                            break;
                        case "ugcid":
                            en.UgcID = e.InnerText;
                            break;
                    }
                }

                list.Add(en);
            }
            return list;
        }
    }
}