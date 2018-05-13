﻿using System;
using System.IO;
using System.Threading.Tasks;
using System.Timers;
using Newtonsoft.Json;
using RetroClash.Database;
using RetroClash.Extensions;
using RetroClash.Logic.Manager;

namespace RetroClash.Logic
{
    public class Player : IDisposable
    {
        [JsonProperty("achievements")] public Achievements Achievements = new Achievements();

        [JsonProperty("heroes")] public LogicHeroManager HeroManager = new LogicHeroManager();

        [JsonIgnore] public DateTime LastChatMessage = DateTime.Now;

        [JsonIgnore] public LogicGameObjectManager LogicGameObjectManager = new LogicGameObjectManager();

        [JsonProperty("shield")] public LogicShield Shield = new LogicShield();

        [JsonIgnore] public Timer Timer = new Timer(5000)
        {
            AutoReset = true
        };

        [JsonProperty("units")] public Units Units = new Units();

        public Player(long id, string token)
        {
            AccountId = id;

            Name = "RetroClash";
            PassToken = token;
            ExpLevel = 1;
            Score = 2000;
            TutorialSteps = 10;
            Language = "en";

            LogicGameObjectManager.Json = Resources.Levels.StartingHome;
        }

        [JsonProperty("account_id")]
        public long AccountId { get; set; }

        [JsonProperty("alliance_id")]
        public long AllianceId { get; set; }

        [JsonProperty("account_name")]
        public string Name { get; set; }

        [JsonProperty("device_name")]
        public string DeviceName { get; set; }

        [JsonProperty("pass_token")]
        public string PassToken { get; set; }

        [JsonProperty("exp_level")]
        public int ExpLevel { get; set; }

        [JsonProperty("tutorial_steps")]
        public int TutorialSteps { get; set; }

        [JsonProperty("ip_address")]
        public string IpAddress { get; set; }

        [JsonIgnore]
        public int Score { get; set; }

        [JsonIgnore]
        public string Language { get; set; }

        [JsonIgnore]
        public Device Device { get; set; }

        public void Dispose()
        {
            Timer.Stop();

            Timer = null;
            Device = null;
            LogicGameObjectManager = null;
            Units = null;
        }

        public async Task LogicClientHome(MemoryStream stream)
        {
            await stream.WriteIntAsync(0);

            await stream.WriteLongAsync(AccountId); // Account Id

            await stream.WriteStringAsync(LogicGameObjectManager.Json);

            await stream.WriteIntAsync(Shield.ShieldSecondsLeft);
            await stream.WriteIntAsync(0);
            await stream.WriteIntAsync(0);
        }

