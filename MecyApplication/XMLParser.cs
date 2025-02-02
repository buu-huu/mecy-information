﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace MecyApplication
{
    /// <summary>
    /// Class for XML parsing.
    /// </summary>
    static class XMLParser
    {
        /// <summary>
        /// Parses all mesocyclones in a directory.
        /// </summary>
        /// <param name="path">Path to XML files</param>
        /// <returns>List of opendata elements</returns>
        public static List<OpenDataElement> ParseAllMesos(string path)
        {
            List<OpenDataElement> openDataElements = new List<OpenDataElement>();
            try
            {
                foreach (var mesoFile in Directory.GetFiles(path))
                {
                    var openDataElement = ParseMesoFile(mesoFile);
                    if (!(openDataElement.Time == DateTime.Parse("01.01.001 00:00"))) // This happens, when an error occurs while parsing the XML File
                    {
                        openDataElements.Add(openDataElement);
                    }
                }
                openDataElements = openDataElements.OrderBy(x => x.Time).Reverse().ToList();
            }
            catch
            {
                // TODO: logging (directory not exists)...
            }
            return openDataElements;
        }

        /// <summary>
        /// Parses a XML file.
        /// </summary>
        /// <param name="path">Path to a XML file.</param>
        /// <returns>Opendata element</returns>
        public static OpenDataElement ParseMesoFile(string path)
        {
            try
            {
                string fileNameDate = Path.GetFileName(path).Replace("meso_", "").Replace(".xml", "");
                DateTime timeStamp = DateTime.ParseExact(fileNameDate, "yyyyMMdd_HHmm", CultureInfo.InvariantCulture);

                List<RadarStation> stations = new List<RadarStation>();
                List<Mesocyclone> parsedMesos = new List<Mesocyclone>();

                XmlDocument xdoc = new XmlDocument();
                xdoc.Load(path);

                foreach (XmlNode node in xdoc.DocumentElement.ChildNodes)
                {
                    if (node.Name == "radar-stations")
                    {
                        stations = RadarStation.ParseStationsFromString(node.InnerText);
                    }
                    if (node.Name == "event")
                    {
                        Mesocyclone meso = new Mesocyclone();
                        meso.Id = Convert.ToInt32(node.Attributes["ID"].Value);

                        foreach (XmlNode childNode in node.ChildNodes)
                        {
                            switch (childNode.Name)
                            {
                                case "time":
                                    meso.Time = DateTime.Parse(childNode.InnerText);
                                    break;
                                case "location":
                                    foreach (XmlNode ellipseNode in childNode.ChildNodes[0].ChildNodes[0])
                                    {
                                        if (ellipseNode.Name == "moving-point")
                                        {
                                            foreach (XmlNode movingPointNode in ellipseNode.ChildNodes)
                                            {
                                                if (movingPointNode.Name == "latitude")
                                                {
                                                    meso.Latitude = Convert.ToDouble(movingPointNode.InnerText, CultureInfo.InvariantCulture);
                                                }
                                                if (movingPointNode.Name == "longitude")
                                                {
                                                    meso.Longitude = Convert.ToDouble(movingPointNode.InnerText, CultureInfo.InvariantCulture);
                                                }
                                                /*
                                                if (movingPointNode.Name == "polar_motion")
                                                {
                                                    meso.PolarMotion = Convert.ToDouble(movingPointNode.ChildNodes[0].InnerText, CultureInfo.InvariantCulture);
                                                }
                                                */
                                            }
                                        }

                                        if (ellipseNode.Name == "major_axis")
                                        {
                                            meso.MajorAxis = Convert.ToDouble(ellipseNode.InnerText, CultureInfo.InvariantCulture);
                                        }
                                        if (ellipseNode.Name == "minor_axis")
                                        {
                                            meso.MinorAxis = Convert.ToDouble(ellipseNode.InnerText, CultureInfo.InvariantCulture);
                                        }
                                        /*
                                        if (ellipseNode.Name == "orientation")
                                        {
                                            meso.Orientation = Convert.ToInt32(ellipseNode.InnerText, CultureInfo.InvariantCulture);
                                        }
                                        */

                                    }
                                    break;
                                case "nowcast-parameters":
                                    foreach (XmlNode paramNode in childNode.ChildNodes)
                                    {
                                        switch (paramNode.Name)
                                        {
                                            case "mesocyclone_shear_mean":
                                                meso.ShearMean = Convert.ToDouble(paramNode.InnerText, CultureInfo.InvariantCulture);
                                                break;
                                            case "mesocyclone_shear_max":
                                                meso.ShearMax = Convert.ToDouble(paramNode.InnerText, CultureInfo.InvariantCulture);
                                                break;
                                            case "mesocyclone_momentum_mean":
                                                meso.MomentumMean = Convert.ToDouble(paramNode.InnerText, CultureInfo.InvariantCulture);
                                                break;
                                            case "mesocyclone_momentum_max":
                                                meso.MomentumMax = Convert.ToDouble(paramNode.InnerText, CultureInfo.InvariantCulture);
                                                break;
                                            case "mesocyclone_diameter":
                                                meso.Diameter = Convert.ToDouble(paramNode.InnerText, CultureInfo.InvariantCulture);
                                                break;
                                            case "mesocyclone_diameter_equivalent":
                                                meso.DiameterEquivalent = Convert.ToDouble(paramNode.InnerText, CultureInfo.InvariantCulture);
                                                break;
                                            case "mesocyclone_top":
                                                meso.Top = Convert.ToDouble(paramNode.InnerText, CultureInfo.InvariantCulture);
                                                break;
                                            case "mesocyclone_base":
                                                meso.MesoBase = Convert.ToDouble(paramNode.InnerText, CultureInfo.InvariantCulture);
                                                break;
                                            case "mesocyclone_echotop":
                                                meso.Echotop = Convert.ToDouble(paramNode.InnerText, CultureInfo.InvariantCulture);
                                                break;
                                            case "mesocyclone_vil":
                                                meso.Vil = Convert.ToDouble(paramNode.InnerText, CultureInfo.InvariantCulture);
                                                break;
                                            case "mesocyclone_shear_vectors":
                                                meso.ShearVectors = Convert.ToInt32(paramNode.InnerText);
                                                break;
                                            case "mesocyclone_shear_features":
                                                meso.ShearFeatures = Convert.ToInt32(paramNode.InnerText);
                                                break;
                                            case "elevations":
                                                List<Elevation> elevations = new List<Elevation>();
                                                foreach (XmlNode elevationNode in paramNode.ChildNodes)
                                                {
                                                    Elevation elevation = new Elevation();
                                                    elevation.RadarSite = elevationNode.Attributes["site"].Value.ToUpper();
                                                    elevation.Elevations = Elevation.ParseElevationsFromString(elevationNode.InnerText);
                                                    elevations.Add(elevation);
                                                }
                                                meso.Elevations = elevations;
                                                break;
                                            case "mean_dbz":
                                                meso.MeanDBZ = Convert.ToDouble(paramNode.InnerText, CultureInfo.InvariantCulture);
                                                break;
                                            case "max_dbz":
                                                meso.MaxDBZ = Convert.ToDouble(paramNode.InnerText, CultureInfo.InvariantCulture);
                                                break;
                                            case "mesocyclone_velocity_max":
                                                meso.VelocityMax = Convert.ToDouble(paramNode.InnerText, CultureInfo.InvariantCulture);
                                                break;
                                            case "mesocyclone_velocity_rotational_max":
                                                meso.VelocityRotationalMax = Convert.ToDouble(paramNode.InnerText, CultureInfo.InvariantCulture);
                                                break;
                                            case "mesocyclone_velocity_rotational_mean":
                                                meso.VelocityRotationalMean = Convert.ToDouble(paramNode.InnerText, CultureInfo.InvariantCulture);
                                                break;
                                            case "mesocyclone_velocity_rotational_max_closest_to_ground":
                                                meso.VelocityRotationalMaxClosestToGround = Convert.ToDouble(paramNode.InnerText, CultureInfo.InvariantCulture);
                                                break;
                                            case "meso_intensity":
                                                meso.Intensity = Convert.ToInt32(paramNode.InnerText);
                                                break;
                                        }
                                    }
                                    break;
                            }
                        }
                        parsedMesos.Add(meso);
                    }
                }
                return new OpenDataElement(timeStamp, stations, parsedMesos);
            }
            catch
            {
                return null;
            }
        }

        public static OpenDataElement ParseMesoFileLatest(string path)
        {
            try
            {
                DateTime timeStamp = DateTime.UtcNow;
                List<RadarStation> stations = new List<RadarStation>();
                List<Mesocyclone> parsedMesos = new List<Mesocyclone>();

                XmlDocument xdoc = new XmlDocument();
                xdoc.Load(path);

                foreach (XmlNode node in xdoc.DocumentElement.ChildNodes)
                {
                    if (node.Name == "radar-stations")
                    {
                        stations = RadarStation.ParseStationsFromString(node.InnerText);
                    }
                    if (node.Name == "event")
                    {
                        Mesocyclone meso = new Mesocyclone();
                        meso.Id = Convert.ToInt32(node.Attributes["ID"].Value);

                        foreach (XmlNode childNode in node.ChildNodes)
                        {
                            switch (childNode.Name)
                            {
                                case "time":
                                    meso.Time = DateTime.Parse(childNode.InnerText);
                                    break;
                                case "location":
                                    foreach (XmlNode ellipseNode in childNode.ChildNodes[0].ChildNodes[0])
                                    {
                                        if (ellipseNode.Name == "moving-point")
                                        {
                                            foreach (XmlNode movingPointNode in ellipseNode.ChildNodes)
                                            {
                                                if (movingPointNode.Name == "latitude")
                                                {
                                                    meso.Latitude = Convert.ToDouble(movingPointNode.InnerText, CultureInfo.InvariantCulture);
                                                }
                                                if (movingPointNode.Name == "longitude")
                                                {
                                                    meso.Longitude = Convert.ToDouble(movingPointNode.InnerText, CultureInfo.InvariantCulture);
                                                }
                                                /*
                                                if (movingPointNode.Name == "polar_motion")
                                                {
                                                    meso.PolarMotion = Convert.ToDouble(movingPointNode.ChildNodes[0].InnerText, CultureInfo.InvariantCulture);
                                                }
                                                */
                                            }
                                        }

                                        if (ellipseNode.Name == "major_axis")
                                        {
                                            meso.MajorAxis = Convert.ToDouble(ellipseNode.InnerText, CultureInfo.InvariantCulture);
                                        }
                                        if (ellipseNode.Name == "minor_axis")
                                        {
                                            meso.MinorAxis = Convert.ToDouble(ellipseNode.InnerText, CultureInfo.InvariantCulture);
                                        }
                                        /*
                                        if (ellipseNode.Name == "orientation")
                                        {
                                            meso.Orientation = Convert.ToInt32(ellipseNode.InnerText, CultureInfo.InvariantCulture);
                                        }
                                        */

                                    }
                                    break;
                                case "nowcast-parameters":
                                    foreach (XmlNode paramNode in childNode.ChildNodes)
                                    {
                                        switch (paramNode.Name)
                                        {
                                            case "mesocyclone_shear_mean":
                                                meso.ShearMean = Convert.ToDouble(paramNode.InnerText, CultureInfo.InvariantCulture);
                                                break;
                                            case "mesocyclone_shear_max":
                                                meso.ShearMax = Convert.ToDouble(paramNode.InnerText, CultureInfo.InvariantCulture);
                                                break;
                                            case "mesocyclone_momentum_mean":
                                                meso.MomentumMean = Convert.ToDouble(paramNode.InnerText, CultureInfo.InvariantCulture);
                                                break;
                                            case "mesocyclone_momentum_max":
                                                meso.MomentumMax = Convert.ToDouble(paramNode.InnerText, CultureInfo.InvariantCulture);
                                                break;
                                            case "mesocyclone_diameter":
                                                meso.Diameter = Convert.ToDouble(paramNode.InnerText, CultureInfo.InvariantCulture);
                                                break;
                                            case "mesocyclone_diameter_equivalent":
                                                meso.DiameterEquivalent = Convert.ToDouble(paramNode.InnerText, CultureInfo.InvariantCulture);
                                                break;
                                            case "mesocyclone_top":
                                                meso.Top = Convert.ToDouble(paramNode.InnerText, CultureInfo.InvariantCulture);
                                                break;
                                            case "mesocyclone_base":
                                                meso.MesoBase = Convert.ToDouble(paramNode.InnerText, CultureInfo.InvariantCulture);
                                                break;
                                            case "mesocyclone_echotop":
                                                meso.Echotop = Convert.ToDouble(paramNode.InnerText, CultureInfo.InvariantCulture);
                                                break;
                                            case "mesocyclone_vil":
                                                meso.Vil = Convert.ToDouble(paramNode.InnerText, CultureInfo.InvariantCulture);
                                                break;
                                            case "mesocyclone_shear_vectors":
                                                meso.ShearVectors = Convert.ToInt32(paramNode.InnerText);
                                                break;
                                            case "mesocyclone_shear_features":
                                                meso.ShearFeatures = Convert.ToInt32(paramNode.InnerText);
                                                break;
                                            case "elevations":
                                                List<Elevation> elevations = new List<Elevation>();
                                                foreach (XmlNode elevationNode in paramNode.ChildNodes)
                                                {
                                                    Elevation elevation = new Elevation();
                                                    elevation.RadarSite = elevationNode.Attributes["site"].Value.ToUpper();
                                                    elevation.Elevations = Elevation.ParseElevationsFromString(elevationNode.InnerText);
                                                    elevations.Add(elevation);
                                                }
                                                meso.Elevations = elevations;
                                                break;
                                            case "mean_dbz":
                                                meso.MeanDBZ = Convert.ToDouble(paramNode.InnerText, CultureInfo.InvariantCulture);
                                                break;
                                            case "max_dbz":
                                                meso.MaxDBZ = Convert.ToDouble(paramNode.InnerText, CultureInfo.InvariantCulture);
                                                break;
                                            case "mesocyclone_velocity_max":
                                                meso.VelocityMax = Convert.ToDouble(paramNode.InnerText, CultureInfo.InvariantCulture);
                                                break;
                                            case "mesocyclone_velocity_rotational_max":
                                                meso.VelocityRotationalMax = Convert.ToDouble(paramNode.InnerText, CultureInfo.InvariantCulture);
                                                break;
                                            case "mesocyclone_velocity_rotational_mean":
                                                meso.VelocityRotationalMean = Convert.ToDouble(paramNode.InnerText, CultureInfo.InvariantCulture);
                                                break;
                                            case "mesocyclone_velocity_rotational_max_closest_to_ground":
                                                meso.VelocityRotationalMaxClosestToGround = Convert.ToDouble(paramNode.InnerText, CultureInfo.InvariantCulture);
                                                break;
                                            case "meso_intensity":
                                                meso.Intensity = Convert.ToInt32(paramNode.InnerText);
                                                break;
                                        }
                                    }
                                    break;
                            }
                        }
                        parsedMesos.Add(meso);
                    }
                }
                return new OpenDataElement(timeStamp, stations, parsedMesos);
            }
            catch
            {
                return null;
            }
        }
    }
}
