using Azure.HabboHotel.Rooms;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Azure.HabboHotel.Events
{
    /// <summary>
    /// Class EventManager.
    /// </summary>
    internal class EventManager
    {
        /// <summary>
        /// The _events
        /// </summary>
        private readonly Dictionary<RoomData, uint> _events;

        /// <summary>
        /// The _add queue
        /// </summary>
        private readonly Queue _addQueue;

        /// <summary>
        /// The _remove queue
        /// </summary>
        private readonly Queue _removeQueue;

        /// <summary>
        /// The _update queue
        /// </summary>
        private readonly Queue _updateQueue;

        /// <summary>
        /// The _event categories
        /// </summary>
        private readonly Dictionary<int, EventCategory> _eventCategories;

        /// <summary>
        /// The _ordered event rooms
        /// </summary>
        private IOrderedEnumerable<KeyValuePair<RoomData, uint>> _orderedEventRooms;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventManager"/> class.
        /// </summary>
        public EventManager()
        {
            this._eventCategories = new Dictionary<int, EventCategory>();
            this._events = new Dictionary<RoomData, uint>();
            this._orderedEventRooms =
                                     from t in this._events
                                     orderby t.Value descending
                                     select t;
            this._addQueue = new Queue();
            this._removeQueue = new Queue();
            this._updateQueue = new Queue();

            {
                for (int i = 0; i < 30; i++)
                    this._eventCategories.Add(i, new EventCategory(i));
            }
        }

        /// <summary>
        /// Gets the rooms.
        /// </summary>
        /// <returns>KeyValuePair&lt;RoomData, System.UInt32&gt;[].</returns>
        internal KeyValuePair<RoomData, uint>[] GetRooms()
        {
            return this._orderedEventRooms.ToArray();
        }

        /// <summary>
        /// Called when [cycle].
        /// </summary>
        internal void OnCycle()
        {
            this.WorkRemoveQueue();
            this.WorkAddQueue();
            this.WorkUpdate();
            this.SortCollection();
            foreach (EventCategory current in this._eventCategories.Values)
                current.OnCycle();
        }

        /// <summary>
        /// Queues the add event.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="roomEventCategory">The room event category.</param>
        internal void QueueAddEvent(RoomData data, int roomEventCategory)
        {
            lock (this._addQueue.SyncRoot)
                this._addQueue.Enqueue(data);
            this._eventCategories[roomEventCategory].QueueAddEvent(data);
        }

        /// <summary>
        /// Queues the remove event.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="roomEventCategory">The room event category.</param>
        internal void QueueRemoveEvent(RoomData data, int roomEventCategory)
        {
            lock (this._removeQueue.SyncRoot)
                this._removeQueue.Enqueue(data);
            this._eventCategories[roomEventCategory].QueueRemoveEvent(data);
        }

        /// <summary>
        /// Queues the update event.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="roomEventCategory">The room event category.</param>
        internal void QueueUpdateEvent(RoomData data, int roomEventCategory)
        {
            lock (this._updateQueue.SyncRoot)
                this._updateQueue.Enqueue(data);
            this._eventCategories[roomEventCategory].QueueUpdateEvent(data);
        }

        /// <summary>
        /// Sorts the collection.
        /// </summary>
        private void SortCollection()
        {
            this._orderedEventRooms =
                                     from t in this._events.Take(40)
                                     orderby t.Value descending
                                     select t;
        }

        /// <summary>
        /// Works the add queue.
        /// </summary>
        private void WorkAddQueue()
        {
            if (this._addQueue.Count <= 0)
                return;
            lock (this._addQueue.SyncRoot)
            {
                while (this._addQueue.Count > 0)
                {
                    var roomData = (RoomData)this._addQueue.Dequeue();
                    if (!this._events.ContainsKey(roomData))
                        this._events.Add(roomData, roomData.UsersNow);
                }
            }
        }

        /// <summary>
        /// Works the remove queue.
        /// </summary>
        private void WorkRemoveQueue()
        {
            if (this._removeQueue.Count <= 0)
                return;
            lock (this._removeQueue.SyncRoot)
            {
                while (this._removeQueue.Count > 0)
                {
                    var key = (RoomData)this._removeQueue.Dequeue();
                    this._events.Remove(key);
                }
            }
        }

        /// <summary>
        /// Works the update.
        /// </summary>
        private void WorkUpdate()
        {
            if (this._removeQueue.Count <= 0)
                return;
            lock (this._removeQueue.SyncRoot)
                while (this._removeQueue.Count > 0)
                {
                    var roomData = (RoomData)this._updateQueue.Dequeue();
                    if (this._events.ContainsKey(roomData))
                        this._events[roomData] = roomData.UsersNow;
                }
        }
    }
}