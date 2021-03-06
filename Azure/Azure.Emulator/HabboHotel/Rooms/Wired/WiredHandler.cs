using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Azure.HabboHotel.Items;
using Azure.HabboHotel.Rooms.Wired.Handlers.Conditions;
using Azure.HabboHotel.Rooms.Wired.Handlers.Effects;
using Azure.HabboHotel.Rooms.Wired.Handlers.Triggers;

namespace Azure.HabboHotel.Rooms.Wired
{
    public class WiredHandler
    {
        private readonly List<IWiredItem> _wiredItems;
        private readonly Room _room;
        private Queue _cycleItems;

        public WiredHandler(Room room)
        {
            _wiredItems = new List<IWiredItem>();
            _room = room;
            _cycleItems = new Queue();
        }

        public static void OnEvent(IWiredItem item)
        {
            if (item.Item.ExtraData == "1") return;
            item.Item.ExtraData = "1";
            item.Item.UpdateState(false, true);
            item.Item.ReqUpdate(1, true);
        }

        public IWiredItem LoadWired(IWiredItem fItem)
        {
            if (fItem == null || fItem.Item == null)
            {
                if (_wiredItems.Contains(fItem)) _wiredItems.Remove(fItem);
                return null;
            }
            using (var queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery("SELECT * FROM items_wireds WHERE id=@id LIMIT 1");
                queryReactor.AddParameter("id", fItem.Item.Id);
                var row = queryReactor.GetRow();
                if (row == null)
                {
                    var wiredItem = GenerateNewItem(fItem.Item);
                    AddWired(wiredItem);
                    SaveWired(wiredItem);
                    return wiredItem;
                }
                fItem.OtherString = row["string"].ToString();
                fItem.OtherBool = (row["bool"].ToString() == "1");
                fItem.Delay = (int) row["delay"];
                fItem.OtherExtraString = row["extra_string"].ToString();
                fItem.OtherExtraString2 = row["extra_string_2"].ToString();
                var array = row["items"].ToString().Split(';');
                foreach (var s in array)
                {
                    int value;
                    if (!int.TryParse(s, out value)) continue;
                    var item = _room.GetRoomItemHandler().GetItem(Convert.ToUInt32(value));
                    fItem.Items.Add(item);
                }
                AddWired(fItem);
            }
            return fItem;
        }

