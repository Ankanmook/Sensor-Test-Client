﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Hardware;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Locations;
using Android.Util;
using System.Net;
using RestSharp;
using RestSharp.Deserializers;
using Microsoft.WindowsAzure.MobileServices;
using System.Net.Http;
using System.Threading.Tasks;




namespace SensorTest
{
	[Activity (Label = "SensorStatusActivity")]			
	public class SensorStatusActivity : Activity,ISensorEventListener,ILocationListener
	{
		/*
		 * Declaring Class Vairables 
		 */
	
		//Azure Client
		private MobileServiceClient client; // Mobile Service Client reference		
		private IMobileServiceTable<DeviceData> sensorTable; // Mobile Service Table used to access data     

		// TODO:: Comment out this line to remove the in-memory list
		public List<DeviceData> sensorList = new List<DeviceData>();


		//Progress Bar
		private ProgressBar progressBar;

		//Sensor variables
		private static readonly object _syncLock = new object();
		private SensorManager _sensorManager;
	
		string latlong = null;

		int indexOfPressureServiceRequest ;


		//Location variables
		Location _currentLocation;
		LocationManager _locationManager;
		string _locationProvider;
		static readonly string LogTag = "GetLocation";

		//Text views
		private TextView _deviceIdTextView;
		private TextView _sensorGravityTextView;
		private TextView _sensorBarometerTextView;
		private TextView _sensorGyroscopeTextView;
		private TextView _sensorMagneticFieldTextView;
		private TextView _sensorGPSTextView;
		private TextView _sensorHeightTextView;
		private TextView _sensorHumidityTextView;

		//Some unused vairables, might be used in future releases
		//private TextView _sensorTemeperatureTextView;


		private string device_Id;
		private string device_Info;

		GravitySensorData gsData;
		DeviceData srData;

		/*
		 * Create Method
		 * Fires when the activity is started
		 */
		protected override void OnCreate (Bundle bundle)
		{
			try{
				base.OnCreate (bundle);

				gsData = new GravitySensorData ();
				srData = new DeviceData ();

				// Create ProgressFilter to handle busy state
				var progressHandler = new ProgressHandler ();
				progressHandler.BusyStateChange += (busy) => {
					if (progressBar != null) 
						progressBar.Visibility = busy ? ViewStates.Visible : ViewStates.Gone;
				};



				//Connecting to Azure Client
				try{

					CurrentPlatform.Init ();
					// Create the Mobile Service Client instance, using the provided
					// Mobile Service URL and key
					client = new MobileServiceClient(
						SenTest.Constants.ApplicationURL,
						SenTest.Constants.ApplicationKey,progressHandler );
						
					sensorTable = client.GetTable<DeviceData>();

				}catch (Java.Net.MalformedURLException) 
				{
					CreateAndShowDialog(new Exception ("There was an error creating the Mobile Service. Verify the URL"), "Error");
				} 
				catch (Exception e) 
				{
					CreateAndShowDialog(e, "Error");
				}


				//Getting device id from form 1
				device_Id = Intent.GetStringExtra("device_Id");
				device_Info = Intent.GetStringExtra ("device_Info");

				//Setting inde of pressure service request to the client to 0
				indexOfPressureServiceRequest = 0;

				SetContentView (Resource.Layout.SensorStatus);

				_sensorManager = (SensorManager) GetSystemService(SensorService);
				InitializeLocationManager ();

				// Initialize the progress bar
				progressBar = FindViewById<ProgressBar>(Resource.Id.loadingProgressBar);
				progressBar.Visibility = ViewStates.Gone;

				_deviceIdTextView = FindViewById<TextView>(Resource.Id.textDeviceId);
				_sensorGravityTextView = FindViewById<TextView>(Resource.Id.textGravity);
				_sensorBarometerTextView = FindViewById<TextView>(Resource.Id.textBarometer);
				_sensorGyroscopeTextView = FindViewById<TextView>(Resource.Id.textGyroscope);
				_sensorGPSTextView = FindViewById<TextView>(Resource.Id.textGPS);
				_sensorMagneticFieldTextView = FindViewById<TextView>(Resource.Id.textMagneticField);
				_sensorHeightTextView = FindViewById<TextView>(Resource.Id.textHeight);
				_sensorHumidityTextView = FindViewById<TextView>(Resource.Id.textHumidity);

				_deviceIdTextView.Text = device_Id;
				var cmd_BackSensorStatus = FindViewById<Button> (Resource.Id.cmdBackSensorStatus);
				var cmd_Record = FindViewById<Button> (Resource.Id.cmdRecord);

				//getGeoLocation();

				cmd_BackSensorStatus.Click += (object sender, EventArgs e) => {
				var intent = new Intent(this, typeof(MainActivity));
				
				StartActivity(intent);
				};



				cmd_Record.Click += (object sender, EventArgs e) => {
					//This will record all the information from sensor screen
					recordInfo();
				};
			}
			catch(Exception ex){
				CreateAndShowDialog (ex.Message, "EXCEPTION");
				}
		}

