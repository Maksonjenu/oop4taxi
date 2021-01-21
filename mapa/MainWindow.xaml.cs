using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using System.Device.Location;
using System.Windows.Forms;
using mapa.Classes;
using System.Threading;

namespace mapa
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
         List<PointLatLng> areaspots = new List<PointLatLng>();
         List<MapObject> mapObjects = new List<MapObject>();
         List<MapObject> SortedList = new List<MapObject>();
        RoutingProvider routingProvider = GMapProviders.OpenStreetMap;
        static PointLatLng startOfRoute;
        static PointLatLng endOfRoute;
        List<PointLatLng> nearestPointPosition = new List<PointLatLng>();
        List<MapObject> nearestObjects = new List<MapObject>();

        public MainWindow()
        {
            InitializeComponent();
            initMap();
            radioButCreate.IsChecked = true;
            createmodecombo.SelectedIndex = 0;
        }

        public void initMap()
        {
            GMaps.Instance.Mode = AccessMode.ServerAndCache;
            Map.MapProvider = OpenStreetMapProvider.Instance;
            
            Map.MinZoom = 2;
            Map.MaxZoom = 17;
            Map.Zoom = 15;
            Map.Position = new PointLatLng(55.012823, 82.950359);
            Map.MouseWheelZoomType = MouseWheelZoomType.MousePositionWithoutCenter;
            Map.CanDragMap = true;
            Map.DragButton = MouseButton.Left;

        }
        

        public void createMarker(List<PointLatLng> points, int index) 
        {
            MapObject mapObject = null;
            switch (index)
            {
                case 0:
                    {
                        mapObject = new Area_class(createObjectName.Text, points);
                        break;
                    }
                case 1:
                    {
                        mapObject = new Location_class(createObjectName.Text, points.Last());
                        break;
                    }
                case 2:
                    {
                        mapObject = new Car_class(createObjectName.Text, points.Last());
                        break;
                    }
                case 3:
                    {
                        mapObject = new Human_class(createObjectName.Text, points.Last());
                        break;
                    }
                case 4:
                    {
                        mapObject = new Route_class(createObjectName.Text, points);
                        break;
                    }
            
            }
            if (mapObject != null)
            {
                mapObjects.Add(mapObject);
                Map.Markers.Add(mapObject.getMarker());
            }
        }

    

        private void MapLoaded(object sender, RoutedEventArgs e)
        {
           
        }


        private void Map_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (radioButSearch.IsChecked == true)
            {
                findsresult.Items.Clear();
                PointLatLng clickedPoint = Map.FromLocalToLatLng((int)e.GetPosition(Map).X, (int)e.GetPosition(Map).Y);
                SortedList = mapObjects.OrderBy(o => o.getDist(clickedPoint)).ToList();
                foreach (MapObject obj in SortedList)
                {
                    string mapObjectAndDistanceString = new StringBuilder()
                        .Append(obj.getTitle())
                        .Append(" - ")
                        .Append(obj.getDist(clickedPoint).ToString("0.##"))
                        .Append(" м.").ToString();
                    findsresult.Items.Add(mapObjectAndDistanceString);
                }
            }

        }

        private void radioButCreate_Checked(object sender, RoutedEventArgs e)
        {
            createmodecombo.IsEnabled = true;
            addbuttoncreate.IsEnabled = true;
            resetpointcreate.IsEnabled = true;
        }

        private void radioButSearch_Checked(object sender, RoutedEventArgs e)
        {
            createmodecombo.IsEnabled = false;
            addbuttoncreate.IsEnabled = false;
            resetpointcreate.IsEnabled = false;
        }

        private void addbuttoncreate_Click(object sender, RoutedEventArgs e)
        {

            if (createObjectName.Text == "")
                System.Windows.MessageBox.Show("Имя объекта пустое");
            else
            {
                createMarker(areaspots, createmodecombo.SelectedIndex);
                areaspots = new List<PointLatLng>();
                createObjectName.Clear();
            }
            checker();
            
        }

        private void Map_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            double minimum = new double();
            PointLatLng clickedPoint = Map.FromLocalToLatLng((int)e.GetPosition(Map).X, (int)e.GetPosition(Map).Y);
            MapObject @object = null;
            if (mapObjects.Count != 0)
            minimum = mapObjects[0].getDist(clickedPoint);
            foreach (MapObject obj in mapObjects)
            {
                if (minimum > obj.getDist(clickedPoint))
                {
                    minimum = obj.getDist(clickedPoint);
                    @object = obj;
                }
                if (@object == null)
                    @object = mapObjects[0];
            }

            if (@object!=null)
            distanceToPoints.Content = $"{Math.Round(minimum)} { @object.getTitle()}"; 

        }

        void checker()
        {
            switch (createmodecombo.SelectedIndex)
            {
                case 0:
                    {
                        if (areaspots.Count > 2)
                            addbuttoncreate.IsEnabled = true;
                        else
                            addbuttoncreate.IsEnabled = false;
                        break;
                    }
                case 4:
                    {
                        if (areaspots.Count > 1)
                            addbuttoncreate.IsEnabled = true;
                        else
                            addbuttoncreate.IsEnabled = false;
                        break;
                    }
                default :
                    {
                        addbuttoncreate.IsEnabled = true;
                        break;
                    }

            }

            if (areaspots.Count == 0)
                addbuttoncreate.IsEnabled = false;
        }

        private void Map_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {

            
          
                areaspots.Add(Map.FromLocalToLatLng((int)e.GetPosition(Map).X, (int)e.GetPosition(Map).Y));
                checker();
                if (areaspots.Count > 1)
                end_spot.Content = $"{areaspots[areaspots.Count - 2].Lng} {areaspots[areaspots.Count - 2].Lat}";
                start_spot.Content = $"{areaspots.Last().Lng} {areaspots.Last().Lat}";
            
           
        }


       

    private void Focus_Follow(object sender, EventArgs args)
{
        Car_class c = (Car_class)sender;
        waybar.Maximum = c.route.Points.Count;
        Map.Position = c.getFocus();

            if (waybar.Value == waybar.Maximum)
                (sender as Car_class).Follow -= Focus_Follow;
          
            if (waybar.Value != waybar.Maximum)
                waybar.Value += 1;
        
            else
                waybar.Value = 0;
       
}

