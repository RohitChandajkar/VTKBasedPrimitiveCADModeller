using Kitware.VTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{
    public class Shapes
    {
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


        public vtkPolyData CreateLine(double[] start, double[] end)
        {
            var points = vtkPoints.New();
            points.InsertNextPoint(start[0], start[1], 0); // Start point in world coordinates
            points.InsertNextPoint(end[0], end[1], 0);     // End point in world coordinates
            Console.WriteLine($"Drawing Line at ({points.InsertNextPoint(start[0], start[1], 0)}, {points.InsertNextPoint(end[0], end[1], 0)}) ");

            var polyData = vtkPolyData.New();
            polyData.SetPoints(points);

            var cellArray = vtkCellArray.New();
            cellArray.InsertNextCell(2); // Line with 2 points
            cellArray.InsertCellPoint(0);
            cellArray.InsertCellPoint(1);

            polyData.SetLines(cellArray);

            return polyData;
        }




        public vtkPolyData CreatePoint(double[] coordinates)
        {
            var points = vtkPoints.New();
            points.InsertNextPoint(coordinates[0], coordinates[1], 0); // Add the point's coordinates

            var polyData = vtkPolyData.New();
            polyData.SetPoints(points);
            Console.WriteLine($"Drawing Point at ({points}");
            return polyData;
        }


        public vtkPolyData CreateArc(double startAngle, double endAngle, double radius, int numSegments = 50)
        {
            var points = vtkPoints.New();

            double angleIncrement = (endAngle - startAngle) / numSegments;

            for (int i = 0; i <= numSegments; i++)
            {
                double angle = Math.PI * (startAngle + i * angleIncrement) / 180; // Convert to radians
                double x = radius * Math.Cos(angle);
                double y = radius * Math.Sin(angle);

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
            Console.WriteLine($"Drawing Arc at ({startAngle}, {endAngle})");
            return polyData;
        }

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

            Console.WriteLine($"Drawing Ellipse at ({semiMajorAxis}, {semiMinorAxis}, and {centerX}, {centerY})");

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
