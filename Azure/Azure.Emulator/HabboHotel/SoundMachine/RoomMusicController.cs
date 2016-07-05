using Azure.HabboHotel.Items;
using Azure.HabboHotel.Rooms;
using Azure.HabboHotel.SoundMachine.Composers;
using System.Collections.Generic;
using System.Linq;

namespace Azure.HabboHotel.SoundMachine
{
    /// <summary>
    /// Class RoomMusicController.
    /// </summary>
    internal class RoomMusicController
    {
        /// <summary>
        /// The _m broadcast needed
        /// </summary>
        private static bool _mBroadcastNeeded;

        /// <summary>
        /// The _m loaded disks
        /// </summary>
        private Dictionary<uint, SongItem> _mLoadedDisks;

        /// <summary>
        /// The _m playlist
        /// </summary>
        private SortedDictionary<int, SongInstance> _mPlaylist;

        /// <summary>
        /// The _m started playing timestamp
        /// </summary>
        private double _mStartedPlayingTimestamp;

        /// <summary>
        /// The _m room output item
        /// </summary>
        private RoomItem _mRoomOutputItem;

        /// <summary>
        /// Initializes a new instance of the <see cref="RoomMusicController"/> class.
        /// </summary>
        public RoomMusicController()
        {
            this._mLoadedDisks = new Dictionary<uint, SongItem>();
            this._mPlaylist = new SortedDictionary<int, SongInstance>();
        }

