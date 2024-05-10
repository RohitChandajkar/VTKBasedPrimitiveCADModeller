using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Shapes;
using Kitware.VTK;  // Required for VTK components



namespace WindowsFormsApp1
{
    public partial class VTKBasedCADModelllerUI : Form
    {
        private bool isSketchMode = false;
        private vtkRenderer renderer;
        private vtkActor selectedActor; // This is where we declare `selectedActor`
  
        private bool isDrawingCircle = false;  // Whether drawing is active
        private bool isDrawingLine = false;    // Whether drawing a line
        private bool isDrawingPoint = false;   // Whether drawing a point
        private bool isDrawingArc = false;     // Whether drawing an arc
        private bool isDrawingEllipse = false; // Whether drawing an ellipse

        private double startX, startY;  // Start coordinates for drawing
        private TextBox outputTextBox;  // Declare the TextBox at the class level
        private vtkRenderWindowInteractor interactor1;

        public VTKBasedCADModelllerUI()
        {
            InitializeComponent();

            // Redirect console output to the TextBox
            var writer = new TextBoxWriter(outputTextBox);
            Console.SetOut(writer);

            this.Load += Form1_Load; // Bind the Load event to a method
            
            Console.WriteLine("Console redirected to TextBox!");  // Test output
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            SetupRenderWindowControl(); // Call setup during form load
        }

        //To render window controller 
        private void RenderWindowControl1_Load(object sender, EventArgs e)
        {
            this.renderer = vtkRenderer.New();
            this.renderWindowControl1.RenderWindow.AddRenderer(this.renderer);
            this.renderer.ResetCamera();

            // Set up a picker to handle actor selection with mouse clicks
            var picker = vtkPropPicker.New(); // Use vtkPropPicker to pick props (actors)
            this.renderWindowControl1.RenderWindow.GetInteractor().SetPicker(picker);

            // Set the background color
            this.renderer.SetBackground(0.678, 0.847, 0.902); // Light blue
 
            //used for delete selected elements
            this.renderWindowControl1.RenderWindow.GetInteractor().LeftButtonPressEvt += (s, args) =>
            {
                picker.Pick(this.renderWindowControl1.RenderWindow.GetInteractor().GetEventPosition()[0],
                            this.renderWindowControl1.RenderWindow.GetInteractor().GetEventPosition()[1],
                            0, // Assuming a 2D scene
                            this.renderer);

                var pickedActor = picker.GetActor(); // Get the picked actor
                if (pickedActor != null)
                {
                    // Deselect the previous actor (resetting color)
                    if (selectedActor != null)
                    {
                        selectedActor.GetProperty().SetColor(1.0, 1.0, 1.0); // Reset to white
                    }

                    // Set the new selected actor and change its color to indicate selection
                    selectedActor = pickedActor;
                    selectedActor.GetProperty().SetColor(1.0, 0.0, 0.0); // Highlight in red
                }


                this.renderWindowControl1.RenderWindow.Render(); // Re-render to reflect color change
            };
        }


