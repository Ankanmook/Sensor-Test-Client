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
    
    public partial class DQIndicator
    {
        public string id { get; set; }
        public System.DateTimeOffset C__createdAt { get; set; }
        public Nullable<System.DateTimeOffset> C__updatedAt { get; set; }
        public byte[] C__version { get; set; }
        public bool C__deleted { get; set; }
        public Nullable<double> gravity { get; set; }
        public Nullable<double> gravity_e { get; set; }
        public Nullable<double> pressure_e { get; set; }
        public Nullable<double> accel_gravity_e { get; set; }
        public Nullable<double> temp_e { get; set; }
        public Nullable<double> azimuth_fusion { get; set; }
        public Nullable<double> pitch_fusion { get; set; }
        public Nullable<double> roll_fusion { get; set; }
        public Nullable<double> azimuth_e_gyro { get; set; }
        public Nullable<double> pitch_e_gyro { get; set; }
        public Nullable<double> roll_e_gyro { get; set; }
        public string deviceid { get; set; }
        public string deviceinfo { get; set; }
        public Nullable<double> percentile_g { get; set; }
        public Nullable<double> percentile_accg { get; set; }
        public Nullable<double> percentile_p { get; set; }
        public Nullable<double> percentile_t { get; set; }
        public Nullable<double> percentile_o { get; set; }
        public Nullable<double> score { get; set; }
        public Nullable<double> cumulative_percentile { get; set; }
        public Nullable<double> score_g { get; set; }
        public Nullable<double> score_ga { get; set; }
        public Nullable<double> score_p { get; set; }
        public Nullable<double> score_t { get; set; }
        public Nullable<double> score_f { get; set; }
    }
}
