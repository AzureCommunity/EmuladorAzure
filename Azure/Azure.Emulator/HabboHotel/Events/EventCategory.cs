using Azure.HabboHotel.Rooms;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Azure.HabboHotel.Events
{
    /// <summary>
    /// Class EventCategory.
    /// </summary>
    internal class EventCategory
    {
        /// <summary>
        /// The _category identifier
        /// </summary>
        private readonly int _categoryId;

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
        /// The _ordered event rooms
        /// </summary>
        private IOrderedEnumerable<KeyValuePair<RoomData, uint>> _orderedEventRooms;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventCategory"/> class.
        /// </summary>
        /// <param name="categoryId">The category identifier.</param>
        internal EventCategory(int categoryId)
        {
            this._categoryId = categoryId;
            this._events = new Dictionary<RoomData, uint>();
            this._orderedEventRooms = from t in this._events orderby t.Value descending select t;
            this._addQueue = new Queue();
            this._removeQueue = new Queue();
            this._updateQueue = new Queue();
        }

        /// <summary>
        /// Gets the active rooms.
        /// </summary>
        /// <returns>KeyValuePair&lt;RoomData, System.UInt32&gt;[].</returns>
        internal KeyValuePair<RoomData, uint>[] GetActiveRooms()
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
        }

        /// <summary>
        /// Queues the add event.
        /// </summary>
        /// <param name="data">The data.</param>
        internal void QueueAddEvent(RoomData data)
        {
            lock (this._addQueue.SyncRoot)
                this._addQueue.Enqueue(data);
        }

        /// <summary>
        /// Queues the remove event.
        /// </summary>
        /// <param name="data">The data.</param>
        internal void QueueRemoveEvent(RoomData data)
        {
            lock (this._removeQueue.SyncRoot)
                this._removeQueue.Enqueue(data);
        }

        /// <summary>
        /// Queues the update event.
        /// </summary>
        /// <param name="data">The data.</param>
        internal void QueueUpdateEvent(RoomData data)
        {
            lock (this._updateQueue.SyncRoot)
                this._updateQueue.Enqueue(data);
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
            if (this._addQueue == null || this._addQueue.Count <= 0)
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
            if (this._removeQueue == null || this._removeQueue.Count <= 0)
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
            if (this._removeQueue == null || this._removeQueue.Count <= 0)
                return;
            lock (this._removeQueue.SyncRoot)
            {
                while (this._removeQueue.Count > 0)
                {
                    var roomData = (RoomData)this._updateQueue.Dequeue();
                    if (!this._events.ContainsKey(roomData))
                        this._events.Add(roomData, roomData.UsersNow);
                    else
                        this._events[roomData] = roomData.UsersNow;
                }
            }
        }
    }
}