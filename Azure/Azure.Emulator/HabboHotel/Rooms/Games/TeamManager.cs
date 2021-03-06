using System.Collections.Generic;
using System.Drawing;
using Azure.HabboHotel.Items;

namespace Azure.HabboHotel.Rooms.Games
{
    public class TeamManager
    {
        public string Game;
        public List<RoomUser> BlueTeam;
        public List<RoomUser> RedTeam;
        public List<RoomUser> YellowTeam;
        public List<RoomUser> GreenTeam;

        public static TeamManager CreateTeamforGame(string game)
        {
            return new TeamManager
            {
                Game = game,
                BlueTeam = new List<RoomUser>(),
                RedTeam = new List<RoomUser>(),
                GreenTeam = new List<RoomUser>(),
                YellowTeam = new List<RoomUser>()
            };
        }

        public bool CanEnterOnTeam(Team t)
        {
            if (t.Equals(Team.blue)) return BlueTeam.Count < 5;
            if (t.Equals(Team.red)) return RedTeam.Count < 5;
            if (t.Equals(Team.yellow)) return YellowTeam.Count < 5;
            return t.Equals(Team.green) && GreenTeam.Count < 5;
        }

        public void AddUser(RoomUser user)
        {
            if (user == null || user.GetClient() == null) return;
            if (user.Team.Equals(Team.blue)) BlueTeam.Add(user);
            else
            {
                if (user.Team.Equals(Team.red)) RedTeam.Add(user);
                else
                {
                    if (user.Team.Equals(Team.yellow)) YellowTeam.Add(user);
                    else if (user.Team.Equals(Team.green)) GreenTeam.Add(user);
                }
            }

            if (string.IsNullOrEmpty(Game)) return;
            switch (Game.ToLower())
            {
                case "banzai":
                    var currentRoom = user.GetClient().GetHabbo().CurrentRoom;
                    using (var enumerator = currentRoom.GetRoomItemHandler().FloorItems.Values.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            var current = enumerator.Current;
                            if (current.GetBaseItem().InteractionType.Equals(Interaction.BanzaiGateBlue))
                            {
                                current.ExtraData = BlueTeam.Count.ToString();
                                current.UpdateState();
                                if (BlueTeam.Count != 5) continue;
                                foreach (
                                    var current2 in currentRoom.GetGameMap().GetRoomUsers(new Point(current.X, current.Y))) current2.SqState = 0;
                                currentRoom.GetGameMap().GameMap[current.X, current.Y] = 0;
                            }
                            else
                            {
                                if (current.GetBaseItem().InteractionType.Equals(Interaction.BanzaiGateRed))
                                {
                                    current.ExtraData = RedTeam.Count.ToString();
                                    current.UpdateState();
                                    if (RedTeam.Count != 5) continue;
                                    foreach (
                                        var current3 in
                                            currentRoom.GetGameMap().GetRoomUsers(new Point(current.X, current.Y))) current3.SqState = 0;
                                    currentRoom.GetGameMap().GameMap[current.X, current.Y] = 0;
                                }
                                else
                                {
                                    if (current.GetBaseItem().InteractionType.Equals(Interaction.BanzaiGateGreen))
                                    {
                                        current.ExtraData = GreenTeam.Count.ToString();
                                        current.UpdateState();
                                        if (GreenTeam.Count != 5) continue;
                                        foreach (
                                            var current4 in
                                                currentRoom.GetGameMap().GetRoomUsers(new Point(current.X, current.Y))) current4.SqState = 0;
                                        currentRoom.GetGameMap().GameMap[current.X, current.Y] = 0;
                                    }
                                    else
                                    {
                                        if (!current.GetBaseItem().InteractionType.Equals(Interaction.BanzaiGateYellow)) continue;
                                        current.ExtraData = YellowTeam.Count.ToString();
                                        current.UpdateState();
                                        if (YellowTeam.Count != 5) continue;
                                        foreach (
                                            var current5 in
                                                currentRoom.GetGameMap().GetRoomUsers(new Point(current.X, current.Y))) current5.SqState = 0;
                                        currentRoom.GetGameMap().GameMap[current.X, current.Y] = 0;
                                    }
                                }
                            }
                        }
                    }
                    break;
                case "freeze":
                    var currentRoom2 = user.GetClient().GetHabbo().CurrentRoom;
                    foreach (var current6 in currentRoom2.GetRoomItemHandler().FloorItems.Values)
                    {
                        switch (current6.GetBaseItem().InteractionType)
                        {
                            case Interaction.FreezeBlueGate:
                                current6.ExtraData = BlueTeam.Count.ToString();
                                current6.UpdateState();
                                break;
                            case Interaction.FreezeRedGate:
                                current6.ExtraData = RedTeam.Count.ToString();
                                current6.UpdateState();
                                break;
                            case Interaction.FreezeGreenGate:
                                current6.ExtraData = GreenTeam.Count.ToString();
                                current6.UpdateState();
                                break;
                            case Interaction.FreezeYellowGate:
                                current6.ExtraData = YellowTeam.Count.ToString();
                                current6.UpdateState();
                                break;
                        }
                    }
                    break;
            }
        }

