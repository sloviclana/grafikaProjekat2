﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PZ2Grafika.Model
{
    public class PowerEntity
    {
        private long id;
        private string name;
        private double x;
        private double y;

        // dodato zbog zadatka
        public double Longitude { get; set; }
        public double Latitude { get; set; }

        public PowerEntity()
        {

        }

        public long Id
        {
            get
            {
                return id;
            }

            set
            {
                id = value;
            }
        }

        public string Name
        {
            get
            {
                return name;
            }

            set
            {
                name = value;
            }
        }

        public double X
        {
            get
            {
                return x;
            }

            set
            {
                x = value;
            }
        }

        public double Y
        {
            get
            {
                return y;
            }

            set
            {
                y = value;
            }
        }
    }
}
