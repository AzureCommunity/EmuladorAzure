﻿namespace Mercury.Connection.Connection
{
    public class DoorOpened
    {
        public DoorOpened(string id)
        {
            Id = id;
        }

        public string Id { get; set; }
    }
}