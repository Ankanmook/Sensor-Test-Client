//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DQEngineFinal
{
    using System;
    using System.Collections.Generic;
    
    public partial class DeviceData
    {
        public string id { get; set; }
        public System.DateTimeOffset C__createdAt { get; set; }
        public Nullable<System.DateTimeOffset> C__updatedAt { get; set; }
        public byte[] C__version { get; set; }
        public bool C__deleted { get; set; }
        public string deviceid { get; set; }
        public string deviceinfo { get; set; }
        public Nullable<double> longitude { get; set; }
        public Nullable<double> latitutde { get; set; }
        public Nullable<double> altitude { get; set; }
        public Nullable<double> gravity_cumulative { get; set; }
        public Nullable<double> gravity_x { get; set; }
        public Nullable<double> gravity_y { get; set; }
        public Nullable<double> gravity_z { get; set; }
        public Nullable<double> error_gravity { get; set; }
        public Nullable<double> pressure_device { get; set; }
        public Nullable<double> pressure_service { get; set; }
        public Nullable<double> error_pressure { get; set; }
        public Nullable<double> accelerometer_x { get; set; }
        public Nullable<double> accelerometer_y { get; set; }
        public Nullable<double> accelerometer_z { get; set; }
        public Nullable<double> magneticfeild_x { get; set; }
        public Nullable<double> magneticfeild_y { get; set; }
        public Nullable<double> magneticfeild_z { get; set; }
        public Nullable<double> gyroscope_x { get; set; }
        public Nullable<double> gyroscope_y { get; set; }
        public Nullable<double> gyroscope_z { get; set; }
        public Nullable<double> height { get; set; }
        public Nullable<double> humidity { get; set; }
        public Nullable<double> temperature { get; set; }
        public Nullable<double> battery_status { get; set; }
        public Nullable<double> battery_level { get; set; }
        public Nullable<double> battery_voltage { get; set; }
        public Nullable<double> battery_temperature { get; set; }
        public string battery_technology { get; set; }
        public Nullable<System.DateTimeOffset> timestamp { get; set; }
        public Nullable<double> azimuth { get; set; }
        public Nullable<double> pitch { get; set; }
        public Nullable<double> roll { get; set; }
        public Nullable<bool> complete { get; set; }
    }
}
