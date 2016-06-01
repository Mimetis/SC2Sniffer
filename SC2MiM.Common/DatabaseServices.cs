using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;
using System.IO;
using System.Diagnostics;

using SC2MiM.Common.Entities;

namespace SC2MiM.Common
{
    public class DatabaseServices
    {
        public static event EventHandler<String> EventOccured;
        public static event EventHandler<ApplicationException> ErrorOccuredEvent;

        public static void SetDeviceInformations(DeviceProperties deviceProperties)
        {
            SqlConnection myConnexion = new SqlConnection(ConfigurationManager.ConnectionStrings["Starcraft2"].ConnectionString);

            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "spInsertOrUpdateDeviceInformations";
            cmd.Connection = myConnexion;

            SqlParameter p = new SqlParameter("@DeviceId", SqlDbType.UniqueIdentifier);
            p.Value = deviceProperties.DeviceId;
            cmd.Parameters.Add(p);

            p = new SqlParameter("@DeviceName", SqlDbType.NVarChar);
            p.Value = deviceProperties.DeviceName;
            cmd.Parameters.Add(p);

            p = new SqlParameter("@DeviceType", SqlDbType.NVarChar);
            p.Value = deviceProperties.DeviceType;
            cmd.Parameters.Add(p);

            p = new SqlParameter("@DeviceFirmwareVersion", SqlDbType.NVarChar);
            p.Value = deviceProperties.DeviceFirmwareVersion;
            cmd.Parameters.Add(p);

            p = new SqlParameter("@DeviceHardwareVersion", SqlDbType.NVarChar);
            p.Value = deviceProperties.DeviceHardwareVersion;
            cmd.Parameters.Add(p);

            p = new SqlParameter("@DeviceManufacturer", SqlDbType.NVarChar);
            p.Value = deviceProperties.DeviceManufacturer;
            cmd.Parameters.Add(p);

            p = new SqlParameter("@LastModifiedDate", SqlDbType.DateTime);
            p.Value = deviceProperties.LastModifiedDate;
            cmd.Parameters.Add(p);

            try
            {
                myConnexion.Open();

                cmd.ExecuteNonQuery();

                myConnexion.Close();
            }
            catch (Exception)
            {
                if (myConnexion.State != ConnectionState.Closed) myConnexion.Close();

                throw;
            }
            return;
        }


        public static List<CharacterLight> Search(string name, int count)
        {
            SqlConnection myConnexion = new SqlConnection(ConfigurationManager.ConnectionStrings["Starcraft2"].ConnectionString);

            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = String.Format("Select Top {0} RegionId, CharacterId, ZoneId, CultureId, Name, Code, MostPlayedRace from Character where Name like @name order by Name asc", count);
            cmd.Connection = myConnexion;

            SqlParameter p = new SqlParameter("@name", SqlDbType.NVarChar, 150);
            p.Value = name + '%';
            cmd.Parameters.Add(p);

            List<CharacterLight> characters = new List<CharacterLight>();

            try
            {
                myConnexion.Open();

                var reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        CharacterLight c = GetCharacterLightFromReader(reader);
                        characters.Add(c);
                    }
                }

                myConnexion.Close();

            }
            catch (Exception exc)
            {
                Trace.WriteLine("Error during DatabaseServices.Search : " + exc.Message);

                if (myConnexion.State != ConnectionState.Closed) myConnexion.Close();

            }