        // To allow user to drag and down primitive shapes 
        private void SetupRenderWindowControl()
        {
          
            // Draw the initial plane for XY
            DrawPlane("XY");


             interactor1 = renderWindowControl1.RenderWindow.GetInteractor();

            if (interactor1 == null) // Ensure it's not null
            {
                Console.WriteLine("Error: Interactor is not initialized correctly.");
                return; // Prevent further execution if interactor is missing
            }
            else
            {
                Console.WriteLine("Interactor1 is  initialized correctly.");
            }

            // Set the background color
            this.renderer.SetBackground(0.678, 0.847, 0.902); // Light blue
            var interactor = renderWindowControl1.RenderWindow.GetInteractor();

            // Capture the start coordinates when left mouse button is pressed
            interactor.LeftButtonPressEvt += (s, args) =>
            {
                var eventPos = interactor.GetEventPosition();
                startX = eventPos[0];
                startY = eventPos[1];
            };

            // Configure the interaction based on the sketch mode
           // ConfigureInteraction();

            // Handle drawing when the left mouse button is released
            interactor.LeftButtonReleaseEvt += (s, args) =>
            {
                if (isSketchMode)
                {
                    var eventPos = interactor.GetEventPosition();
                    double endX = eventPos[0];
                    double endY = eventPos[1];

                    // Convert screen coordinates to world coordinates
                    var worldStart = new vtkCoordinate();
                    worldStart.SetCoordinateSystemToDisplay();
                    worldStart.SetValue(startX, startY, 0);

                    var worldEnd = new vtkCoordinate();
                    worldEnd.SetCoordinateSystemToDisplay();
                    worldEnd.SetValue(endX, endY, 0);

                    var worldStartPos = worldStart.GetComputedWorldValue(renderer);
                    var worldEndPos = worldEnd.GetComputedWorldValue(renderer);

                    // Calculate the center and radius for the circle
                    double radius = Math.Sqrt(Math.Pow(worldEndPos[0] - worldStartPos[0], 2) + Math.Pow(worldEndPos[1] - worldStartPos[1], 2));
                    double centerX = (worldStartPos[0] + worldEndPos[0]) / 2;
                    double centerY = (worldStartPos[1] + worldEndPos[1]) / 2;

                    // Draw the appropriate shape based on the active flag
                    if (isDrawingCircle)
                    {
                        Console.WriteLine("right click to draw circle..");
                        DrawCircle(centerX, centerY, radius);  // Use the computed values
                        isDrawingCircle = false;  // Reset the flag
                    }
                    else if (isDrawingLine)
                    {
                        Console.WriteLine("right click to draw Line..");
                        DrawLine(worldStartPos, worldEndPos);  // Use the computed world coordinates
                        isDrawingLine = false;  // Reset the flag
                    }
                    else if (isDrawingPoint)
                    {
                        Console.WriteLine("right click to draw Point..");
                        DrawPoint(worldStartPos);  // Use the start position
                        isDrawingPoint = false;  // Reset the flag
                    }
                    else if (isDrawingArc)
                    {
                        Console.WriteLine("right click to draw Arc..");
                        DrawArc();
                        //DrawArc(worldStartPos, worldEndPos);  // Use the computed world coordinates
                        isDrawingArc = false;  // Reset the flag
                    }
                    else if (isDrawingEllipse)
                    {
                        Console.WriteLine("right click to draw Ellipse..");
                        DrawEllipse(worldStartPos, worldEndPos);  // Use the computed world coordinates
                        isDrawingEllipse = false;  // Reset the flag
                    }

                    renderWindowControl1.RenderWindow.Render();  // Re-render the scene to apply changes
                }

            };

            //// Disable zooming and rotation in sketch mode
            //interactor1.StartInteractionEvt += (s, args) =>
            //{
            //    if (!isSketchMode) // If not in sketch mode
            //    {
            //        Console.WriteLine("code..");
            //        var camera = renderer.GetActiveCamera();
            //        camera.SetParallelScale(2); // Reset zoom for orthographic camera
            //    }
            //};

            try
            {
                if (interactor == null)
                {
                    Console.WriteLine("Error: Interactor is not initialized.");
                    return;
                }

                if (isSketchMode)
                {
                    interactor.SetInteractorStyle(vtkInteractorStyleTrackballActor.New()); // Disable zoom/rotate
                }
                else
                {
                    interactor.SetInteractorStyle(vtkInteractorStyleTrackballCamera.New()); // Enable full interactions
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in ConfigureInteraction: {ex.Message}");
                // Additional error handling
            }

            // Initial render to set up the scene
            renderWindowControl1.RenderWindow.Render();
        }



        //private void ConfigureInteraction()
        //{
        //    try
        //    {
        //        if (interactor1 == null)
        //        {
        //            Console.WriteLine("Error: Interactor is not initialized.");
        //            return;
        //        }

        //        if (isSketchMode)
        //        {
        //            interactor1.SetInteractorStyle(vtkInteractorStyleTrackballActor.New()); // Disable zoom/rotate
        //        }
        //        else
        //        {
        //            interactor1.SetInteractorStyle(vtkInteractorStyleTrackballCamera.New()); // Enable full interactions
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Exception in ConfigureInteraction: {ex.Message}");
        //        // Additional error handling
        //    }
        //}


        private void SketchButton_Click(object sender, EventArgs e)
        {
            //isSketchMode = !isSketchMode; // Toggle sketch mode

            isSketchMode = true; // Activate sketch mode
            EnableSketchButtons(true);
            Console.WriteLine("Sketch button click..");
            //EnablePlaneButtons(true); // Activate XY, YZ, ZX buttons
        }

        private void ViewButton_Click(object sender, EventArgs e)
        {
            isSketchMode = false;
            isSketchMode = false; // Deactivate sketch mode
            //EnablePlaneButtons(false); // Deactivate XY, YZ, ZX buttons
            EnableSketchButtons(false);
            Console.WriteLine("View button click..");
        }

        // Enable or disable drawing buttons based on the sketch mode
        private void EnableSketchButtons(bool enable)
        {
            this.circleButton.Enabled = enable;
            this.lineButton.Enabled = enable;
            this.pointButton.Enabled = enable;
            this.arcButton.Enabled = enable;
            this.ellipseButton.Enabled = enable;
            this.xyButton.Enabled = enable; 
        }

        private void CircleButton_Click(object sender, EventArgs e)
        {
            if (isSketchMode)
            {
                isDrawingCircle = true;  // Set flag to enable drawing circles
                Console.WriteLine("Circle drawing enabled..");
            }
        }

        private void LineButton_Click(object sender, EventArgs e)
        {
            if (isSketchMode)
            {
                isDrawingLine = true;  // Set flag to enable line drawing
                Console.WriteLine("Line drawing enabled..");
            }
        }

        private void PointButton_Click(object sender, EventArgs e)
        {
            if (isSketchMode)
            {
                isDrawingPoint = true;  // Set flag to enable point drawing
                Console.WriteLine("Point drawing enabled..");
            }
        }

        private void ArcButton_Click(object sender, EventArgs e)
        {
            if (isSketchMode)
            {
                isDrawingArc = true;  // Set flag to enable arc drawing
                Console.WriteLine("Arc drawing enabled..");
            }
        }

        private void EllipseButton_Click(object sender, EventArgs e)
        {
            if (isSketchMode)
            {
                isDrawingEllipse = true;  // Set flag to enable ellipse drawing
                Console.WriteLine("Ellipse drawing enabled..");
            }
        }

        private void DrawCircle(double centerX, double centerY, double radius)
        {
            Console.WriteLine($"Drawing circle at ({centerX}, {centerY}) with radius {radius}");
            var Shape = new Shapes();
            var polyData = Shape.CreateCircle(centerX, centerY, radius);

            // Now that we have the polyData, let's set up the mapper, actor, and renderer
            var mapper = vtkPolyDataMapper.New();
            mapper.SetInputConnection(polyData.GetProducerPort());

            var actor = vtkActor.New();
            actor.SetMapper(mapper);
            actor.GetProperty().SetColor(0, 0, 0); // Black color

            renderer.AddActor(actor);
            renderer.ResetCamera(); // Ensure the circle is visible
            renderWindowControl1.RenderWindow.Render(); // Render the scene
        }

       private void DrawLine(double[] worldStartPos, double[] worldEndPos)
        {

            if (isSketchMode)
            {
                var lineShape = new Shapes(); // Instantiate the model
                var polyData = lineShape.CreateLine(worldStartPos, worldEndPos); // Get the line's polyData

                // Create the mapper, actor, and add to the renderer
                var mapper = vtkPolyDataMapper.New();
                mapper.SetInputConnection(polyData.GetProducerPort());

                var actor = vtkActor.New();
                actor.SetMapper(mapper);
                actor.GetProperty().SetColor(0, 0, 0); // Black color

                renderer.AddActor(actor); // Add the line to the renderer
                renderer.ResetCamera(); // Ensure the line is visible

                renderWindowControl1.RenderWindow.Render(); // Render the updated scene
            }
        }
  
       private void DrawPoint(double[] worldStartPos)
        {
            if (isSketchMode) // Ensure sketch mode is active
            {
                var pointShape = new Shapes(); // Instantiate the model
                var polyData = pointShape.CreatePoint(worldStartPos); // Create the point's polyData

                var mapper = vtkPolyDataMapper.New();
                mapper.SetInputConnection(polyData.GetProducerPort());

                var actor = vtkActor.New();
                actor.SetMapper(mapper);
                actor.GetProperty().SetPointSize(100); // Set the point size
                actor.GetProperty().SetColor(0, 0, 0); // Set the point color to black

                renderer.AddActor(actor); // Add the point to the renderer
                renderer.ResetCamera(); // Ensure visibility
                renderWindowControl1.RenderWindow.Render(); // Re-render the scene
            }
        }
       
        private void DrawArc()
        {
            if (isSketchMode)
            {
                var arcShape = new Shapes(); // Instantiate the model
                var polyData = arcShape.CreateArc(0, 180, 5); // Create the arc with specified parameters

                // Create a vtkPolyDataMapper to map the polyData
                var mapper = vtkPolyDataMapper.New();
                mapper.SetInputConnection(polyData.GetProducerPort());

                // Create a vtkActor to represent the arc in the scene
                var actor = vtkActor.New();
                actor.SetMapper(mapper);
                actor.GetProperty().SetColor(0, 0, 0); // Set arc color to black

                // Add the actor to the renderer
                renderer.AddActor(actor);
                renderer.ResetCamera(); // Ensure the arc is in view
                renderWindowControl1.RenderWindow.Render(); // Render the updated scene
            }
        }

        private void DrawEllipse(double[] worldStartPos, double[] worldEndPos)
        {
            if (isSketchMode)  // Check if sketch mode is active
            {
                var ellipseShape = new Shapes(); // Instantiate the model
                var polyData = ellipseShape.CreateEllipse(worldStartPos, worldEndPos); // Get the polyData for the ellipse

                var mapper = vtkPolyDataMapper.New();
                mapper.SetInputConnection(polyData.GetProducerPort());

                var actor = vtkActor.New();
                actor.SetMapper(mapper);
                actor.GetProperty().SetColor(0, 0, 0); // Set the ellipse's color to black

                renderer.AddActor(actor); // Add the ellipse to the renderer
                renderer.ResetCamera(); // Ensure the ellipse is visible
                renderWindowControl1.RenderWindow.Render(); // Re-render the s
            }
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            if (selectedActor != null) // If an actor is selected
            {
                this.renderer.RemoveActor(selectedActor); // Remove the actor
                selectedActor = null; // Reset the selected actor
                this.renderer.ResetCamera(); // Reset the camera
                this.renderWindowControl1.RenderWindow.Render(); // Re-render the scene
            }
        }

        private void RemoveAllActors()
        {
            // Get all the actors in the renderer
            var actorsCollection = renderer.GetActors();

            // Iterate through all actors and remove them
            var actors = new List<vtkActor>(); // Use a list to store actors safely

            // Retrieve each actor from the collection and cast to vtkActor or vtkProp
            for (int i = 0; i < actorsCollection.GetNumberOfItems(); i++)
            {
                // Ensure correct type by casting
                var actor = actorsCollection.GetItemAsObject(i) as vtkProp;

                if (actor != null)  // Confirm casting was successful
                {
                    actors.Add((vtkActor)actor);  // Add actor to the list
                }
            }

            // Now safely remove all actors from the renderer
            foreach (var actor in actors)
            {
                renderer.RemoveActor(actor);
            }
        }

        private void DrawPlane(string plane)
        {
            // Create a simple grid to represent the plane
            var planeSource = vtkPlaneSource.New();
            planeSource.SetXResolution(10);  // Set resolution for the grid
            planeSource.SetYResolution(10);

            var mapper = vtkPolyDataMapper.New();
            mapper.SetInputConnection(planeSource.GetOutputPort());

            var actor = vtkActor.New();
            actor.SetMapper(mapper);
            actor.GetProperty().SetRepresentationToWireframe();  // Show as wireframe

            // Adjust the orientation and position based on the selected plane
            switch (plane)
            {
                case "XY":
                    actor.SetOrientation(0, 0, 0);  // No rotation for XY
                    actor.SetPosition(0, 0, 0);     // Position at origin
                    break;

                case "YZ":
                    actor.SetOrientation(0, 90, 0);  // Rotate for YZ
                    actor.SetPosition(0, 0, 0);     // Position at origin
                    break;

                case "XZ":
                    actor.SetOrientation(90, 0, 0);  // Rotate for XZ
                    actor.SetPosition(0, 0, 0);     // Position at origin
                    break;
            }

            actor.PickableOff(); // Prevent selection or interaction with the plane


            renderer.AddActor(actor);  // Add the plane to the renderer
        }

        private void XYButton_Click(object sender, EventArgs e)
        {
            Console.WriteLine("XY-plain selected...");
            // Clear all actors before setting the new plane
            RemoveAllActors();  // Custom method to remove all actors
            DrawPlane("XY");

            var camera = renderer.GetActiveCamera();
            camera.SetPosition(0, 0, 1);  // Look down from the Z-axis
            camera.SetViewUp(0, 1, 0);   // Orient along the Y-axis

            // Set parallel projection scale to control zoom level
            camera.SetParallelScale(300);  // Adjust to achieve desired zoom


            var interactor = renderWindowControl1.RenderWindow.GetInteractor();
            interactor.SetInteractorStyle(vtkInteractorStyleTrackballCamera.New()); // Use fixed camera


            renderWindowControl1.RenderWindow.Render();  // Re-render the scene
        }

        private void YZButton_Click(object sender, EventArgs e)
        {
            Console.WriteLine("XY-plain selected...");
            RemoveAllActors();  // Custom method to remove all actors
            DrawPlane("YZ");

            var camera = renderer.GetActiveCamera();
            camera.SetPosition(1, 0, 0);  // Look down from the X-axis
            camera.SetViewUp(0, 0, 1);   // Orient along Z-axis

            renderWindowControl1.RenderWindow.Render();  // Re-render the scene
        }
        private void ZXButton_Click(object sender, EventArgs e)
        {
            Console.WriteLine("XY-plain selected...");
            RemoveAllActors();  // Custom method to remove all actors
            DrawPlane("XZ");

            var camera = renderer.GetActiveCamera();
            camera.SetPosition(0, 1, 0);  // Look down from the Y-axis
            camera.SetViewUp(0, 0, 1);   // Orient along Z-axis

            renderWindowControl1.RenderWindow.Render();  // Re-render the scene
        }
        

    }
}