private void createmodecombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            checker();
        }

        private void findsresult_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
                if ((SortedList.Count != 0) || (findsresult.Items.Count != 0))
                    Map.Position = SortedList[findsresult.SelectedIndex]
                    .getFocus();
        }

      
        

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            findsresult.Items.Clear();
            for (int i = 0; i < mapObjects.Count; i++)
                if (mapObjects[i].objectName.Contains(whatineedtofound.Text))
                {
                    findsresult.Items.Add(mapObjects[i].objectName);
                    SortedList.Add(mapObjects[i]);
                }
        }

        private void resetpointcreate_Click(object sender, RoutedEventArgs e)
        {
            areaspots = new List<PointLatLng>();
            clickinfoX.Content = "0";
            clickinfoY.Content = "0";

        }
        
        private void stuvk_Click(object sender, RoutedEventArgs e)
        {

            {
                waybar.Value = 0;
                foreach (MapObject obj in mapObjects)
                if (obj is Human_class)
                startOfRoute = (obj.getFocus());
                endOfRoute = areaspots.Last();
                var besidedObj = mapObjects.OrderBy(mapObject => mapObject.getDist(startOfRoute));

                Car_class nearestCar = null;
                Human_class h = null;

                foreach (MapObject obj in mapObjects)
                {
                    if (obj is Human_class)
                    {
                        h = (Human_class)obj;
                        h.destinationPoint = endOfRoute;
                        break;
                    }
                }

                foreach (MapObject obj in besidedObj)
                {
                    if (obj is Car_class)
                    {
                        nearestCar = (Car_class)obj;
                        break;
                    }
                }

                
                var localRoute =  nearestCar.MoveTo(startOfRoute);
                if (localRoute != null)
                    createMarker(localRoute.Points, 4);
                else System.Windows.MessageBox.Show("маршрут не назначен");

                RoutingProvider routingProvider = GMapProviders.OpenStreetMap;
                MapRoute route = routingProvider.GetRoute(
                    startOfRoute,
                    endOfRoute,
                    false,
                    false,
                    15);
                createMarker(route.Points, 4);
                nearestCar.Arrived += h.CarArrived;
                h.seated += nearestCar.getintocar;
                nearestCar.Follow += Focus_Follow;
                
                nearestCar.Arrived += nearestCar.test;


              //  nearestCar.Arrived -= h.CarArrived;
              //  h.seated -= nearestCar.getintocar;
            }

        }

        private void MoveByRoute()
        {
            // test  test  test  test  test  test  test  test  test 
            MapRoute route = routingProvider.GetRoute(
                areaspots.Last(), // начальная точка маршрута
                areaspots[0], // конечная точка маршрута
                false, // поиск по шоссе (false - включен)
                false, // режим пешехода (false - выключен)
                (int)15);
            // получение точек маршрута
            List<PointLatLng> routePoints = route.Points;
            // последовательный перебор точек маршрута
            foreach (var point in route.Points)
            {
                // делегат, возвращающий управление в главный поток
                System.Windows.Application.Current.Dispatcher.Invoke(delegate {
                    // изменение позиции маркера
                    Map.Position = point;
                });
                // задержка 500 мс
                Thread.Sleep(500);
            }
        }
    }
    
}




//public ref GMapMarker findRef()
//{
//    return ref new GMapMarker(new PointLatLng());
//}

//PointLatLng point = new PointLatLng(55.016511, 82.946152);

//GMapMarker marker = new GMapMarker(point)
//{
//    Shape = new Image
//    {
//        Width = 32, // ширина маркера
//        Height = 32, // высота маркера
//        ToolTip = "Honda CR-V", // всплывающая подсказка
//        Source = new BitmapImage(new Uri("pack://application:,,,/Resources/men.png")) // картинка
//    }
//};
//Map.Markers.Add(marker);

//GMapMarker marker = new GMapMarker(point)
//{
//    Shape = new Image
//    {
//        Width = 32, // ширина маркера
//        Height = 32, // высота маркера
//        ToolTip = "timetime", // всплывающая подсказка
//        Source = new BitmapImage(new Uri("pack://application:,,,/Resources/notMainSpot.png")) // картинка
//    }
//};





//if (Map.Markers.Count == 0)
//{
//    Map.Markers.Add(marker);
//    clickinfoX.Content = point.Lat;
//    clickinfoY.Content = point.Lng;

//}
//else
//{

//    Map.Markers.Add(marker);     // после выхода из метода ссылка обнуляется 
//    clickinfoX.Content = point.Lat;
//    clickinfoY.Content = point.Lng;
//}