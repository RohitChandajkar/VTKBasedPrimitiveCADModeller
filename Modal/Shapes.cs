using Kitware.VTK;
using System;

namespace WindowsFormsApp1
{
    public class Shapes
    {

        //To create circle
        public vtkPolyData CreateCircle(double centerX, double centerY, double radius, int numSegments = 50)
        {
            var points = vtkPoints.New();
            double angleIncrement = 2 * Math.PI / numSegments;

            // Create the points along the circle's perimeter
            for (int i = 0; i < numSegments; i++)
            {
                double angle = i * angleIncrement;
                double x = centerX + radius * Math.Cos(angle);
                double y = centerY + radius * Math.Sin(angle);
                points.InsertNextPoint(x, y, 0);
            }

            // Close the circle by adding the first point again
            double firstAngle = 0;
            points.InsertNextPoint(centerX + radius * Math.Cos(firstAngle), centerY + radius * Math.Sin(firstAngle), 0);

            var polyData = vtkPolyData.New();
            polyData.SetPoints(points);

            var cellArray = vtkCellArray.New();
            cellArray.InsertNextCell(numSegments + 1);

            for (int i = 0; i <= numSegments; i++)
            {
                cellArray.InsertCellPoint(i);
            }

            polyData.SetLines(cellArray);

            return polyData;
        }

        // To create Line 
        public vtkPolyData CreateLine(double[] start, double[] end)
        {
            var points = vtkPoints.New();
            points.InsertNextPoint(start[0], start[1], 0); // Start point in world coordinates
            points.InsertNextPoint(end[0], end[1], 0);     // End point in world coordinates

            var polyData = vtkPolyData.New();
            polyData.SetPoints(points);

            var cellArray = vtkCellArray.New();
            cellArray.InsertNextCell(2); // Line with 2 points
            cellArray.InsertCellPoint(0);
            cellArray.InsertCellPoint(1);

            polyData.SetLines(cellArray);

            return polyData;
        }

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


        //To Create Arc
        public vtkPolyData CreateArc(double[] worldStartPos, double[] worldEndPos)
        {
            var points = vtkPoints.New();

            // Calculate the center and radius of the circle that the arc belongs to
            double centerX = (worldStartPos[0] + worldEndPos[0]) / 2;
            double centerY = (worldStartPos[1] + worldEndPos[1]) / 2;
            double radius = Math.Sqrt(Math.Pow(worldEndPos[0] - worldStartPos[0], 2) + Math.Pow(worldEndPos[1] - worldStartPos[1], 2)) / 2;

            // Calculate the angles of the start and end points relative to the center
            double startAngle = Math.Atan2(worldStartPos[1] - centerY, worldStartPos[0] - centerX) * 180 / Math.PI;
            double endAngle = Math.Atan2(worldEndPos[1] - centerY, worldEndPos[0] - centerX) * 180 / Math.PI;

            // Ensure end angle is greater than start angle (clockwise direction)
            if (endAngle < startAngle)
            {
                endAngle += 360;
            }

            // Number of segments to approximate the arc
            int numSegments = 50;

            double angleIncrement = (endAngle - startAngle) / numSegments;

            for (int i = 0; i <= numSegments; i++)
            {
                double angle = Math.PI * (startAngle + i * angleIncrement) / 180; // Convert to radians
                double x = centerX + radius * Math.Cos(angle);
                double y = centerY + radius * Math.Sin(angle);

                points.InsertNextPoint(x, y, 0); // Add the point to the collection
            }

            var polyData = vtkPolyData.New();
            polyData.SetPoints(points);

            var cellArray = vtkCellArray.New();
            cellArray.InsertNextCell(numSegments + 1); // Create a cell for the arc
            for (int i = 0; i <= numSegments; i++)
            {
                cellArray.InsertCellPoint(i); // Add each point to the cell array
            }

            polyData.SetLines(cellArray); // Add the lines to the polyData
            return polyData;
        }

        //To create Ellipse 
        public vtkPolyData CreateEllipse(double[] worldStartPos, double[] worldEndPos, int numSegments = 100)
        {
            // Calculate the semi-major and semi-minor axes and the center of the ellipse
            double semiMajorAxis = Math.Abs(worldEndPos[0] - worldStartPos[0]) / 2;
            double semiMinorAxis = Math.Abs(worldEndPos[1] - worldStartPos[1]) / 2;
            double centerX = (worldStartPos[0] + worldEndPos[0]) / 2;
            double centerY = (worldStartPos[1] + worldEndPos[1]) / 2;

            var points = vtkPoints.New();
            double angleIncrement = 2 * Math.PI / numSegments;

            // Generate the points for the ellipse
            for (int i = 0; i < numSegments; i++)
            {
                double angle = i * angleIncrement; // Angle in radians
                double x = semiMajorAxis * Math.Cos(angle) + centerX;
                double y = semiMinorAxis * Math.Sin(angle) + centerY;

                points.InsertNextPoint(x, y, 0); // Add point to vtkPoints
            }

            var polyData = vtkPolyData.New();
            polyData.SetPoints(points);

            var cellArray = vtkCellArray.New();
            cellArray.InsertNextCell(numSegments); // Create a cell for the ellipse
            for (int i = 0; i < numSegments; i++)
            {
                cellArray.InsertCellPoint(i); // Add each point to the cell array
            }

            polyData.SetLines(cellArray);

            return polyData;
        }

    }
}




