using System;
using Azure.HabboHotel.Items;
using System.Collections.Generic;
using System.Linq;

namespace Azure.HabboHotel.Rooms.Wired.Handlers.Conditions
{
    internal class FurniHasFurni : IWiredItem
    {
        public FurniHasFurni(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
            Items = new List<RoomItem>();
        }

        public Interaction Type
        {
            get { return Interaction.ConditionFurniHasFurni; }
        }

        public RoomItem Item { get; set; }

        public Room Room { get; set; }

        public List<RoomItem> Items { get; set; }

        public string OtherString
        {
            get { return string.Empty; }
            set { }
        }

        public string OtherExtraString
        {
            get { return string.Empty; }
            set { }
        }

        public string OtherExtraString2
        {
            get { return string.Empty; }
            set { }
        }

        public bool OtherBool { get; set; }

        public int Delay
        {
            get { return 0; }
            set { }
        }

        public bool Execute(params object[] stuff)
        {
            if (!Items.Any())
                return true;

            return OtherBool ? AllItemsHaveFurni() : AnyItemHaveFurni();
        }

        public bool AllItemsHaveFurni()
        {
            foreach (var current in Items.Where(item => item != null && Room.GetRoomItemHandler().FloorItems.ContainsKey(item.Id)))
            {
                if (
                    current.AffectedTiles.Values.Where(
                        square => Room.GetGameMap().SquareHasFurni(square.X, square.Y)).Any(
                            square =>
                                !Room.GetGameMap()
                                    .GetRoomItemForSquare(square.X, square.Y)
                                    .Any(squareItem => squareItem.Id != current.Id && squareItem.Z + squareItem.Height >= current.Z + current.Height)))

                    return false;
            }

            return true;
        }

        public bool AnyItemHaveFurni()
        {
            foreach (var current in Items.Where(item => item != null && Room.GetRoomItemHandler().FloorItems.ContainsKey(item.Id)))
            {
                if (
                    current.AffectedTiles.Values.Where(
                        square => Room.GetGameMap().SquareHasFurni(square.X, square.Y))
                        .Any(
                            square =>
                                Room.GetGameMap()
                                    .GetRoomItemForSquare(square.X, square.Y)
                                    .Any(squareItem => squareItem.Id != current.Id && squareItem.Z + squareItem.Height >= current.Z + current.Height)))
                 
                    return true;
            }

            return false;
        }
    }
}