        public async Task LogicClientAvatar(MemoryStream stream)
        {
            await stream.WriteIntAsync(0);
            await stream.WriteLongAsync(AccountId); // Account Id
            await stream.WriteLongAsync(AccountId); // Home Id

            if (AllianceId > 0)
            {
                var alliance = await MySQL.GetAlliance(AllianceId);

                if (alliance != null)
                {
                    stream.WriteByte(1);
                    await stream.WriteLongAsync(AllianceId); // Alliance Id
                    await stream.WriteStringAsync(alliance.Name); // Alliance Name
                    await stream.WriteIntAsync(alliance.Badge); // Alliance Badge
                    await stream.WriteIntAsync(alliance.Members.Find(x => x.AccountId == AccountId)
                        .Role); // Alliance Role

                    stream.WriteByte(1);
                    await stream.WriteLongAsync(AllianceId); // Alliance Id

                    stream.WriteByte(1);
                    await stream.WriteLongAsync(AllianceId); // Alliance Id
                }
                else
                {
                    stream.WriteByte(0);
                }
            }
            else
            {
                stream.WriteByte(0);
            }

            await stream.WriteIntAsync(LogicUtils.GetLeagueByScore(Score)); // League Type

            await stream.WriteIntAsync(0); // Alliance Castle Level
            await stream.WriteIntAsync(10); // Alliance Total Capacity
            await stream.WriteIntAsync(0); // Alliance Used Capacity
            await stream.WriteIntAsync(2); // Townhall Level

            await stream.WriteStringAsync(Name); // Name
            await stream.WriteStringAsync(null); // Facebook Id

            await stream.WriteIntAsync(ExpLevel); // Exp Level
            await stream.WriteIntAsync(0); // Exp Points

            await stream.WriteIntAsync(100000000); // Diamonts
            await stream.WriteIntAsync(0); // Current Diamonts
            await stream.WriteIntAsync(0); // Free Diamonts

            await stream.WriteIntAsync(0); // Unknown

            await stream.WriteIntAsync(Score); // Score

            await stream.WriteIntAsync(0); // Attack Win Count
            await stream.WriteIntAsync(0); // Attack Lose Count

            await stream.WriteIntAsync(0); // Defense Win Count
            await stream.WriteIntAsync(0); // Defense Lose Count

            stream.WriteByte(0); // Name Set By User (bool)
            await stream.WriteIntAsync(0);

            await stream.WriteIntAsync(0); // Resource Caps Count

            await stream.WriteIntAsync(3); // Resource DataSlot Count
            await stream.WriteIntAsync(3000001); // Gold
            await stream.WriteIntAsync(1000000000); // Count
            await stream.WriteIntAsync(3000002); // Elixir
            await stream.WriteIntAsync(1000000000); // Count
            await stream.WriteIntAsync(3000003); // Dark Elixir
            await stream.WriteIntAsync(100000000); // Count

            // Troops
            await stream.WriteIntAsync(Units.Troops.Count);
            foreach (var troop in Units.Troops)
            {
                await stream.WriteIntAsync(troop.Id);
                await stream.WriteIntAsync(troop.Count);
            }

            // Spells
            await stream.WriteIntAsync(Units.Spells.Count);
            foreach (var spell in Units.Spells)
            {
                await stream.WriteIntAsync(spell.Id);
                await stream.WriteIntAsync(spell.Count);
            }

            // Troop Levels
            await stream.WriteIntAsync(Units.Troops.Count);
            foreach (var troop in Units.Troops)
            {
                await stream.WriteIntAsync(troop.Id);
                await stream.WriteIntAsync(troop.Level);
            }

            // Spell Levels
            await stream.WriteIntAsync(Units.Spells.Count);
            foreach (var spell in Units.Spells)
            {
                await stream.WriteIntAsync(spell.Id);
                await stream.WriteIntAsync(spell.Level);
            }

            await stream.WriteIntAsync(HeroManager.Count); // Hero Upgrade DataSlot Count         
            foreach (var hero in HeroManager)
            {
                await stream.WriteIntAsync(hero.Id);
                await stream.WriteIntAsync(hero.Level);
            }

            await stream.WriteIntAsync(HeroManager.Count); // Hero Health DataSlot Count
            foreach (var hero in HeroManager)
            {
                await stream.WriteIntAsync(hero.Id);
                await stream.WriteIntAsync(hero.Health);
            }

            await stream.WriteIntAsync(HeroManager.Count); // Hero State DataSlot Count
            foreach (var hero in HeroManager)
            {
                await stream.WriteIntAsync(hero.Id);
                await stream.WriteIntAsync(hero.State);
            }

            await stream.WriteIntAsync(0); // Alliance Unit DataSlot Count

            await stream.WriteIntAsync(TutorialSteps); // TutorialSteps
            for (var index = 21000000; index < 21000000 + TutorialSteps; index++)
                await stream.WriteIntAsync(index);

            await stream.WriteIntAsync(Achievements.Count);
            foreach (var achievement in Achievements)
                await stream.WriteIntAsync(achievement.Id);

            await stream.WriteIntAsync(Achievements.Count); // Achievement Progress DataSlot Count
            foreach (var achievement in Achievements)
            {
                await stream.WriteIntAsync(achievement.Id);
                await stream.WriteIntAsync(achievement.Data);
            }

            await stream.WriteIntAsync(50); // Npc Map Progress DataSlot Count
            for (var index = 17000000; index < 17000050; index++)
            {
                await stream.WriteIntAsync(index);
                await stream.WriteIntAsync(3);
            }

            await stream.WriteIntAsync(0); // Npc Looted Gold DataSlot
            await stream.WriteIntAsync(0); // Npc Looted Elixir DataSlot
        }

        public async Task AvatarRankingEntry(MemoryStream stream)
        {
            await stream.WriteIntAsync(ExpLevel); // Exp Level
            await stream.WriteIntAsync(0); // Attack Win Count
            await stream.WriteIntAsync(0); // Attack Lose Count
            await stream.WriteIntAsync(0); // Defense Win Count
            await stream.WriteIntAsync(0); // Defense Lose Count
            await stream.WriteIntAsync(LogicUtils.GetLeagueByScore(Score)); // League Type

            await stream.WriteStringAsync(Language); // Country
            await stream.WriteLongAsync(AccountId); // Home Id

            if (AllianceId > 0)
            {
                var alliance = await MySQL.GetAlliance(AllianceId);

                if (alliance != null)
                {
                    stream.WriteByte(1);
                    await stream.WriteLongAsync(AllianceId); // Clan Id
                    await stream.WriteStringAsync(alliance.Name); // Clan Name
                    await stream.WriteIntAsync(alliance.Badge); // Badge
                }
                else
                {
                    AllianceId = 0;
                    stream.WriteByte(0);
                }
            }
            else
            {
                stream.WriteByte(0);
            }
        }

        public async void SaveCallback(object state, ElapsedEventArgs args)
        {
            await MySQL.SavePlayer(this);
        }
    }
}