		/*
		 * On resuming the activity class
		 * It registers sensor and geolocation event
		 */
		protected override void OnResume()
		{
			base.OnResume();

			try{
				_locationManager.RequestLocationUpdates(_locationProvider, 0, 0, this);
				Log.Debug(LogTag, "Listening for location updates using " + _locationProvider + ".");
			}
			//To catch exception when location service is disabled
			catch(ArgumentException ex){
				_sensorGPSTextView.Text = "Please put location on";
			}catch(Exception ex){
				//Do nothing
			}

			//Registering all sensor events
			_sensorManager.RegisterListener(this, _sensorManager.GetDefaultSensor(SensorType.Gravity), SensorDelay.Ui);
			_sensorManager.RegisterListener(this, _sensorManager.GetDefaultSensor(SensorType.Accelerometer), SensorDelay.Ui);
			_sensorManager.RegisterListener(this, _sensorManager.GetDefaultSensor(SensorType.Gyroscope), SensorDelay.Ui);
			_sensorManager.RegisterListener(this, _sensorManager.GetDefaultSensor(SensorType.Pressure), SensorDelay.Ui);
			_sensorManager.RegisterListener(this, _sensorManager.GetDefaultSensor(SensorType.Temperature), SensorDelay.Ui);
			_sensorManager.RegisterListener(this, _sensorManager.GetDefaultSensor(SensorType.MagneticField), SensorDelay.Ui);
			_sensorManager.RegisterListener(this, _sensorManager.GetDefaultSensor(SensorType.RelativeHumidity), SensorDelay.Ui);
		}

		/*
		 * For pausing the sesnor listener
		 * It unregisters all the listeners 
		 */
		protected override void OnPause()
		{
			base.OnPause();
			_sensorManager.UnregisterListener(this);

			_locationManager.RemoveUpdates(this);
			Log.Debug(LogTag, "No longer listening for location updates.");
		}

		/*
		 * Records gravity and pressure sensor information
		 */ 
		public void recordInfo(){
			gsData.recount = true;
			gsData.collect = true;
		}
			
