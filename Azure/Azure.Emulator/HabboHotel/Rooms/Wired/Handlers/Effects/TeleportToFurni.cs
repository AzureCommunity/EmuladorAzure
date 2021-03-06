using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Azure.HabboHotel.Items;

namespace Azure.HabboHotel.Rooms.Wired.Handlers.Effects
{
    internal class TeleportToFurni : IWiredItem, IWiredCycler
    {
        private readonly List<Interaction> _mBanned;

        private long _mNext;

        public TeleportToFurni(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
            ToWorkConcurrentQueue = new ConcurrentQueue<RoomUser>();
            Items = new List<RoomItem>();
            Delay = 0;
            _mNext = 0L;
            _mBanned = new List<Interaction>
            {
                Interaction.TriggerRepeater,
                Interaction.TriggerLongRepeater
            };
        }

        public Interaction Type
        {
            get { return Interaction.ActionTeleportTo; }
        }

        public RoomItem Item { get; set; }

        public Room Room { get; set; }

        public List<RoomItem> Items { get; set; }

        public string OtherString
        {
            get { return ""; }
            set { }
        }

        public string OtherExtraString
        {
            get { return ""; }
            set { }
        }

        public string OtherExtraString2
        {
            get { return ""; }
            set { }
        }

        public bool OtherBool
        {
            get { return true; }
            set { }
        }

        public int Delay { get; set; }

        public Queue ToWork { get; set; }

        public ConcurrentQueue<RoomUser> ToWorkConcurrentQueue { get; set; }

        public bool Execute(params object[] stuff)
        {
            if (stuff[0] == null) return false;
            var roomUser = (RoomUser) stuff[0];
            var item = (Interaction) stuff[1];

            if (_mBanned.Contains(item)) return false;
            if (!Items.Any()) return false;

            if (!ToWorkConcurrentQueue.Contains(roomUser)) ToWorkConcurrentQueue.Enqueue(roomUser);
            if (Delay < 500) Delay = 500;

            if (Room.GetWiredHandler().IsCycleQueued(this)) return false;

            if (_mNext == 0L || _mNext < Azure.Now()) _mNext = (Azure.Now() + (Delay));

            Room.GetWiredHandler().EnqueueCycle(this);
            return true;
        }

        public bool OnCycle()
        {
            if (!ToWorkConcurrentQueue.Any()) return true;
            if (Room == null || Room.GetRoomItemHandler() == null || Room.GetRoomItemHandler().FloorItems == null) return false;

            var num = Azure.Now();
            var toAdd = new List<RoomUser>();
            RoomUser roomUser;
            while (ToWorkConcurrentQueue.TryDequeue(out roomUser))
            {
                if (roomUser == null || roomUser.GetClient() == null) continue;
                if (_mNext <= num)
                {
                    if (Teleport(roomUser)) continue;
                    return false;
                }
                if (_mNext - num < 500L && roomUser.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent() != null)
                {
                    roomUser.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().ActivateCustomEffect(4);
                }

                toAdd.Add(roomUser);
            }

            foreach (var roomUserToAdd in toAdd.Where(roomUserToAdd => !ToWorkConcurrentQueue.Contains(roomUserToAdd)))
            {
                ToWorkConcurrentQueue.Enqueue(roomUserToAdd);
            }

            toAdd.Clear();
            toAdd = null;

            if (_mNext >= num) return false;
            _mNext = 0L;
            return true;
        }

        private bool Teleport(RoomUser user)
        {
            if (!Items.Any()) return true;
            if (user == null || user.GetClient() == null || user.GetClient().GetHabbo() == null) return true;
            var rnd = new Random();
            Items = (
                from x in Items
                orderby rnd.Next()
                select x).ToList<RoomItem>();
            RoomItem roomItem = null;
            foreach (
                var current in
                    Items.Where(
                        current => current != null && Room.GetRoomItemHandler().FloorItems.ContainsKey(current.Id))) roomItem = current;
            if (roomItem == null)
            {
                user.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().ActivateCustomEffect(0);
                return false;
            }
            Room.GetGameMap().TeleportToItem(user, roomItem);
            Room.GetRoomUserManager().OnUserUpdateStatus();
            user.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().ActivateCustomEffect(0);
            return true;
        }
    }
}