        /// <summary>
        /// Gets the current song.
        /// </summary>
        /// <value>The current song.</value>
        public SongInstance CurrentSong { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is playing.
        /// </summary>
        /// <value><c>true</c> if this instance is playing; otherwise, <c>false</c>.</value>
        public bool IsPlaying { get; private set; }

        /// <summary>
        /// Gets the time playing.
        /// </summary>
        /// <value>The time playing.</value>
        public double TimePlaying
        {
            get
            {
                return Azure.GetUnixTimeStamp() - this._mStartedPlayingTimestamp;
            }
        }

        /// <summary>
        /// Gets the song synchronize timestamp.
        /// </summary>
        /// <value>The song synchronize timestamp.</value>
        public int SongSyncTimestamp
        {
            get
            {
                if (!this.IsPlaying || this.CurrentSong == null)
                {
                    return 0;
                }

                {
                    if (this.TimePlaying >= this.CurrentSong.SongData.LengthSeconds)
                    {
                        return (int)this.CurrentSong.SongData.LengthSeconds;
                    }
                    return (int)(this.TimePlaying * 1000.0);
                }
            }
        }

        /// <summary>
        /// Gets the playlist.
        /// </summary>
        /// <value>The playlist.</value>
        public SortedDictionary<int, SongInstance> Playlist
        {
            get
            {
                var sortedDictionary = new SortedDictionary<int, SongInstance>();
                lock (this._mPlaylist)
                {
                    foreach (KeyValuePair<int, SongInstance> current in this._mPlaylist)
                    {
                        sortedDictionary.Add(current.Key, current.Value);
                    }
                }
                return sortedDictionary;
            }
        }

        /// <summary>
        /// Gets the playlist capacity.
        /// </summary>
        /// <value>The playlist capacity.</value>
        public int PlaylistCapacity
        {
            get
            {
                return 20;
            }
        }

        /// <summary>
        /// Gets the size of the playlist.
        /// </summary>
        /// <value>The size of the playlist.</value>
        public int PlaylistSize
        {
            get
            {
                return this._mPlaylist.Count;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has linked item.
        /// </summary>
        /// <value><c>true</c> if this instance has linked item; otherwise, <c>false</c>.</value>
        public bool HasLinkedItem
        {
            get
            {
                return this._mRoomOutputItem != null;
            }
        }

        /// <summary>
        /// Gets the linked item identifier.
        /// </summary>
        /// <value>The linked item identifier.</value>
        public uint LinkedItemId
        {
            get
            {
                return this._mRoomOutputItem == null ? 0u : this._mRoomOutputItem.Id;
            }
        }

        /// <summary>
        /// Gets the song queue position.
        /// </summary>
        /// <value>The song queue position.</value>
        public int SongQueuePosition { get; private set; }

        /// <summary>
        /// Links the room output item.
        /// </summary>
        /// <param name="item">The item.</param>
        public void LinkRoomOutputItem(RoomItem item)
        {
            this._mRoomOutputItem = item;
        }

        /// <summary>
        /// Adds the disk.
        /// </summary>
        /// <param name="diskItem">The disk item.</param>
        /// <returns>System.Int32.</returns>
        public int AddDisk(SongItem diskItem)
        {
            uint songId = diskItem.SongId;
            if (songId == 0u)
            {
                return -1;
            }
            SongData song = SongManager.GetSong(songId);
            if (song == null)
            {
                return -1;
            }
            if (this._mLoadedDisks.ContainsKey(diskItem.ItemId))
            {
                return -1;
            }
            this._mLoadedDisks.Add(diskItem.ItemId, diskItem);
            int count = this._mPlaylist.Count;
            lock (this._mPlaylist)
            {
                this._mPlaylist.Add(count, new SongInstance(diskItem, song));
            }
            return count;
        }

        /// <summary>
        /// Removes the disk.
        /// </summary>
        /// <param name="playlistIndex">Index of the playlist.</param>
        /// <returns>SongItem.</returns>
        public SongItem RemoveDisk(int playlistIndex)
        {
            SongInstance songInstance;
            lock (this._mPlaylist)
            {
                if (!this._mPlaylist.ContainsKey(playlistIndex))
                {
                    return null;
                }
                songInstance = this._mPlaylist[playlistIndex];
                this._mPlaylist.Remove(playlistIndex);
            }
            lock (this._mLoadedDisks)
            {
                this._mLoadedDisks.Remove(songInstance.DiskItem.ItemId);
            }
            this.RepairPlaylist();
            if (playlistIndex == this.SongQueuePosition)
            {
                this.PlaySong();
            }
            return songInstance.DiskItem;
        }

        /// <summary>
        /// Updates the specified instance.
        /// </summary>
        /// <param name="instance">The instance.</param>
        public void Update(Room instance)
        {
            if (this.IsPlaying && (this.CurrentSong == null || this.TimePlaying >= this.CurrentSong.SongData.LengthSeconds + 1.0))
            {
                if (!this._mPlaylist.Any())
                {
                    this.Stop();
                    this._mRoomOutputItem.ExtraData = "0";
                    this._mRoomOutputItem.UpdateState();
                }
                else
                {
                    this.SetNextSong();
                }
                _mBroadcastNeeded = true;
            }
            if (!_mBroadcastNeeded)
                return;
            this.BroadcastCurrentSongData(instance);
            _mBroadcastNeeded = false;
        }

        /// <summary>
        /// Repairs the playlist.
        /// </summary>
        public void RepairPlaylist()
        {
            List<SongItem> list;
            lock (this._mLoadedDisks)
            {
                list = this._mLoadedDisks.Values.ToList();
                this._mLoadedDisks.Clear();
            }
            lock (this._mPlaylist)
            {
                this._mPlaylist.Clear();
            }
            foreach (SongItem current in list)
            {
                this.AddDisk(current);
            }
        }

        /// <summary>
        /// Sets the next song.
        /// </summary>
        public void SetNextSong()
        {
            {
                this.SongQueuePosition++;
                this.PlaySong();
            }
        }

        /// <summary>
        /// Plays the song.
        /// </summary>
        public void PlaySong()
        {
            if (this.SongQueuePosition >= this._mPlaylist.Count)
            {
                this.SongQueuePosition = 0;
            }
            if (!this._mPlaylist.Any())
            {
                this.Stop();
                return;
            }
            this.CurrentSong = this._mPlaylist[this.SongQueuePosition];
            this._mStartedPlayingTimestamp = Azure.GetUnixTimeStamp();
            _mBroadcastNeeded = true;
        }

        /// <summary>
        /// Starts this instance.
        /// </summary>
        public void Start()
        {
            this.IsPlaying = true;
            this.SongQueuePosition = -1;
            this.SetNextSong();
        }

        /// <summary>
        /// Stops this instance.
        /// </summary>
        public void Stop()
        {
            this.CurrentSong = null;
            this.IsPlaying = false;
            this.SongQueuePosition = -1;
            _mBroadcastNeeded = true;
        }

        /// <summary>
        /// Resets this instance.
        /// </summary>
        public void Reset()
        {
            lock (this._mLoadedDisks)
            {
                this._mLoadedDisks.Clear();
            }
            lock (this._mPlaylist)
            {
                this._mPlaylist.Clear();
            }
            this._mRoomOutputItem = null;
            this.SongQueuePosition = -1;
            this._mStartedPlayingTimestamp = 0.0;
        }

        /// <summary>
        /// Broadcasts the current song data.
        /// </summary>
        /// <param name="instance">The instance.</param>
        internal void BroadcastCurrentSongData(Room instance)
        {
            if (this.CurrentSong != null)
            {
                instance.SendMessage(JukeboxComposer.ComposePlayingComposer(this.CurrentSong.SongData.Id, this.SongQueuePosition, 0));
                return;
            }
            instance.SendMessage(JukeboxComposer.ComposePlayingComposer(0u, 0, 0));
        }

        /// <summary>
        /// Called when [new user enter].
        /// </summary>
        /// <param name="user">The user.</param>
        internal void OnNewUserEnter(RoomUser user)
        {
            if (user.IsBot || user.GetClient() == null || this.CurrentSong == null)
            {
                return;
            }
            user.GetClient()
                .SendMessage(JukeboxComposer.ComposePlayingComposer(this.CurrentSong.SongData.Id, this.SongQueuePosition,
                    this.SongSyncTimestamp));
        }

        /// <summary>
        /// Destroys this instance.
        /// </summary>
        internal void Destroy()
        {
            if (this._mLoadedDisks != null)
            {
                this._mLoadedDisks.Clear();
            }
            if (this._mPlaylist != null)
            {
                this._mPlaylist.Clear();
            }
            this._mPlaylist = null;
            this._mLoadedDisks = null;
            this.CurrentSong = null;
            this._mRoomOutputItem = null;
        }
    }
}