		[Java.Interop.Export()]
		public async void sendData(){

		try{

			sensorTable = client.GetTable<DeviceData>();
			
			// Create json request to be sent to azure web service
			var item = new DeviceData() {
				complete= true, deviceid = device_Id, deviceinfo = device_Info, latitutde = _currentLocation.Latitude,
				longitude = _currentLocation.Longitude,altitude = _currentLocation.Altitude, 
				gravity_cumulative =  gsData.getGravity(), gravity_x = gsData.xComponent, gravity_y = gsData.yComponent,
				gravity_z = gsData.zComponent, error_gravity = gsData.error_gravity,
				accelerometer_x = gsData.accelerometer_x , accelerometer_y = gsData.accelerometer_y , accelerometer_z = gsData.accelerometer_z,
					pressure_device = gsData.pressureComponent, pressure_service = gsData.pressure_service , error_pressure = gsData.error_pressure,
				magneticfeild_x = gsData.magneticfeild_x , magneticfeild_y = gsData.magneticfeild_y, magneticfeild_z= gsData.magneticfeild_z,
				gyroscope_x = gsData.gyroscope_x , gyroscope_y = gsData.gyroscope_y, gyroscope_z = gsData.gyroscope_z,
				height = gsData.height,temperature = 0.0f, 
				battery_status = "None" , battery_level = 0.0f , battery_voltage = 0.0f ,battery_temperature = 0.0f ,	battery_technology = "Li-ion" ,
				timestamp = DateTime.Now
			};

				//sensorTable = client.GetTable("DeviceData",sendData);
				// Insert the new item
				await sensorTable.InsertAsync(item); 

				if (item.complete){

					sensorList.Add(item);
					//NotifyDataSetChanged();
					//adapter.Add(item);
				}
			}
			catch (Exception e) 
			{
				CreateAndShowDialog (e.Message, "EXCEPTION");
			}


			// TODO:: Comment out these lines to remove the in-memory list
			//sensorList.Add(item);
			//adapter.Add(item);
			// NOTE:: End of lines to comment out

			//textNewTodo.Text = "";
		}

		/*
		 * Checks valid data to be sent to json file to be added to device info table
		 */ 
		public bool checkValidData(){
			try{
				
				if ( checkDoubleData (_currentLocation.Latitude) && checkDoubleData(_currentLocation.Latitude)
				   && checkDoubleData (_currentLocation.Altitude)
				   && checkDoubleData (gsData.cumulative_gravity) && checkDoubleData (gsData.error_gravity)
				   && checkDoubleData (gsData.xComponent) && checkDoubleData (gsData.yComponent) && checkDoubleData (gsData.zComponent)
				   && checkDoubleData (gsData.pressureComponent) && checkDoubleData (gsData.pressure_service)) {

					return true;
				}else
				{
					return false;
				}
			}catch(Exception ex){
			
				return false;
			}
			
		}

		/*
		 * Checks valid double data
		 */ 
		public bool checkDoubleData(double d) {

			if (d == null || d == double.NaN || d == 0.0f) {
				return false;
			} else {
				return true;
			}
		}

		/*
		 * Gets the geolocation from longiture and latitude for
		 * an altitude of 10. Hitherto unsued will be used for future methods
		 */
		async void getGeoLocation()
		{
			Geocoder geocoder = new Geocoder(this);
			IList<Address> addressList = await geocoder.GetFromLocationAsync(_currentLocation.Latitude, _currentLocation.Longitude, 10);

			Address address = addressList.FirstOrDefault();
			if (address != null)
			{
				StringBuilder deviceAddress = new StringBuilder();
				for (int i = 0; i < address.MaxAddressLineIndex; i++)
				{
					deviceAddress.Append(address.GetAddressLine(i))
						.AppendLine(",");
				}
				_sensorGPSTextView.Text = deviceAddress.ToString();
			}
			else
			{
				_sensorGPSTextView.Text = "Unable to determine the address.";
			}
		}

		/*
		 * This one fires up during accuracy change event of sensors
		 */
		public void OnAccuracyChanged(Sensor sensor, SensorStatus accuracy)
		{
			// We don't want to do anything here.
		}