        public void OnUserLeave(RoomUser user)
        {
            if (user == null) return;
            if (user.Team.Equals(Team.blue)) BlueTeam.Remove(user);
            else
            {
                if (user.Team.Equals(Team.red)) RedTeam.Remove(user);
                else
                {
                    if (user.Team.Equals(Team.yellow)) YellowTeam.Remove(user);
                    else if (user.Team.Equals(Team.green)) GreenTeam.Remove(user);
                }
            }
            if (string.IsNullOrEmpty(Game)) return;

            var currentRoom = user.GetClient().GetHabbo().CurrentRoom;
            if (currentRoom == null) return;

            switch (Game.ToLower())
            {
                case "banzai":
                    using (var enumerator = currentRoom.GetRoomItemHandler().FloorItems.Values.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            var current = enumerator.Current;
                            if (current.GetBaseItem().InteractionType.Equals(Interaction.BanzaiGateBlue))
                            {
                                current.ExtraData = BlueTeam.Count.ToString();
                                current.UpdateState();
                                if (currentRoom.GetGameMap().GameMap[current.X, current.Y] != 0) continue;
                                foreach (
                                    var current2 in
                                        currentRoom.GetGameMap().GetRoomUsers(new Point(current.X, current.Y))) current2.SqState = 1;
                                currentRoom.GetGameMap().GameMap[current.X, current.Y] = 1;
                            }
                            else
                            {
                                if (current.GetBaseItem().InteractionType.Equals(Interaction.BanzaiGateRed))
                                {
                                    current.ExtraData = RedTeam.Count.ToString();
                                    current.UpdateState();
                                    if (currentRoom.GetGameMap().GameMap[current.X, current.Y] != 0) continue;
                                    foreach (
                                        var current3 in
                                            currentRoom.GetGameMap().GetRoomUsers(new Point(current.X, current.Y))) current3.SqState = 1;
                                    currentRoom.GetGameMap().GameMap[current.X, current.Y] = 1;
                                }
                                else
                                {
                                    if (current.GetBaseItem().InteractionType.Equals(Interaction.BanzaiGateGreen))
                                    {
                                        current.ExtraData = GreenTeam.Count.ToString();
                                        current.UpdateState();
                                        if (currentRoom.GetGameMap().GameMap[current.X, current.Y] != 0) continue;
                                        foreach (
                                            var current4 in
                                                currentRoom.GetGameMap().GetRoomUsers(new Point(current.X, current.Y))) current4.SqState = 1;
                                        currentRoom.GetGameMap().GameMap[current.X, current.Y] = 1;
                                    }
                                    else
                                    {
                                        if (!current.GetBaseItem().InteractionType.Equals(Interaction.BanzaiGateYellow)) continue;
                                        current.ExtraData = YellowTeam.Count.ToString();
                                        current.UpdateState();
                                        if (currentRoom.GetGameMap().GameMap[current.X, current.Y] != 0) continue;
                                        foreach (
                                            var current5 in
                                                currentRoom.GetGameMap().GetRoomUsers(new Point(current.X, current.Y))) current5.SqState = 1;
                                        currentRoom.GetGameMap().GameMap[current.X, current.Y] = 1;
                                    }
                                }
                            }
                        }
                    }
                    break;
                case "freeze":
                    foreach (var current6 in currentRoom.GetRoomItemHandler().FloorItems.Values)
                    {
                        switch (current6.GetBaseItem().InteractionType)
                        {
                            case Interaction.FreezeBlueGate:
                                current6.ExtraData = BlueTeam.Count.ToString();
                                current6.UpdateState();
                                break;
                            case Interaction.FreezeRedGate:
                                current6.ExtraData = RedTeam.Count.ToString();
                                current6.UpdateState();
                                break;
                            case Interaction.FreezeGreenGate:
                                current6.ExtraData = GreenTeam.Count.ToString();
                                current6.UpdateState();
                                break;
                            case Interaction.FreezeYellowGate:
                                current6.ExtraData = YellowTeam.Count.ToString();
                                current6.UpdateState();
                                break;
                        }
                    }
                    break;
            }
        }
    }
}