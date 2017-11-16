/*
    Copyright (c) 2017 Inkton.

    Permission is hereby granted, free of charge, to any person obtaining
    a copy of this software and associated documentation files (the "Software"),
    to deal in the Software without restriction, including without limitation
    the rights to use, copy, modify, merge, publish, distribute, sublicense,
    and/or sell copies of the Software, and to permit persons to whom the Software
    is furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in
    all copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
    EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
    OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
    IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
    CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
    TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE
    OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;

namespace Nester.Admin
{
    public class Log : Cloud.ManagedEntity
    {
        private string _id;
        private static readonly DateTime _epoch = 
            new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        protected bool _epochTimeFromMicroseconds = false;
        public Dictionary<string, object> _fields = 
            new Dictionary<string, object>();

        public Log() 
            : base("log")
        {
        }

        [JsonProperty("id")]
        public string Id
        {
            get { return _id; }
            set { _id = value; }
        }

        public DateTime Time
        {
            get {
                long epocTime = long.Parse(_id);
                if (_epochTimeFromMicroseconds)
                {
                    return _epoch.AddSeconds(epocTime / 1000);
                }
                else
                {
                    return _epoch.AddSeconds(epocTime);
                }
            }
        }

        public Dictionary<string, object> Fields
        {
            get { return _fields; }
            set { SetProperty(ref _fields, value); }
        }

        protected override bool SetProperty<T>(ref T storage, T value,
            [CallerMemberName] string propertyName = null)
        {
            Fields[propertyName] = value;
            return base.SetProperty<T>(ref storage, value, propertyName);
        }
    }

    public class NestLog : Log
    {
        private string _eventId;
        private string _level;
        private string _name;
        private string _nestTag;
        private string _cushionId;
        private string _message;

        public NestLog()
        {
            _epochTimeFromMicroseconds = true;
        }

        public string Source
        {
            get { return _nestTag + "." + _cushionId; }
        }

        [JsonProperty("event_id")]
        public string EventId
        {
            get { return _eventId; }
            set { SetProperty(ref _eventId, value); }
        }

        [JsonProperty("level")]
        public string Level
        {
            get { return _level; }
            set { SetProperty(ref _level, value); }
        }

        [JsonProperty("name")]
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        [JsonProperty("nest")]
        public string NestTag
        {
            get { return _nestTag; }
            set
            {
                SetProperty(ref _nestTag, value);
            }
        }

        [JsonProperty("cushion")]
        public string CushionId
        {
            get { return _cushionId; }
            set { _cushionId = value; }
        }

        [JsonProperty("message")]
        public string Message
        {
            get { return _message; }
            set
            {
                SetProperty(ref _message, value);
            }
        }
    }

    public class SystemCPULog : Log
    {
        private double _guestNice;
        private double _guest;
        private double _steal;
        private double _softirq;
        private double _irq;
        private double _user;
        private double _system;
        private double _nice;
        private double _iowait;

        public SystemCPULog()
        {
            _epochTimeFromMicroseconds = false;
        }

        [JsonProperty("guest_nice")]  
        public double GuestNice
        {
            get { return _guestNice; }
            set {
                SetProperty(ref _guestNice, value);
            }
        }

        [JsonProperty("guest")]
        public double Guest
        {
            get { return _guest; }
            set { SetProperty(ref _guest, value); }
        }

        [JsonProperty("steal")]
        public double Steal
        {
            get { return _steal; }
            set { SetProperty(ref _steal, value); }
        }

        [JsonProperty("softirq")]
        public double SoftIRQ
        {
            get { return _softirq; }
            set { SetProperty(ref _softirq, value); }
        }

        [JsonProperty("irq")]
        public double IRQ
        {
            get { return _irq; }
            set { SetProperty(ref _irq, value); }
        }

        [JsonProperty("user")]
        public double User
        {
            get { return _user; }
            set { SetProperty(ref _user, value); }
        }

        [JsonProperty("system")]
        public double System
        {
            get { return _system; }
            set { SetProperty(ref _system, value); }
        }

        [JsonProperty("nice")]
        public double Nice
        {
            get { return _nice; }
            set { SetProperty(ref _nice, value); }
        }

        [JsonProperty("iowait")]
        public double IOWait
        {
            get { return _iowait; }
            set { SetProperty(ref _iowait, value); }
        }
    }

    public class DiskSpaceLog : Log
    {
        private double _avail;
        private double _used;
        private double _reservedForRoot;

        public DiskSpaceLog()
        {
            _epochTimeFromMicroseconds = false;
        }

        [JsonProperty("avail")]
        public double Available
        {
            get { return _avail; }
            set { SetProperty(ref _avail, value); }
        }

        [JsonProperty("used")]
        public double Used
        {
            get { return _used; }
            set { SetProperty(ref _used, value); }
        }

        [JsonProperty("reserved_for_root")]
        public double ReservedForRoot
        {
            get { return _reservedForRoot; }
            set { SetProperty(ref _reservedForRoot, value); }
        }
    }

    public class SystemIPV4Log : Log
    {
        private double _received;
        private double _sent;

        public SystemIPV4Log()
        {
            _epochTimeFromMicroseconds = false;
        }

        [JsonProperty("sent")]
        public double Sent
        {
            get { return _sent; }
            set { SetProperty(ref _sent, value); }
        }

        [JsonProperty("received")]
        public double Received
        {
            get { return _received; }
            set { SetProperty(ref _received, value); }
        }
    }

    public class SystemRAMLog : Log
    {
        private double _free;
        private double _used;
        private double _cached;
        private double _buffers;

        public SystemRAMLog()
        {
            _epochTimeFromMicroseconds = false;
        }

        [JsonProperty("free")]
        public double Free
        {
            get { return _free; }
            set { SetProperty(ref _free, value); }
        }

        [JsonProperty("used")]
        public double Used
        {
            get { return _used; }
            set { SetProperty(ref _used, value); }
        }

        [JsonProperty("cached")]
        public double Cached
        {
            get { return _cached; }
            set { SetProperty(ref _cached, value); }
        }

        [JsonProperty("buffers")]
        public double Buffers
        {
            get { return _buffers; }
            set { SetProperty(ref _buffers, value); }
        }
    }
}