		/*
		 * Rest client to get the weather api difference from the openweather api
		 */ 
		public void calculateBarometerDataFromWeatherAPI(){

			//to ensure the rquest is sent only once
			if (indexOfPressureServiceRequest > 1) {
			
				//do nothing
			
			} else {

				try{

					if (latlong != null || latlong != string.Empty || latlong != "") {

						var client = new RestClient("http://api.openweathermap.org/data/2.5/weather");
						var request = new RestRequest ("",Method.GET);
						request.AddParameter("lat", _currentLocation.Latitude);
						request.AddParameter("lon", _currentLocation.Longitude); 

						var response = client.Execute(request);

						RestSharp.Deserializers.JsonDeserializer deserial= new JsonDeserializer();
						var JSONObj = deserial.Deserialize<Dictionary<string,string>>(response); 

						string main = JSONObj["main"] ;

						// this how main looks like: {"temp":293.61,"pressure":1019,"humidity":17,"temp_min":291.15,"temp_max":295.15}
						string[] splitSrt = main.Split(',');
						string[] pStr = splitSrt[1].Split(':');	

						gsData.pressure_service = Convert.ToDouble( pStr[1] );
					}
				}catch(Exception ex){
					gsData.pressure_service = double.NaN;
					//calculateBarometerDataFromWeatherAPI ();
				}
					
			}

			++indexOfPressureServiceRequest;
		}

		public void getTimeStamp(){
		
			DateTime dt = DateTime.Now;
		}

		/*
		 * Records sensor change event and stores the data along with 
		 * displaying it in the ui
		 */
		public void OnSensorChanged(SensorEvent e)
		{
			lock (_syncLock)
			{
				if (e.Sensor.Type == SensorType.Gravity) {
					var text = new StringBuilder("x = ")
						.Append(e.Values[0])
						.Append(", y=")
						.Append(e.Values[1])
						.Append(", z=")
						.Append(e.Values[2]);
						
					if (gsData.collect == true) {

						if (gsData.recount == true) {
							gsData.absolutionAndAdd ((double)e.Values [0], (double)e.Values [1], (double)e.Values [2]);
							gsData.calculateBarometerGravityData ();

						} else if( (gsData.recount == false) && checkValidData()) {

							//Time to send the values to the server
							srData.AddGravitySensorData(gsData);
							sendData ();

							gsData.recount = false;
							gsData.collect = false;
							gsData.clear (); //this sets recount to false again
							indexOfPressureServiceRequest = 0;
						}
					}
					_sensorGravityTextView.Text = text.ToString();
				}

				if (e.Sensor.Type == SensorType.Pressure) {
					var text = new StringBuilder ("")
						.Append (e.Values [0]).Append ("hPA");
		
					if (gsData.collect == true) {
						if (gsData.recount == true) {
							gsData.addPressure ((double)e.Values [0]);
							gsData.calculateBarometerGravityData ();
						
						}else if( (gsData.recount == false) && checkValidData()) {

							//Time to send the values to the server
							srData.AddGravitySensorData (gsData);
							sendData ();

							gsData.recount = false;
							gsData.collect = false;
							gsData.clear ();//this sets recount to false again
						}
					}
					_sensorBarometerTextView.Text = text.ToString();
					gsData.height = calculateAltitudeInFeet (e.Values [0]);
					_sensorHeightTextView.Text = gsData.height.ToString();
					indexOfPressureServiceRequest = 0;
				}

				if (e.Sensor.Type == SensorType.Accelerometer) {

					gsData.accelerometer_x = (double) e.Values[0];
					gsData.accelerometer_y = (double) e.Values[1];
					gsData.accelerometer_z = (double) e.Values[2];
				}


				if (e.Sensor.Type == SensorType.Gyroscope) {
					var text = new StringBuilder("x = ")
						.Append(e.Values[0])
						.Append(", y=")
						.Append(e.Values[1])
						.Append(", z=")
						.Append(e.Values[2]);
					_sensorGyroscopeTextView.Text = text.ToString();

					gsData.gyroscope_x = (double) e.Values[0];
					gsData.gyroscope_y = (double) e.Values[1];
					gsData.gyroscope_z = (double) e.Values[2];
				}

				if (e.Sensor.Type == SensorType.MagneticField) {
					var text = new StringBuilder("x = ")
						.Append(e.Values[0])
						.Append(", y=")
						.Append(e.Values[1])
						.Append(", z=")
						.Append(e.Values[2]);
					_sensorMagneticFieldTextView.Text = text.ToString();

					gsData.magneticfeild_x = (double) e.Values[0];
					gsData.magneticfeild_y = (double) e.Values[1];
					gsData.magneticfeild_z = (double) e.Values[2];
				}

				if (e.Sensor.Type == SensorType.RelativeHumidity) {
				
					var text = new StringBuilder("")
						.Append(e.Values[0]);
					_sensorHumidityTextView.Text = text.ToString();

					gsData.humidity = (double) e.Values[0];
				}

				/*
				if (e.Sensor.Type != SensorType.AmbientTemperature) {
					var text = new StringBuilder("x = ")
						.Append(e.Values[0])
						.Append(", y=")
						.Append(e.Values[1])
						.Append(", z=")
						.Append(e.Values[2]);
					_sensorTemeperatureTextView.Text = text.ToString();
				}
				*/

			}
		}

