using System;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using GMap.NET;
using GMap.NET.WindowsPresentation;
using System.Device.Location;
using mapa.Classes;
using System.Collections.Generic;
using GMap.NET.MapProviders;
using System.Threading;
using System.Windows;

namespace mapa
{
    class Car_class : MapObject
    {
        private PointLatLng point;
        private List<Human_class> passengers = new List<Human_class>();
        public MapRoute route { get; private set; }
        private GMapMarker marker;

        private Human_class human;

        public event EventHandler Arrived;
        public event EventHandler Follow;
        

        public Car_class(string name,PointLatLng Point):base(name)
        {
            this.point = Point;
        }



        public override PointLatLng getFocus() => point;
        
      

        public override GMapMarker getMarker()
        {
             marker = new GMapMarker(point)
            {
                Shape = new Image
                {
                    Width = 32, // ширина маркера
                    Height = 32, // высота маркера
                    ToolTip = objectName, // всплывающая подсказка
                    Margin = new System.Windows.Thickness(-16, -16, 0, 0),
                    Source = new BitmapImage(new Uri("pack://application:,,,/Resources/car.jpg")) // картинка
                }
            };
            return marker;
        }

       

        public override double getDist(PointLatLng point1)
        {
            GeoCoordinate geo1 = new GeoCoordinate(point.Lat, point.Lng);
            GeoCoordinate geo2 = new GeoCoordinate(point1.Lat, point1.Lng);
            return geo1.GetDistanceTo(geo2);
        }

        public MapRoute MoveTo(PointLatLng endpoint)
        {
            RoutingProvider routingProvider = GMapProviders.OpenStreetMap;
            route = routingProvider.GetRoute(
                point,
                endpoint,
                false,
                false,
                15);

            Thread ridingCar = new Thread(MoveByRoute);
            ridingCar.Start();
            return route;
        }

        private void MoveByRoute()
        {
            try
            {
                foreach (var point in route.Points)
                {
                    Application.Current.Dispatcher.Invoke(delegate
                    {
                        this.point = point;
                        marker.Position = point;

                        if (human != null) // human = null
                        {
                            human.marker.Position = point;
                            Follow?.Invoke(this, null);
                        }
                    });

                    Thread.Sleep(700);
                }

                

                if (human == null)
                    Arrived?.Invoke(this, null);
                else
                {
                    human.point = point;
                    MessageBox.Show("Прибыли");
                    human = null;
                    Arrived?.Invoke(this, null);
                }
            }
            catch
            {

            }
        }

        public void getintocar(object sender, EventArgs args)
        {
            
            human = (Human_class)sender;
            MoveTo(human.destinationPoint);
            human.point = point;
            (sender as Human_class).seated -= getintocar;
        }

        public void test (object sender, EventArgs args)
        {

        }
    }
}
