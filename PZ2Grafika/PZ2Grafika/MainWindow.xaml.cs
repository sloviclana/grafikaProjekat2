using PZ2Grafika.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using System.Xml;
using Point = PZ2Grafika.Model.Point;
using System.Windows.Controls.Primitives;

namespace PZ2Grafika
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public double noviX, noviY;
        // donji levi ugao
        public const double minLongitude = 19.793909;
        public const double minLatitude = 45.2325;
        // gornji desni 
        public const double maxLongitude = 19.894459;
        public const double maxLatitude = 45.277031;
        // liste za iscitavanje iz xmla
        public List<LineEntity> listaVodova = new List<LineEntity>();

       

        public List<NodeEntity> listaNodova = new List<NodeEntity>();
        public List<SubstationEntity> listaSubstationa = new List<SubstationEntity>();
        public List<SwitchEntity> listaSwitcheva = new List<SwitchEntity>();
        // za testiranje 
        public List<NodeEntity> listaNodovaProba = new List<NodeEntity>();
        public List<SubstationEntity> listaSubstationaProba = new List<SubstationEntity>();
        public List<SwitchEntity> listaSwitchevaProba = new List<SwitchEntity>();

        // za dodatni zadatak
        //public List<GeometryModel3D> SwitchGeometryModel = new List<GeometryModel3D>();
        //public List<GeometryModel3D> NodeGeometryModel = new List<GeometryModel3D>();
        //public List<GeometryModel3D> SubstationsGeometryModel = new List<GeometryModel3D>();
        public List<GeometryModel3D> steelVodovi = new List<GeometryModel3D>();
        public List<GeometryModel3D> copperVodovi = new List<GeometryModel3D>();
        public List<GeometryModel3D> acsrVodovi = new List<GeometryModel3D>();


        //
        private static Dictionary<long, PowerEntity> collectionOfNodesID = new Dictionary<long, PowerEntity>();
        // da vidim da li se nalazi na istom mestu 
        private static Dictionary<System.Windows.Point, int> numberOfEntityOnPoint = new Dictionary<System.Windows.Point, int>();
        //menjanje boje switcheva
        Dictionary<GeometryModel3D, long> allEntities = new Dictionary<GeometryModel3D, long>();

        Dictionary<GeometryModel3D, long> allVodes = new Dictionary<GeometryModel3D, long>();
        //tooltip
        System.Windows.Point mousePositionForToolTip = new System.Windows.Point();
        ToolTip toolTip = new ToolTip();
        private GeometryModel3D hitgeo;
        Dictionary<GeometryModel3D, PowerEntity> geometryAndEntyty = new Dictionary<GeometryModel3D, PowerEntity>();
        Dictionary<GeometryModel3D, LineEntity> geometryAndVod = new Dictionary<GeometryModel3D, LineEntity>();
        //skroll za zoom 
        private int zoomMax = 50;
        private int zoomCurent = 30;
        int currentZoom = 1;
        int maxZoom = 20;
        int minZoom = -5;
        //pan
        private System.Windows.Point start = new System.Windows.Point();
        private System.Windows.Point diffOffset = new System.Windows.Point();
        private bool MiddleClicked = false;
        DoubleAnimation doubleAnimation = new DoubleAnimation(0, 360, TimeSpan.FromSeconds(15));
        //neaktivan deo 
        static Dictionary<long, SwitchEntity> switches = new Dictionary<long, SwitchEntity>();

        static List<string> materijali = new List<string>();
        static Dictionary<int, int> otpornosti = new Dictionary<int, int>();
        //rotiranje
        private System.Windows.Point startPosition = new System.Windows.Point();
        public List<PowerEntity> allXmlElements = new List<PowerEntity>();
        public List<LineEntity> allLines = new List<LineEntity>();

        public MainWindow()
        {
            InitializeComponent();

            ParseXml();
            NacrtajNode();
            NacrtajSubstations();
            NacrtajSwitcheve();
            NacrtajVodove();
        }

        // Ucita iz xmla 
        private void ParseXml()
        {
            double longit = 0; // izlaz iz ToLatLon funkcije 
            double latid = 0;

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load("Geographic.xml");
            XmlNodeList nodeList;
            List<SubstationEntity> subis = new List<SubstationEntity>();
            List<NodeEntity> nobis = new List<NodeEntity>();
            List<SwitchEntity> switchis = new List<SwitchEntity>();
            List<LineEntity> linis = new List<LineEntity>();

            var filename = "Geographic.xml";
            var currentDirectory = Directory.GetCurrentDirectory();
            var purchaseOrderFilepath = System.IO.Path.Combine(currentDirectory, filename);
            StringBuilder result = new StringBuilder();
            XDocument xdoc = XDocument.Load(filename);

            #region substations
            nodeList = xmlDoc.DocumentElement.SelectNodes("/NetworkModel/Substations/SubstationEntity");
            foreach (XmlNode node in nodeList)
            {

                SubstationEntity sub = new SubstationEntity();
                sub.Id = long.Parse(node.SelectSingleNode("Id").InnerText);
                sub.Name = node.SelectSingleNode("Name").InnerText;
                sub.X = double.Parse(node.SelectSingleNode("X").InnerText);
                sub.Y = double.Parse(node.SelectSingleNode("Y").InnerText);


                subis.Add(sub); // dodat odmah iz xml
                allXmlElements.Add(sub);
            }

            for (int i = 0; i < subis.Count(); i++)
            {
                listaSubstationaProba.Add(subis[i]);
                var item = subis[i];
                ToLatLon(item.X, item.Y, 34, out latid, out longit);
                if (latid >= minLatitude && latid <= maxLatitude && longit >= minLongitude && longit <= maxLongitude)
                {
                    subis[i].Latitude = latid;
                    subis[i].Longitude = longit;
                    listaSubstationa.Add(subis[i]);
                    collectionOfNodesID.Add(subis[i].Id, subis[i]);
                }

            }
            #endregion

            #region nodes
            nodeList = xmlDoc.DocumentElement.SelectNodes("/NetworkModel/Nodes/NodeEntity");
            foreach (XmlNode node in nodeList)
            {

                NodeEntity nodeobj = new NodeEntity();
                nodeobj.Id = long.Parse(node.SelectSingleNode("Id").InnerText);
                nodeobj.Name = node.SelectSingleNode("Name").InnerText;
                nodeobj.X = double.Parse(node.SelectSingleNode("X").InnerText);
                nodeobj.Y = double.Parse(node.SelectSingleNode("Y").InnerText);

                nobis.Add(nodeobj);
                allXmlElements.Add(nodeobj);

            }
            for (int i = 0; i < nobis.Count(); i++)
            {
                listaNodovaProba.Add(nobis[i]);
                var item = nobis[i];
                ToLatLon(item.X, item.Y, 34, out latid, out longit);
                if (latid >= minLatitude && latid <= maxLatitude && longit >= minLongitude && longit <= maxLongitude)
                {
                    nobis[i].Latitude = latid;
                    nobis[i].Longitude = longit;
                    listaNodova.Add(nobis[i]);
                    collectionOfNodesID.Add(nobis[i].Id, nobis[i]);
                }

            }
            #endregion

            #region switches
            nodeList = xmlDoc.DocumentElement.SelectNodes("/NetworkModel/Switches/SwitchEntity");
            foreach (XmlNode node in nodeList)
            {
                SwitchEntity switchobj = new SwitchEntity();
                switchobj.Id = long.Parse(node.SelectSingleNode("Id").InnerText);
                switchobj.Name = node.SelectSingleNode("Name").InnerText;
                switchobj.X = double.Parse(node.SelectSingleNode("X").InnerText);
                switchobj.Y = double.Parse(node.SelectSingleNode("Y").InnerText);
                switchobj.Status = node.SelectSingleNode("Status").InnerText;

                switchis.Add(switchobj);
                switches.Add(switchobj.Id, switchobj);

            }

            for (int i = 0; i < switchis.Count(); i++)
            {
                listaSwitchevaProba.Add(switchis[i]);
                var item = switchis[i];
                ToLatLon(item.X, item.Y, 34, out latid, out longit);
                if (latid >= minLatitude && latid <= maxLatitude && longit >= minLongitude && longit <= maxLongitude)
                {
                    switchis[i].Latitude = latid;
                    switchis[i].Longitude = longit;
                    listaSwitcheva.Add(switchis[i]);
                    collectionOfNodesID.Add(switchis[i].Id, switchis[i]);
                }


            }
            #endregion

            #region lines

            var lines = xdoc.Descendants("LineEntity")
                     .Select(line => new LineEntity
                     {
                         Id = (long)line.Element("Id"),
                         Name = (string)line.Element("Name"),
                         ConductorMaterial = (string)line.Element("ConductorMaterial"),
                         IsUnderground = (bool)line.Element("IsUnderground"),
                         R = (float)line.Element("R"),
                         FirstEnd = (long)line.Element("FirstEnd"),
                         SecondEnd = (long)line.Element("SecondEnd"),
                         LineType = (string)line.Element("LineType"),
                         ThermalConstantHeat = (long)line.Element("ThermalConstantHeat"),
                         Vertices = line.Element("Vertices").Descendants("Point").Select(p => new Point
                         {
                             X = (double)p.Element("X"),
                             Y = (double)p.Element("Y"),
                         }).ToList()
                     }).ToList();

            for (int i = 0; i < lines.Count(); i++)
            {
                if (collectionOfNodesID.ContainsKey(lines[i].SecondEnd) && collectionOfNodesID.ContainsKey(lines[i].FirstEnd))
                {
                    var line = lines[i];
                    foreach (var point in line.Vertices)
                    {

                        ToLatLon(point.X, point.Y, 34, out latid, out longit);
                        point.Latitude = latid;
                        point.Longitude = longit;

                    }

                    if (!materijali.Contains(line.ConductorMaterial))
                        materijali.Add(line.ConductorMaterial);

                    listaVodova.Add(line);
                    allLines.Add(line);
                }
            }

            otpornosti.Add(0, 0);
            otpornosti.Add(1, 0);
            otpornosti.Add(2, 0);

            foreach(var line in lines)
            {
                if(line.R<1)
                {   //crvena
                    otpornosti[0]++;

                } else if(line.R >= 1 && line.R<2)
                {   //narandzasta
                    otpornosti[1]++;

                } else if(line.R>=2)
                {   //zuta
                    otpornosti[2]++;
                }
            }

            #endregion

        }

        //From UTM to Latitude and longitude in decimal
        public static void ToLatLon(double utmX, double utmY, int zoneUTM, out double latitude, out double longitude)
        {
            bool isNorthHemisphere = true;

            var diflat = -0.00066286966871111111111111111111111111;
            var diflon = -0.0003868060578;

            var zone = zoneUTM;
            var c_sa = 6378137.000000;
            var c_sb = 6356752.314245;
            var e2 = Math.Pow((Math.Pow(c_sa, 2) - Math.Pow(c_sb, 2)), 0.5) / c_sb;
            var e2cuadrada = Math.Pow(e2, 2);
            var c = Math.Pow(c_sa, 2) / c_sb;
            var x = utmX - 500000;
            var y = isNorthHemisphere ? utmY : utmY - 10000000;

            var s = ((zone * 6.0) - 183.0);
            var lat = y / (c_sa * 0.9996);
            var v = (c / Math.Pow(1 + (e2cuadrada * Math.Pow(Math.Cos(lat), 2)), 0.5)) * 0.9996;
            var a = x / v;
            var a1 = Math.Sin(2 * lat);
            var a2 = a1 * Math.Pow((Math.Cos(lat)), 2);
            var j2 = lat + (a1 / 2.0);
            var j4 = ((3 * j2) + a2) / 4.0;
            var j6 = ((5 * j4) + Math.Pow(a2 * (Math.Cos(lat)), 2)) / 3.0;
            var alfa = (3.0 / 4.0) * e2cuadrada;
            var beta = (5.0 / 3.0) * Math.Pow(alfa, 2);
            var gama = (35.0 / 27.0) * Math.Pow(alfa, 3);
            var bm = 0.9996 * c * (lat - alfa * j2 + beta * j4 - gama * j6);
            var b = (y - bm) / v;
            var epsi = ((e2cuadrada * Math.Pow(a, 2)) / 2.0) * Math.Pow((Math.Cos(lat)), 2);
            var eps = a * (1 - (epsi / 3.0));
            var nab = (b * (1 - epsi)) + lat;
            var senoheps = (Math.Exp(eps) - Math.Exp(-eps)) / 2.0;
            var delt = Math.Atan(senoheps / (Math.Cos(nab)));
            var tao = Math.Atan(Math.Cos(delt) * Math.Tan(nab));

            longitude = ((delt * (180.0 / Math.PI)) + s) + diflon;
            latitude = ((lat + (1 + e2cuadrada * Math.Pow(Math.Cos(lat), 2) - (3.0 / 2.0) * e2cuadrada * Math.Sin(lat) * Math.Cos(lat) * (tao - lat)) * (tao - lat)) * (180.0 / Math.PI)) + diflat;
        }

        //nacrtaj node
        public void NacrtajNode()
        {
            foreach (var node in listaNodova)
            {

                MeshGeometry3D meshGeometry3D = createCubeMeshGeometry(node.Longitude, node.Latitude, node.Id);
                DiffuseMaterial diffuseMaterial = new DiffuseMaterial(new SolidColorBrush(Colors.AliceBlue));

                GeometryModel3D geometryModel3D = new GeometryModel3D(meshGeometry3D, diffuseMaterial);

                RotateTransform3D rotateTransform3D = new RotateTransform3D();
                //rotateTransform3D.Rotation = myAngleRotation;

                Transform3DGroup myTransform3DGroup = new Transform3DGroup();
                myTransform3DGroup.Children.Add(rotateTransform3D);

                geometryModel3D.Transform = myTransform3DGroup;

                geometryAndEntyty.Add(geometryModel3D, node);
                //NodeGeometryModel.Add(geometryModel3D);
                model3dGroup.Children.Add(geometryModel3D);

                allEntities.Add(geometryModel3D, node.Id);
            }

        }

        //nacrtaj substations
        public void NacrtajSubstations()
        {
            foreach (var sub in listaSubstationa)
            {
                MeshGeometry3D meshGeometry3D = createCubeMeshGeometry(sub.Longitude, sub.Latitude, sub.Id);
                DiffuseMaterial diffuseMaterial = new DiffuseMaterial(new SolidColorBrush(Colors.DarkOrange));

                GeometryModel3D geometryModel3D = new GeometryModel3D(meshGeometry3D, diffuseMaterial);

                RotateTransform3D rotateTransform3D = new RotateTransform3D();
                //rotateTransform3D.Rotation = myAngleRotation;

                Transform3DGroup myTransform3DGroup = new Transform3DGroup();
                myTransform3DGroup.Children.Add(rotateTransform3D);

                geometryModel3D.Transform = myTransform3DGroup;

                geometryAndEntyty.Add(geometryModel3D, sub);
                //SubstationsGeometryModel.Add(geometryModel3D);
                model3dGroup.Children.Add(geometryModel3D);

                allEntities.Add(geometryModel3D, sub.Id);
            }
        }

        //nacrtaj switcheve
        public void NacrtajSwitcheve()
        {
            foreach (var sw in listaSwitcheva)
            {
                MeshGeometry3D meshGeometry3D = createCubeMeshGeometry(sw.Longitude, sw.Latitude, sw.Id);
                DiffuseMaterial diffuseMaterial = new DiffuseMaterial(new SolidColorBrush(Colors.Violet));

                GeometryModel3D geometryModel3D = new GeometryModel3D(meshGeometry3D, diffuseMaterial);

                RotateTransform3D rotateTransform3D = new RotateTransform3D();
                //rotateTransform3D.Rotation = myAngleRotation;

                Transform3DGroup myTransform3DGroup = new Transform3DGroup();
                myTransform3DGroup.Children.Add(rotateTransform3D);

                geometryModel3D.Transform = myTransform3DGroup;

                geometryAndEntyty.Add(geometryModel3D, sw);
                //SwitchGeometryModel.Add(geometryModel3D);
                model3dGroup.Children.Add(geometryModel3D);

                allEntities.Add(geometryModel3D, sw.Id);
            }
        }

        //nacrtaj vodove
        public void NacrtajVodove()
        {
            foreach (var line in listaVodova)
            {
                System.Windows.Point point1 = new System.Windows.Point();
                System.Windows.Point point2 = new System.Windows.Point();
                var temp = 1;
                System.Windows.Point Startpoint = new System.Windows.Point();
                System.Windows.Point Endpoint = new System.Windows.Point();
                GeometryModel3D vod = new GeometryModel3D();
                GeometryModel3D vodDeo1 = new GeometryModel3D();
                GeometryModel3D vodDeo2 = new GeometryModel3D();
                GeometryModel3D vodDeo3 = new GeometryModel3D();
                DiffuseMaterial boja = new DiffuseMaterial();

                if (collectionOfNodesID.ContainsKey(line.FirstEnd) && collectionOfNodesID.ContainsKey(line.SecondEnd))
                {
                    Startpoint = CreatePoint(collectionOfNodesID[line.FirstEnd].Longitude, collectionOfNodesID[line.FirstEnd].Latitude);
                    Endpoint = CreatePoint(collectionOfNodesID[line.SecondEnd].Longitude, collectionOfNodesID[line.SecondEnd].Latitude);

                }
                else
                {
                    continue;
                }

                // na osnovu materijala 
                if (line.ConductorMaterial == "Steel")
                {
                    boja = new DiffuseMaterial(System.Windows.Media.Brushes.Green);
                }
                else if (line.ConductorMaterial == "Acsr")
                {
                    boja = new DiffuseMaterial(System.Windows.Media.Brushes.Red);
                }
                else if (line.ConductorMaterial == "Copper")
                {
                    boja = new DiffuseMaterial(System.Windows.Media.Brushes.Black);
                }

                foreach (var point in line.Vertices)
                {
                    if (temp == 1)
                    {
                        point1 = CreatePoint(point.Longitude, point.Latitude);
                        vodDeo1 = createLineGeometryModel3D(Startpoint, point1, boja);
                        model3dGroup.Children.Add(vodDeo1);
                        geometryAndVod.Add(vodDeo1, line);
                        temp++;
                        continue;
                    }
                    else if (temp == 2)
                    {
                        point2 = CreatePoint(point.Longitude, point.Latitude);
                        vodDeo2 = createLineGeometryModel3D(point1, point2, boja);
                        model3dGroup.Children.Add(vodDeo2);
                        geometryAndVod.Add(vodDeo2, line);
                        point1 = point2;
                    }

                    if (line.ConductorMaterial == "Steel")
                    {
                        //steelVodovi.Add(vod);
                        steelVodovi.Add(vodDeo1);
                        steelVodovi.Add(vodDeo2);
                    }
                    else if (line.ConductorMaterial == "Acsr")
                    {
                        //acsrVodovi.Add(vod);
                        acsrVodovi.Add(vodDeo1);
                        acsrVodovi.Add(vodDeo2);
                    }
                    else if (line.ConductorMaterial == "Copper")
                    {
                        //copperVodovi.Add(vod);
                        copperVodovi.Add(vodDeo1);
                        copperVodovi.Add(vodDeo2);
                    }

                }

                
                //geometryAndVod.Add(vodDeo1, line);
                //geometryAndVod.Add(vodDeo2, line);

                vod = createLineGeometryModel3D(point2, Endpoint, boja);

                allVodes.Add(vod, line.Id);
                geometryAndVod.Add(vod, line);

                
                if (line.ConductorMaterial == "Steel")
                {
                    steelVodovi.Add(vod);
                    //steelVodovi.Add(vodDeo1);
                    //steelVodovi.Add(vodDeo2);
                }
                else if (line.ConductorMaterial == "Acsr")
                {
                    acsrVodovi.Add(vod);
                    //acsrVodovi.Add(vodDeo1);
                    //acsrVodovi.Add(vodDeo2);
                }
                else if (line.ConductorMaterial == "Copper")
                {
                    copperVodovi.Add(vod);
                    //copperVodovi.Add(vodDeo1);
                    //copperVodovi.Add(vodDeo2);
                }
                

                model3dGroup.Children.Add(vod);

                allEntities.Add(vod, line.Id);
            }
        }

        // stvori cubes za entitete
        public static MeshGeometry3D createCubeMeshGeometry(double longitude, double latitude, long entityID)
        {
            var point = CreatePoint(longitude, latitude);
            int numEntity = 0;

            if (numberOfEntityOnPoint.ContainsKey(point))
            {
                numberOfEntityOnPoint[point]++;
                numEntity = numberOfEntityOnPoint[point];
            }
            else
            {
                numberOfEntityOnPoint[point] = 1;
                numEntity = 1;
            }

            MeshGeometry3D meshGeometry3D = new MeshGeometry3D();

            List<Point3D> pointovi = new List<Point3D>();
            // kocka ima 8 tacaka
            pointovi.Add(new Point3D(point.X - 4, point.Y - 4, numEntity * 10 - 10));
            pointovi.Add(new Point3D(point.X + 4, point.Y - 4, numEntity * 10 - 10));
            pointovi.Add(new Point3D(point.X + 4, point.Y - 4, numEntity * 10));
            pointovi.Add(new Point3D(point.X - 4, point.Y - 4, numEntity * 10));
            //dole
            pointovi.Add(new Point3D(point.X - 4, point.Y + 4, numEntity * 10));
            pointovi.Add(new Point3D(point.X + 4, point.Y + 4, numEntity * 10));
            pointovi.Add(new Point3D(point.X + 4, point.Y + 4, numEntity * 10 - 10));
            pointovi.Add(new Point3D(point.X - 4, point.Y + 4, numEntity * 10 - 10));

            meshGeometry3D.Positions = new Point3DCollection(pointovi);
            // stranice
            meshGeometry3D.TriangleIndices.Add(0);
            meshGeometry3D.TriangleIndices.Add(1);
            meshGeometry3D.TriangleIndices.Add(2);
            meshGeometry3D.TriangleIndices.Add(0);
            meshGeometry3D.TriangleIndices.Add(2);
            meshGeometry3D.TriangleIndices.Add(3);

            meshGeometry3D.TriangleIndices.Add(3);
            meshGeometry3D.TriangleIndices.Add(5);
            meshGeometry3D.TriangleIndices.Add(4);
            meshGeometry3D.TriangleIndices.Add(3);
            meshGeometry3D.TriangleIndices.Add(2);
            meshGeometry3D.TriangleIndices.Add(5);

            meshGeometry3D.TriangleIndices.Add(2);
            meshGeometry3D.TriangleIndices.Add(6);
            meshGeometry3D.TriangleIndices.Add(5);
            meshGeometry3D.TriangleIndices.Add(2);
            meshGeometry3D.TriangleIndices.Add(1);
            meshGeometry3D.TriangleIndices.Add(6);

            meshGeometry3D.TriangleIndices.Add(3);
            meshGeometry3D.TriangleIndices.Add(7);
            meshGeometry3D.TriangleIndices.Add(0);
            meshGeometry3D.TriangleIndices.Add(4);
            meshGeometry3D.TriangleIndices.Add(7);
            meshGeometry3D.TriangleIndices.Add(3);

            meshGeometry3D.TriangleIndices.Add(4);
            meshGeometry3D.TriangleIndices.Add(6);
            meshGeometry3D.TriangleIndices.Add(7);
            meshGeometry3D.TriangleIndices.Add(4);
            meshGeometry3D.TriangleIndices.Add(5);
            meshGeometry3D.TriangleIndices.Add(6);

            // dodaj donju stanicu 

            return meshGeometry3D;
        }

        public static GeometryModel3D createLineGeometryModel3D(System.Windows.Point point1, System.Windows.Point point2, DiffuseMaterial boja)
        {
            MeshGeometry3D meshGeometry3D = new MeshGeometry3D();
            List<Point3D> points = new List<Point3D>();
            points.Add(new Point3D(point1.X - 1, point1.Y - 1, 0)); //0
            points.Add(new Point3D(point1.X + 1, point1.Y + 1, 0)); //1
            points.Add(new Point3D(point1.X - 1, point1.Y - 1, 10)); //2
            points.Add(new Point3D(point1.X + 1, point1.Y + 1, 10)); //3

            points.Add(new Point3D(point2.X - 1, point2.Y - 1, 0)); //4
            points.Add(new Point3D(point2.X + 1, point2.Y + 1, 0)); //5
            points.Add(new Point3D(point2.X - 1, point2.Y - 1, 10)); //6
            points.Add(new Point3D(point2.X + 1, point2.Y + 1, 10)); //7


            meshGeometry3D.Positions = new Point3DCollection(points);
            meshGeometry3D.TriangleIndices.Add(0);
            meshGeometry3D.TriangleIndices.Add(2);
            meshGeometry3D.TriangleIndices.Add(3);
            meshGeometry3D.TriangleIndices.Add(3);
            meshGeometry3D.TriangleIndices.Add(1);
            meshGeometry3D.TriangleIndices.Add(0); // back

            meshGeometry3D.TriangleIndices.Add(4);
            meshGeometry3D.TriangleIndices.Add(6);
            meshGeometry3D.TriangleIndices.Add(2);
            meshGeometry3D.TriangleIndices.Add(2);
            meshGeometry3D.TriangleIndices.Add(0);
            meshGeometry3D.TriangleIndices.Add(4); // left

            meshGeometry3D.TriangleIndices.Add(5);
            meshGeometry3D.TriangleIndices.Add(7);
            meshGeometry3D.TriangleIndices.Add(6);
            meshGeometry3D.TriangleIndices.Add(6);
            meshGeometry3D.TriangleIndices.Add(4);
            meshGeometry3D.TriangleIndices.Add(5); //front

            meshGeometry3D.TriangleIndices.Add(1);
            meshGeometry3D.TriangleIndices.Add(3);
            meshGeometry3D.TriangleIndices.Add(7);
            meshGeometry3D.TriangleIndices.Add(7);
            meshGeometry3D.TriangleIndices.Add(5);
            meshGeometry3D.TriangleIndices.Add(1); // right

            meshGeometry3D.TriangleIndices.Add(7);
            meshGeometry3D.TriangleIndices.Add(3);
            meshGeometry3D.TriangleIndices.Add(2);
            meshGeometry3D.TriangleIndices.Add(2);
            meshGeometry3D.TriangleIndices.Add(6);
            meshGeometry3D.TriangleIndices.Add(7); // top

            meshGeometry3D.TriangleIndices.Add(1);
            meshGeometry3D.TriangleIndices.Add(5);
            meshGeometry3D.TriangleIndices.Add(4);
            meshGeometry3D.TriangleIndices.Add(4);
            meshGeometry3D.TriangleIndices.Add(0);
            meshGeometry3D.TriangleIndices.Add(1); // bottom

            GeometryModel3D geometryModel3D = new GeometryModel3D(meshGeometry3D, boja);

            RotateTransform3D rotateTransform3D = new RotateTransform3D();
            //rotateTransform3D.Rotation = myAngleRotation;

            Transform3DGroup myTransform3DGroup = new Transform3DGroup();
            myTransform3DGroup.Children.Add(rotateTransform3D);

            geometryModel3D.Transform = myTransform3DGroup;

            return geometryModel3D;
        }

        // skaliranje
        public static System.Windows.Point CreatePoint(double longitude, double latitude)
        {
            // vel canvasa 2200x2200
            double vrednostJednogLongitude = (maxLongitude - minLongitude) / 1175; // width slike mape
            double vrednostJednogLatitude = (maxLatitude - minLatitude) / 775; // height slike mape 

            // koliko stane u rastojanje izmedju ttrenutne i minimalne 
            double x = Math.Round((longitude - minLongitude) / vrednostJednogLongitude) - 587.5; // pozicija na xamlu
            double y = Math.Round((latitude - minLatitude) / vrednostJednogLatitude) - 387.5;

            // zaokruzi na prvi broj deljiv sa 10 
            x = x - x % 8; // rastojanje izmedju dva susedna x
            y = y - y % 8; // rastojanje izmedju dva susedna y 

            System.Windows.Point point = new System.Windows.Point();
            point.X = x;
            point.Y = y;

            return point;
        }


        #region dodatne opcije u okviru interfejsa 

        public void SwitchMenjanjeBoje(object sender, RoutedEventArgs e)
        {
            if (promenaBojeSwitch.IsChecked == true)
            {
                foreach (SwitchEntity svic in listaSwitcheva)
                {
                    if (svic.Status.ToLower().Equals("open"))
                    {
                        allEntities.FirstOrDefault(x => x.Value == svic.Id).Key.Material = new DiffuseMaterial(System.Windows.Media.Brushes.Green);
                    }
                    else if (svic.Status.ToLower().Equals("closed"))
                    {
                        allEntities.FirstOrDefault(x => x.Value == svic.Id).Key.Material = new DiffuseMaterial(System.Windows.Media.Brushes.Red);
                    }
                }
            }
            else if (promenaBojeSwitch.IsChecked == false)
            {
                foreach (SwitchEntity svic in listaSwitcheva)
                {
                    allEntities.FirstOrDefault(x => x.Value == svic.Id).Key.Material = new DiffuseMaterial(System.Windows.Media.Brushes.Violet);
                }
            }
        }

        public void VodoviMenjanjeBoje(object sender, RoutedEventArgs e)
        {

            if (promenaBojeVodova.IsChecked == true)
            {   /*
                foreach (LineEntity line in listaVodova)
                {
                    foreach (KeyValuePair<GeometryModel3D, long> gm in allVodes)
                    {
                        if (gm.Value == line.Id)
                        {
                            if (line.R < 1)
                                ((DiffuseMaterial)gm.Key.Material).Brush = System.Windows.Media.Brushes.Red;
                            else if (line.R >= 1 && line.R <= 2)
                                ((DiffuseMaterial)gm.Key.Material).Brush = System.Windows.Media.Brushes.Orange;
                            else if (line.R > 2)
                                ((DiffuseMaterial)gm.Key.Material).Brush = System.Windows.Media.Brushes.Yellow;
                        }
                    }
                }*/

                foreach(KeyValuePair<GeometryModel3D, LineEntity> vod1 in geometryAndVod)
                {
                    if(vod1.Value.R < 1)
                        ((DiffuseMaterial)vod1.Key.Material).Brush = System.Windows.Media.Brushes.Red;
                    else if(vod1.Value.R >= 1 && vod1.Value.R < 2)
                        ((DiffuseMaterial)vod1.Key.Material).Brush = System.Windows.Media.Brushes.Orange;
                    else if(vod1.Value.R > 2)
                        ((DiffuseMaterial)vod1.Key.Material).Brush = System.Windows.Media.Brushes.Yellow;
                }
            }
            else if (promenaBojeVodova.IsChecked == false)
            {
                foreach (LineEntity line in listaVodova)
                {
                    foreach (KeyValuePair<GeometryModel3D, long> gm in allVodes)
                    {
                        if (gm.Value == line.Id)
                        {
                            if (line.ConductorMaterial == "Steel")
                                ((DiffuseMaterial)gm.Key.Material).Brush = System.Windows.Media.Brushes.DarkGreen;
                            else if (line.ConductorMaterial == "Acsr")
                                ((DiffuseMaterial)gm.Key.Material).Brush = System.Windows.Media.Brushes.DarkRed;
                            else if (line.ConductorMaterial == "Copper")
                                ((DiffuseMaterial)gm.Key.Material).Brush = System.Windows.Media.Brushes.Black;
                        }
                    }
                }
            }

        }

        private List<GeometryModel3D> vodoviZaBrisanje = new List<GeometryModel3D>();
        public void NeaktivanDeoMreze(object sender, RoutedEventArgs e)
        {
            if (neaktivanDeo.IsChecked == true)
            {
                foreach (KeyValuePair<GeometryModel3D, long> entity in allEntities)
                {
                    if (switches.ContainsKey(entity.Value))
                    {
                        if (switches[entity.Value].Status.ToLower().Equals("open"))
                        {
                            foreach (var line in listaVodova)
                            {
                                if (line.FirstEnd == entity.Value) // samo ako je first switch open 
                                {

                                    model3dGroup.Children.Remove(entity.Key);

                                    foreach (KeyValuePair<GeometryModel3D, LineEntity> vodd in geometryAndVod)
                                    {
                                        if (line.Id == vodd.Value.Id)
                                        {
                                            //model3dGroup.Children.Remove(vodd.Key);
                                            vodoviZaBrisanje.Add(vodd.Key);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                foreach (GeometryModel3D vod in vodoviZaBrisanje)
                {
                    model3dGroup.Children.Remove(vod);
                    
                }
                    
            }
            else if (neaktivanDeo.IsChecked == false)
            {
                foreach (KeyValuePair<GeometryModel3D, long> entity in allEntities)
                {
                    if (model3dGroup.Children.Contains(entity.Key) == false)
                    {
                        model3dGroup.Children.Add(entity.Key);
                    }
                }

                foreach (GeometryModel3D vod in vodoviZaBrisanje)
                {
                    model3dGroup.Children.Add(vod);
                    //model3dGroup.Children.Remove(vod);

                }
            }

        }

        #endregion

        #region zoom i pan
        //zoom 
        private void ViewPort_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            toolTip.IsOpen = false;

            System.Windows.Point p = e.MouseDevice.GetPosition(this);

            double scaleX = 1;
            double scaleY = 1;
            double scaleZ = 1;

            if (e.Delta > 0 && currentZoom < maxZoom)
            {
                scaleX = skaliranje.ScaleX + 0.1;
                scaleY = skaliranje.ScaleY + 0.1;
                scaleZ = skaliranje.ScaleZ + 0.1;
                currentZoom++;
                skaliranje.ScaleX = scaleX;
                skaliranje.ScaleY = scaleY;
                skaliranje.ScaleZ = scaleZ;
            }
            else if (e.Delta <= 0 && currentZoom > minZoom)
            {
                scaleX = skaliranje.ScaleX - 0.1;
                scaleY = skaliranje.ScaleY - 0.1;
                scaleZ = skaliranje.ScaleZ - 0.1;
                currentZoom--;
                skaliranje.ScaleX = scaleX;
                skaliranje.ScaleY = scaleY;
                skaliranje.ScaleZ = scaleZ;
            }
        }

        // pan 
        private void ViewPort_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //toolTip.IsOpen = false; //UKLONI Tooltip nakon klika

            ViewPort.CaptureMouse();
            start = e.GetPosition(ViewPort);
            diffOffset.X = translacija.OffsetX;
            diffOffset.Y = translacija.OffsetY;
 

            //hit testing
            System.Windows.Point mouseposition = e.GetPosition(ViewPort);
            Point3D testpoint3D = new Point3D(mouseposition.X, mouseposition.Y, 0);
            Vector3D testdirection = new Vector3D(mouseposition.X, mouseposition.Y, 10);

            PointHitTestParameters pointparams = new PointHitTestParameters(mouseposition);
            RayHitTestParameters rayparams = new RayHitTestParameters(testpoint3D, testdirection);

            //test for a result in the Viewport3D     
            hitgeo = null;
            VisualTreeHelper.HitTest(ViewPort, null, HTResult, pointparams);

        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            otvorenTooltip();
            ViewPort_MouseLeftButtonDown(ViewPort, e);
        }

        private HitTestResultBehavior HTResult(System.Windows.Media.HitTestResult rawresult)
        {
            RayHitTestResult rayResult = rawresult as RayHitTestResult;

            if (rayResult != null)
            {
                DiffuseMaterial darkSide = new DiffuseMaterial(new SolidColorBrush(System.Windows.Media.Colors.Red));
                bool gasit = false;

                #region Kada se klikne entitet - ToolTip

                foreach(KeyValuePair<GeometryModel3D, PowerEntity> model in geometryAndEntyty)
                {
                    if(model.Key == rayResult.ModelHit)
                    {
                        hitgeo = (GeometryModel3D)rayResult.ModelHit;
                        gasit = true;

                        if (model.Value is NodeEntity)
                        {
                            toolTip.Content = "\tNode entity:\n\nID: " + model.Value.Id.ToString() + "\nName: " + model.Value.Name + "\nType: " + model.Value.GetType().Name;

                        }
                        else if (model.Value is SubstationEntity)
                        {
                            toolTip.Content = "\tSubstation entity:\n\nID: " + model.Value.Id.ToString() + "\nName: " + model.Value.Name + "\nType: " + model.Value.GetType().Name;

                        }
                        else if (model.Value is SwitchEntity)
                        {
                            toolTip.Content = "\tSwitch entity:\n\nID: " + model.Value.Id.ToString() + "\nName: " + model.Value.Name + "\nType: " + model.Value.GetType().Name;

                        }
                        toolTip.Height = 100;
                        toolTip.IsOpen = true;

                        ToolTipService.SetPlacement(ViewPort, PlacementMode.Mouse);
                    }
                }
                #endregion

                #region Kada se klikne na vod
                //firstEnd i SecondEnd su ID-jevi entiteta

                if(geometryAndVod.ContainsKey((GeometryModel3D)rayResult.ModelHit))
                {
                    hitgeo = (GeometryModel3D)rayResult.ModelHit;
                    gasit = true;

                    var prviEntitet = geometryAndVod[(GeometryModel3D)rayResult.ModelHit].FirstEnd;
                    var drugiEntitet = geometryAndVod[(GeometryModel3D)rayResult.ModelHit].SecondEnd;
                    toolTip.Content = "\tLine entity:\n\nID: " + geometryAndVod[(GeometryModel3D)rayResult.ModelHit].Id.ToString() + "\nName: " + geometryAndVod[(GeometryModel3D)rayResult.ModelHit].Name + "\nType: " + geometryAndVod[(GeometryModel3D)rayResult.ModelHit].GetType().Name;

                    toolTip.Height = 100;
                    toolTip.IsOpen = true;

                    ToolTipService.SetPlacement(ViewPort, PlacementMode.Mouse);

                    foreach (KeyValuePair<GeometryModel3D, PowerEntity> ent1 in geometryAndEntyty)
                    {
                        if (ent1.Value.Id == prviEntitet || ent1.Value.Id == drugiEntitet)
                        {
                            ((DiffuseMaterial)((GeometryModel3D)ent1.Key).Material).Brush = Brushes.Red;
                            // hitgeo.Material = darkSide;// boji vod koji je kliknut
                        }
                    }
                }

               
                #endregion

                if (!gasit)
                {
                    hitgeo = null;
                }
            }
            return HitTestResultBehavior.Stop;
        }
        
        private void otvorenTooltip()
        {
            if (toolTip.IsOpen == true)
            {
                System.Threading.Thread.Sleep(100);
                toolTip.IsOpen = false;
            }
        }

        private void ViewPort_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ViewPort.ReleaseMouseCapture();
        }

        #endregion

        #region rotiranje
        private void ViewPort_MouseMove(object sender, MouseEventArgs e)
        {

            if (ViewPort.IsMouseCaptured)
            {
                System.Windows.Point end = e.GetPosition(this);
                double offsetX = end.X - start.X;
                double offsetY = end.Y - start.Y;
                double w = this.Width;
                double h = this.Height;
                double translateX = (offsetX * 100) / w * 100; // UBICU SE :)
                double translateY = -(offsetY * 100) / h * 100;
                translacija.OffsetX = diffOffset.X + (translateX / (100 * skaliranje.ScaleX));
                translacija.OffsetY = diffOffset.Y + (translateY / (100 * skaliranje.ScaleX));
                Console.WriteLine($"offsetX = {translacija.OffsetX}, offsetY = {translacija.OffsetY}");
            }

            System.Windows.Point currentPosition = e.GetPosition(this);
            if (e.MiddleButton == MouseButtonState.Pressed)
            {

                double pomerajX = currentPosition.X - startPosition.X;
                double pomerajY = currentPosition.Y - startPosition.Y;
                double step = 0.2;
                if ((rotateX.Angle + step * pomerajY) < 90 && (rotateX.Angle + step * pomerajY) > -90)
                    rotateX.Angle += step * pomerajY;
                if ((rotateY.Angle + step * pomerajX) < 90 && (rotateY.Angle + step * pomerajX) > -90)
                    rotateY.Angle += step * pomerajX;
            }

            startPosition = currentPosition;



        }

        #endregion 

        #region Tooltip 
        
        
        private void ViewPort_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            mousePositionForToolTip = e.GetPosition(ViewPort);
            Point3D testpoint3D = new Point3D(mousePositionForToolTip.X, mousePositionForToolTip.Y, 0);
            Vector3D testdirection = new Vector3D(mousePositionForToolTip.X, mousePositionForToolTip.Y, 10);

            PointHitTestParameters pointparams = new PointHitTestParameters(mousePositionForToolTip);
            RayHitTestParameters rayparams = new RayHitTestParameters(testpoint3D, testdirection);

            hitgeo = null;
            VisualTreeHelper.HitTest(ViewPort, null, HTResult, pointparams);
        }
        
        #endregion

        #region prikazivanje / sakrivanje vodova na osnovu materijala

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            if (checkBox.Content.ToString() == "STEEL")
            {
                foreach (var geoModel in steelVodovi)
                {
                    model3dGroup.Children.Remove(geoModel);
                }
            }
            else if (checkBox.Content.ToString() == "COPPER")
            {
                foreach (var geoModel in copperVodovi)
                {
                    model3dGroup.Children.Remove(geoModel);
                }
            }
            else if (checkBox.Content.ToString() == "ACSR")
            {
                foreach (var geoModel in acsrVodovi)
                {
                    model3dGroup.Children.Remove(geoModel);
                }
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;

            if (checkBox.Content.ToString() == "STEEL")
            {
                foreach (var geoModel in steelVodovi)
                {
                    model3dGroup.Children.Add(geoModel);
                }
            }
            else if (checkBox.Content.ToString() == "COPPER")
            {
                foreach (var geoModel in copperVodovi)
                {
                    model3dGroup.Children.Add(geoModel);
                }
            }
            else if (checkBox.Content.ToString() == "ACSR")
            {
                foreach (var geoModel in acsrVodovi)
                {
                    model3dGroup.Children.Add(geoModel);
                }
            }
        }

        #endregion

    }
}