        public static void SaveWired(IWiredItem fItem)
        {
            if (fItem == null) return;
            using (var queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
            {
                var text = string.Empty;
                var num = 0;
                foreach (var current in fItem.Items)
                {
                    if (num != 0) text += ";";
                    text += current.Id;
                    num++;
                }
                if (fItem.OtherString == null) fItem.OtherString = string.Empty;
                if (fItem.OtherExtraString == null) fItem.OtherExtraString = string.Empty;
                if (fItem.OtherExtraString2 == null) fItem.OtherExtraString2 = string.Empty;
                queryReactor.SetQuery(
                    "REPLACE INTO items_wireds VALUES (@id, @items, @delay, @string, @bool, @extrastring, @extrastring2)");
                queryReactor.AddParameter("id", fItem.Item.Id);
                queryReactor.AddParameter("items", text);
                queryReactor.AddParameter("delay", fItem.Delay);
                queryReactor.AddParameter("string", fItem.OtherString);
                queryReactor.AddParameter("bool", Azure.BoolToEnum(fItem.OtherBool));
                queryReactor.AddParameter("extrastring", fItem.OtherExtraString);
                queryReactor.AddParameter("extrastring2", fItem.OtherExtraString2);
                queryReactor.RunQuery();
            }
        }

        public void ReloadWired(IWiredItem item)
        {
            SaveWired(item);
            RemoveWired(item);
            AddWired(item);
        }

        public void ResetExtraString(Interaction type)
        {
            lock (_wiredItems)
            {
                foreach (var current in _wiredItems.Where(current => current != null && current.Type == type)) current.OtherExtraString = "0";
            }
        }

        public bool ExecuteWired(Interaction type, params object[] stuff)
        {
            try
            {
                if (!IsTrigger(type) || stuff == null) return false;
                if (type == Interaction.TriggerCollision)
                {
                    foreach (
                        var wiredItem in _wiredItems.Where(wiredItem => wiredItem != null && wiredItem.Type == type)) wiredItem.Execute(stuff);
                }
                else if (_wiredItems.Any(current => current != null && current.Type == type && current.Execute(stuff))) return true;
            }
            catch (Exception e)
            {
                Writer.Writer.HandleException(e, "WiredHandler.cs:ExecuteWired Type: " + type);
            }
            return false;
        }

        public void OnCycle()
        {
            try
            {
                var queue = new Queue();
                lock (_cycleItems.SyncRoot)
                {
                    while (_cycleItems.Count > 0)
                    {
                        var wiredItem = (IWiredItem) _cycleItems.Dequeue();
                        var item = wiredItem as IWiredCycler;
                        if (item == null) continue;
                        var wiredCycler = item;
                        if (!wiredCycler.OnCycle()) queue.Enqueue(item);
                    }
                }

                _cycleItems = queue;
            }
            catch (Exception e)
            {
                Writer.Writer.HandleException(e, "WiredHandler.cs:OnCycle");
            }
        }

        public void EnqueueCycle(IWiredItem item) { if (!_cycleItems.Contains(item)) _cycleItems.Enqueue(item); }

        public bool IsCycleQueued(IWiredItem item) { return _cycleItems.Contains(item); }

        public void AddWired(IWiredItem item)
        {
            if (_wiredItems.Contains(item)) _wiredItems.Remove(item);
            _wiredItems.Add(item);
        }

        public void RemoveWired(IWiredItem item)
        {
            if (!_wiredItems.Contains(item)) _wiredItems.Remove(item);
            _wiredItems.Remove(item);
        }

        public void RemoveWired(RoomItem item)
        {
            foreach (var current in _wiredItems)
            {
                if (current == null || current.Item == null || current.Item.Id != item.Id) continue;
                var queue = new Queue();
                lock (_cycleItems.SyncRoot)
                {
                    while (_cycleItems.Count > 0)
                    {
                        var wiredItem = (IWiredItem) _cycleItems.Dequeue();
                        if (wiredItem.Item.Id != item.Id) queue.Enqueue(wiredItem);
                    }
                }
                _cycleItems = queue;
                _wiredItems.Remove(current);
                break;
            }
        }

        public IWiredItem GenerateNewItem(RoomItem item)
        {
            switch (item.GetBaseItem().InteractionType)
            {
                case Interaction.TriggerRoomEnter:
                    return new UserEntersRoom(item, _room);

                case Interaction.TriggerGameEnd:
                    return new GameEnds(item, _room);

                case Interaction.TriggerGameStart:
                    return new GameStarts(item, _room);

                case Interaction.TriggerRepeater:
                    return new Repeater(item, _room);

                case Interaction.TriggerLongRepeater:
                    return new LongRepeater(item, _room);

                case Interaction.TriggerOnUserSay:
                    return new SaysKeyword(item, _room);

                case Interaction.TriggerScoreAchieved:
                    return new ScoreAchieved(item, _room);

                case Interaction.TriggerStateChanged:
                    return new FurniStateToggled(item, _room);

                case Interaction.TriggerWalkOnFurni:
                    return new WalksOnFurni(item, _room);

                case Interaction.TriggerWalkOffFurni:
                    return new WalksOffFurni(item, _room);

                case Interaction.TriggerCollision:
                    return new Collision(item, this, _room);

                case Interaction.ActionMoveRotate:
                    return new MoveRotateFurni(item, _room);

                case Interaction.ActionMoveToDir:
                    return new MoveToDir(item, _room);

                case Interaction.ActionShowMessage:
                    return new ShowMessage(item, _room);

                case Interaction.ActionTeleportTo:
                    return new TeleportToFurni(item, _room);

                case Interaction.ActionToggleState:
                    return new ToggleFurniState(item, _room);

                case Interaction.ActionResetTimer:
                    return new ResetTimers(item, _room);

                case Interaction.ActionKickUser:
                    return new KickUser(item, _room);

                case Interaction.ConditionFurnisHaveUsers:
                    return new FurniHasUsers(item, _room);

                // CONDICIONES NUEVAS:

                case Interaction.ConditionItemsMatches:
                    return new ItemsCoincide(item, _room);

                case Interaction.ConditionFurniTypeMatches:
                    return new ItemsTypeMatches(item, _room);

                case Interaction.ConditionHowManyUsersInRoom:
                    return new HowManyUsers(item, _room);

                case Interaction.ConditionGroupMember:
                    return new IsGroupMember(item, _room);

                case Interaction.ConditionTriggerOnFurni:
                    return new TriggererOnFurni(item, _room);

                case Interaction.ConditionFurniHasFurni:
                    return new FurniHasFurni(item, _room);

                case Interaction.ConditionUserWearingEffect:
                    return new UserIsWearingEffect(item, _room);

                case Interaction.ConditionUserWearingBadge:
                    return new UserIsWearingBadge(item, _room);

                case Interaction.ConditionUserHasFurni:
                    return new UserHasFurni(item, _room);

                case Interaction.ConditionDateRangeActive:
                    return new DateRangeActive(item, _room);

                case Interaction.ConditionTimeMoreThan:
                    return new TimeMoreThan(item, _room);

                case Interaction.ConditionTimeLessThan:
                    return new TimeLessThan(item, _room);

                // CONDICIONES NEGATIVAS:
                case Interaction.ConditionTriggererNotOnFurni:
                    return new TriggererNotOnFurni(item, _room);

                case Interaction.ConditionFurniHasNotFurni:
                    return new FurniHasNotFurni(item, _room);

                case Interaction.ConditionFurnisHaveNotUsers:
                    return new FurniHasNotUsers(item, _room);

                case Interaction.ConditionItemsDontMatch:
                    return new ItemsNotCoincide(item, _room);

                case Interaction.ConditionFurniTypeDontMatch:
                    return new ItemsTypeDontMatch(item, _room);

                case Interaction.ConditionNotGroupMember:
                    return new IsNotGroupMember(item, _room);

                case Interaction.ConditionNegativeHowManyUsers:
                    return new NotHowManyUsersInRoom(item, _room);

                case Interaction.ConditionUserNotWearingEffect:
                    return new UserIsNotWearingEffect(item, _room);

                case Interaction.ConditionUserNotWearingBadge:
                    return new UserIsNotWearingBadge(item, _room);

                // Efectos NUEVOS:
                case Interaction.ActionGiveReward:
                    return new GiveReward(item, _room);

                case Interaction.ActionPosReset:
                    return new ResetPosition(item, _room);

                case Interaction.ActionGiveScore:
                    return new GiveScore(item, _room);

                case Interaction.ActionMuteUser:
                    return new MuteUser(item, _room);

                case Interaction.ActionJoinTeam:
                    return new JoinTeam(item, _room);

                case Interaction.ActionLeaveTeam:
                    return new LeaveTeam(item, _room);

                case Interaction.ActionChase:
                    return new Chase(item, _room);

                case Interaction.ActionCallStacks:
                    return new CallStacks(item, _room);

                // Bots
                case Interaction.TriggerBotReachedStuff:
                    return new BotReachedStuff(item, _room);

                case Interaction.TriggerBotReachedAvatar:
                    return new BotReachedAvatar(item, _room);

                case Interaction.ActionBotClothes:
                    return new BotClothes(item, _room);

                case Interaction.ActionBotFollowAvatar:
                    return new BotFollowAvatar(item, _room);

                case Interaction.ActionBotGiveHanditem:
                    return new BotGiveHanditem(item, _room);

                case Interaction.ActionBotMove:
                    return new BotMove(item, _room);

                case Interaction.ActionBotTalk:
                    return new BotTalk(item, _room);

                case Interaction.ActionBotTalkToAvatar:
                    return new BotTalkToAvatar(item, _room);

                case Interaction.ActionBotTeleport:
                    return new BotTeleport(item, _room);
            }
            return null;
        }

        public List<IWiredItem> GetConditions(IWiredItem item)
        {
            return
                _wiredItems.Where(
                    current =>
                        current != null && IsCondition(current.Type) && current.Item.X == item.Item.X &&
                        current.Item.Y == item.Item.Y)
                    .ToList();
        }

        public List<IWiredItem> GetEffects(IWiredItem item)
        {
            return
                _wiredItems.Where(
                    current =>
                        current != null && IsEffect(current.Type) && current.Item.X == item.Item.X &&
                        current.Item.Y == item.Item.Y)
                    .ToList();
        }

        public IWiredItem GetWired(RoomItem item)
        {
            return _wiredItems.FirstOrDefault(current => current != null && item.Id == current.Item.Id);
        }

        public List<IWiredItem> GetWiredsByType(Interaction type)
        {
            return _wiredItems.Where(item => item != null && item.Type == type).ToList();
        }

        public List<IWiredItem> GetWiredsByTypes(GlobalInteractions type)
        {
            return
                _wiredItems.Where(
                    item => item != null && InteractionTypes.AreFamiliar(type, item.Item.GetBaseItem().InteractionType))
                    .ToList();
        }

        public void MoveWired(RoomItem item)
        {
            var wired = GetWired(item);
            if (wired == null) return;
            wired.Item = item;
            RemoveWired(item);
            AddWired(wired);
        }

        public void Destroy()
        {
            _wiredItems.Clear();
            _cycleItems.Clear();
        }

        private static bool IsTrigger(Interaction type)
        {
            return InteractionTypes.AreFamiliar(GlobalInteractions.WiredTrigger, type);
        }

        private static bool IsEffect(Interaction type)
        {
            return InteractionTypes.AreFamiliar(GlobalInteractions.WiredEffect, type);
        }

        private static bool IsCondition(Interaction type)
        {
            return InteractionTypes.AreFamiliar(GlobalInteractions.WiredCondition, type);
        }
    }
}