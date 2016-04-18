using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Collector
{
    public struct structViewStateInfo
    {
        public string id;
        public DateTime dateCreated;
        public DateTime dateModified;
    }

    public class ViewState
    {
        [IgnoreDataMember]
        private Core S;
        public Page Page; //only store the page class within viewstate

        public void Load(Core CollectorCore)
        {
            S = CollectorCore;
            Page = S.Page;
        }
    }

    public class ViewStates
    {
        public List<structViewStateInfo> Views = new List<structViewStateInfo>();
    }
}