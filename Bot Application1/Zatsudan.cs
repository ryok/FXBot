using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bot_Application1
{
    public class SpeakTo
    {
        public string utt { get; set; }
        public string context { get; set; }
        public string nickname { get; set; }
        public string nickname_y { get; set; }
        public string sex { get; set; }
        public string bloodtype { get; set; }
        public string birthdateY { get; set; }
        public string birthdateM { get; set; }
        public string birthdateD { get; set; }
        public string age { get; set; }
        public string constellations { get; set; }
        public string place { get; set; }
        public string mode { get; set; }
        public string t { get; set; }

    }

    public class Reply
    {
        public string utt { get; set; }
        public string yomi { get; set; }
        public string mode { get; set; }
        public string da { get; set; }
        public string context { get; set; }

    }
}