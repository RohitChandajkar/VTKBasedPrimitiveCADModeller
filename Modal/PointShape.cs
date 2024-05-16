using Kitware.VTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1.Modal
{
    public class PointShape
    {
        public double X { get; set; }
        public double Y { get; set; }

        //To create Point 
        public vtkPolyData CreatePoint(double[] coordinates)
        {
            // Create a vtkPoints object to store the point
            var points = vtkPoints.New();
            points.InsertNextPoint(coordinates[0], coordinates[1], 0); // Assuming z-coordinate is 0

            // Create a vtkPolyData object
            var polyData = vtkPolyData.New();
            polyData.SetPoints(points);

            return polyData;
        }
    }
}