            return characters;
        }

        public static Character GetCharacter(int characterId, string characterName, String regionId, Int32 zoneId)
        {

            SqlConnection myConnexion = new SqlConnection(ConfigurationManager.ConnectionStrings["Starcraft2"].ConnectionString);

            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = "Select * from Character where RegionId=@regionId and ZoneId=@zoneId and CharacterId=@characterId and Name=@characterName";
            cmd.Connection = myConnexion;

            SqlParameter p = new SqlParameter("@regionId", SqlDbType.NVarChar, 5);
            p.Value = regionId;
            cmd.Parameters.Add(p);

            p = new SqlParameter("@characterId", SqlDbType.Int);
            p.Value = characterId;
            cmd.Parameters.Add(p);

            p = new SqlParameter("@zoneId", SqlDbType.Int);
            p.Value = zoneId;
            cmd.Parameters.Add(p);

            p = new SqlParameter("@characterName", SqlDbType.NVarChar, 150);
            p.Value = characterName;
            cmd.Parameters.Add(p);

            Character character = null;

            try
            {
                myConnexion.Open();

                var reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    reader.Read();

                    character = GetCharacterFromReader(reader);
                }

                myConnexion.Close();

            }
            catch (Exception exc)
            {
                Trace.WriteLine("Error during DatabaseServices.GetCharacter : " + exc.Message);

                if (myConnexion.State != ConnectionState.Closed) myConnexion.Close();

            }

            return character;
        }

        public static List<CharacterReward> GetCharacterRewards(string regionId, Int32 characterId, int zoneId)
        {
            SqlConnection myConnexion = new SqlConnection(ConfigurationManager.ConnectionStrings["Starcraft2"].ConnectionString);

            List<CharacterReward> lst = new List<CharacterReward>();

            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = "Select * from CharacterRewards where RegionId=@regionId and CharacterId=@characterId and ZoneId=@zoneId";
            cmd.Connection = myConnexion;

            SqlParameter p = new SqlParameter("@regionId", SqlDbType.NVarChar);
            p.Value = regionId;
            cmd.Parameters.Add(p);

            p = new SqlParameter("@characterId", SqlDbType.Int);
            p.Value = characterId;
            cmd.Parameters.Add(p);

            p = new SqlParameter("@zoneId", SqlDbType.Int);
            p.Value = zoneId;
            cmd.Parameters.Add(p);

            try
            {
                myConnexion.Open();

                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    CharacterReward c = new CharacterReward();

                    c.CharacterId = (Int32)reader["CharacterId"];
                    c.RewardId = (String)reader["RewardId"];
                    c.ZoneId = reader["ZoneId"] != DBNull.Value ? (Int32)reader["ZoneId"] : 1;
                    c.Name = (String)reader["Name"];
                    c.Description = (String)reader["Description"];
                    c.FullDescription = (String)reader["FullDescription"];
                    c.LastModifiedDate = reader["LastModifiedDate"] != DBNull.Value ? (DateTime)reader["LastModifiedDate"] : new DateTime(2000, 1, 1);

                    lst.Add(c);
                }

                myConnexion.Close();
            }
            catch (Exception)
            {
                if (myConnexion.State != ConnectionState.Closed) myConnexion.Close();

                throw;

            }
            return lst;
        }

        public static Dictionary<Int32, League> GetMinimumLeaguesFields(string regionId, Int32 zoneId)
        {
            SqlConnection myConnexion = new SqlConnection(ConfigurationManager.ConnectionStrings["Starcraft2"].ConnectionString);

            Dictionary<Int32, League> lst = new Dictionary<Int32, League>();

            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = "Select LeagueId, RegionId, ZoneId, DivisionName from League where LastModifiedDate is not null and RegionId=@regionId and ZoneId=@zoneId";
            cmd.Connection = myConnexion;

            SqlParameter param = new SqlParameter("@regionId", SqlDbType.NVarChar, 5);
            param.Value = regionId;
            cmd.Parameters.Add(param);

            param = new SqlParameter("@zoneId", SqlDbType.Int);
            param.Value = zoneId;
            cmd.Parameters.Add(param);

            try
            {
                myConnexion.Open();

                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    League l = new League();
                    l.LeagueId = (int)reader["LeagueId"];
                    l.RegionId = reader["RegionId"] as String;
                    l.ZoneId = reader["ZoneId"] != DBNull.Value ? (Int32)reader["ZoneId"] : 1;
                    l.DivisionName = (String)reader["DivisionName"];

                    lst.Add(l.LeagueId, l);
                }

                myConnexion.Close();
            }
            catch (Exception)
            {

                if (myConnexion.State != ConnectionState.Closed) myConnexion.Close();

                throw;
            }
            return lst;
        }

        public static Dictionary<String, Character> GetMinimumCharactersFields(String regionId, DateTime? olderThan = null)
        {
            if (!olderThan.HasValue)
                olderThan = DateTime.Now;

            SqlConnection myConnexion = new SqlConnection(ConfigurationManager.ConnectionStrings["Starcraft2"].ConnectionString);

            Dictionary<String, Character> lst = new Dictionary<String, Character>();

            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = "Select CharacterId, RegionId, ZoneId, Name from Character where RegionId=@regionId and LastModifiedDate < @olderThan ";
            cmd.Connection = myConnexion;

            SqlParameter param = new SqlParameter("@regionId", SqlDbType.NVarChar, 5);
            param.Value = regionId;
            cmd.Parameters.Add(param);

            param = new SqlParameter("@olderThan", SqlDbType.DateTime);
            param.Value = olderThan;
            cmd.Parameters.Add(param);

            try
            {
                myConnexion.Open();

                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    Character c = new Character();
                    c.CharacterId = (int)reader["CharacterId"];
                    c.RegionId = reader["RegionId"] as String;
                    c.ZoneId = reader["ZoneId"] != DBNull.Value ? (Int32)reader["ZoneId"] : 1;
                    c.Name = (String)reader["Name"];

                    var key = String.Format("{0}/{1}/{2}", c.CharacterId, c.ZoneId, c.Name);
                    lst.Add(key, c);
                }

                myConnexion.Close();
            }
            catch (Exception)
            {

                if (myConnexion.State != ConnectionState.Closed) myConnexion.Close();

                throw;
            }
            return lst;

        }
        
        public static List<League> GetLeagues(string regionId, TeamType teamType, int zoneId)
        {
            SqlConnection myConnexion = new SqlConnection(ConfigurationManager.ConnectionStrings["Starcraft2"].ConnectionString);

            List<League> lst = new List<League>();

            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = "Select * from League where RegionId=@regionId and TeamType=@teamType and ZoneId=@zoneId Order By LeagueId asc";
            cmd.Connection = myConnexion;

            SqlParameter p = new SqlParameter("@regionId", SqlDbType.NVarChar);
            p.Value = regionId;
            cmd.Parameters.Add(p);

            p = new SqlParameter("@teamType", SqlDbType.Int);
            p.Value = (int)teamType;
            cmd.Parameters.Add(p);

            p = new SqlParameter("@zoneId", SqlDbType.Int);
            p.Value = zoneId;
            cmd.Parameters.Add(p);

            try
            {
                myConnexion.Open();

                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    League l = GetLeagueFromReader(reader);

                    lst.Add(l);
                }

                myConnexion.Close();
            }
            catch (Exception)
            {
                if (myConnexion.State != ConnectionState.Closed) myConnexion.Close();

                throw;

            }
            return lst;
        }

        public static List<CharacterLeague> GetCharacterLeagues(League league)
        {
            SqlConnection myConnexion = new SqlConnection(ConfigurationManager.ConnectionStrings["Starcraft2"].ConnectionString);

            List<CharacterLeague> lst = new List<CharacterLeague>();

            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = "Select CL.* from CharacterLeague CL " +
                                "Where CL.LeagueId=@leagueId and CL.RegionId=@regionId and CL.ZoneId=@zoneId " +
                                "Order by CL.LeagueId asc";
            cmd.Connection = myConnexion;

            SqlParameter p = new SqlParameter("@regionId", SqlDbType.NVarChar);
            p.Value = league.RegionId;
            cmd.Parameters.Add(p);

            p = new SqlParameter("@leagueId", SqlDbType.Int);
            p.Value = league.LeagueId;
            cmd.Parameters.Add(p);

            p = new SqlParameter("@zoneId", SqlDbType.Int);
            p.Value = league.ZoneId;
            cmd.Parameters.Add(p);

            try
            {
                myConnexion.Open();

                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    CharacterLeague l = GetCharacterLeagueFromReader(reader);

                    lst.Add(l);
                }

                myConnexion.Close();
            }
            catch (Exception)
            {
                if (myConnexion.State != ConnectionState.Closed) myConnexion.Close();

                throw;

            }
            return lst;
        }

        public static List<CharacterLeague> GetTopCharactersLeagues(string regionId)
        {
            SqlConnection myConnexion = new SqlConnection(ConfigurationManager.ConnectionStrings["Starcraft2"].ConnectionString);

            List<CharacterLeague> lst = new List<CharacterLeague>();

            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = "SELECT Top 200 CL.Name, CL.CharacterId, CL.ZoneId, CL.RegionId, CL.Points, CL.VictoriesCount, CL.LossesCount, CL.MostPlayedRace, " +
                                "ROW_NUMBER() OVER (ORDER BY Points Desc) AS 'Rank'  " +
                                "FROM [CharacterLeague]  CL " +
                                "Inner Join League L on L.LeagueId = CL.LeagueId and L.RegionId = CL.RegionId and CL.ZoneId = L.ZoneId " +
                                "Where CL.RegionId = @regionId  " +
                                "And L.LeagueType = 7 And L.TeamType = 1 " +
                                "Order by CL.Points desc; ";

            cmd.Connection = myConnexion;

            SqlParameter p = new SqlParameter("@regionId", SqlDbType.NVarChar);
            p.Value = regionId;
            cmd.Parameters.Add(p);

            try
            {
                myConnexion.Open();

                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    CharacterLeague l = GetCharacterLeagueLightFromReader(reader);

                    lst.Add(l);
                }

                myConnexion.Close();
            }
            catch (Exception)
            {
                if (myConnexion.State != ConnectionState.Closed) myConnexion.Close();

                throw;

            }
            return lst;
        }


        public static List<Character> GetGrandMasters(string regionId)
        {
            SqlConnection myConnexion = new SqlConnection(ConfigurationManager.ConnectionStrings["Starcraft2"].ConnectionString);

            List<Character> lst = new List<Character>();

            SqlCommand cmd = new SqlCommand();
            cmd.Connection = myConnexion;

            if (string.IsNullOrEmpty(regionId) || regionId.ToLower() == "all")
            {
                cmd.CommandText = "SELECT C.* " +
                                  "From [Character] C " +
                                  "Inner Join [GrandMasterLeague]  GM On GM.CharacterId = C.CharacterId and GM.ZoneId = C.ZoneId and GM.RegionId = C.RegionId " +
                                  "Order by C.RegionId, GM.Rank asc; ";
            }
            else
            {
                cmd.CommandText = "SELECT C.* " +
                                  "From [Character] C " +
                                  "Inner Join [GrandMasterLeague]  GM On GM.CharacterId = C.CharacterId and GM.ZoneId = C.ZoneId and GM.RegionId = C.RegionId " +
                                  "Where GM.RegionId = @regionId  " +
                                  "Order by GM.Rank asc; ";


                SqlParameter p = new SqlParameter("@regionId", SqlDbType.NVarChar);
                p.Value = regionId;
                cmd.Parameters.Add(p);
            }

            try
            {
                myConnexion.Open();

                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    Character l = GetCharacterFromReader(reader);

                    lst.Add(l);
                }

                myConnexion.Close();
            }
            catch (Exception)
            {
                if (myConnexion.State != ConnectionState.Closed) myConnexion.Close();

                throw;

            }
            return lst;
        }


        public static List<CharacterLeague> GetGrandMastersLeagues(string regionId)
        {
            SqlConnection myConnexion = new SqlConnection(ConfigurationManager.ConnectionStrings["Starcraft2"].ConnectionString);

            List<CharacterLeague> lst = new List<CharacterLeague>();

            SqlCommand cmd = new SqlCommand();
            cmd.Connection = myConnexion;

            if (string.IsNullOrEmpty(regionId) || regionId.ToLower() == "all")
            {
                cmd.CommandText = "SELECT * " +
                                  "From [GrandMasterLeague]  " +
                                  "Order by RegionId, Rank asc; ";
            }
            else
            {
                cmd.CommandText = "SELECT * " +
                                  "From [GrandMasterLeague]  " +
                                  "Where RegionId = @regionId  " +
                                  "Order by Rank asc; ";


                SqlParameter p = new SqlParameter("@regionId", SqlDbType.NVarChar);
                p.Value = regionId;
                cmd.Parameters.Add(p);
            }

            try
            {
                myConnexion.Open();

                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    CharacterLeague l = GetCharacterLeagueFromReader(reader);

                    lst.Add(l);
                }

                myConnexion.Close();
            }
            catch (Exception)
            {
                if (myConnexion.State != ConnectionState.Closed) myConnexion.Close();

                throw;

            }
            return lst;
        }

        public static List<League> GetCharacterLeagues(int characterId, String regionId, Int32 zoneId, TeamType tt)
        {
            SqlConnection myConnexion = new SqlConnection(ConfigurationManager.ConnectionStrings["Starcraft2"].ConnectionString);

            List<League> lst = new List<League>();

            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = "Select L.* " +
                                "From CharacterLeague CL " +
                                "Inner Join League L on L.LeagueId = CL.LeagueId and L.RegionId = CL.RegionId and L.ZoneId = CL.ZoneId " +
                                "Where CL.CharacterId=@characterId and CL.RegionId=@regionId and CL.ZoneId=@zoneId and L.TeamType=@teamType Order By CL.[Rank] asc";

            cmd.Connection = myConnexion;

            SqlParameter p = new SqlParameter("@characterId", SqlDbType.Int);
            p.Value = characterId;
            cmd.Parameters.Add(p);

            p = new SqlParameter("@regionId", SqlDbType.NVarChar);
            p.Value = regionId;
            cmd.Parameters.Add(p);

            p = new SqlParameter("@zoneId", SqlDbType.Int);
            p.Value = zoneId;
            cmd.Parameters.Add(p);

            p = new SqlParameter("@teamType", SqlDbType.Int);
            p.Value = (Int32)tt;
            cmd.Parameters.Add(p);

            try
            {
                myConnexion.Open();

                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {

                    League currentLeague = GetLeagueFromReader(reader);

                    currentLeague.Characters = GetCharacterLeagues(currentLeague);

                    lst.Add(currentLeague);
                }

                myConnexion.Close();
            }
            catch (Exception)
            {
                if (myConnexion.State != ConnectionState.Closed) myConnexion.Close();

                throw;

            }
            return lst;
        }


        public static void DeleteCharacter(Character c)
        {
            SqlConnection myConnexion = new SqlConnection(ConfigurationManager.ConnectionStrings["Starcraft2"].ConnectionString);

            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = "Delete from CharacterLeague where CharacterId = @CharacterId and RegionId=@RegionId and ZoneId=@ZoneId; " +
                              "Delete from CharacterRewards where CharacterId = @CharacterId and RegionId=@RegionId and ZoneId=@ZoneId; " +
                              "Delete from Character where CharacterId = @CharacterId and RegionId=@RegionId and ZoneId=@ZoneId; ";

            cmd.Connection = myConnexion;

            SqlParameter p = new SqlParameter("@RegionId", SqlDbType.NVarChar);
            p.Value = c.RegionId;
            cmd.Parameters.Add(p);

            p = new SqlParameter("@CharacterId", SqlDbType.Int);
            p.Value = c.CharacterId;
            cmd.Parameters.Add(p);

            p = new SqlParameter("@ZoneId", SqlDbType.Int);
            p.Value = c.ZoneId;
            cmd.Parameters.Add(p);

            try
            {
                myConnexion.Open();

                cmd.ExecuteNonQuery();

                myConnexion.Close();
            }
            catch (Exception)
            {
                if (myConnexion.State != ConnectionState.Closed) myConnexion.Close();

                throw;

            }
            return;
        }


        public static void DeleteLeague(League league)
        {
            SqlConnection myConnexion = new SqlConnection(ConfigurationManager.ConnectionStrings["Starcraft2"].ConnectionString);

            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = "Delete from CharacterLeague where LeagueId = @LeagueId and RegionId=@RegionId and ZoneId=@ZoneId; " +
                              "Delete from League where LeagueId=@LeagueId and RegionId=@RegionId and ZoneId=@ZoneId";
            cmd.Connection = myConnexion;

            SqlParameter p = new SqlParameter("@regionId", SqlDbType.NVarChar);
            p.Value = league.RegionId;
            cmd.Parameters.Add(p);

            p = new SqlParameter("@leagueId", SqlDbType.Int);
            p.Value = league.LeagueId;
            cmd.Parameters.Add(p);

            p = new SqlParameter("@zoneId", SqlDbType.Int);
            p.Value = league.ZoneId;
            cmd.Parameters.Add(p);

            try
            {
                myConnexion.Open();

                cmd.ExecuteNonQuery();

                myConnexion.Close();
            }
            catch (Exception)
            {
                if (myConnexion.State != ConnectionState.Closed) myConnexion.Close();

                throw;

            }
            return;
        }

        // ------------------------------------------------------------------------------------
        // Merge elements (Insert / Update / Delete
        // ------------------------------------------------------------------------------------

        public static void MergeCharacter(Character character)
        {
            List<Character> lst = new List<Character>();
            lst.Add(character);
            MergeCharacters(lst);
        }

        public static void MergeCharacters(List<Character> chars)
        {
            if (chars == null || chars.Count == 0)
                return;

            SqlConnection myConnexion = new SqlConnection(ConfigurationManager.ConnectionStrings["Starcraft2"].ConnectionString);

            SqlCommand cmd = new SqlCommand();
            cmd.Connection = myConnexion;
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.CommandText = "spInsertOrUpdateCharacters";

            CreateTableValueParameter(cmd, chars);

            try
            {
                myConnexion.Open();

                cmd.ExecuteNonQuery();

                myConnexion.Close();

                RaiseMessage("Characters Inserted / Updated : " + chars.Count);

            }
            catch (Exception exc)
            {
                if (myConnexion.State != ConnectionState.Closed) myConnexion.Close();

                RaiseError(new ApplicationException("Error : [DatabaseServices].[MergeCharacters]", exc));
            }
        }

        public static void MergeCharacterRewards(List<CharacterReward> cr)
        {
            if (cr == null || cr.Count == 0)
                return;

            SqlConnection myConnexion = new SqlConnection(ConfigurationManager.ConnectionStrings["Starcraft2"].ConnectionString);

            SqlCommand cmd = new SqlCommand();
            cmd.Connection = myConnexion;
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.CommandText = "spInsertOrUpdateCharacterRewards";

            CreateTableValueParameter(cmd, cr);


            try
            {
                myConnexion.Open();

                cmd.ExecuteNonQuery();

                myConnexion.Close();
            }

            catch (Exception exc)
            {

                if (myConnexion.State != ConnectionState.Closed) myConnexion.Close();

                RaiseError(new ApplicationException("Error : [DatabaseServices].[MergeCharacterRewards]", exc));

            }

        }

        public static void MergeGrandMasterLeague(List<CharacterLeague> grandMasters)
        {
            if (grandMasters == null || grandMasters.Count == 0)
                return;

            SqlConnection myConnexion = new SqlConnection(ConfigurationManager.ConnectionStrings["Starcraft2"].ConnectionString);

            SqlCommand cmd = new SqlCommand();
            cmd.Connection = myConnexion;
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.CommandText = "spInsertOrUpdateGrandMasterLeague";

            try
            {
                CreateTableValueParameter(cmd, grandMasters);

            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }

            try
            {
                myConnexion.Open();

                cmd.ExecuteNonQuery();

                myConnexion.Close();
            }

            catch (Exception exc)
            {

                if (myConnexion.State != ConnectionState.Closed) myConnexion.Close();

                RaiseError(new ApplicationException("Error : [DatabaseServices].[MergeCharacterLeagues]", exc));

            }
        }

        public static void MergeCharacterLeagues(List<League> leagues)
        {
            if (leagues == null || leagues.Count == 0)
                return;

            SqlConnection myConnexion = new SqlConnection(ConfigurationManager.ConnectionStrings["Starcraft2"].ConnectionString);

            SqlCommand cmd = new SqlCommand();
            cmd.Connection = myConnexion;
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.CommandText = "spInsertOrUpdateCharacterLeague";

            try
            {
                CreateTableValueParameter(cmd, leagues);

            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }

            try
            {
                myConnexion.Open();

                cmd.ExecuteNonQuery();

                myConnexion.Close();
            }

            catch (Exception exc)
            {

                if (myConnexion.State != ConnectionState.Closed) myConnexion.Close();

                RaiseError(new ApplicationException("Error : [DatabaseServices].[MergeCharacterLeagues]", exc));

            }
        }

        // ------------------------------------------------------------------------------------
        // Get Objects From SqlDataReader
        // ------------------------------------------------------------------------------------

        private static Character GetCharacterFromReader(SqlDataReader reader)
        {
            Character character = new Character();

            character.RegionId = reader["RegionId"] as String;
            character.CharacterId = reader["CharacterId"] != DBNull.Value ? (Int32)reader["CharacterId"] : 0;
            character.Name = reader["Name"] as String;
            character.Code = reader["Code"] as String;
            character.ZoneId = reader["ZoneId"] != DBNull.Value ? (Int32)reader["ZoneId"] : 1;

            character.AchievementPoints = reader["AchievementPoints"] != DBNull.Value ? (Int32)reader["AchievementPoints"] : 0;

            character.CampaignBadge = reader["CampaignBadge"] as String;
            character.LastModifiedDate = reader["LastModifiedDate"] != DBNull.Value ? (DateTime)reader["LastModifiedDate"] : DateTime.Now.AddYears(-1);
            character.LeaguesVictoriesCount = reader["LeaguesVictoriesCount"] != DBNull.Value ? (Int32)reader["LeaguesVictoriesCount"] : 0;
            character.LeaguesMatchesCount = reader["LeaguesMatchesCount"] != DBNull.Value ? (Int32)reader["LeaguesMatchesCount"] : 0;

            character.MostPlayedRace = reader["MostPlayedRace"] != DBNull.Value ? (RaceType)(Byte)reader["MostPlayedRace"] : RaceType.None;

            if (reader["Leagues"] != DBNull.Value)
                character.LeaguesSummary = GetLeaguesFromXml((String)reader["Leagues"]);

            character.PortraitJpgName = reader["PortraitJpgName"] as String;
            character.PortraitUrl = reader["PortraitUrl"] as String;
            character.PortraitPositionX = reader["PortraitPositionX"] != DBNull.Value ? (Int32)reader["PortraitPositionX"] : 0;
            character.PortraitPositionY = reader["PortraitPositionY"] != DBNull.Value ? (Int32)reader["PortraitPositionY"] : 0;


            return character;
        }

        private static CharacterLight GetCharacterLightFromReader(SqlDataReader reader)
        {
            CharacterLight character = new CharacterLight();

            character.RegionId = reader["RegionId"] as String;
            character.CharacterId = (Int32)reader["CharacterId"];
            character.ZoneId = reader["ZoneId"] != DBNull.Value ? (Int32)reader["ZoneId"] : 1;
            character.Code = reader["Code"] as String;
            character.Name = reader["Name"] as String;
            character.MostPlayedRace = reader["MostPlayedRace"] != DBNull.Value ? (RaceType)(Byte)reader["MostPlayedRace"] : RaceType.None;

            return character;
        }

        private static League GetLeagueFromReader(SqlDataReader reader)
        {
            League league = new League();

            league.LeagueId = reader["LeagueId"] != DBNull.Value ? (Int32)reader["LeagueId"] : 0;
            league.RegionId = reader["RegionId"] as String;
            league.ZoneId = reader["ZoneId"] != DBNull.Value ? (Int32)reader["ZoneId"] : 1;
            league.DivisionName = reader["DivisionName"] as String;
            league.TeamType = reader["TeamType"] != DBNull.Value ? (TeamType)reader["TeamType"] : TeamType.OneVOne;
            league.LastModifiedDateTime = reader["LastModifiedDate"] != DBNull.Value ? (DateTime)reader["LastModifiedDate"] : DateTime.Now.AddYears(-1);
            return league;
        }

        private static CharacterLeague GetCharacterLeagueFromReader(SqlDataReader reader)
        {
            CharacterLeague characterLeague = new CharacterLeague();

            characterLeague.CharacterId = reader["CharacterId"] != DBNull.Value ? (Int32)reader["CharacterId"] : 0;
            characterLeague.LossesCount = reader["LossesCount"] != DBNull.Value ? (Int32)reader["LossesCount"] : 0;
            characterLeague.Points = reader["Points"] != DBNull.Value ? (Int32)reader["Points"] : 0;
            characterLeague.Rank = reader["Rank"] != DBNull.Value ? (Byte)reader["Rank"] : (Byte)0;
            characterLeague.VictoriesCount = reader["VictoriesCount"] != DBNull.Value ? (Int32)reader["VictoriesCount"] : 0;
            characterLeague.Name = reader["Name"] as String;
            characterLeague.MostPlayedRace = reader["MostPlayedRace"] != DBNull.Value ? (RaceType)(Byte)reader["MostPlayedRace"] : RaceType.None;
            characterLeague.RegionId = reader["RegionId"] as String;
            characterLeague.ZoneId = reader["ZoneId"] != DBNull.Value ? (Int32)reader["ZoneId"] : 1;
            characterLeague.LastModifiedDate = reader["LastModifiedDate"] != DBNull.Value ? (DateTime)reader["LastModifiedDate"] : DateTime.Now.AddYears(-1);

            return characterLeague;
        }

        private static CharacterLeague GetCharacterLeagueLightFromReader(SqlDataReader reader)
        {
            CharacterLeague characterLeague = new CharacterLeague();

            characterLeague.CharacterId = reader["CharacterId"] != DBNull.Value ? (Int32)reader["CharacterId"] : 0;
            characterLeague.LossesCount = reader["LossesCount"] != DBNull.Value ? (Int32)reader["LossesCount"] : 0;
            characterLeague.Points = reader["Points"] != DBNull.Value ? (Int32)reader["Points"] : 0;
            characterLeague.VictoriesCount = reader["VictoriesCount"] != DBNull.Value ? (Int32)reader["VictoriesCount"] : 0;
            characterLeague.Rank = reader["Rank"] != DBNull.Value ? (Byte)reader["Rank"] : (Byte)0;
            characterLeague.Name = reader["Name"] as String;
            characterLeague.MostPlayedRace = reader["MostPlayedRace"] != DBNull.Value ? (RaceType)(Byte)reader["MostPlayedRace"] : RaceType.None;
            characterLeague.RegionId = reader["RegionId"] as String;
            characterLeague.ZoneId = reader["ZoneId"] != DBNull.Value ? (Int32)reader["ZoneId"] : 1;
            characterLeague.LastModifiedDate = reader["LastModifiedDate"] != DBNull.Value ? (DateTime)reader["LastModifiedDate"] : DateTime.Now.AddYears(-1);

            return characterLeague;
        }

        private static DataTable CreateLeaguesDataTable()
        {
            DataSet ds = new DataSet("Leagues");
            DataTable dt = new DataTable("League");
            ds.Tables.Add(dt);

            DataColumn dc = new DataColumn("LeagueType", typeof(Int32));
            dc.ColumnMapping = MappingType.Attribute;
            dt.Columns.Add(dc);

            dc = new DataColumn("TeamType", typeof(Int32));
            dc.ColumnMapping = MappingType.Attribute;
            dt.Columns.Add(dc);

            dc = new DataColumn("Division", typeof(String));
            dc.ColumnMapping = MappingType.Attribute;
            dt.Columns.Add(dc);

            dc = new DataColumn("BestRank", typeof(Int32));
            dc.ColumnMapping = MappingType.Attribute;
            dt.Columns.Add(dc);

            dc = new DataColumn("VictoriesCount", typeof(Int32));
            dc.ColumnMapping = MappingType.Attribute;
            dt.Columns.Add(dc);

            dc = new DataColumn("MatchesCount", typeof(Int32));
            dc.ColumnMapping = MappingType.Attribute;
            dt.Columns.Add(dc);
            return dt;
        }

        private static List<LeagueSummary> GetLeaguesFromXml(String xml)
        {
            DataTable dt = CreateLeaguesDataTable();
            StringReader reader = new StringReader(xml);
            List<LeagueSummary> leagues = new List<LeagueSummary>();

            try
            {
                dt.ReadXml(reader);

                if (dt.Rows.Count != 0)
                {

                    foreach (DataRow dr in dt.Rows)
                    {
                        LeagueSummary summary = new LeagueSummary();

                        summary.LeagueType = dr["LeagueType"] != DBNull.Value ? (LeagueType)dr["LeagueType"] : LeagueType.Bronze;
                        summary.TeamType = dr["TeamType"] != DBNull.Value ? (TeamType)dr["TeamType"] : TeamType.OneVOne;
                        summary.BestRank = dr["BestRank"] != DBNull.Value ? (int)dr["BestRank"] : 0;
                        summary.Division = dr["Division"] as String;
                        summary.MatchesCount = dr["MatchesCount"] != DBNull.Value ? (int)dr["MatchesCount"] : 0;
                        summary.VictoriesCount = dr["VictoriesCount"] != DBNull.Value ? (int)dr["VictoriesCount"] : 0;

                        leagues.Add(summary);
                    }

                }
            }
            catch (Exception exc)
            {
                Trace.WriteLine("Error during DatabaseServices.GetLeaguesFromXml : " + exc.Message);
            }
            reader.Close();
            return leagues;

        }

        private static String GetXmlFromLeagues(List<LeagueSummary> list)
        {
            DataTable dt = CreateLeaguesDataTable();

            if (list != null)
            {

                foreach (var ls in list)
                {
                    DataRow dr = dt.NewRow();
                    dr["LeagueType"] = (int)ls.LeagueType;
                    dr["TeamType"] = (int)ls.TeamType;
                    dr["Division"] = ls.Division;
                    dr["BestRank"] = ls.BestRank;
                    dr["VictoriesCount"] = ls.VictoriesCount;
                    dr["MatchesCount"] = ls.MatchesCount;
                    dt.Rows.Add(dr);
                }
            }

            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            dt.WriteXml(sw);
            sw.Close();

            return sb.ToString();
        }

        // ------------------------------------------------------------------------------------
        // Create Table Value Parameter
        // ------------------------------------------------------------------------------------

        private static void CreateTableValueParameter(SqlCommand cmd, List<League> leagues)
        {
            SqlParameter param = new SqlParameter("@tmpCharacterLeagues", SqlDbType.Structured);

            DataTable table = new DataTable("CharacterLeaguesTvp");

            table.Columns.Add(new DataColumn("LeagueId", typeof(Int32)));
            table.Columns.Add(new DataColumn("RegionId", typeof(String)));
            table.Columns.Add(new DataColumn("CharacterId", typeof(Int32)));
            table.Columns.Add(new DataColumn("ZoneId", typeof(Int32)));
            table.Columns.Add(new DataColumn("Name", typeof(String)));
            table.Columns.Add(new DataColumn("MostPlayedRace", typeof(Byte)));
            table.Columns.Add(new DataColumn("DivisionName", typeof(String)));
            table.Columns.Add(new DataColumn("TeamType", typeof(Int32)));
            table.Columns.Add(new DataColumn("LeagueType", typeof(Int32)));
            table.Columns.Add(new DataColumn("Rank", typeof(Byte)));
            table.Columns.Add(new DataColumn("Points", typeof(Int32)));
            table.Columns.Add(new DataColumn("VictoriesCount", typeof(Int32)));
            table.Columns.Add(new DataColumn("LossesCount", typeof(Int32)));
            table.Columns.Add(new DataColumn("LastModifiedDate", typeof(DateTime)));
            table.Columns["LastModifiedDate"].AllowDBNull = true;

            foreach (League l in leagues)
            {
                foreach (var c in l.Characters)
                {
                    DataRow dr = table.NewRow();
                    dr["LeagueId"] = l.LeagueId;
                    dr["RegionId"] = l.RegionId;
                    dr["CharacterId"] = c.CharacterId;
                    dr["ZoneId"] = c.ZoneId;
                    dr["Name"] = c.Name;
                    dr["MostPlayedRace"] = (Byte)c.MostPlayedRace;
                    dr["DivisionName"] = l.DivisionName;
                    dr["TeamType"] = (Int32)l.TeamType;
                    dr["LeagueType"] = (Int32)l.LeagueType;
                    dr["Rank"] = c.Rank;
                    dr["Points"] = c.Points;
                    dr["VictoriesCount"] = c.VictoriesCount;
                    dr["LossesCount"] = c.LossesCount;
                    dr["LastModifiedDate"] = DateTime.Now;

                    table.Rows.Add(dr);
                }
            }

            param.Value = table;

            cmd.Parameters.Add(param);
        }

        private static void CreateTableValueParameter(SqlCommand cmd, List<CharacterLeague> grandMasters)
        {
            SqlParameter param = new SqlParameter("@tmpCharacterLeagues", SqlDbType.Structured);

            DataTable table = new DataTable("CharacterLeaguesTvp");

            table.Columns.Add(new DataColumn("LeagueId", typeof(Int32)));
            table.Columns.Add(new DataColumn("RegionId", typeof(String)));
            table.Columns.Add(new DataColumn("CharacterId", typeof(Int32)));
            table.Columns.Add(new DataColumn("ZoneId", typeof(Int32)));
            table.Columns.Add(new DataColumn("Name", typeof(String)));
            table.Columns.Add(new DataColumn("MostPlayedRace", typeof(Byte)));
            table.Columns.Add(new DataColumn("DivisionName", typeof(String)));
            table.Columns.Add(new DataColumn("TeamType", typeof(Int32)));
            table.Columns.Add(new DataColumn("LeagueType", typeof(Int32)));
            table.Columns.Add(new DataColumn("Rank", typeof(Byte)));
            table.Columns.Add(new DataColumn("Points", typeof(Int32)));
            table.Columns.Add(new DataColumn("VictoriesCount", typeof(Int32)));
            table.Columns.Add(new DataColumn("LossesCount", typeof(Int32)));
            table.Columns.Add(new DataColumn("LastModifiedDate", typeof(DateTime)));
            table.Columns["LastModifiedDate"].AllowDBNull = true;

            foreach (var c in grandMasters)
            {
                DataRow dr = table.NewRow();
                dr["LeagueId"] = 0;
                dr["RegionId"] = c.RegionId;
                dr["CharacterId"] = c.CharacterId;
                dr["ZoneId"] = c.ZoneId;
                dr["Name"] = c.Name;
                dr["MostPlayedRace"] = (Byte)c.MostPlayedRace;
                dr["Rank"] = c.Rank;
                dr["Points"] = c.Points;
                dr["VictoriesCount"] = c.VictoriesCount;
                dr["LossesCount"] = c.LossesCount;
                dr["LastModifiedDate"] = DateTime.Now;

                table.Rows.Add(dr);
            }

            param.Value = table;

            cmd.Parameters.Add(param);
        }

        private static void CreateTableValueParameter(SqlCommand cmd, List<CharacterReward> cr)
        {
            SqlParameter param = new SqlParameter("@tmpCharacterRewards", SqlDbType.Structured);

            DataTable dtCharacterRewards = new DataTable("CharacterRewardsTvp");

            dtCharacterRewards.Columns.Add(new DataColumn("RegionId", typeof(String)));
            dtCharacterRewards.Columns.Add(new DataColumn("CharacterId", typeof(Int32)));
            dtCharacterRewards.Columns.Add(new DataColumn("ZoneId", typeof(Int32)));
            dtCharacterRewards.Columns.Add(new DataColumn("RewardId", typeof(String)));
            dtCharacterRewards.Columns.Add(new DataColumn("Name", typeof(String)));
            dtCharacterRewards.Columns.Add(new DataColumn("Description", typeof(String)));
            dtCharacterRewards.Columns["Description"].AllowDBNull = true;
            dtCharacterRewards.Columns.Add(new DataColumn("FullDescription", typeof(String)));
            dtCharacterRewards.Columns["FullDescription"].AllowDBNull = true;
            dtCharacterRewards.Columns.Add(new DataColumn("LastModifiedDate", typeof(DateTime)));
            dtCharacterRewards.Columns["LastModifiedDate"].AllowDBNull = true;


            foreach (CharacterReward c in cr)
            {

                DataRow dr = dtCharacterRewards.NewRow();
                dr["RegionId"] = c.RegionId;
                dr["CharacterId"] = c.CharacterId;
                dr["ZoneId"] = c.ZoneId;
                dr["RewardId"] = c.RewardId;
                dr["Name"] = c.Name;
                dr["Description"] = c.Description;
                dr["FullDescription"] = c.FullDescription;
                dr["LastModifiedDate"] = DateTime.Now;

                dtCharacterRewards.Rows.Add(dr);
            }

            param.Value = dtCharacterRewards;

            cmd.Parameters.Add(param);
        }

        private static void CreateTableValueParameter(SqlCommand cmd, List<Character> chars)
        {
            SqlParameter param = new SqlParameter("@tmpCharacters", SqlDbType.Structured);

            DataTable dtCharacters = new DataTable("CharactersTvp");

            dtCharacters.Columns.Add(new DataColumn("RegionId", typeof(String)));

            dtCharacters.Columns.Add(new DataColumn("CharacterId", typeof(Int32)));

            dtCharacters.Columns.Add(new DataColumn("ZoneId", typeof(Int32)));

            dtCharacters.Columns.Add(new DataColumn("CultureId", typeof(String)));

            dtCharacters.Columns.Add(new DataColumn("Name", typeof(String)));

            dtCharacters.Columns.Add(new DataColumn("Code", typeof(String)));
            dtCharacters.Columns["Code"].AllowDBNull = true;

            dtCharacters.Columns.Add(new DataColumn("Leagues", typeof(String)));
            dtCharacters.Columns["Leagues"].AllowDBNull = true;

            dtCharacters.Columns.Add(new DataColumn("LeaguesVictoriesCount", typeof(Int32)));
            dtCharacters.Columns["LeaguesVictoriesCount"].AllowDBNull = true;

            dtCharacters.Columns.Add(new DataColumn("LeaguesMatchesCount", typeof(Int32)));
            dtCharacters.Columns["LeaguesMatchesCount"].AllowDBNull = true;

            dtCharacters.Columns.Add(new DataColumn("AchievementPoints", typeof(Int32)));
            dtCharacters.Columns["AchievementPoints"].AllowDBNull = true;

            dtCharacters.Columns.Add(new DataColumn("MostPlayedRace", typeof(Byte)));
            dtCharacters.Columns["MostPlayedRace"].AllowDBNull = true;

            dtCharacters.Columns.Add(new DataColumn("CampaignBadge", typeof(String)));
            dtCharacters.Columns["CampaignBadge"].AllowDBNull = true;

            dtCharacters.Columns.Add(new DataColumn("LastModifiedDate", typeof(DateTime)));
            dtCharacters.Columns["LastModifiedDate"].AllowDBNull = true;

            //dtCharacters.Columns.Add(new DataColumn("PortraitUrl", typeof(String)));
            //dtCharacters.Columns["PortraitUrl"].AllowDBNull = true;

            //dtCharacters.Columns.Add(new DataColumn("PortraitJpgName", typeof(String)));
            //dtCharacters.Columns["PortraitJpgName"].AllowDBNull = true;

            //dtCharacters.Columns.Add(new DataColumn("PortraitPositionX", typeof(Int32)));
            //dtCharacters.Columns["PortraitPositionX"].AllowDBNull = true;

            //dtCharacters.Columns.Add(new DataColumn("PortraitPositionY", typeof(Int32)));
            //dtCharacters.Columns["PortraitPositionY"].AllowDBNull = true;

            foreach (Character c in chars)
            {
                DataRow dr = dtCharacters.NewRow();

                dr["RegionId"] = c.RegionId;
                dr["CharacterId"] = c.CharacterId;
                dr["ZoneId"] = c.ZoneId;
                dr["CultureId"] = "en";
                dr["Name"] = c.Name;
                dr["Code"] = c.Code;
                dr["Leagues"] = GetXmlFromLeagues(c.LeaguesSummary);
                dr["LeaguesVictoriesCount"] = c.LeaguesVictoriesCount;
                dr["LeaguesMatchesCount"] = c.LeaguesMatchesCount;
                dr["AchievementPoints"] = c.AchievementPoints;
                dr["MostPlayedRace"] = (Int32)c.MostPlayedRace;
                dr["CampaignBadge"] = c.CampaignBadge;
                dr["LastModifiedDate"] = DateTime.Now;
                //dr["PortraitUrl"] = c.PortraitUrl;
                //dr["PortraitJpgName"] = c.PortraitJpgName;
                //dr["PortraitPositionX"] = c.PortraitPositionX;
                //dr["PortraitPositionY"] = c.PortraitPositionY;

                dtCharacters.Rows.Add(dr);
            }

            param.Value = dtCharacters;

            cmd.Parameters.Add(param);

        }

        // ------------------------------------------------------------------------------------
        // Raise messages
        // ------------------------------------------------------------------------------------

        private static void RaiseMessage(String message)
        {
            if (EventOccured != null)
                EventOccured(null, message);
        }

        private static void RaiseError(ApplicationException ex)
        {
            if (ErrorOccuredEvent != null)
                ErrorOccuredEvent(null, ex);
        }







    }
}
