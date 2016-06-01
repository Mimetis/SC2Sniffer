using System;
using System.Net;
using System.ComponentModel;

namespace SC2MiM.Common.Entities
{
    public class Region
    {
        private string name;
        private string regionId;
    

        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                if (value != name)
                {
                    name = value;
                }
            }
        }

        public string RegionId
        {
            get
            {
                return regionId;
            }
            set
            {
                if (value != regionId)
                {
                    regionId = value;
                }
            }
        }
    }
}