		/*
		 * Calculate Altitute in meters
		 * from the sea level by looking at
		 * pressure variation
		 */
		public double calculateAltitudeInFeet(float hPAs)
		{
			var pstd = 1013.25;
			var altpress =  (1 - Math.Pow((hPAs/pstd), 0.190284)) * 47692.4063818 ;
			return (double) altpress;
		}


		public void OnLocationChanged(Location location)
		{
			_currentLocation = location;
			if (_currentLocation == null)
			{
				_sensorGPSTextView.Text = "Not Available";
			}
			else
			{
				_sensorGPSTextView.Text = String.Format("{0},{1}", _currentLocation.Latitude, _currentLocation.Longitude);

				latlong = string.Concat( "?lat=",(_currentLocation.Latitude).ToString(),"&lon=",(_currentLocation.Latitude).ToString());
				_sensorHeightTextView.Text = _currentLocation.Altitude.ToString();

				//Calculating pressure from api
				calculateBarometerDataFromWeatherAPI ();
			}
		}

		/*
		 * ILocationListener method implements
		 */
		public void OnStatusChanged(string provider, Availability status, Bundle extras)
		{
			Log.Debug(LogTag, "{0}, {1}", provider, status);
		}

		/*
		 * Initializes the location manager for listening to the 
		 * geolocation
		 * ILocationListener methods
		 */
		public void InitializeLocationManager()
		{
			_locationManager = (LocationManager)GetSystemService(LocationService);
			Criteria criteriaForLocationService = new Criteria
			{
				Accuracy = Accuracy.Fine
			};
			IList<string> acceptableLocationProviders = _locationManager.GetProviders(criteriaForLocationService, true);

			if (acceptableLocationProviders.Any())
			{
				_locationProvider = acceptableLocationProviders.First();
			}
			else
			{
				_locationProvider = String.Empty;
			}
		}

		/*
		 * ILocationListener method implements
		 */ 
		public void OnProviderDisabled(string provider)
		{
		}

		/*
		 * ILocationListener method implements
		 */
		public void OnProviderEnabled(string provider)
		{
		}


		void CreateAndShowDialog(Exception exception, String title)
		{
			CreateAndShowDialog(exception.Message, title);
		}

		void CreateAndShowDialog(string message, string title)
		{
			AlertDialog.Builder builder = new AlertDialog.Builder(this);

			builder.SetMessage(message);
			builder.SetTitle(title);
			builder.Create().Show();
		}


		class ProgressHandler : DelegatingHandler
		{
			int busyCount = 0;

			public event Action<bool> BusyStateChange;

			#region implemented abstract members of HttpMessageHandler

			protected override async Task<HttpResponseMessage> SendAsync (HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
			{
				//assumes always executes on UI thread
				if (busyCount++ == 0 && BusyStateChange != null)
					BusyStateChange (true);

				var response = await base.SendAsync (request, cancellationToken);

				// assumes always executes on UI thread
				if (--busyCount == 0 && BusyStateChange != null)
					BusyStateChange (false);

				return response;
			}

			#endregion

		}
	}
}

