using System;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using GMap.NET;
using GMap.NET.WindowsPresentation;
using System.Device.Location;
using mapa.Classes;
using System.Windows;

namespace mapa
{
    class Human_class : MapObject
    {
        public PointLatLng point { get; set; }
        public PointLatLng destinationPoint { get; set; }
        public GMapMarker marker { get; private set; }
        
        public event EventHandler seated;
        public Human_class(string name, PointLatLng Point) : base(name)
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
                    Source = new BitmapImage(new Uri("pack://application:,,,/Resources/man.jpg")) // картинка
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

        public void CarArrived(object sender, EventArgs args)
        {
            
         MessageBox.Show("Машина прибыла ");
         seated?.Invoke(this, EventArgs.Empty);
         (sender as Car_class).Arrived -= CarArrived;


        }
           
            
          
        

    }
}
